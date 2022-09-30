using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Aquilent.Navy.EntityAccess.Infrastructure;
using Aquilent.Navy.Infrastructure.Managers;
using Aquilent.Navy.Infrastructure.Events;
using Aquilent.Navy.Core;
using Aquilent.Navy.Lookup;
using Aquilent.Navy.Core.Lookups;
using Aquilent.Navy.Pricing;
using Aquilent.Navy.Settings.Lookups;
using Aquilent.Navy.Email.Client;
using System.Data.Entity.SqlServer;
using Aquilent.Navy.Vendor.Enum;
using Kendo.Mvc.Extensions;
using Aquilent.Navy.Audit;
using Aquilent.Navy.Vendor.Lookups;
using Aquilent.Navy.Infrastructure.Enum;
using Aquilent.Navy.Security;
using log4net;
using Aquilent.Navy.Vendor.Hackathon;

namespace Aquilent.Navy.Vendor
{
    public class OpportunityManager : AbstractEntityManager
    {
        #region Properties
        private AmazonQLDB AmazonQLDB { get; set; }
        private IOpportunityRepository OpportunityRepository { get; set; }
        private IRepository<OpportunityInvitee> OpportunityInviteeRepository { get; set; }
        private IRepository<OpportunityInviteExtension> OpportunityInviteExtensionRepository { get; set; }
        private IRepository<Solicitation> SolicitationRepository { get; set; }
        private IRepository<VendorUser> VendorUserRepository { get; set; }
        private IRepository<Procurement> ProcurementRepository { get; set; }
        private IRepository<Company> CompanyRepository { get; set; }
        private IRepository<Question> QuestionRepository { get; set; }
        private IRepository<Proposal> ProposalRepository { get; set; }

        private IRepository<ProposalAttachment> ProposalAttachmentRepository { get; set; }
        private IRepository<OpportunityAttachment> OpportunityAttachmentRepository { get; set; }
        private IRepository<SmallBusinessSubcontractingGoal> SmallBusinessSubcontractingGoalRepository { get; set; }
        private PricingManager PricingManager { get; set; }
        private GovernmentUserManager GovernmentUserManager { get; set; }
        private SystemUserManager SystemUserManager { get; set; }
        private ProcurementManager ProcurementManager { get; set; }
        private VendorUserManager VendorUserManager { get; set; }
        private UserManager<BasicUser> BasicUserManager { get; set; }
        private CompanyManager CompanyManager { get; set; }

        private ContractManager ContractManager { get; set; }
        private AuditManager AuditManager { get; set; }

        private IPackageRepository PackageRepository { get; set; }
        #endregion

        #region Constructors

