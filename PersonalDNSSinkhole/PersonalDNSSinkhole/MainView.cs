using SinkholeLibrary;
using System;   

namespace PersonalDNSSinkhole
{
    public partial class MainView : Form
    {
        private Sinkhole hole;

        public MainView()
        {
            InitializeComponent();
            DatabaseHandler.InitializeDatabase();
            hole = new Sinkhole();
            hole.NewDomain += OnNewDomain;
        }

        private void OnNewDomain(object sender, string domain)
        {
            richTextBox1.AppendText(domain + "\n");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            hole.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            PersonalBlocklist blockPage = new PersonalBlocklist();
            blockPage.Show();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox1.Checked)
            {
                hole.Start();
                WipeTextBox();
            }
            else
            {
                hole.Stop();
            }
        }

        //When the app is closing
        private void MainView_FormClosing(object sender, FormClosingEventArgs e)
        {
            hole.Stop();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            StatsPage statsPage = new StatsPage();
            statsPage.Show();
        }

        public async void WipeTextBox()
        {
            await Task.Delay(5000);
            this.richTextBox1.Text = string.Empty;
        }
    }
}
