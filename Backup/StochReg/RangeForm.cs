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
    public partial class RangeForm : Form
    {
        public RangeForm(int n)
        {
            InitializeComponent();
            tbRMin.Text = "0";
            tbRMax.Text = string.Format("{0}", n / 2);
            tbEMin.Text = string.Format("{0}", n / 2 + 1);
            tbEMax.Text = string.Format("{0}", n - 1);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
