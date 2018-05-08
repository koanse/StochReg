using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace StochReg
{
    public partial class StageForm : Form
    {
        public StageForm(string[] arr)
        {
            InitializeComponent();
            foreach (string s in arr)
                lbData.Items.Add(s);
        }

        private void btnX_Click(object sender, EventArgs e)
        {
            try
            {
                int i = lbData.SelectedIndex;
                if (i == -1)
                    return;
                lbX.Items.Add(lbData.SelectedItem);
                lbData.Items.RemoveAt(i);
                lbData.SelectedIndex = i;
            }
            catch { }
        }

        private void btnU_Click(object sender, EventArgs e)
        {
            try
            {
                int i = lbData.SelectedIndex;
                if (i == -1)
                    return;
                lbU.Items.Add(lbData.SelectedItem);
                lbData.Items.RemoveAt(i);
                lbData.SelectedIndex = i;
            }
            catch { }
        }

        private void btnY_Click(object sender, EventArgs e)
        {
            try
            {
                int i = lbData.SelectedIndex;
                if (i == -1)
                    return;
                lbY.Items.Add(lbData.SelectedItem);
                lbData.Items.RemoveAt(i);
                lbData.SelectedIndex = i;
            }
            catch { }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            lbX.Items.Clear();
            lbU.Items.Clear();
            lbY.Items.Clear();
        }
    }
}