        /// <exception cref="ArgumentNullException"><paramref name="opportunityRepository"/> is <see langword="null" />.</exception>
        public OpportunityManager(IOpportunityRepository opportunityRepository,
            IRepository<Solicitation> solicitationRepository,
            IRepository<VendorUser> vendorUserRepository,
            IRepository<Procurement> procurementRepository,
            IRepository<OpportunityInvitee> opportunityInviteeRepository,
            IRepository<OpportunityInviteExtension> opportunityInviteExtensionRepository,
            IRepository<Company> companyRepository,
            IRepository<Question> questionRepository,
            IRepository<Proposal> proposalRepository,
            IRepository<ProposalAttachment> proposalAttachmentRepository,
            IRepository<OpportunityAttachment> opportunityAttachmentReposity,
            IRepository<SmallBusinessSubcontractingGoal> smallBusinessSubcontractingGoalRepository,
            PricingManager pricingManager,
            GovernmentUserManager governmentUserManager,
            VendorUserManager vendorUserManager,
            SystemUserManager systemUserManager,
            UserManager<BasicUser> basicUserManager,
            CompanyManager companyManager,
            ContractManager contractManager,
            ProcurementManager procurementManager,
            AuditManager auditManager,
            IPackageRepository packageRepository,
            AmazonQLDB amazonQLDB)
        {
            AmazonQLDB = amazonQLDB;

            // Make sure all of the repositories that reference the same
            // type of DbContext share the same context

            opportunityRepository.ShareContext(solicitationRepository);
            opportunityRepository.ShareContext(vendorUserRepository);
            opportunityRepository.ShareContext(procurementRepository);
            opportunityRepository.ShareContext(opportunityRepository);
            opportunityRepository.ShareContext(opportunityInviteeRepository);
            opportunityRepository.ShareContext(opportunityInviteExtensionRepository);
            opportunityRepository.ShareContext(companyRepository);
            opportunityRepository.ShareContext(questionRepository);
            opportunityRepository.ShareContext(proposalRepository);
            opportunityRepository.ShareContext(proposalAttachmentRepository);
            opportunityRepository.ShareContext(opportunityAttachmentReposity);
            opportunityRepository.ShareContext(smallBusinessSubcontractingGoalRepository);

            OpportunityRepository = opportunityRepository;
            SmallBusinessSubcontractingGoalRepository = smallBusinessSubcontractingGoalRepository;
            SolicitationRepository = solicitationRepository;
            VendorUserRepository = vendorUserRepository;
            ProcurementRepository = procurementRepository;
            OpportunityInviteeRepository = opportunityInviteeRepository;
            OpportunityInviteExtensionRepository = opportunityInviteExtensionRepository;
            CompanyRepository = companyRepository;
            QuestionRepository = questionRepository;
            ProposalRepository = proposalRepository;
            ProposalAttachmentRepository = proposalAttachmentRepository;
            OpportunityAttachmentRepository = opportunityAttachmentReposity;
            PricingManager = pricingManager;
            GovernmentUserManager = governmentUserManager;
            VendorUserManager = vendorUserManager;
            SystemUserManager = systemUserManager;
            BasicUserManager = basicUserManager;
            ContractManager = contractManager;
            CompanyManager = companyManager;
            ProcurementManager = procurementManager;
            PackageRepository = packageRepository;
            AuditManager = auditManager;
        }
        #endregion

        public void EditProposal(Proposal proposal)
        {
            var opportunity = Get(proposal.OpportunityId);
            var isSubmission = false;

            string auditNotes = "Proposal Updated for opportunity " + opportunity.OpportunityName;

            if (proposal.DateSubmitted.HasValue)
            {
                auditNotes = "Proposal Submitted for opportunity " + opportunity.OpportunityName;
                proposal.ProposalStatusId = (int) ProposalStatus.Submitted;
                LinkProposals(proposal);

                isSubmission = true;
            }

            ProposalRepository.Edit(proposal);
            ProposalRepository.Save();

            if (isSubmission)
            {
                var teamProposals = AmazonQLDB.GetTeamProposals(opportunity.OpportunityId, proposal.ContractId);
                var currentProposal = teamProposals.Where(x => x.CompanyId == proposal.CompanyId).FirstOrDefault();
                if (currentProposal != null)
                {
                    currentProposal.IsSuperseded = true;
                    AmazonQLDB.EditTeamProposal(currentProposal);
                }

                var awsTeamProposal = new AwsTeamProposal();
                awsTeamProposal.OpportunityId = proposal.OpportunityId;
                awsTeamProposal.ProposalId = proposal.ProposalId;
                awsTeamProposal.PrimeCompany = proposal.Contract.Company.Name;
                awsTeamProposal.PrimeCompanyId = proposal.Contract.CompanyId;
                awsTeamProposal.Company = proposal.Company.Name;
                awsTeamProposal.CompanyId = proposal.Company.OrganizationId;
                awsTeamProposal.ContractId = proposal.ContractId;
                awsTeamProposal.ContractNumber = proposal.Contract.ContractNumber;
                awsTeamProposal.DateSubmitted = proposal.DateSubmitted.Value;
                awsTeamProposal.BidderId = proposal.LastBidderId;
                awsTeamProposal.BidderName = $"{proposal.BasicUser.FirstName + " " + proposal.BasicUser.LastName}";
                awsTeamProposal.IsPrime = proposal.IsPrime;
                awsTeamProposal.IsSuperseded = false;

                AmazonQLDB.AddTeamProposal(awsTeamProposal);
            }

            OnEntityModified(proposal, new EntityActionEventArgs(EntityActionEventType.Edit, true, auditNotes));
        }
    }
}