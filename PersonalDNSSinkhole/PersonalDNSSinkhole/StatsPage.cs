using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PersonalDNSSinkhole
{
    public partial class StatsPage : Form
    {
        public StatsPage()
        {
            InitializeComponent();
            this.SetupStats();
        }

        private DataAnalysisHandler _analytics;

        public List<(string Domain, long Count)> TopDomains;
        public List<(string Domain, double AvgMs)> SlowestDomains;


        public void SetupStats()
        {
            _analytics = new DataAnalysisHandler($"{Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.Parent.FullName}\\personalSinkhole.db");
            this.label11.Text = _analytics.GetTotalQueries().ToString();
            this.label12.Text = _analytics.GetAverageQueriesPerDay().ToString();
            this.label13.Text = _analytics.GetBlockRate().ToString();
            this.label14.Text = _analytics.GetAverageResponseTime().ToString();
            this.label15.Text = _analytics.GetUniqueDomainsToday().ToString();
            this.label16.Text = _analytics.GetLastQuery().ToString();

            TopDomains = _analytics.GetTopQueriedDomains();
            SlowestDomains = _analytics.GetSlowestDomains();

            dataGridView1.ColumnCount = 2;
            dataGridView2.ColumnCount = 2;

            dataGridView1.Rows.Add("Domain", "Count");
            dataGridView2.Rows.Add("Domain", "Average Time in Ms");

            foreach( (string domain, long count) in TopDomains)
            {
                dataGridView1.Rows.Add(domain, count.ToString());
            }

            foreach ((string domain, double ms) in SlowestDomains)
            {
                dataGridView2.Rows.Add(domain, ms.ToString());
            }
        }
    }
}
