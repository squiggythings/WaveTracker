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
    public partial class EditSongSettings : Form
    {
        public EditSongSettings()
        {
            Application.EnableVisualStyles();
            InitializeComponent();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void songSpeed_TextChanged(object sender, EventArgs e)
        {

        }

        private void Reset_Click(object sender, EventArgs e)
        {
            tickRate.Value = 60;
        }

        private void quantizeChannelAmplitude_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void tickRate_Scroll(object sender, EventArgs e)
        {
            hertzLabel.Text = tickRate.Value + " Hz";
            samplesLabel.Text = "(" + 1 / tickRate.Value * (41000) + " samples per tick)";
        }

        private void tickRate_ValueChanged(object sender, EventArgs e)
        {
            hertzLabel.Text = tickRate.Value + " Hz";
            samplesLabel.Text = "(" + (41000 / tickRate.Value) + " samples per tick)";
        }
    }
}
