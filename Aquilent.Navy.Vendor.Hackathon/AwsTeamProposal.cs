using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aquilent.Navy.Vendor.Hackathon
{
    public partial class AwsTeamProposal
    {
        public int OpportunityId { get; set; }
        public string TeamProposalId { get; set; }
        public int ProposalId { get; set; }
        public string PrimeCompany { get; set; }
        public string ContractNumber { get; set; }
        public string Company { get; set; }
        public DateTime DateSubmitted { get; set; }
        public string BidderName { get; set; }
        public bool IsPrime { get; set; }
        public int PrimeCompanyId { get; set; }
        public int ContractId { get; set; }
        public int CompanyId { get; set; }
        public int BidderId { get; set; }
        public bool IsSuperseded { get; set; }
    }
}
