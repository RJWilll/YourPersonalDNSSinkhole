using SinkholeLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PersonalDNSSinkhole
{
    public partial class PersonalBlocklist : Form
    {
        string BLOCK_PATH = $"{Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.Parent.FullName}\\personalblocklist.txt";

        public PersonalBlocklist()
        {
            InitializeComponent();
            this.InitalizeBlockList();

        }

        public void InitalizeBlockList()
        {
            using (FileStream fileStream = new FileStream(BLOCK_PATH, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    while (!reader.EndOfStream)
                    {
                        string textContent = reader.ReadLine();
                        this.listBox1.Items.Add(textContent);
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //add button
            if(this.textBox1.Text != string.Empty)
            {
                this.listBox1.Items.Add(this.textBox1.Text);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //delete button
            if(this.listBox1.SelectedItem?.ToString() != string.Empty)
            {
                this.listBox1.Items.RemoveAt(this.listBox1.SelectedIndex);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //save button
            using (FileStream fileStream = new FileStream(BLOCK_PATH, FileMode.Open, FileAccess.Write))
            {
                using (var writer = new StreamWriter(fileStream))
                {
                    foreach (object item in listBox1.Items)
                    {
                        writer.WriteLine($"{item.ToString()}");
                    }
                }
            }

            this.Close();
        }
    }
}
