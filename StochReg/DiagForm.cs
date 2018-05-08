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
    public partial class DiagForm : Form
    {
        Variable[] arrBefore,arrAfter;
        public DiagForm(Variable[] arrBefore, Variable[] arrAfter)
        {
            this.arrBefore = arrBefore;
            this.arrAfter = arrAfter;
            InitializeComponent();
            foreach (Variable v in arrBefore)
            {
                lb.Items.Add(v.name);
            }            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void lb_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int i = lb.SelectedIndex;
                Sample sb = new Sample(arrBefore[i].arr), sa = new Sample(arrAfter[i].arr);
                sb.DoHistogram(Properties.Settings.Default.grid);
                sa.DoHistogram(sb.arrMid);
                chart1.Series[0].Points.Clear();
                chart1.Series[1].Points.Clear();
                for (int j = 0; j < sb.arrMid.Length; j++)
                {
                    chart1.Series[0].Points.AddXY(arrBefore[i].Inv(sb.arrMid[j]), sb.arrP[j]);
                    chart1.Series[1].Points.AddXY(arrAfter[i].Inv(sa.arrMid[j]), sa.arrP[j]);
                }
            }
            catch { }
        }
    }
}
