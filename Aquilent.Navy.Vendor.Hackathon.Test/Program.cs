using System;
using Amazon.IonDotnet.Tree;
using Amazon.IonDotnet.Tree.Impl;
using Amazon.QLDB.Driver;
using Aquilent.Navy.Vendor.Hackathon;

namespace Amazon.QLDB.QuickStartGuide
{
    class Program
    {
        static IValueFactory valueFactory = new ValueFactory();

        static void Main(string[] args)
        {
            var driver = new AmazonQLDB();


            var opportunityId = 1;
            var contractId = 2;

            var proposal = new AwsTeamProposal();
            proposal.OpportunityId = opportunityId;
            proposal.ProposalId = 1;
            proposal.PrimeCompany = "Prime Company A";
            proposal.PrimeCompanyId = 3;
            proposal.Company = "Company Z";
            proposal.CompanyId = 4;
            proposal.ContractId = contractId;
            proposal.ContractNumber = "N0024";
            proposal.DateSubmitted = DateTime.Now;
            proposal.BidderId = 38;
            proposal.BidderName = "Teams Demo";
            proposal.IsPrime = true;
            proposal.IsSuperseded = false;


            driver.AddTeamProposal(proposal);

            var teamProposals = driver.GetTeamProposals(opportunityId, contractId);

            Console.Write("Press any key to finish...");
            Console.ReadKey();
        }
    }
}