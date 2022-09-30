using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.IonDotnet.Tree;
using Amazon.IonDotnet.Tree.Impl;
using Amazon.QLDB.Driver;
using Amazon.QLDBSession;

namespace Aquilent.Navy.Vendor.Hackathon
{
    public class AmazonQLDB
    {
        private static readonly IValueFactory valueFactory = new ValueFactory();

        private readonly IQldbDriver AwsDriver;

        public AmazonQLDB()
        {
            AwsDriver = QldbDriver.Builder().WithLedger("navsea-nxg").Build();
        }

        public void AddTeamProposal(AwsTeamProposal awsTeamProposal)
        {
            var teamProposal = valueFactory.NewEmptyStruct();

            teamProposal.SetField("OpportunityId", valueFactory.NewInt(awsTeamProposal.OpportunityId));

            teamProposal.SetField("ContractId", valueFactory.NewInt(awsTeamProposal.ContractId));
            teamProposal.SetField("ContractNumber", valueFactory.NewString(awsTeamProposal.ContractNumber));

            teamProposal.SetField("CompanyId", valueFactory.NewInt(awsTeamProposal.CompanyId));
            teamProposal.SetField("CompanyName", valueFactory.NewString(awsTeamProposal.Company));

            teamProposal.SetField("PrimeCompanyId", valueFactory.NewInt(awsTeamProposal.PrimeCompanyId));
            teamProposal.SetField("PrimeCompany", valueFactory.NewString(awsTeamProposal.PrimeCompany));

            teamProposal.SetField("ProposalId", valueFactory.NewInt(awsTeamProposal.ProposalId));

            teamProposal.SetField("DateSubmitted", valueFactory.NewString(awsTeamProposal.DateSubmitted.ToString("MM/dd/yyyy")));

            teamProposal.SetField("BidderId", valueFactory.NewInt(awsTeamProposal.BidderId));
            teamProposal.SetField("BidderName", valueFactory.NewString(awsTeamProposal.BidderName));

            teamProposal.SetField("IsPrime", valueFactory.NewBool(awsTeamProposal.IsPrime));
            teamProposal.SetField("IsSuperseded", valueFactory.NewBool(awsTeamProposal.IsSuperseded));


            var result = AwsDriver.Execute(txn =>
            {
                return txn.Execute("INSERT INTO TeamProposal ?", teamProposal);
            });

            foreach (var row in result)
            {
                Console.WriteLine($"Record added: {row.ToPrettyString()}");
            }
        }

        public void EditTeamProposal(AwsTeamProposal awsTeamProposal)
        {
        }

        public void DeleteTeamProposal(int opportunityId, int contractId, int proposalId)
        {
            var result = AwsDriver.Execute(txn =>
            {
                return txn.Execute("DELETE FROM TeamProposal WHERE ProposalId = ? and OpportunityId = ? and ContractId = ?", valueFactory.NewInt(proposalId), valueFactory.NewInt(opportunityId), valueFactory.NewInt(contractId));
            });

            foreach (var row in result)
            {
                Console.WriteLine($"Record deleted: {row.ToPrettyString()}");
            }
        }

        public IList<AwsTeamProposal> GetTeamProposals(int opportunityId, int contractId)
        {
            var teamProposals = new List<AwsTeamProposal>();

            var result = AwsDriver.Execute(txn =>
            {
                return txn.Execute("SELECT * FROM TeamProposal WHERE OpportunityId = ? and ContractId = ?", valueFactory.NewInt(opportunityId), valueFactory.NewInt(contractId));
            });

            foreach(var row in result)
            {
                var teamProposal = new AwsTeamProposal();
                teamProposal.OpportunityId = row.GetField("OpportunityId").IntValue;
                teamProposal.ProposalId = row.GetField("ProposalId").IntValue;

                teamProposal.ContractId = row.GetField("ContractId").IntValue;
                teamProposal.ContractNumber = row.GetField("ContractNumber").StringValue;

                teamProposal.CompanyId = row.GetField("CompanyId").IntValue;
                teamProposal.Company = row.GetField("CompanyName").StringValue;

                teamProposal.PrimeCompanyId = row.GetField("PrimeCompanyId").IntValue;
                teamProposal.PrimeCompany = row.GetField("PrimeCompany").StringValue;

                teamProposal.ProposalId = row.GetField("ProposalId").IntValue;

                teamProposal.DateSubmitted = DateTime.Parse(row.GetField("DateSubmitted").StringValue);

                teamProposal.BidderId = row.GetField("BidderId").IntValue;
                teamProposal.BidderName = row.GetField("BidderName").StringValue;

                teamProposal.IsPrime = row.GetField("IsPrime").BoolValue;
                teamProposal.IsSuperseded = row.GetField("IsSuperseded").BoolValue;


                teamProposals.Add(teamProposal);
            }

            return teamProposals;
       }
    }
}
