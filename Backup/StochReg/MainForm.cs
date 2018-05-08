using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.OleDb;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace StochReg
{
    public partial class MainForm : Form
    {
        List<Technology> lTech = new List<Technology>();
        public MainForm()
        {
            InitializeComponent();
            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream fs = new FileStream("tech.dt", FileMode.Open);
                lTech = (List<Technology>)bf.Deserialize(fs);
                fs.Close();
                foreach (Technology t in lTech)
                {
                    lbTech.Items.Add(t.name);
                }
            }
            catch { }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            TechForm tf = new TechForm();
            if (tf.ShowDialog() != DialogResult.OK)
                return;
            lTech.Add(new Technology() { name = tf.tbName.Text, lStage = tf.lStage });
            lbTech.Items.Add(tf.tbName.Text);
            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream fs = new FileStream("tech.dt", FileMode.Create);
                bf.Serialize(fs, lTech);
                fs.Close();
            }
            catch { }
        }

        private void btnDel_Click(object sender, EventArgs e)
        {
            try
            {
                int i = lbTech.SelectedIndex;
                lbTech.Items.RemoveAt(i);
                lTech.RemoveAt(i);
                BinaryFormatter bf = new BinaryFormatter();
                FileStream fs = new FileStream("tech.dt", FileMode.Create);
                bf.Serialize(fs, lTech);
                fs.Close();
            }
            catch { }
        }

        private void lbStage_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int i = lbTech.SelectedIndex;
                //wbStage.DocumentText = lStage[i].ToString();
            }
            catch { }
        }

        private void normToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*string s = "";
            foreach (Stage st in lStage)
            {
                s += "Входы<br>";
                foreach (Variable v in st.arrX)
                {
                    Sample smp = new Sample(v.name, v.id, v.arr);
                    s += smp.GetHypReport();
                }
                s += "Управления<br>";
                foreach (Variable v in st.arrU)
                {
                    Sample smp = new Sample(v.name, v.id, v.arr);
                    s += smp.GetHypReport();
                }
                s += "Выходы<br>";
                foreach (Variable v in st.arrY)
                {
                    Sample smp = new Sample(v.name, v.id, v.arr);
                    s += smp.GetHypReport();
                }
            }
            RepForm rf = new RepForm(s, "Проверка нормальности");
            rf.Show();*/
        }

        private void btnEmul_Click(object sender, EventArgs e)
        {
            EmulForm ef = new EmulForm(lTech);
            if (ef.ShowDialog() != DialogResult.OK)
                return;
            
            Technology tEmul = (Technology)ef.cbTEmul.SelectedItem, tReg = (Technology)ef.cbTReg.SelectedItem;
            double p = double.Parse(ef.tbP.Text), mult = double.Parse(ef.tbMult.Text);
            double du = double.Parse(ef.tbDU.Text), df = double.Parse(ef.tbDF.Text);
            double uInit = double.Parse(ef.tbUInit.Text), duInit = double.Parse(ef.tbDUInit.Text);
            double sInit = double.Parse(ef.tbSInit.Text), dsInit = double.Parse(ef.tbDSInit.Text);
            int iter = int.Parse(ef.tbIter.Text);
            double R = double.Parse(ef.tbR.Text), C = double.Parse(ef.tbC.Text);
            Variable[] arrU, arrY;
            Program.Unite(tReg.lStage.ToArray(), out arrU, out arrY);
            double[] arrUMin = new double[arrU.Length], arrUMax = new double[arrU.Length],
                arrUSMin = new double[arrU.Length], arrYMin = new double[arrY.Length],
                arrYMax = new double[arrY.Length], arrAlpha = new double[arrY.Length],
                arrYOpt = new double[arrY.Length];
            for (int i = 0; i < ef.dgvU.Rows.Count; i++)
			{
                arrUMin[i] = double.Parse(ef.dgvU[1, i].Value.ToString());
                arrUMax[i] = double.Parse(ef.dgvU[2, i].Value.ToString());
                arrUSMin[i] = double.Parse(ef.dgvU[3, i].Value.ToString());
			}
            for (int i = 0; i < ef.dgvY.Rows.Count; i++)
			{
                arrYMin[i] = double.Parse(ef.dgvY[1, i].Value.ToString());
                arrYMax[i] = double.Parse(ef.dgvY[2, i].Value.ToString());
                arrAlpha[i] = double.Parse(ef.dgvY[3, i].Value.ToString());
                arrYOpt[i] = double.Parse(ef.dgvY[4, i].Value.ToString());
			}
            double[][] arrC;
            string[,] matrF;
            string repMod;
            Program.Model(arrU, arrY, out arrC, out matrF, out repMod);
            string rep;
            Program.Emulate(tReg.lStage.ToArray(), tEmul.lStage.ToArray(), p,
                arrUMin, arrUMax, arrUSMin, arrYMin, arrYMax, arrAlpha,
                arrYOpt, mult, du, df, uInit, duInit, sInit, dsInit, iter, R, C, out arrU, out arrY, out rep);
            wb.DocumentText = rep;    
        }


        /*       
        private void modelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string rep = "";
            Variable[] arrY, arrU;
            Import(out arrY, out arrU);
            Sample[] arrSmp = new Sample[arrU.Length];
            double[][] arrCoeff = new double[arrY.Length][];
            for (int i = 0; i < arrU.Length; i++)
                arrSmp[i] = new Sample(arrU[i].name, arrU[i].id, arrU[i].arr);
            for (int i = 0; i < arrY.Length; i++)
            {
                Sample[] arrTSmp = new Sample[arrSmp.Length];
                for (int j = 0; j < arrTSmp.Length; j++)
                    arrTSmp[j] = new TranSample(arrSmp[j], dgvU[i + 2, j].Value.ToString());
                Sample smp = new Sample(arrY[i].name, arrY[i].id, arrY[i].arr);
                Regression r = new Regression(smp, arrSmp);
                arrCoeff[i] = r.arrB;
                rep += r.GetRegReport() + "<br>";
            }
            string[] arrStage = new string[dgvStage.Rows.Count];
            for (int i = 0; i < arrStage.Length; i++)
                arrStage[i] = dgvStage[0, i].Value.ToString();
            StageForm sf = new StageForm(arrStage);
            sf.ShowDialog();
            List<string> lNU = new List<string>(), lU = new List<string>();
            int k;
            for (k = 0; k < arrStage.Length; k++)
                if (sf.stage == arrStage[k])
                    break;
                else
                    lNU.AddRange(dgvStage[1, k].Value.ToString().Split(new char[] { ' ', ',' },
                        StringSplitOptions.RemoveEmptyEntries));
            ExpForm ef = new ExpForm(lNU.ToArray(), arrU);
            ef.ShowDialog();
            for (int i = 0; i < arrU.Length; i++)
                if (!lNU.Contains(string.Format("{0}", i + 1)))
                    lU.Add(arrU[i].name);
            int p = lU.Count, n = arrY.Length;
            int[] arrI = new int[p], arrNI = new int[arrU.Length - p];
            for (int i = 0; i < p; i++)
                for (int j = 0; j < arrU.Length; j++)
                    if (arrU[j].name == lU[i])
                    {
                        arrI[i] = j;
                        break;
                    }
            for (int i = 0; i < arrNI.Length; i++)
                for (int j = 0; j < arrU.Length; j++)
                    if (arrU[j].name == lNU[i])
                    {
                        arrNI[i] = j;
                        break;
                    }
            string[,] matrFunc = new string[p, n];
            for (int i = 0; i < p; i++)
                for (int j = 0; j < n; j++)
                    matrFunc[i, j] = dgvU[j + 2, arrI[i]].Value.ToString();
            double[][] arrC = new double[n][];
            for (int j = 0; j < n; j++)
            {
                arrC[j] = new double[p + 1];
                double s = arrCoeff[j][0];
                for (int i = 0; i < arrU.Length; i++)
                {
                    for (k = 0; k < p; k++)
                        if (arrI[k] == i)
                            break;
                    if (k < p)
                        arrC[j][k + 1] = arrCoeff[j][i + 1];
                    else
                    {
                        for (k = 0; k < p; k++)
                            if (arrNI[k] == i)
                                break;
                        s += arrCoeff[j][i + 1] * F(ef.arr[k], dgvU[j + 2, i].Value.ToString());
                    }
                }
                arrC[j][0] = s;
            }
            double[] arrUMin = new double[p], arrUMax = new double[p], arrUSMin = new double[p];
            for (int i = 0; i < p; i++)
            {
                arrUMin[i] = double.Parse(dgvUParam[1, arrI[i]].Value.ToString());
                arrUMax[i] = double.Parse(dgvUParam[2, arrI[i]].Value.ToString());
                arrUSMin[i] = double.Parse(dgvUParam[3, arrI[i]].Value.ToString());
            }
            double[] arrYOpt = new double[n], arrYMin = new double[n], arrYMax = new double[n], arrAlpha = new double[n];
            for (int i = 0; i < n; i++)
            {
                arrYOpt[i] = double.Parse(dgvQParam[4, i].Value.ToString());
                arrYMin[i] = double.Parse(dgvQParam[2, i].Value.ToString());
                arrYMax[i] = double.Parse(dgvQParam[3, i].Value.ToString());
                arrAlpha[i] = double.Parse(dgvQParam[1, i].Value.ToString());
            }
            double R = double.Parse(tbR.Text);
            HJInitialParams init = new HJInitialParams(double.Parse(tbDU.Text), double.Parse(tbDF.Text),
                arrCoeff, matrFunc, arrUMin, arrUMax, arrUSMin, arrYOpt, arrYMin, arrYMax, arrAlpha, R);
            int iterNum = 0;
            List<HJIteration> lIter = new List<HJIteration>();
            double[] arrX = new double[2 * p], arrXDelta = new double[2 * p];
            double xEps = double.Parse(tbDU.Text);
            for (int i = 0; i < 2 * p; i++)
            {
                arrX[i] = 0.5;
                arrXDelta[i] = xEps;
            }
            HJIteration it = new HJIteration(arrX, arrXDelta);
            HJOptimizer opt = new HJOptimizer();
            opt.Initialize(init);
            //double C = double.Parse(tbC.Text);
            double f = double.MaxValue, fEps = double.Parse(tbDF.Text);
            rep += "ОПТИМИЗАЦИЯ МЕТОДОМ ХУКА-ДЖИВСА<br><table border = 1 cellspacing = 0><tr>";
            for (int i = 0; i < p; i++)
            {
                rep += "<td>" + lU[i];
            }
            rep += "<td>Целевая функция";
            rep += it.ToHtml(init, 3);
            //do
            {
                do
                {
                    it = (HJIteration)opt.DoIteration(it);
                    if (it == null)
                        break;
                    lIter.Add(it);
                    iterNum++;
                    rep += it.ToHtml(init, 3);
                }
                while (iterNum < 1000);
                //double FNext = lIter.Last().fRes;
                //if (Math.Abs(FNext - f) < fEps)
                //    break;
                //f = FNext;
                //R *= C;
                //init = new HJInitialParams(double.Parse(tbDU.Text), double.Parse(tbDF.Text),
                //    arrCoeff, matrFunc, arrUMin, arrUMax, arrUSMin, arrYOpt, arrYMin, arrYMax, arrAlpha, R);
                //it = new HJIteration(lIter.Last().arrX, arrXDelta);
            }
            //while (true);
            rep += "</table><br>";
            rep += "РЕЗУЛЬТАТЫ ОПТИМИЗАЦИИ<br>";
            for (int i = 0; i < p; i++)
                rep += string.Format("m<sub>{0}</sub> = {1:f4}<br>", i + 1, lIter.Last().arrX[i]);
            Random rnd = new Random();
            for (int i = 0; i < p; i++)
                rep += string.Format("s<sub>{0}</sub> = {1:f4}<br>", i + 1, lIter.Last().arrX[i + p]);
            rep += string.Format("<br>Целевая функция:<br>F = {0:f4}<br>", init.GetFuncValue(lIter.Last().arrX));

            rep += "<br>Значения параметров распределений показателей качества<br>";
            for (int i = 0; i < n; i++)
            {
                rep += string.Format("mu<sub>{0}</sub> = {1:f4}<br>", i + 1, init.arrMu[i]);
                rep += string.Format("sigma<sub>{0}</sub> = {1:f4}<br>", i + 1, init.arrSigma[i]);
            }
            rep += "<br>Значения показателей качества<br>";
            for (int i = 0; i < n; i++)
            {
                rep += string.Format("y<sub>{0}</sub> = {1:f4}<br>", i + 1, init.arrY[i]);
            }
            rep += "<br>Технологические факторы до и после оптимизации<br>";
            rep += "Нормированные средние значения до и после оптимизации<table border = 1 cellspacing = 0><tr>" +
                "<td>Величина<td>Среднее до оптимизации<td>Среднее после оптимизации";
            for (int i = 0; i < p; i++)
            {
                rep += string.Format("<tr><td>{0}<td>{1:f4}<td>{2:f4}", arrU[i].name, 0,
                    lIter.Last().arrX[i]);
            }
            rep += "</table><br>Средние значения до и после оптимизации<table border = 1 cellspacing = 0><tr>" +
                "<td>Величина<td>Среднее до оптимизации<td>Среднее после оптимизации";
            for (int i = 0; i < p; i++)
            {
                rep += string.Format("<tr><td>{0}<td>{1:f4}<td>{2:f4}", arrU[i].name, arrU[i].Inv(0),
                    arrU[i].Inv(lIter.Last().arrX[i]));
            }
            rep += "</table><br>Нормированные средние квадратические отклонения до и после оптимизации<table border = 1 cellspacing = 0><tr>" +
                "<td>Величина<td>СКО до оптимизации<td>СКО после оптимизации";
            for (int i = 0; i < p; i++)
            {
                rep += string.Format("<tr><td>{0}<td>{1:f4}<td>{2:f4}", arrU[i].name, 1,
                    lIter.Last().arrX[p + i]);
            }
            rep += "</table><br>Средние квадратические отклонения до и после оптимизации<table border = 1 cellspacing = 0><tr>" +
                "<td>Величина<td>СКО до оптимизации<td>СКО после оптимизации";
            for (int i = 0; i < p; i++)
            {
                rep += string.Format("<tr><td>{0}<td>{1:f4}<td>{2:f4}", arrU[i].name, arrU[i].sigma,
                    arrU[i].sigma * lIter.Last().arrX[p + i]);
            }
            rep += "</table>";

            rep += "<br>Показатели качества до и после оптимизации<br>";
            rep += "Нормированные средние значения до и после оптимизации<table border = 1 cellspacing = 0><tr>" +
                "<td>Величина<td>Среднее до оптимизации<td>Среднее после оптимизации";
            for (int i = 0; i < n; i++)
            {
                rep += string.Format("<tr><td>{0}<td>{1:f4}<td>{2:f4}", arrY[i].name, 0,
                    init.arrMu[i]);
            }
            rep += "</table><br>Средние значения до и после оптимизации<table border = 1 cellspacing = 0><tr>" +
                "<td>Величина<td>Среднее до оптимизации<td>Среднее после оптимизации";
            for (int i = 0; i < n; i++)
            {
                rep += string.Format("<tr><td>{0}<td>{1:f4}<td>{2:f4}", arrY[i].name, arrY[i].Inv(0),
                    arrY[i].Inv(init.arrMu[i]));
            }
            rep += "</table><br>Нормированные средние квадратические отклонения до и после оптимизации<table border = 1 cellspacing = 0><tr>" +
                "<td>Величина<td>СКО до оптимизации<td>СКО после оптимизации";
            for (int i = 0; i < n; i++)
            {
                rep += string.Format("<tr><td>{0}<td>{1:f4}<td>{2:f4}", arrY[i].name, 1,
                    init.arrSigma[i]);
            }
            rep += "</table><br>Средние квадратические отклонения до и после оптимизации<table border = 1 cellspacing = 0><tr>" +
                "<td>Величина<td>СКО до оптимизации<td>СКО после оптимизации";
            for (int i = 0; i < n; i++)
            {
                rep += string.Format("<tr><td>{0}<td>{1:f4}<td>{2:f4}", arrY[i].name, arrY[i].sigma,
                    arrY[i].sigma * init.arrSigma[i]);
            }
            rep += "</table>";

            List<Sample> lSmp = new List<Sample>();
            for (int i = 1; i < arrY.Length; i++)
            {
                lSmp.Add(new Sample(arrY[i].name, string.Format("y{0}", i + 1), arrY[i].arr));
            }
            lSmp.AddRange(arrSmp);
            Regression reg = new Regression(new Sample(arrY[0].name, "y0", arrY[0].arr), lSmp.ToArray());
            rep += reg.GetCorrReport();
            ResForm rf = new ResForm(rep);
            rf.Show();
            List<double[]> lArr = new List<double[]>();
            List<string> lName = new List<string>();
            List<double> lMu = new List<double>(), lSigma = new List<double>();
            for (int i = 0; i < arrU.Length; i++)
            {
                lName.Add(arrU[i].name);
                lMu.Add(arrU[i].Inv(lIter.Last().arrX[i]));
                lSigma.Add(arrU[i].sigma * lIter.Last().arrX[i + p]);
                double[] arr = new double[arrU[i].arr.Length];
                for (int j = 0; j < arr.Length; j++)
                {
                    arr[j] = arrU[i].Inv(arrU[i].arr[j]);
                }
                lArr.Add(arr);
            }
            for (int i = 0; i < arrY.Length; i++)
            {
                lName.Add(arrY[i].name);
                lMu.Add(arrY[i].Inv(init.arrMu[i]));
                lSigma.Add(arrY[i].sigma * init.arrSigma[i]);
                double[] arr = new double[arrY[i].arr.Length];
                for (int j = 0; j < arr.Length; j++)
                {
                    arr[j] = arrY[i].Inv(arrY[i].arr[j]);
                }
                lArr.Add(arr);
            }
            DistForm df = new DistForm(lName.ToArray(), lArr.ToArray(), lMu.ToArray(), lSigma.ToArray());
            df.Show();            
        }*/
    }
}
