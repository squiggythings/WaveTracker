using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WaveTracker.Forms
{
    public partial class EnterText : Form
    {

        public string result;
        public EnterText()
        {
            Application.EnableVisualStyles();
            InitializeComponent();
        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            result = textBox.Text + "";
            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            result = "\tcanceled";
            Close();
        }

        private void tbKeyDown(object sender, KeyEventArgs e)
        {
            
        }
    }
}
