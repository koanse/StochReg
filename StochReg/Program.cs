using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Collections;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Distributions;
using System.Data.OleDb;
using System.Data;
using muWrapper;

namespace StochReg
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
        public static void Model(Variable[] arrU, Variable[] arrY,
            out double[][] arrC, out string[,] mFunc, out string rep)
        {
            rep = "";
            arrC = new double[arrY.Length][];
            mFunc = new string[arrY.Length, arrU.Length];
            //string[] arrT = { "x", "x^2", "x^3", "1/x", "1/x^2", "1/x^3", "sqrt(x)", "1/sqrt(x)", "ln(x)", "exp(x)" };
            string[] arrT = { "x", "x^2", "x^3", "sqrt(x)", "ln(x)", "exp(x)" };
            for (int i = 0; i < arrY.Length; i++)
            {
                Sample sy = new Sample(arrY[i].name, arrY[i].id, arrY[i].arr);
                List<Sample> lSU = new List<Sample>();
                for (int j = 0; j < arrU.Length; j++)
                {
                    Sample su = new Sample(arrU[j].name, arrU[j].id, arrU[j].arr);
                    Sample suMax = su;
                    double max = double.MinValue;
                    string tMax = "x";
                    foreach (string t in arrT)
                    {
                        Sample ts;
                        try
                        {
                            ts = Sample.Transform(su, t);
                        }
                        catch
                        {
                            continue;
                        }
                        Regression r = new Regression(sy, new Sample[] { ts });
                        double corr = Math.Abs(r.Ryx1());
                        if (corr > max)
                        {
                            max = corr;
                            tMax = t;
                            suMax = ts;
                        }
                    }
                    mFunc[i, j] = tMax;
                    lSU.Add(suMax);
                }
                Regression reg = new Regression(sy, lSU.ToArray());
                arrC[i] = reg.arrB;
                rep += reg.RegReport() + "<br>";
            }
        }
        public static void Optimize(int m, int n, // число факторов и показателей
            double[][] arrC, string[,] mFunc,
            double[] arrUMin, double[] arrUMax, double[] arrUSMin, double[] arrYMin, double[] arrYMax,
            double[] arrAlpha, double[] arrYOpt, double mult,
            double dx, double df, double uInit, double duInit, double sInit, double dsInit,
            int iterMax, double R, double C, double[] arrZ,
            out double[] arrUOpt, out double[] arrSOpt,
            out double[] arrMu, out double[] arrSigma, out double[] arrY, out double FOpt, out string rep)
        {
            int p = m - arrZ.Length; // число нереализованных факторов
            double[][] arrCNew = new double[n][];
            for (int i = 0; i < n; i++)
            {
                arrCNew[i] = new double[p + 1];
                arrCNew[i][0] = arrC[i][0];
                for (int j = 0; j < m; j++)
                {
                    if (j < arrZ.Length)
                        arrCNew[i][0] += arrC[i][j + 1] * F(arrZ[j], mFunc[i, j]);
                    else
                        arrCNew[i][j - arrZ.Length + 1] = arrC[i][j + 1];        
                }
            }
            double[] arrUMinNew = new double[p], arrUMaxNew = new double[p], arrUSMinNew = new double[p];
            for (int i = 0; i < p; i++)
            {
                arrUMinNew[i] = arrUMin[arrZ.Length + i];
                arrUMaxNew[i] = arrUMax[arrZ.Length + i];
                arrUSMinNew[i] = arrUSMin[arrZ.Length + i];
            }
            OptParams init = new OptParams(arrC, mFunc, arrUMinNew, arrUMaxNew,
                arrUSMinNew, arrYOpt, arrYMin, arrYMax, arrAlpha, R, mult);                
            HJIteration[] arrIter;
            double[] arrX = new double[2 * p], arrXDelta = new double[2 * p];
            for (int i = 0; i < p; i++)
            {
                arrX[i] = uInit;
                arrXDelta[i] = duInit;
            }
            for (int i = p; i < 2 * p; i++)
            {
                arrX[i] = sInit;
                arrXDelta[i] = dsInit;
            }
            HJOptimizer opt = new HJOptimizer();
            FOpt = double.MaxValue;
            do
            {
                arrIter=opt.Optimize(df, dx, iterMax, new HJOptimizer.OptFunc(OptFunc), init, arrX, (double[])arrXDelta.Clone());
                double FNext = arrIter.Last().fRes;
                double s = 0;
                for (int i = 0; i < arrX.Length; i++)
                {
                    s += (arrX[i] - arrIter.Last().arrX[i]) * (arrX[i] - arrIter.Last().arrX[i]);
                }
                arrX = (double[])arrIter.Last().arrX.Clone();
                if (FNext < df || s<0.0000000001)
                    break;
                FOpt = FNext;
                R *= C;
                init = new OptParams(arrC, mFunc, arrUMinNew, arrUMaxNew, arrUSMinNew,
                    arrYOpt, arrYMin, arrYMax, arrAlpha, R, mult);
            }
            while (true);
            arrUOpt = new double[p];
            arrSOpt = new double[p];
            for (int i = 0; i < p; i++)
            {
                arrUOpt[i] = arrX[i];
                arrSOpt[i] = arrX[i + p];
            }
            CalcStoch(init, arrX, out arrMu, out arrSigma, out arrY);
            rep = "";            
        }
        public static void Unite(Stage[] arrStage, out Variable[] arrU, out Variable[] arrY, out string rep)
        {
            int rows = arrStage[0].arrU[0].arr.Length;
            List<Variable> lU = new List<Variable>();
            for (int i = 0; i < arrStage.Length; i++)
            {
                foreach (Variable u in arrStage[i].arrU)
                {
                    lU.Add(u);
                }
            }
            int columns = lU.Count;
            double[,] matr = new double[rows, columns + arrStage.Last().arrY.Length];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < arrStage[0].arrU.Length; j++)
                {
                    matr[i, j] = arrStage[0].arrU[j].arr[i];
                }
                int c = arrStage[0].arrU.Length;
                int kPrev = i;
                for (int j = 1; j < arrStage.Length; j++)
                {
                    if (arrStage[j].arrX.Length != arrStage[j - 1].arrY.Length)
                        throw new Exception("Нестыковка переделов");
                    double sMin = double.MaxValue;
                    int kMin = -1;
                    for (int k = 0; k < arrStage[j].arrX[0].arr.Length; k++)
                    {
                        double s = 0;
                        for (int l = 0; l < arrStage[j].arrX.Length; l++)
                        {
                            s += (arrStage[j].arrX[l].arr[k] - arrStage[j - 1].arrY[l].arr[kPrev]) *
                                (arrStage[j].arrX[l].arr[k] - arrStage[j - 1].arrY[l].arr[kPrev]);
                        }
                        if (s < sMin)
                        {
                            kMin = k;
                            sMin = s;
                        }
                    }
                    for (int k = 0; k < arrStage[j].arrU.Length; k++)
                    {
                        matr[i, c++] = arrStage[j].arrU[k].arr[kMin];
                    }
                    kPrev = kMin;
                }
                for (int j = 0; j < arrStage.Last().arrY.Length; j++)
                {
                    matr[i, c++] = arrStage.Last().arrY[j].arr[kPrev];
                }
            }

            arrU = new Variable[columns];
            arrY = new Variable[arrStage.Last().arrY.Length];
            for (int i = 0; i < arrU.Length; i++)
            {
                double[] arr = new double[rows];
                for (int j = 0; j < rows; j++)
                {
                    arr[j] = matr[j, i];
                }
                arrU[i] = (Variable)lU[i].Clone();
                arrU[i].arr = arr;
            }
            for (int i = 0; i < arrY.Length; i++)
            {
                double[] arr = new double[rows];
                for (int j = 0; j < rows; j++)
                {
                    arr[j] = matr[j, columns + i];
                }
                arrY[i] = (Variable)arrStage.Last().arrY[i].Clone();
                arrY[i].arr = arr;
            }

            rep = "<table border = 1 cellspacing = 0><tr><td>№ набл.";
            foreach (Variable v in arrU)
            {
                rep += "<td>" + v.name;
            }
            foreach (Variable v in arrY)
            {
                rep += "<td>" + v.name;
            }
            for (int i = 0; i < arrU[0].arr.Length; i++)
            {
                rep += string.Format("<tr><td>{0}", i + 1);
                foreach (Variable v in arrU)
                {
                    rep += string.Format("<td>{0:g4}", v.Inv(v.arr[i]));
                }
                foreach (Variable v in arrY)
                {
                    rep += string.Format("<td>{0:g4}", v.Inv(v.arr[i]));
                }
            }
            rep += "</table>";
        }
        public static void Emulate(Stage[] arrSReg, Stage[] arrSEmul, int[] arrIStage,
            double[] arrUMin, double[] arrUMax, double[] arrUSMin, double[] arrYMin, double[] arrYMax,
            double[] arrAlpha, double[] arrYOpt, double mult,
            double dx, double df, double uInit, double duInit, double sInit, double dsInit,
            int iterCount, double R, double C,
            out Variable[] arrUEmul, out Variable[] arrYEmul,
            out Variable[] arrU, out Variable[] arrY, out string rep)
        {
            Variable[] arrUReg, arrYReg;
            string s;
            rep = "Объединение переделов для построения регрессии";
            Unite(arrSReg, out arrUReg, out arrYReg, out s);
            rep += s+"<br>Объединение переделов для эмуляции";
            Unite(arrSEmul, out arrUEmul, out arrYEmul,out s);
            rep += s + "<br>Совместная информация технологии и качества до гибкого управления<br>";
            int[] arrNU=new int[arrUEmul.Length], arrNY=new int[arrYEmul.Length];
            for (int i = 0; i < arrUEmul.Length; i++)
            {
                arrNU[i] = Properties.Settings.Default.grid;
            }
            for (int i = 0; i < arrYEmul.Length; i++)
            {
                arrNY[i] = Properties.Settings.Default.grid;
            }
            double[] arrStepU, arrMinU, arrStepY, arrMinY;
            Grid(arrUEmul, arrNU, out arrStepU, out arrMinU);
            Grid(arrYEmul, arrNY, out arrStepY, out arrMinY);
            Iuy(arrUEmul, arrYEmul, arrNU, arrNY, arrStepU,arrStepY,arrMinU,arrMinY,out s);
            rep += s;
            double[][] arrC;
            string[,] mFunc;
            Model(arrUReg, arrYReg, out arrC, out mFunc, out s);
            rep += "Построение модели технологии<br>"+s;
            int iu = 0;
            List<int> lIU = new List<int>(), lITau=new List<int>();
            for (int i = 0; i < arrSReg.Length; i++)
            {
                if (arrIStage.Contains(i))
                {
                    for (int j = 0; j < arrSReg[i].arrU.Length; j++)
                    {
                        lIU.Add(iu + j);
                    }
                }
                else
                {
                    for (int j = 0; j < arrSReg[i].arrU.Length; j++)
                    {
                        lITau.Add(iu + j);
                    }
                }
                iu+=arrSReg[i].arrU.Length;
            }
            int uCount = lIU.Count;
            arrU = new Variable[arrUEmul.Length];
            for (int i = 0; i < arrU.Length; i++)
            {
                arrU[i] = (Variable)arrUEmul[i].Clone();
            }
            string[,] mFuncNew = new string[arrYReg.Length, uCount];
            for (int i = 0; i < arrYReg.Length; i++)
            {
                for (int j = 0; j < lIU.Count; j++)
                {
                    mFuncNew[i, j] = mFunc[i, lIU[j]];
                }
            }
            double[][] arrCNew = new double[arrYReg.Length][];
            for (int j = 0; j < arrYReg.Length; j++)
            {
                arrCNew[j] = new double[uCount + 1];
                for (int k = 0; k < lIU.Count; k++)
                {
                    arrCNew[j][k + 1] = arrC[j][lIU[k] + 1];
                }
            }

            double[][] arrArrSOpt = new double[arrUEmul[0].arr.Length][];
            string[] arrRep = new string[arrUEmul[0].arr.Length];
            for (int i = 0; i < arrUEmul[0].arr.Length; i++)
            {
                for (int j = 0; j < arrYReg.Length; j++)
                {
                    arrCNew[j][0] = arrC[j][0];
                    foreach (int k in lITau)
                    {
                        arrCNew[j][0] += F(arrUEmul[k].arr[i],mFunc[j,k]) * arrC[j][k + 1];
                    }
                }
                double[] arrUOpt, arrSOpt, arrMu,arrSigma,arrYUOpt;
                double fvalue;
                Optimize(uCount, arrYEmul.Length, arrCNew, mFuncNew, arrUMin, arrUMax, arrUSMin, arrYMin,
                    arrYMax, arrAlpha, arrYOpt, mult, dx, df, uInit, duInit, sInit, dsInit, iterCount, R, C, new double[] { },
                    out arrUOpt, out arrSOpt, out arrMu,out arrSigma,out arrYUOpt,out fvalue,out s);
                arrRep[i] += "M(Y)±S(Y)=(";
                for (int j = 0; j < arrYUOpt.Length; j++)
                {
                    arrRep[i] += string.Format("{0:g4}±{1:g4}", arrYEmul[j].Inv(arrMu[j]), arrYEmul[j].sigma*arrSigma[j]);
                    if (j < arrYUOpt.Length - 1)
                        arrRep[i] += "; ";
                }
                arrRep[i] += string.Format("); F={0:g4}", fvalue);
                arrArrSOpt[i] = arrSOpt;
                for (int j = 0; j < lIU.Count; j++)
                {
                    arrU[lIU[j]].arr[i] = arrUOpt[j];
                }
            }
            
            arrY = new Variable[arrYEmul.Length];
            for (int i = 0; i < arrY.Length; i++)
            {
                arrY[i] = (Variable)arrYEmul[i].Clone();
                for (int j = 0; j < arrY[i].arr.Length; j++)
                {
                    double y = arrC[i][0];
                    for (int k = 0; k < arrU.Length; k++)
                    {
                        y += arrC[i][k + 1] * F(arrU[k].arr[j],mFunc[i,k]);
                    }
                    arrY[i].arr[j] = y;
                }
            }
            rep +=  "<table border = 1 cellspacing = 0><tr><td>№ набл.";
            for (int i = 0; i < arrU.Length; i++)
            {
                rep += string.Format("<td>{0}", arrU[i].name);
            }
            for (int i = 0; i < arrY.Length; i++)
            {
                rep += string.Format("<td>{0}", arrY[i].name);
            }
            rep += "<td>Оптимизация";
            for (int i = 0; i < arrU[0].arr.Length; i++)
            {
                rep += string.Format("<tr><td>{0}", i+1);
                for (int j = 0; j < arrU.Length; j++)
                {
                    int k=lIU.BinarySearch(j);
                    if (k>=0)
                        rep += string.Format("<td>{0:g4}±{1:g4}", arrU[j].Inv(arrU[j].arr[i]),arrArrSOpt[i][k]*arrU[j].sigma);
                    else
                        rep += string.Format("<td>{0:g4}", arrU[j].Inv(arrU[j].arr[i]));
                    if (k >= 0)
                        rep += string.Format("<br>{0:g4}±{1:g4}", arrU[j].arr[i], arrArrSOpt[i][k]);
                    else
                        rep += string.Format("<br>{0:g4}", arrU[j].arr[i]);
                }
                for (int j = 0; j < arrY.Length; j++)
                {
                    rep += string.Format("<td>{0:g4}", arrY[j].Inv(arrY[j].arr[i]));
                }
                rep += "<td>"+arrRep[i];
            }
            rep += "</table>";
            rep += "Совместная информация технологии и качества после гибкого управления<br>";
            Iuy(arrU, arrY, arrNU, arrNY, arrStepU, arrStepY, arrMinU, arrMinY, out s);
            rep += s + "Технологические факторы до и после гибкого управления<table cellspacing=0 border=1>" +
                "<tr><td>Наименование<td>Среднее до<td>Среднее после<td>СКО до<td>СКО после<td>Энтропия до<td>Энтропия после";
            for (int i = 0; i < arrU.Length; i++)
            {
                Sample uBefore = new Sample(arrUEmul[i].arr), uAfter = new Sample(arrU[i].arr);
                uBefore.DoHistogram(Properties.Settings.Default.grid);
                uAfter.DoHistogram(uBefore.arrMid);
                rep += string.Format("<tr><td>{0}<td>{1:g4}<td>{2:g4}<td>{3:g4}<td>{4:g4}<td>{5:g4}<td>{6:g4}",
                    arrU[i].name, arrUEmul[i].Inv(uBefore.av), arrU[i].Inv(uAfter.av), uBefore.sigma * arrUEmul[i].sigma,
                    uAfter.sigma * arrU[i].sigma, uBefore.H, uAfter.H);
            }
            rep += "</table>Показатели качества до и после гибкого управления<table cellspacing=0 border=1>" +
                "<tr><td>Наименование<td>Среднее до<td>Среднее после<td>СКО до<td>СКО после<td>Энтропия до<td>Энтропия после";
            for (int i = 0; i < arrY.Length; i++)
            {
                Sample yBefore = new Sample(arrYEmul[i].arr), yAfter = new Sample(arrY[i].arr);
                yBefore.DoHistogram(Properties.Settings.Default.grid);
                yAfter.DoHistogram(yBefore.arrMid);
                rep += string.Format("<tr><td>{0}<td>{1:g4}<td>{2:g4}<td>{3:g4}<td>{4:g4}<td>{5:g4}<td>{6:g4}",
                    arrY[i].name, arrYEmul[i].Inv(yBefore.av), arrY[i].Inv(yAfter.av), yBefore.sigma * arrYEmul[i].sigma,
                    yAfter.sigma * arrY[i].sigma, yBefore.H, yAfter.H);
            
            }
            rep += "</table>";
        }
        public static void Grid(Variable[] arrV, int[] arrN, out double[] arrStep, out double[] arrMin)
        {
            arrStep = new double[arrV.Length];
            arrMin = new double[arrV.Length];
            for (int i = 0; i < arrV.Length; i++)
            {
                arrMin[i] = arrV[i].arr.Min();
                arrStep[i] = (arrV[i].arr.Max() - arrMin[i]) / arrN[i];
            }
        }
        public static void Clust(Variable[] arrV, int[] arrN,double[] arrStep,double[] arrMin, out string[] arrClust)
        {            
            arrClust = new string[arrV[0].arr.Length];
            for (int i = 0; i < arrV[0].arr.Length; i++)
            {
                string c = "(";
                for (int j = 0; j < arrV.Length; j++)
                {
                    int ic = (int)((arrV[j].arr[i]-arrMin[j]) / arrStep[j]);
                    if (ic == arrN[j])
                        ic--;
                    if (ic < 0)
                        ic = 0;
                    c += string.Format("{0}{1}", ic,j<arrV.Length-1?";":"");
                }
                arrClust[i] = c+")";
            }            
        }
        public static double Iuy(Variable[] arrU, Variable[] arrY, int[] arrNU, int[] arrNY,
            double[] arrStepU, double[] arrStepY,double[]arrUMin, double[] arrYMin,out string rep)
        {
            string[] arrCU, arrCY;
            Clust(arrU, arrNU, arrStepU,arrUMin,out arrCU);
            Clust(arrY, arrNY, arrStepY,arrYMin,out arrCY);
            SortedDictionary<string, double> distU = new SortedDictionary<string, double>(),
                distY = new SortedDictionary<string, double>(), distUY = new SortedDictionary<string, double>();
            for (int i = 0; i < arrCU.Length; i++)
            {
                if (distU.ContainsKey(arrCU[i]))
                {
                    distU[arrCU[i]] += 1;
                }
                else
                {
                    distU.Add(arrCU[i], 1);
                }
                if (distY.ContainsKey(arrCY[i]))
                {
                    distY[arrCY[i]] += 1;
                }
                else
                {
                    distY.Add(arrCY[i], 1);
                }
                string k = arrCU[i] + " " + arrCY[i];
                if (distUY.ContainsKey(k))
                {
                    distUY[k] += 1;
                }
                else
                {
                    distUY.Add(k, 1);
                }
            }
            double hu = 0, hy = 0, huy = 0;
            rep = "Распределение U<table cellspacing=0 border=1><tr><td>Кластер<td>Отн. частота";
            string[] arrK = distU.Keys.ToArray();
            foreach (string k in arrK)
            {
                double p = distU[k] / arrCU.Length;
                distU[k] = p;
                hu -= p * Math.Log(p);
                rep += string.Format("<tr><td>{0}<td>{1:g4}", k, p);
            }
            rep += "</table>Распределение Y<table cellspacing=0 border=1><tr><td>Кластер<td>Отн. частота";
            arrK = distY.Keys.ToArray();
            foreach (string k in arrK)
            {
                double p = distY[k] / arrCU.Length;
                distY[k] = p;
                hy -= p * Math.Log(p);
                rep += string.Format("<tr><td>{0}<td>{1:g4}", k, p);
            }
            rep += "</table>Распределение (U,Y)<table cellspacing=0 border=1><tr><td>Кластер<td>Отн. частота";
            arrK = distUY.Keys.ToArray();
            foreach (string k in arrK)
            {
                double p = distUY[k] / arrCU.Length;
                distUY[k] = p;
                huy -= p * Math.Log(p);
                rep += string.Format("<tr><td>{0}<td>{1:g4}", k, p);
            }
            double I = hu + hy - huy;
            rep += string.Format("</table>I(U,Y) = H(U) + H(Y) - H(U,Y) = {0:g4} + {1:g4} - {2:g4} = {3:g4}<br>", hu, hy, huy, I);
            return I;
        }
        public static double OptFunc(object optParams, double[] arrX)
        {
            OptParams op = (OptParams)optParams;
            int p = op.arrUMin.Length, n = op.arrYOpt.Length;
            double[] arrMu,arrSigma,arrY;
            CalcStoch(op, arrX, out arrMu, out arrSigma, out arrY);
            double res = 0, z;
            for (int i = 0; i < p; i++)
            {
                if ((z = op.arrUSMin[i] - arrX[p + i]) > 0)
                    res += z;
                if ((z = arrX[i] + op.mult * arrX[p + i] - op.arrUMax[i]) > 0)
                    res += z;
                if ((z = op.arrUMin[i] - arrX[i] + op.mult * arrX[p + i]) > 0)
                    res += z;
            }
            for (int j = 0; j < n; j++)
            {
                if ((z = arrMu[j] + op.mult * arrSigma[j] - op.arrYMax[j]) > 0)
                    res += z;
                if ((z = op.arrYMin[j] - arrMu[j] + op.mult * arrSigma[j]) > 0)
                    res += z;
            }
            res *= op.r;
            for (int j = 0; j < n; j++)
                res += op.arrAlpha[j] * (arrY[j] - op.arrYOpt[j]) * (arrY[j] - op.arrYOpt[j]);
            return res;
        }
        public static void CalcStoch(OptParams op, double[] arrX,
            out double[] arrMu, out double[] arrSigma, out double[] arrY)
        {
            int p = op.arrUMin.Length, n = op.arrYOpt.Length;
            arrMu = new double[n];
            for (int j = 0; j < n; j++)
            {
                arrMu[j] = op.arrCoeff[j][0];
                for (int i = 0; i < p; i++)
                    arrMu[j] += op.arrCoeff[j][i + 1] * (F(op.arrUMid[i], op.mFunc[j, i]) +
                        f(op.arrUMid[i], op.mFunc[j, i]) * (arrX[i] - op.arrUMid[i]));
            }
            arrSigma = new double[n];
            for (int j = 0; j < n; j++)
            {
                for (int i = 0; i < p; i++)
                    arrSigma[j] += (op.arrCoeff[j][i + 1] * f(op.arrUMid[i], op.mFunc[j, i]) * arrX[p + i]) *
                        (op.arrCoeff[j][i + 1] * f(op.arrUMid[i], op.mFunc[j, i]) * arrX[p + i]);
                arrSigma[j] = Math.Sqrt(arrSigma[j]);
            }
            arrY = new double[n];
            for (int j = 0; j < n; j++)
            {
                arrY[j] = op.arrCoeff[j][0];
                for (int i = 0; i < p; i++)
                {
                    arrY[j] += op.arrCoeff[j][i + 1] * F(arrX[i], op.mFunc[j, i]);
                }
            }
        }
        public static double F(double x, string func)
        {
            switch (func)
            {
                case "x":
                    return x;
                case "x^2":
                    return x * x;
                case "x^3":
                    return x * x * x;
                case "1/x":
                    return 1 / x;
                case "1/x^2":
                    return 1 / (x * x);
                case "1/x^3":
                    return 1 / (x * x * x);
                case "sqrt(x)":
                    return Math.Sqrt(x);
                case "1/sqrt(x)":
                    return 1 / Math.Sqrt(x);
                case "ln(x)":
                    return Math.Log(x);
                case "exp(x)":
                    return Math.Exp(x);
                default:
                    throw new Exception();
            }
        }
        public static double f(double x, string func)
        {
            switch (func)
            {
                case "x":
                    return 1;
                case "x^2":
                    return 2 * x;
                case "x^3":
                    return 3 * x * x;
                case "1/x":
                    return -1 / (x * x);
                case "1/x^2":
                    return -2 / (x * x * x);
                case "1/x^3":
                    return -3 / (x * x * x * x);
                case "sqrt(x)":
                    return 1 / (2 * Math.Sqrt(x));
                case "1/sqrt(x)":
                    return -1 / (2 * x * Math.Sqrt(x));
                case "ln(x)":
                    return 1 / x;
                case "exp(x)":
                    return Math.Exp(x);
                default:
                    throw new Exception();
            }
        }
    }
    [Serializable]
    public class Stage
    {
        public string name;
        public Variable[] arrX, arrU, arrY;
        public override string ToString()
        {
            string s = name + "<br>Входы: ";
            foreach (Variable v in arrX)
                s += v.name + " ";
            s += "<br>Управления: ";
            foreach (Variable v in arrU)
                s += v.name + " ";
            s += "<br>Выходы: ";
            foreach (Variable v in arrY)
                s += v.name + " ";
            return s;
        }
    }    
    [Serializable]
    public class Variable : ICloneable
    {
        public double av, sigma;
        public string name, id;
        public double[] arr;
        public void Norm()
        {
            av = 0;
            double av2 = 0;
            for (int i = 0; i < arr.Length ; i++)
            {
                av += arr[i];
                av2 += arr[i] * arr[i];
            }
            av /= arr.Length;
            av2 /= arr.Length;
            sigma = Math.Sqrt(av2 - av * av);
            for (int i = 0; i < arr.Length; i++)
                arr[i] = (arr[i] - av) / sigma;
        }
        public void ReNorm()
        {            
            for (int i = 0; i < arr.Length; i++)
                arr[i] = (arr[i] - av) / sigma;
        }
        public double Norm(double x)
        {
            return (x - av) / sigma;
        }
        public double Inv(double z)
        {
            return z * sigma + av;
        }
        public object Clone()
        {
            return new Variable() { av = av, sigma = sigma, name = name, id = id, arr = (double[])arr.Clone() };
        }
    }
    [Serializable]
    public class Technology
    {
        public string name;
        public List<Stage> lStage;
        public override string ToString()
        {
            return name;
        }
    }

    [Serializable]
    public class Sample : ICloneable
    {
        public string name, id, idHtml;
        public double[] arr;
        public int indexMin, indexMax;
        public double min, max, av, av2;
        public double sum, sum2, dev, devStd, sigma, sigmaStd, H;
        public double[] arrFreq, arrP, arrMid;    // частоты и середины интервалов
        public double var;                  // коэф. вариации
        public double mc1, mc2, mc3, mc4;   // центр. моменты
        public double mb1, mb2, mb3, mb4;   // нач. моменты
        public double asym, exc;            // асим. и эксцесс

        public Sample() { }
        public Sample(string name, string id, double[] arr)
        {
            this.name = name;
            this.id = idHtml = id;
            this.arr = arr;
            Calculate();
        }
        public Sample(string name, string id, string idHtml, double[] arr)
        {
            this.name = name;
            this.id = id;
            this.idHtml = idHtml;
            this.arr = arr;
            Calculate();
        }
        public Sample(double[] arr)
        {
            this.arr = arr;
            Calculate();
        }
        public double this[int index]
        {
            get { return arr[index]; }
            set { arr[index] = value; }
        }
        public void Calculate()
        {
            min = double.MaxValue;
            max = double.MinValue;
            sum = 0;
            sum2 = 0;
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] < min)
                {
                    min = arr[i];
                    indexMin = i;
                }
                if (arr[i] > max)
                {
                    max = arr[i];
                    indexMax = i;
                }
                sum += arr[i];
                sum2 += arr[i] * arr[i];
            }
            av = sum / arr.Length;
            av2 = sum2 / arr.Length;
            dev = av2 - (av * av);
            devStd = dev / arr.Length * (arr.Length - 1);
            sigma = (double)Math.Sqrt(dev);
            sigmaStd = (double)Math.Sqrt(devStd);

            // моменты
            mb1 = av;
            mb2 = sum2 / arr.Length;
            mc2 = dev;
            mc1 = mc3 = mc4 = 0;
            mb1 = mb2 = mb3 = mb4 = 0;
            for (int i = 0; i < arr.Length; i++)
            {
                mb3 += arr[i] * arr[i] * arr[i];
                mb4 += arr[i] * arr[i] * arr[i] * arr[i];
                mc1 += arr[i] - mb1;
                mc3 += (arr[i] - mb1) * (arr[i] - mb1) * (arr[i] - mb1);
                mc4 += (arr[i] - mb1) * (arr[i] - mb1) * (arr[i] - mb1) * (arr[i] - mb1);
            }
            mb3 /= arr.Length;
            mb4 /= arr.Length;
            mc1 /= arr.Length;
            mc3 /= arr.Length;
            mc4 /= arr.Length;

            // вариация, асим. и эксцесс
            var = sigmaStd / av;
            asym = mc3 / (sigmaStd * sigmaStd * sigmaStd);
            exc = mc4 / (sigmaStd * sigmaStd * sigmaStd * sigmaStd);
        }
        public int[] DropoutErrors(double alpha, out string rep)
        {
            rep = string.Format("ОТСЕВ ПОГРЕШНОСТЕЙ ДЛЯ {0}<br>Уровень значимости: α = {1}<table border = 1 cellspacing = 0>" +
                "<tr><td>№ наблюдения<td>Значение<td>τ<sub>набл</sub><td>τ<sub>теор</sub>", name, alpha);
            List<int> lIgnore = new List<int>(), lI = new List<int>();
            for (int i = 0; i < arr.Length; i++)
            {
                lI.Add(i);
            }
            while (true)
            {
                double min = double.MaxValue, max = double.MinValue;
                double sum = 0, sum2 = 0;
                int iMin = -1, iMax = -1;
                foreach (int i in lI)
                {
                    if (arr[i] < min)
                    {
                        min = arr[i];
                        iMin = i;
                    }
                    if (arr[i] > max)
                    {
                        max = arr[i];
                        iMax = i;
                    }
                    sum += arr[i];
                    sum2 += arr[i] * arr[i];
                }
                int n = lI.Count;
                double average = sum / n;
                double devStd = (sum2 / n - (sum / n) * (sum / n)) * n / (n - 1);
                double sigmaStd = (double)Math.Sqrt(devStd);
                double tauXMin, tauXMax;
                tauXMin = Math.Abs(min - average) / sigmaStd;
                tauXMax = Math.Abs(max - average) / sigmaStd;
                int index;
                double tauMax;
                if (tauXMin >= tauXMax)
                {
                    index = iMin;
                    tauMax = tauXMin;
                }
                else
                {
                    index = iMax;
                    tauMax = tauXMax;
                }
                double tauCrit;
                if (n <= 25)
                    tauCrit = StatTables.GetTau(1 - alpha, n);
                else
                {
                    double t = StatTables.GetStudInv(n - 2, alpha);
                    tauCrit = t * Math.Sqrt(n - 1) / Math.Sqrt(n - 2 + t * t);
                }
                if (tauMax > tauCrit)
                {
                    rep += string.Format("<tr><td>{0}<td>{1:g5}<td>{2:g3}<td>{3:g3}{4}", index, arr[index], tauMax, tauCrit,
                        n <= 25 ? "" : "(исп. t(α,n-2))");
                    int pos = lIgnore.BinarySearch(index);
                    lIgnore.Insert(~pos, index);
                    pos = lI.BinarySearch(index);
                    lI.RemoveAt(pos);
                }
                else
                    break;
            }
            return lIgnore.ToArray();
        }
        public void DoHistogram()
        {
            int k = 1 + (int)(3.32 * Math.Log10(arr.Length));
            if (k < 6 && arr.Length >= 6)
                k = 6;
            else if (k > 20)
                k = 20;
            arrFreq = new double[k];
            double h = (max - min) / k;
            for (int i = 0; i < arr.Length; i++)
            {
                int j = (int)((arr[i] - min) / h);
                if (j == arrFreq.Length)
                    j--;
                arrFreq[j]++;
            }
            arrP = new double[k];
            H = 0;
            for (int i = 0; i < arrP.Length; i++)
            {
                arrP[i] = arrFreq[i] / (double)arr.Length;
                if(arrP[i]>0)
                    H -= arrP[i] * Math.Log(arrP[i]);
            }
            arrMid = new double[k];
            for (int i = 0; i < arrMid.Length; i++)
                arrMid[i] = min + h * (i + 0.5);
        }
        public void DoHistogram(int k)
        {
            arrFreq = new double[k];
            double h = (max - min) / k;
            for (int i = 0; i < arr.Length; i++)
            {
                int j = (int)((arr[i] - min) / h);
                if (j == arrFreq.Length)
                    j--;
                arrFreq[j]++;
            }
            arrP = new double[k];
            H = 0;
            for (int i = 0; i < arrP.Length; i++)
            {
                arrP[i] = arrFreq[i] / (double)arr.Length;
                if (arrP[i] > 0)
                    H -= arrP[i] * Math.Log(arrP[i]);
            }
            arrMid = new double[k];
            for (int i = 0; i < arrMid.Length; i++)
                arrMid[i] = min + h * (i + 0.5);
        }
        public void DoHistogram(double[] arrMid)
        {
            this.arrMid = (double[])arrMid.Clone();
            int k = arrMid.Length;
            arrFreq = new double[k];
            double h = arrMid[1]-arrMid[0],m=arrMid[0]-h/2;
            for (int i = 0; i < arr.Length; i++)
            {
                int j = (int)((arr[i] - m) / h);
                if (j >= arrFreq.Length)
                    j=arrFreq.Length-1;
                if (j < 0)
                    j = 0;
                arrFreq[j]++;
            }
            arrP = new double[k];
            for (int i = 0; i < arrP.Length; i++)
            {
                arrP[i] = arrFreq[i] / (double)arr.Length;
                if (arrP[i] > 0)
                    H -= arrP[i] * Math.Log(arrP[i]);
            }
        }
        public void AddValue(double x)
        {
            double[] arrNew = new double[arr.Length + 1];
            arr.CopyTo(arrNew, 0);
            arrNew[arr.Length] = x;
            arr = arrNew;
        }
        public void RemoveValues(int[] arrIndex)
        {
            double[] arrNew = new double[arr.Length - arrIndex.Length];
            List<int> lI = new List<int>(arrIndex);
            lI.Sort();
            lI.Reverse();
            List<double> l = new List<double>(arr);
            foreach (int i in lI)
            {
                l.RemoveAt(i);
            }
            arr = l.ToArray();
        }
        public string Report()
        {
            string s = string.Format("<P>ВЫБОРОЧНЫЕ ХАРАКТЕРИСТИКИ {0}, {1}<BR>" +
                "Минимум: {1}<SUB>min</SUB> = {2:g5}<BR>" +
                "Максимум: {1}<SUB>max</SUB> = {3:g5}<BR>" +
                "Размах выборки: w = {4:g5}<BR>" +
                "Среднее: {1}<SUB>ср</SUB> = {5:g5}<BR>" +
                "Средний квадрат: {1}<SUP>2</SUP><SUB>ср</SUB> = {6:g5}<BR>" +
                "Дисперсия: s<SUP>2</SUP> = {7:g5}<BR>" +
                "Среднее квадр. откл.: s = {8:g5}<BR>" +
                "Испр. дисперсия: s<SUP>2</SUP><SUB>испр</SUB> = {9:g5}<BR>" +
                "Испр. среднее квадр. откл.: s<SUB>испр</SUB> = {10:g5}<BR>" +
                "Асимметрия: A = {11:g5}<BR>" +
                "Эксцесс: E = {12:g5}<BR>" +
                "Коэффициент вариации: v = {13:g5}<BR></P>",
                name, idHtml, min, max, min - max, av, av2, dev, sigma, devStd, sigmaStd, asym, exc, var);
            return s;
        }
        public string CheckNorm(double alpha, out double[] arrPNorm)
        {
            NormalDistribution ndist = new NormalDistribution(av, sigmaStd);
            double step = arrMid[1] - arrMid[0];
            arrPNorm = new double[arrP.Length];
            arrPNorm[0] = ndist.CumulativeDistribution(arrMid[0] + step / 2);
            for (int i = 1; i < arrPNorm.Length - 1; i++)
            {
                arrPNorm[i] = ndist.CumulativeDistribution(arrMid[i] + step / 2) - arrPNorm[i - 1];
            }
            arrPNorm[arrPNorm.Length - 1] = 1 - arrPNorm[arrPNorm.Length - 2];
            double chi2 = 0;
            for (int i = 0; i < arrP.Length; i++)
            {
                chi2 += (arrP[i] - arrPNorm[i]) * (arrP[i] - arrPNorm[i]) / arrPNorm[i];
            }
            double chi2Theor = StatTables.GetChi2Inv(arrP.Length - 1 - 2, alpha);

            return string.Format("Проверка нормальности распределения по критерию хи-квадрат Пирсона<br>Уровень значимости: α = {0}<br>" +
                "Наблюдаемое значение критерия: χ<sup>2</sup><sub>набл</sub> = {1:g3}<br>" +
                "Теоретическое значение: χ<sup>2</sup><sub>теор</sub> = χ<sup>2</sup>(n-3,1-α) = χ<sup>2</sup>({2};{3}) = {4:g3}<br>{5}",
                alpha, chi2, arrP.Length - 3, 1 - alpha, chi2Theor,
                chi2 < chi2Theor ? "χ<sup>2</sup><sub>набл</sub><χ<sup>2</sup><sub>теор</sub> => гипотеза о нормальности принимается" :
                "χ<sup>2</sup><sub>набл</sub>≥<χ<sup>2</sup><sub>теор</sub> => гипотеза о нормальности отвергается");
        }
        public object Clone()
        {
            return new Sample(name, id, idHtml, (double[])arr.Clone());
        }
        public void Norm()
        {
            for (int i = 0; i < arr.Length; i++)
                arr[i] = (arr[i] - av) / sigma;
            Calculate();
        }
        public double Norm(double x)
        {
            return (x - av) / sigma;
        }
        public double Inv(double z)
        {
            return z * sigma + av;
        }

        public static Sample Transform(Sample s, string t)
        {
            Sample st = new Sample();
            double[] arrT = new double[s.arr.Length];
            switch (t)
            {
                case "x":
                    st.idHtml = s.idHtml;
                    for (int i = 0; i < arrT.Length; i++)
                        arrT[i] = s.arr[i];
                    break;
                case "x^2":
                    st.idHtml = string.Format("{0}<sup>2</sup>", s.idHtml);
                    for (int i = 0; i < arrT.Length; i++)
                        arrT[i] = s.arr[i] * s.arr[i];
                    break;
                case "x^3":
                    for (int i = 0; i < arrT.Length; i++)
                        arrT[i] = s.arr[i] * s.arr[i] * s.arr[i];
                    st.idHtml = string.Format("{0}<sup>3</sup>", s.idHtml);
                    break;
                case "1/x":
                    for (int i = 0; i < arrT.Length; i++)
                        arrT[i] = 1 / s.arr[i];
                    st.idHtml = string.Format("{0}<sup>-1</sup>", s.idHtml);
                    break;
                case "1/x^2":
                    for (int i = 0; i < arrT.Length; i++)
                        arrT[i] = 1 / (s.arr[i] * s.arr[i]);
                    st.idHtml = string.Format("{0}<sup>-2</sup>", s.idHtml);
                    break;
                case "1/x^3":
                    for (int i = 0; i < arrT.Length; i++)
                        arrT[i] = 1 / (s.arr[i] * s.arr[i] * s.arr[i]);
                    st.idHtml = string.Format("{0}<sup>-3</sup>", s.idHtml);
                    break;
                case "sqrt(x)":
                    for (int i = 0; i < arrT.Length; i++)
                        arrT[i] = Math.Sqrt(s.arr[i]);
                    st.idHtml = string.Format("{0}<sup>1/2</sup>", s.idHtml);
                    break;
                case "1/sqrt(x)":
                    for (int i = 0; i < arrT.Length; i++)
                        arrT[i] = 1 / Math.Sqrt(s.arr[i]);
                    st.idHtml = string.Format("{0}<sup>-1/2</sup>", s.idHtml);
                    break;
                case "ln(x)":
                    for (int i = 0; i < arrT.Length; i++)
                        arrT[i] = Math.Log(s.arr[i]);
                    st.idHtml = string.Format("ln({0})", s.idHtml);
                    break;
                case "e(x)":
                    for (int i = 0; i < arrT.Length; i++)
                        arrT[i] = Math.Exp(s.arr[i]);
                    st.idHtml = string.Format("exp({0})", s.idHtml);
                    break;
                default:
                    throw new Exception();
            }
            for (int i = 0; i < arrT.Length; i++)
                if (double.IsInfinity(arrT[i]) || double.IsNaN(arrT[i]))
                    throw new Exception();
            st.arr = arrT;
            st.id = string.Format(t.Replace("x", "{0}"), s.id);
            st.name = string.Format(t.Replace("x", "{0}"), s.name);
            st.Calculate();
            return st;
        }
        public static Sample Transform(Sample[] arrS, string expr)
        {
            Sample st = new Sample();
            Parser prs = new Parser();
            prs.SetExpr(expr);
            ParserVariable[] arrV = new ParserVariable[arrS.Length];
            for (int i = 0; i < arrS.Length; i++)
            {
                arrV[i] = new ParserVariable();
                prs.DefineVar(arrS[i].id, arrV[i]);
            }
            double[] arrT = new double[arrS[0].arr.Length];
            for (int i = 0; i < arrT.Length; i++)
            {
                for (int j = 0; j < arrV.Length; j++)
                {
                    arrV[j].Value = arrS[j].arr[i];
                }
                arrT[i] = prs.Eval();
            }
            for (int i = 0; i < arrT.Length; i++)
                if (double.IsInfinity(arrT[i]) || double.IsNaN(arrT[i]))
                    throw new Exception();
            st.arr = arrT;
            st.idHtml = st.id = expr;
            st.name = expr;
            st.Calculate();
            return st;
        }
        public static Sample Multiply(Sample s1, Sample s2)
        {
            if (s1.arr.Length != s2.arr.Length)
                throw new Exception();
            Sample s = new Sample();
            double[] arrT = new double[s1.arr.Length];
            for (int i = 0; i < arrT.Length; i++)
                arrT[i] = s1.arr[i] * s2.arr[i];
            for (int i = 0; i < arrT.Length; i++)
                if (double.IsInfinity(arrT[i]) || double.IsNaN(arrT[i]))
                    throw new Exception();
            s.idHtml = string.Format("{0}{1}", s1.idHtml, s2.idHtml);
            s.arr = arrT;
            s.id = string.Format("{0}*{1}", s1.id, s2.id);
            s.name = string.Format("{0}*{1}", s1.name, s2.name);
            s.Calculate();
            return s;
        }
        public static string[] ExcelSheets(string file)
        {
            string s = "provider = Microsoft.Jet.OLEDB.4.0;" +
                   "data source = " + file + ";" +
                   "extended properties = Excel 8.0;";
            OleDbConnection conn = new OleDbConnection(s);
            conn.Open();
            DataTable t = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
            string[] arr = new string[t.Rows.Count];
            for (int i = 0; i < t.Rows.Count; i++)
                arr[i] = t.Rows[i]["TABLE_NAME"].ToString();
            return arr;
        }
        public static string[] ExcelColumns(string file, string sheet)
        {
            string s = "provider = Microsoft.Jet.OLEDB.4.0;" +
                    "data source = " + file + ";" +
                    "extended properties = Excel 8.0;";
            OleDbConnection conn = new OleDbConnection(s);
            s = string.Format("SELECT * FROM [{0}]", sheet);
            OleDbDataAdapter da = new OleDbDataAdapter(s, conn);
            DataTable t = new DataTable();
            da.Fill(t);
            conn.Close();
            string[] arr = new string[t.Columns.Count];
            for (int i = 0; i < t.Columns.Count; i++)
                arr[i] = t.Columns[i].Caption;
            return arr;
        }
        public static Sample[] FromExcel(string file, string sheet, string[] arrColumn, string mark)
        {
            string s = "provider = Microsoft.Jet.OLEDB.4.0;" +
                    "data source = " + file + ";" +
                    "extended properties = Excel 8.0;";
            OleDbConnection conn = new OleDbConnection(s);
            s = string.Format("SELECT * FROM [{0}]", sheet);
            OleDbDataAdapter da = new OleDbDataAdapter(s, conn);
            DataTable t = new DataTable();
            da.Fill(t);
            conn.Close();
            if (arrColumn == null)
            {
                arrColumn = new string[t.Columns.Count];
                for (int i = 0; i < t.Columns.Count; i++)
                    arrColumn[i] = t.Columns[i].Caption;
            }
            double tmp;
            List<int> lIndex = new List<int>();
            for (int i = 0; i < t.Rows.Count; i++)
            {
                int j;
                for (j = 0; j < arrColumn.Length; j++)
                    if (!double.TryParse(t.Rows[i][arrColumn[j]].ToString(), out tmp))
                        break;
                if (j == arrColumn.Length)
                    lIndex.Add(i);
            }
            Sample[] arrS = new Sample[arrColumn.Length];
            for (int i = 0; i < arrS.Length; i++)
            {
                string name = arrColumn[i];
                double[] arrValue = new double[lIndex.Count];
                for (int j = 0; j < arrValue.Length; j++)
                    arrValue[j] = double.Parse(t.Rows[lIndex[j]][name].ToString());
                arrS[i] = new Sample(name, string.Format("{0}{1}", mark, i + 1), string.Format("{0}<sub>{1}</sub>", mark, i + 1), arrValue);
            }
            return arrS;
        }
    }
    public class Regression
    {
        public double[] arrYMod;
        public double[] arrB;      // коэффициенты регрессии
        public double[,] matrC;    // матр. сист. норм. ур.
        public double[,] matrCInv; // обр. матр. C
        public double[,] matrR;    // матрица коэф. кор.
        public double[,] matrRPart;// матр. част. коэфф. кор.
        public double R;           // коэф. множ. кор.
        public double devRem;      // ост. дисп.
        public int n, p;           // кол. набл. и кол. факторов        
        public Sample sY;
        public Sample[] arrSX;
        public Regression(Sample sY, Sample[] arrSX)
        {
            this.sY = sY;
            this.arrSX = arrSX;
            n = sY.arr.Length;
            p = arrSX.Length;
            double[][] matrX = new double[p + 1][];
            matrX[0] = new double[n];
            for (int j = 0; j < matrX[0].Length; j++)
                matrX[0][j] = 1;
            for (int i = 1; i < p + 1; i++)
                matrX[i] = (double[])arrSX[i - 1].arr.Clone();
            Matrix mX = Matrix.Create(matrX);
            mX.Transpose();
            Matrix mY = Matrix.Create(new double[][] { sY.arr });
            mY.Transpose();
            Matrix mXTr = mX.Clone();
            mXTr.Transpose();
            Matrix mB = mXTr * mX;
            matrC = mB.CopyToArray();
            mB = mB.Inverse();
            matrCInv = mB.CopyToArray();
            Matrix mXTrY = mXTr * mY;
            mB = mB * mXTrY;
            arrB = new double[mB.RowCount];
            for (int i = 0; i < mB.RowCount; i++)
                arrB[i] = mB[i, 0];

            // матр. коэф. кор.
            int smpCount = p + 1;
            List<Sample> lS = new List<Sample>(arrSX);
            lS.Insert(0, sY);
            matrR = new double[smpCount, smpCount];
            for (int i = 0; i < smpCount; i++)
                for (int j = 0; j <= i; j++)
                {
                    if (i == j)
                    {
                        matrR[i, j] = 1;
                        continue;
                    }
                    double sum = 0;
                    for (int k = 0; k < n; k++)
                        sum += lS[i][k] * lS[j][k];
                    matrR[i, j] = (sum / n - lS[i].av * lS[j].av) / (lS[i].sigma * lS[j].sigma);
                }
            for (int i = 0; i < smpCount; i++)
                for (int j = i + 1; j < smpCount; j++)
                    matrR[i, j] = matrR[j, i];

            // матр. част. коэф. кор.
            matrRPart = new double[smpCount, smpCount];
            Matrix mR = Matrix.Create(matrR);
            double minor11 = Minor(mR, 0, 0);
            for (int i = 0; i < mR.RowCount; i++)
                for (int j = 0; j < i; j++)
                    matrRPart[i, j] = Minor(mR, 1, j) /
                        Math.Sqrt(minor11 * Minor(mR, j, j));
            for (int i = 0; i < smpCount; i++)
                for (int j = i + 1; j < smpCount; j++)
                    matrRPart[i, j] = matrRPart[j, i];

            // эмп. знач. y
            arrYMod = new double[n];
            double[] arrX = new double[p];
            for (int i = 0; i < arrYMod.Length; i++)
            {
                for (int j = 0; j < arrX.Length; j++)
                    arrX[j] = arrSX[j][i];
                arrYMod[i] = RegValue(arrX);
            }

            // ост. дисп.
            devRem = 0;
            for (int i = 0; i < n; i++)
                devRem += (sY.arr[i] - arrYMod[i]) * (sY.arr[i] - arrYMod[i]);
            devRem /= n - p - 1;

            // коэф. множ. кор.
            R = Math.Sqrt(1 - devRem / sY.devStd);
            //R = Math.Sqrt(1 - mR.Determinant() / minor11);            
        }
        public double RegValue(double[] arrX)
        {
            double res = arrB[0];
            for (int i = 0; i < arrX.Length; i++)
                res += arrX[i] * arrB[i + 1];
            return res;
        }
        public string RegReport()
        {
            int d = 5;
            string s = string.Format(//"УРАВНЕНИЕ РЕГРЕССИИ ДЛЯ {0}<BR>" +
                //"Матрица системы нормальных уравнений:<BR>{1}<BR>" +
                //"Обратная матрица системы нормальных уравнений:<BR>{2}<BR>" +
                //"Уравнение регрессии:<BR> {3} = ",
                "{3} = ",
                sY.name, MatrixToHtml(matrC, d), MatrixToHtml(matrCInv, d), sY.idHtml);
            s += Math.Round(arrB[0], d).ToString();
            for (int i = 1; i < arrB.Length; i++)
            {
                if (Math.Round(arrB[i], d) >= 0)
                    s += "+";
                s += Math.Round(arrB[i], d).ToString() + arrSX[i - 1].idHtml;
            }
            s += string.Format("<BR>Остаточная дисперсия: s<SUB>ост</SUB> = {0}<BR>" +
                "Множественный коэффициент корреляции: R = {1}</P>",
                Math.Round(devRem, d), Math.Round(R, d));
            return s;
        }
        public string CheckReg(double alpha)
        {
            string s = string.Format("Уровень значимости: α = {0}<br>", alpha);
            // знач. регр.
            double fishRegrTheor = StatTables.GetFishInv(n - 1, n - p - 1, alpha);
            double fishRegr = sY.devStd / devRem;
            s += "ПРОВЕРКА ЗНАЧИМОСТИ УРАВНЕНИЯ РЕГРЕССИИ<br>";
            s += string.Format("Наблюдаемое значение критерия Фишера: F<sub>набл</sub> = {0:g3}<br>" +
                "Теоретическое значение критерия Фишера: F<sub>теор</sub> = F(1-α,n-1,n-p-1) = F({1};{2};{3}) = {4:g3}<br>{5}<br>",
                fishRegr, 1 - alpha, n - 1, n - p - 1, fishRegrTheor,
                fishRegr > fishRegrTheor ? "F<sub>набл</sub> > F<sub>теор</sub> => уравнение регрессии значимо" :
                "F<sub>набл</sub> ≤ F<sub>теор</sub> => уравнение регрессии не значимо");

            // знач. коэф. ур. регр.            
            s += "ПРОВЕРКА ЗНАЧИМОСТИ КОЭФФИЦИЕНТОВ УРАВНЕНИЯ РЕГРЕССИИ<br>";
            double studBTheor = StatTables.GetStudInv(n - p - 1, alpha);
            double[] arrSigmaB = new double[p + 1], arrStudB = new double[p + 1], arrBMin = new double[p + 1], arrBMax = new double[p + 1];
            s += string.Format("Теоретическое значение критерия Стьюдента: t<sub>теор</sub> = {0:g3}<br>", studBTheor);
            for (int i = 0; i < arrSigmaB.Length; i++)
            {
                arrSigmaB[i] = Math.Sqrt(devRem * matrCInv[i, i]);
                arrStudB[i] = arrB[i] / arrSigmaB[i];
                arrBMin[i] = arrB[i] - studBTheor * arrSigmaB[i];
                arrBMax[i] = arrB[i] + studBTheor * arrSigmaB[i];
                s += string.Format("Коэффициент b<sub>{0}</sub> ({1}): наблюдаемое значение критерия Стьюдента t<sub>набл</sub> = " +
                    "{2:g3}; {3}; доверительный интервал: [{4:g3}, {5:g3}]<br>",
                    i, i > 0 ? arrSX[i - 1].name : "свободный член", arrStudB[i],
                    Math.Abs(arrStudB[i]) > studBTheor ? "|t<sub>набл</sub>| > t<sub>теор</sub> => коэффициент значим" :
                    "|t<sub>набл</sub>| ≤ t<sub>теор</sub> => коэффициент не значим", arrBMin[i], arrBMax[i]);
            }

            // знач. множ. коэф. кор.
            s += "ПРОВЕРКА ЗНАЧИМОСТИ МНОЖЕСТВЕННОГО КОЭФФИЦИЕНТА КОРРЕЛЯЦИИ<br>";
            double sigmaR = (1 - R * R) / Math.Sqrt(n - p - 1), fishR = R * R * (n - p - 1) / (1 - R * R) * p, studR = R / sigmaR;
            double studRTheor = StatTables.GetStudInv(n - p - 1, alpha);
            double fishRTheor = StatTables.GetFishInv(n - p - 1, p, alpha);
            s += string.Format("Множественный коэффициент корреляции: R = {0:g3}<br>Наблюдаемое значение критерия Стьюдента: " +
                "t<sub>набл</sub> = {1:g3}<br>Теоретическое значение критерия Стьюдента: " +
                "t<sub>теор</sub> = t(1-α;n-p-1) = t({2};{3}) = {4:g3}<br>{5}<br>" +
                "Наблюдаемое значение критерия Фишера: F<sub>набл</sub> = {6:g3}<br>" +
                "Теоретическое значение критерия Фишера: F<sub>теор</sub> = F(1-α;n-p-1;p) = F({7};{8};{9}) = {10:g3}<br>{11}<br>",
                R, studR, 1 - alpha, n - p - 1, studRTheor, Math.Abs(studR) > studRTheor ? "|t<sub>набл</sub>| > t<sub>теор</sub> => коэффициент значим" :
            "|t<sub>набл</sub>| ≤ t<sub>теор</sub> => коэффициент не значим", fishR, 1 - alpha, n - p - 1, p, fishRTheor,
             fishR > fishRTheor ? "F<sub>набл</sub> > F<sub>теор</sub> => коэффициент значим" :
            "F<sub>набл</sub> ≤ F<sub>теор</sub> => коэффициент не значим");
            return s;
        }
        public string CorrReport()
        {
            int d = 3;
            List<string> l = new List<string>();
            l.Add(sY.name);
            foreach (Sample s in arrSX)
                l.Add(s.name);
            return string.Format("Корреляционная матрица<br>{0}",//Матрица частных коэффициентов корреляции{1}",
                MatrixToHtml(matrR, d, l.ToArray()),
                 MatrixToHtml(matrRPart, d, l.ToArray()));
        }
        public string CheckCorr(double alpha)
        {
            double tTheor = StatTables.GetStudInv(n - 2, alpha);
            string s = string.Format("Уровень значимости: α = {0}<br>", alpha);
            s += string.Format("ПРОВЕРКА ЗНАЧИМОСТИ КОЭФФИЦИЕНТОВ КОРРЕЛЯЦИИ<br>Теоретическое значение критерия Стьюдента: " +
                "t<sub>теор</sub> = t(1-α,n-2) = t({0};{1}) = {2:g3}<table border = 1 cellspacing = 0>", 1 - alpha, n - 2, tTheor);
            string[,] matr = new string[p + 1, p + 1];
            for (int i = 0; i < matrR.GetLength(0); i++)
            {
                for (int j = 0; j < matrR.GetLength(1); j++)
                {
                    double sigma = (1 - matrR[i, j]) / Math.Sqrt(n - 1);
                    double t = matrR[i, j] / sigma;
                    if (Math.Abs(t) > tTheor)
                        matr[i, j] = string.Format("|t<sub>набл</sub>| = {0:g3} > t<sub>теор</sub> => значим", Math.Abs(t));
                    else
                        matr[i, j] = string.Format("|t<sub>набл</sub>| = {0:g3} ≤ t<sub>теор</sub> => не значим", Math.Abs(t));
                }
            }
            List<string> l = new List<string>();
            l.Add(sY.name);
            foreach (Sample smp in arrSX)
                l.Add(smp.name);
            s += MatrixToHtml(matr, l.ToArray());
            return s;
        }
        public double Ryx1()
        {
            return matrR[0, 1];
        }
        double Minor(Matrix m, int rowIndex, int colIndex)
        {
            Matrix mTmp = new Matrix(m.RowCount - 1, m.ColumnCount - 1);
            for (int i = 0; i < mTmp.RowCount; i++)
                for (int j = 0; j < mTmp.ColumnCount; j++)
                {
                    int i1 = i, j1 = j;
                    if (i >= rowIndex)
                        i1++;
                    if (j >= colIndex)
                        j1++;
                    mTmp[i, j] = m[i1, j1];
                }
            return mTmp.Determinant();
        }
        string MatrixToHtml(double[,] matr, int d)
        {
            string s = "<TABLE border = 1 cellspacing = 0 align = center>";
            for (int i = 0; i < matr.GetLength(0); i++)
            {
                s += "<TR>";
                for (int j = 0; j < matr.GetLength(1); j++)
                    s += "<TD>" + Math.Round(matr[i, j], d).ToString() + "</TD>";
                s += "</TR>";
            }
            s += "</TABLE>";
            return s;
        }
        string MatrixToHtml(double[,] matr, int d, string[] arrRCHeader)
        {
            string s = "<TABLE border = 1 cellspacing = 0 align = center><TR><TD></TD>";
            for (int i = 0; i < arrRCHeader.Length; i++)
                s += "<TD>" + arrRCHeader[i] + "</TD>";
            s += "</TR>";
            for (int i = 0; i < matr.GetLength(0); i++)
            {
                s += "<TR><TD>" + arrRCHeader[i] + "</TD>";
                for (int j = 0; j < matr.GetLength(1); j++)
                    s += "<TD>" + Math.Round(matr[i, j], d).ToString() + "</TD>";
                s += "</TR>";
            }
            s += "</TABLE>";
            return s;
        }
        string MatrixToHtml(double[,] matr, int d, string[] arrRHeader, string[] arrCHeader)
        {
            string s = "<TABLE border = 1 cellspacing = 0 align = center><TR><TD></TD>";
            for (int i = 0; i < arrCHeader.Length; i++)
                s += "<TD>" + arrCHeader[i] + "</TD>";
            s += "</TR>";
            for (int i = 0; i < matr.GetLength(0); i++)
            {
                s += "<TR><TD>" + arrRHeader[i] + "</TD>";
                for (int j = 0; j < matr.GetLength(1); j++)
                    s += "<TD>" + Math.Round(matr[i, j], d).ToString() + "</TD>";
                s += "</TR>";
            }
            s += "</TABLE>";
            return s;
        }
        string MatrixToHtml(string[,] matr, string[] arrRCHeader)
        {
            string s = "<TABLE border = 1 cellspacing = 0 align = center><TR><TD></TD>";
            for (int i = 0; i < arrRCHeader.Length; i++)
                s += "<TD>" + arrRCHeader[i] + "</TD>";
            s += "</TR>";
            for (int i = 0; i < matr.GetLength(0); i++)
            {
                s += "<TR><TD>" + arrRCHeader[i] + "</TD>";
                for (int j = 0; j < matr.GetLength(1); j++)
                    s += "<TD>" + matr[i, j] + "</TD>";
                s += "</TR>";
            }
            s += "</TABLE>";
            return s;
        }

        public static SortedDictionary<double, Sample> TranSamples(Sample sx, Sample sy)
        {
            string[] arrT = { "x", "x^2", "x^3", "1/x", "1/x^2", "1/x^3", "sqrt(x)", "1/sqrt(x)", "ln(x)", "e(x)" };
            SortedDictionary<double, Sample> dic = new SortedDictionary<double, Sample>();
            for (int i = 0; i < arrT.Length; i++)
            {
                Sample ts;
                try
                {
                    ts = Sample.Transform(sx, arrT[i]);
                }
                catch
                {
                    continue;
                }
                Regression reg = new Regression(sy, new Sample[] { ts });
                dic.Add(Math.Abs(reg.Ryx1()), ts);
            }
            return dic;
        }
        public static SortedDictionary<double, Sample> TranSamples(Sample[] arrSX, Sample sY,
            Form form, ProgressDelegate method)
        {
            string[] arrT = { "x" };//, "x^2", "x^3", "1/x", "1/x^2", "1/x^3", "sqrt(x)", "1/sqrt(x)", "ln(x)", "e(x)" };
            SortedDictionary<double, Sample> dic = new SortedDictionary<double, Sample>();
            int percent = 0;
            foreach (Sample sx1 in arrSX)
            {
                foreach (Sample sx2 in arrSX)
                {
                    foreach (string t1 in arrT)
                    {
                        foreach (string t2 in arrT)
                        {
                            Sample tsx1, tsx2;
                            try
                            {
                                tsx1 = Sample.Transform(sx1, t1);
                                tsx2 = Sample.Transform(sx2, t1);
                            }
                            catch
                            {
                                continue;
                            }
                            Sample s = Sample.Multiply(tsx1, tsx2);
                            Regression r = new Regression(sY, new Sample[] { s });
                            try
                            {
                                dic.Add(Math.Abs(r.Ryx1()), s);
                            }
                            catch { }
                        }
                    }
                }
                percent += (int)(1.0 / arrSX.Length * 100);
                form.Invoke(method, percent);
            }
            return dic;
        }
        public delegate void ProgressDelegate(int percent);
    }
    public class StatTables
    {
        static double epsilon = 0.0000001;
        static StudentsTDistribution distStud = new StudentsTDistribution();
        static FisherSnedecorDistribution distFish = new FisherSnedecorDistribution();
        static ChiSquareDistribution distChi2 = new ChiSquareDistribution();
        static public double GetStudInv(int n, double alpha)
        {
            distStud.DegreesOfFreedom = n;
            return GetX(1 - alpha, distStud);
        }
        static public double GetChi2Inv(int n, double alpha)
        {
            distChi2.DegreesOfFreedom = n;
            return GetX(1 - alpha, distChi2);
        }
        static public double GetFishInv(int n1, int n2, double alpha)
        {
            distFish.Alpha = n1;
            distFish.Beta = n2;
            return GetX(1 - alpha, distFish);
        }
        public static double GetTau(double alpha, double v)
        {
            int i, j;
            for (i = arrAlpha.Length - 1; i >= 0; i--)
                if (arrAlpha[i] >= alpha)
                    break;
            for (j = arrV.Length - 1; j >= 0; j--)
                if (arrV[j] <= v)
                    break;
            return arrTau[j, i];
        }
        static double GetX(double p, ContinuousDistribution dist)
        {
            double x, xNext = 1;
            do
            {
                x = xNext;
                xNext = x - (dist.CumulativeDistribution(x) - p) /
                    dist.ProbabilityDensity(x);
            }
            while (Math.Abs(x - xNext) > epsilon);
            return xNext;
        }
        static double[] arrAlpha = new double[]
                {
                    0.10f, 0.05f, 0.025f, 0.01f
                };
        static int[] arrV = new int[]
                {
                    3, 4, 5, 6, 7, 8, 9, 10,
                    11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
                    21, 22, 23, 24, 25
                };
        static double[,] arrTau = new double[,]
                {
                    { 1.41f, 1.41f, 1.41f, 1.41f },
                    { 1.65f, 1.69f, 1.71f, 1.72f },
                    { 1.79f, 1.87f, 1.92f, 1.96f },
                    { 1.89f, 2.00f, 2.07f, 2.13f },
                    { 1.97f, 2.09f, 2.18f, 2.27f },
                    { 2.04f, 2.17f, 2.27f, 2.37f },
                    { 2.10f, 2.24f, 2.35f, 2.46f },
                    { 2.15f, 2.29f, 2.41f, 2.54f },                    
                    { 2.19f, 2.34f, 2.47f, 1.41f },
                    { 2.23f, 2.39f, 2.52f, 1.72f },
                    { 2.26f, 2.43f, 2.56f, 1.96f },
                    { 2.30f, 2.46f, 2.60f, 2.13f },                    
                    { 2.33f, 2.49f, 2.64f, 2.80f },
                    { 2.35f, 2.52f, 2.67f, 2.84f },
                    { 2.38f, 2.55f, 2.70f, 2.87f },
                    { 2.40f, 2.58f, 2.73f, 2.90f },
                    { 2.43f, 2.60f, 2.75f, 2.93f },
                    { 2.45f, 2.62f, 2.78f, 2.96f },
                    { 2.47f, 2.64f, 2.80f, 2.98f },
                    { 2.49f, 2.66f, 2.82f, 3.01f },
                    { 2.50f, 2.68f, 2.84f, 3.03f },
                    { 2.52f, 2.70f, 2.86f, 3.05f },
                    { 2.54f, 2.72f, 2.88f, 3.07f }
                };
    }

    public class OptParams
    {
        public double[][] arrCoeff;
        public double[] arrUMin, arrUMax, arrUSMin, arrYOpt, arrYMin, arrYMax, arrAlpha, arrUMid;
        public double r, mult;
        public string[,] mFunc;
        public OptParams(double[][] arrCoeff, string[,] mFunc,
            double[] arrUMin, double[] arrUMax, double[] arrUSMin, double[] arrYOpt,
            double[] arrYMin, double[] arrYMax, double[] arrAlpha, double r, double mult)
        {
            this.arrCoeff = arrCoeff;
            this.mFunc = mFunc;
            this.arrUMin = arrUMin;
            this.arrUMax = arrUMax;
            this.arrUSMin = arrUSMin;
            this.arrYOpt = arrYOpt;
            this.arrYMin = arrYMin;
            this.arrYMax = arrYMax;
            this.arrAlpha = arrAlpha;
            this.r = r;
            this.mult = mult;
            arrUMid = new double[arrUMin.Length];
            for (int i = 0; i < arrUMid.Length; i++)
                arrUMid[i] = (arrUMax[i] + arrUMin[i]) / 2;
        }
    }
    public class HJIteration
    {
        public double[] arrX, arrE; // arrE - направление
        public double[] arrXDelta;
        public double mult;         // mult - множитель в поиске по обр.
        public double fRes;        
        public HJIteration() { }
        public HJIteration(double[] arrXToClone, double[] arrXDeltaToClone)
        {
            arrX = (double[])arrXToClone.Clone();
            arrXDelta = (double[])arrXDeltaToClone.Clone();
            arrE = null;
            mult = 1;
        }
        public HJIteration(double[] arrXToClone, double[] arrXDeltaToClone,
            double[] arrEToClone, double mult)
        {
            arrX = (double[])arrXToClone.Clone();
            arrXDelta = (double[])arrXDeltaToClone.Clone();
            if (arrEToClone != null)
                arrE = (double[])arrEToClone.Clone();
            this.mult = mult;
        }
        public override string ToString()
        {
            return "F = " + fRes.ToString();
        }
    }
    public class HJOptimizer
    {
        public delegate double OptFunc(object optParams, double[] arrX);
        OptFunc func;
        object optParams;
        double xEpsilon, fEpsilon;                
        public HJIteration[] Optimize(double fEps, double xEps, int iterMax, OptFunc func,
            object optParams, double[] arrXInit, double[] arrXDeltaInit)
        {
            this.func = func;
            this.optParams = optParams;
            this.xEpsilon = xEps;
            this.fEpsilon = fEps;
            HJIteration it = new HJIteration() { arrX = arrXInit, arrXDelta = arrXDeltaInit };
            List<HJIteration> lIter=new List<HJIteration>();
            lIter.Add(it);
            int iterNum = 0;
            do
            {
                if ((it = (HJIteration)DoIteration(it)) == null)
                    break;
                lIter.Add(it);
                iterNum++;
            }
            while (iterNum < iterMax);
            return lIter.ToArray();
        }
        HJIteration DoIteration(HJIteration it)
        {
            double f = F(it.arrX);
            while (it.arrE == null)
            {
                f = F(it.arrX);
                double[] arrENext, arrXNext;
                double fNext = Research(it.arrX, it.arrXDelta, out arrENext, out arrXNext);
                if (Math.Abs(fNext - f) < fEpsilon)
                {
                    for (int i = 0; i < it.arrXDelta.Length; i++)
                        it.arrXDelta[i] /= 2;
                    for (int i = 0; i < it.arrXDelta.Length; i++)
                        if (it.arrXDelta[i] < xEpsilon)
                        {
                            it.fRes = F(it.arrX);
                            return null;
                        }
                }
                else
                    it.arrE = arrENext;
            }
            double[] arrXSmp = new double[it.arrX.Length];
            for (int i = 0; i < arrXSmp.Length; i++)
                arrXSmp[i] = it.arrX[i] + it.arrE[i] * it.mult;
            double fSmp = F(arrXSmp);
            if (Math.Abs(fSmp - f) < fEpsilon || fSmp > f)
            {
                it.fRes = f;
                return new HJIteration(it.arrX, it.arrXDelta, null, 1);
            }
            double[] arrXRes, arrERes;
            double fRes = Research(arrXSmp, it.arrXDelta, out arrERes, out arrXRes);
            HJIteration itNext;
            if (Math.Abs(fRes - f) < fEpsilon)
            {
                it.fRes = f;
                itNext = new HJIteration(it.arrX, it.arrXDelta, null, 1);
            }
            else
            {
                it.fRes = fSmp;
                itNext = new HJIteration(arrXSmp, it.arrXDelta, it.arrE, it.mult + 1);
            }
            return itNext;
        }
        double F(double[] arrX)
        {
            return func(optParams, arrX);
        }
        double Research(double[] arrX, double[] arrXDelta,
            out double[] arrE, out double[] arrXNext)
        {
            arrE = new double[arrX.Length];
            arrXNext = (double[])arrX.Clone();
            for (int i = 0; i < arrE.Length; i++)
            {
                double f = F(arrXNext);
                double xOld = arrXNext[i];
                arrXNext[i] -= arrXDelta[i];
                double f1 = F(arrXNext);
                arrXNext[i] = xOld + arrXDelta[i];
                double f2 = F(arrXNext);
                arrXNext[i] = xOld;
                if (f1 < f && f1 <= f2)
                    arrE[i] = -arrXDelta[i];
                else if (f2 < f && f2 < f1)
                    arrE[i] = arrXDelta[i];
                else
                    arrE[i] = 0;
                arrXNext[i] += arrE[i];
            }
            return F(arrXNext);
        }
    }       
}
