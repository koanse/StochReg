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
    public partial class EmulForm : Form
    {
        public EmulForm(List<Technology> lTech)
        {
            InitializeComponent();
            cbTEmul.Items.AddRange(lTech.ToArray());
            cbTReg.Items.AddRange(lTech.ToArray());
            tbP.Text = "0,5";
            tbMult.Text = "3";
            tbDU.Text = "0,001";
            tbDF.Text = "0,001";
            tbUInit.Text = "0";
            tbDUInit.Text = "0,1";
            tbSInit.Text = "1";
            tbDSInit.Text = "0,1";
            tbIter.Text = "100";
            tbR.Text = "1";
            tbC.Text = "4";
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void cbTEmul_SelectedIndexChanged(object sender, EventArgs e)
        {
            //try
            {
                dgvU.Rows.Clear();
                Technology t = (Technology)cbTEmul.SelectedItem;
                Variable[] arrU, arrY;
                Program.Unite(t.lStage.ToArray(), out arrU, out arrY);
                for (int i = 0; i < arrU.Length; i++)
                {
                    dgvU.Rows.Add(arrU[i].name, -1, 1, 0.1);
                }
                for (int i = 0; i < arrY.Length; i++)
                {
                    dgvY.Rows.Add(arrY[i].name, -1, 1, 1, 0);
                }
                cbTReg.SelectedItem = t;
            }
            //catch { }
        }
    }
}
