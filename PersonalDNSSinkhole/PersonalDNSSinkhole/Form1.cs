using SinkholeLibrary;
using System;   

namespace PersonalDNSSinkhole
{
    public partial class Form1 : Form
    {
        private Sinkhole hole;

        public Form1()
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
    }
}
