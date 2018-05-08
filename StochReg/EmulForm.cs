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
            tbI.Text = "0 1";
            tbMult.Text = "3";
            tbDU.Text = "0,001";
            tbDF.Text = "0,001";
            tbUInit.Text = "0,01";
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
            checkBox1.Checked = true;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void cbTEmul_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                dgvU.Rows.Clear();
                dgvY.Rows.Clear();
                Technology t = (Technology)cbTEmul.SelectedItem;
                Variable[] arrU, arrY;
                string rep;
                Program.Unite(t.lStage.ToArray(), out arrU, out arrY, out rep);
                for (int i = 0; i < arrU.Length; i++)
                {
                    if (checkBox1.Checked)
                        dgvU.Rows.Add(arrU[i].name, -1, 1, 1);
                    else
                        dgvU.Rows.Add(arrU[i].name, arrU[i].Inv(-1), arrU[i].Inv(1), arrU[i].sigma);

                }
                for (int i = 0; i < arrY.Length; i++)
                {
                    if (checkBox1.Checked)
                        dgvY.Rows.Add(arrY[i].name, -1, 1, 1, 0);
                    else
                        dgvY.Rows.Add(arrY[i].name, arrY[i].Inv(-1), arrY[i].Inv(1), 1, arrY[i].av);

                }
                cbTReg.SelectedItem = t;
            }
            catch { }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                Technology t = (Technology)cbTEmul.SelectedItem;
                Variable[] arrU, arrY;
                string rep;
                Program.Unite(t.lStage.ToArray(), out arrU, out arrY, out rep);
                for (int i = 0; i < arrU.Length; i++)
                {
                    if (checkBox1.Checked)
                    {
                        dgvU[1, i].Value = arrU[i].Norm(double.Parse(dgvU[1, i].Value.ToString())).ToString("g5");
                        dgvU[2, i].Value = arrU[i].Norm(double.Parse(dgvU[2, i].Value.ToString())).ToString("g5");
                        dgvU[3, i].Value = (double.Parse(dgvU[3, i].Value.ToString()) / arrU[i].sigma).ToString("g5");
                    }
                    else
                    {
                        dgvU[1, i].Value = arrU[i].Inv(double.Parse(dgvU[1, i].Value.ToString())).ToString("g5");
                        dgvU[2, i].Value = arrU[i].Inv(double.Parse(dgvU[2, i].Value.ToString())).ToString("g5");
                        dgvU[3, i].Value = (double.Parse(dgvU[3, i].Value.ToString()) * arrU[i].sigma).ToString("g5");;
                    }
                }
                for (int i = 0; i < arrY.Length; i++)
                {
                    if (checkBox1.Checked)
                    {
                        dgvY[1, i].Value = arrY[i].Norm(double.Parse(dgvY[1, i].Value.ToString())).ToString("g5");
                        dgvY[2, i].Value = arrY[i].Norm(double.Parse(dgvY[2, i].Value.ToString())).ToString("g5");
                        dgvY[4, i].Value = arrY[i].Norm(double.Parse(dgvY[4, i].Value.ToString())).ToString("g5");
                    }
                    else
                    {
                        dgvY[1, i].Value = arrY[i].Inv(double.Parse(dgvY[1, i].Value.ToString())).ToString("g5");
                        dgvY[2, i].Value = arrY[i].Inv(double.Parse(dgvY[2, i].Value.ToString())).ToString("g5");
                        dgvY[4, i].Value = arrY[i].Inv(double.Parse(dgvY[4, i].Value.ToString())).ToString("g5");
                    }
                }
            }
            catch { }
        }
    }
}
