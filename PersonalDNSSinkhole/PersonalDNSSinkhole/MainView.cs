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
    }
}
