using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PersonalDNSWeb.Pages
{
    public class IndexModel : PageModel
    {
        private DataAnalysisHandler _analytics;

        // Bound properties
        public long TotalQueries { get; set; }
        public double AvgQueriesPerDay { get; set; }
        public double BlockRate { get; set; }
        public double AvgResponseTime { get; set; }
        public long UniqueDomainsToday { get; set; }
        public (string Timestamp, string Domain)? LastQuery { get; set; }

        public List<(string Domain, long Count)> TopDomains { get; set; }
        public List<(string Domain, double AvgMs)> SlowestDomains { get; set; }
        public List<(string Date, double BlockRate)> BlockRateOverTime { get; set; }
        public List<(int Hour, long Count)> PeakUsage { get; set; }
        public List<(int Hour, long Count)> TopQueryTimes { get; set; }

        public void OnGet()
        {
            _analytics = new DataAnalysisHandler($"C:\\Users\\reedj\\random_git_repos\\YourPersonalDNSSinkhole\\PersonalDNSSinkhole\\personalSinkhole.db");
            TotalQueries = _analytics.GetTotalQueries();
            AvgQueriesPerDay = _analytics.GetAverageQueriesPerDay();
            BlockRate = _analytics.GetBlockRate();
            AvgResponseTime = _analytics.GetAverageResponseTime();
            UniqueDomainsToday = _analytics.GetUniqueDomainsToday();
            LastQuery = _analytics.GetLastQuery();
            TopDomains = _analytics.GetTopQueriedDomains();
            SlowestDomains = _analytics.GetSlowestDomains();
            BlockRateOverTime = _analytics.GetBlockRateOverTime();
            PeakUsage = _analytics.GetPeakUsageTimes();
            TopQueryTimes = _analytics.GetTopQueryTimes();
        }
    }
}
