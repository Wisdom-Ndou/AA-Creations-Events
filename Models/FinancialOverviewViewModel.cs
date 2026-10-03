using System.Collections.Generic;

namespace WebApplication1.Models
{
    public class FinancialOverviewViewModel
    {
        public decimal Income { get; set; }
        public decimal TotalExpenditure { get; set; }
        public decimal Profit { get { return Income - TotalExpenditure; } }
        public List<Expenditure> Expenditures { get; set; } = new List<Expenditure>();
    }
}
