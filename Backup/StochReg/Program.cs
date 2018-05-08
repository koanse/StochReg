using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Collections;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Distributions;

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
        public static void Model(Variable[] arrU, Variable[] arrY, out double[][] arrC, out string[,] matrF, out string rep)
        {
            rep = "";
            arrC = new double[arrY.Length][];
            matrF = new string[arrY.Length, arrU.Length];
            string[] arrT = { "x", "x^2", "x^3", "1/x", "1/x^2", "sqrt(x)", "1/sqrt(x)", "ln(x)", "exp(x)" };
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
                        TranSample ts;
                        try
                        {
                            ts = new TranSample(su, t);
                        }
                        catch
                        {
                            continue;
                        }
                        Regression r = new Regression(sy, new Sample[] { ts });
                        double corr;
                        if ((corr = Math.Abs(r.GetRyx1())) >= max)
                        {
                            max = corr;
                            tMax = t;
                            suMax = ts;
                        }
                    }
                    matrF[i, j] = tMax;
                    lSU.Add(suMax);
                }
                Regression reg = new Regression(sy, lSU.ToArray());
                arrC[i] = reg.arrB;
                rep += reg.RegReport() + "<br>";
            }
        }
        public static void Optimize(int m, int n, // число факторов и показателей
            double[][] arrC, string[,] matrF,
            double[] arrUMin, double[] arrUMax, double[] arrUSMin, double[] arrYMin, double[] arrYMax,
            double[] arrAlpha, double[] arrYOpt, double mult,
            double dx, double df, double uInit, double duInit, double sInit, double dsInit,
            int iterCount, double R, double C, double[] arrZ,
            out double[] arrUOpt, out double[] arrSOpt, out string rep)
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
                        arrCNew[i][0] += arrC[i][j + 1] * F(arrZ[j], matrF[i, j]);
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
            HJInitialParams init = new HJInitialParams(dx, df, arrC, matrF, arrUMinNew, arrUMaxNew,
                arrUSMinNew, arrYOpt, arrYMin, arrYMax, arrAlpha, R, mult);                
            List<HJIteration> lIter = new List<HJIteration>();
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
            opt.Initialize(init);
            HJIteration it = new HJIteration(arrX, arrXDelta);
            double f = double.MaxValue;            
            do
            {
                int iterNum = 0;
                do
                {
                    if ((it = (HJIteration)opt.DoIteration(it)) == null)
                        break;
                    lIter.Add(it);
                    iterNum++;
                }
                while (iterNum < iterCount);
                double FNext = lIter.Last().fRes;
                if (Math.Abs(FNext - f) < df)
                    break;
                f = FNext;
                R *= C;
                init = new HJInitialParams(dx, df, arrC, matrF, arrUMinNew, arrUMaxNew, arrUSMinNew,
                    arrYOpt, arrYMin, arrYMax, arrAlpha, R, mult);
                opt = new HJOptimizer();
                opt.Initialize(init);
                it = new HJIteration(lIter.Last().arrX, arrXDelta);
            }
            while (true);
            arrUOpt = new double[p];
            for (int i = 0; i < p; i++)
            {
                arrUOpt[i] = lIter.Last().arrX[i];
            }
            arrSOpt = new double[p];
            for (int i = 0; i < p; i++)
            {
                arrSOpt[i] = lIter.Last().arrX[i + p];
            }
            rep = lIter.Last().ToHtml(init, 4);
            /*rep += "</table><br>";
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
            df.Show();*/
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
                    return 1 / x / x;
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
        public static string F(string id, string func)
        {
            switch (func)
            {
                case "x":
                    return id;
                case "x^2":
                    return string.Format("{0}<sup>2</sup>", id);
                case "x^3":
                    return string.Format("{0}<sup>3</sup>", id);
                case "1/x":
                    return string.Format("1/{0}", id);
                case "1/x^2":
                    return string.Format("1/{0}<sub>2</sub>", id);
                case "sqrt(x)":
                    return string.Format("{0}<sub>0.5</sub>", id);
                case "1/sqrt(x)":
                    return string.Format("1/{0}<sub>0.5</sub>", id);
                case "ln(x)":
                    return string.Format("ln({0})", id);
                case "exp(x)":
                    return string.Format("exp({0})", id); ;
                default:
                    throw new Exception();
            }
        }
        /*public static string Reg(int i, string id, double[] C, Variable[] arrU, string[,] matrF)
        {
            string s = string.Format("{0} = {1:f4}", id, C[0]);
            for (int i = 0; i < length; i++)
            {
                
            }
        }*/
        public static void Unite(Stage[] arrStage, out Variable[] arrU, out Variable[] arrY)
        {
            int columns = 0, rows = arrStage[0].arrU[0].arr.Length;
            List<Variable> lU = new List<Variable>();
            for (int i = 0; i < arrStage.Length; i++)
            {
                columns += arrStage[i].arrU.Length;
                foreach (Variable u in arrStage[i].arrU)
                {
                    lU.Add(u);
                }
            }
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
                    for (int k = 0; k < arrStage[j].arrU.Length; k++)
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
        }
        public static void Emulate(Stage[] arrSReg, Stage[] arrSEmul, double p, // p - вер. опт. знач. фактора
            double[] arrUMin, double[] arrUMax, double[] arrUSMin, double[] arrYMin, double[] arrYMax,
            double[] arrAlpha, double[] arrYOpt, double mult,
            double dx, double df, double uInit, double duInit, double sInit, double dsInit,
            int iterCount, double R, double C,
            out Variable[] arrU, out Variable[] arrY, out string rep)
        {
            Variable[] arrUReg, arrYReg, arrUEmul, arrYEmul;
            Unite(arrSReg, out arrUReg, out arrYReg);
            double[][] arrC;
            string[,] matrF;
            string repMod;
            Model(arrUReg, arrYReg, out arrC, out matrF, out repMod);
            Unite(arrSEmul, out arrUEmul, out arrYEmul);
            arrU = new Variable[arrUEmul.Length];
            for (int i = 0; i < arrU.Length; i++)
            {
                arrU[i] = (Variable)arrUEmul[i].Clone();
            }
            arrY = new Variable[arrYEmul.Length];
            for (int i = 0; i < arrY.Length; i++)
            {
                arrY[i] = (Variable)arrYEmul[i].Clone();
            }
            rep = repMod + "<table border = 1 cellspacing = 0><tr><td>№ ед. прод.";
            for (int i = 0; i < arrU.Length; i++)
            {
                rep += string.Format("<td>{0}", arrU[i].name);
            }
            for (int i = 0; i < arrY.Length; i++)
            {
                rep += string.Format("<td>{0}", arrY[i].name);
            }
            Random rnd = new Random();
            for (int i = 0; i < arrUEmul[0].arr.Length / 10; i++)
            {
                rep += string.Format("<tr><td>{0}", i);
                List<double> lZ = new List<double>();
                for (int j = 0; j < arrUEmul.Length; j++)
                {
                    if (rnd.NextDouble() > p)
                    {
                        rep += string.Format("<td>{0:f4}", arrU[j].arr[i]);
                        lZ.Add(arrU[j].arr[i]);
                        continue;
                    }
                    double[] arrUOpt, arrSOpt;
                    string repOpt;
                    Optimize(arrUEmul.Length, arrYEmul.Length, arrC, matrF, arrUMin, arrUMax, arrUSMin, arrYMin,
                        arrYMax, arrAlpha, arrYOpt, mult, dx, df, uInit, duInit, sInit, dsInit, iterCount, R, C, lZ.ToArray(),
                        out arrUOpt, out arrSOpt, out repOpt);
                    double u = arrUOpt[0];
                    lZ.Add(u);
                    arrU[j].arr[i] = u;
                    rep += string.Format("<td>{0:f4}<br>{1}", u, repOpt);
                }
                for (int j = 0; j < arrY.Length; j++)
                {
                    arrY[j].arr[i] = arrC[j][0];
                    for (int k = 0; k < lZ.Count; k++)
                    {
                        arrY[j].arr[i] += arrC[j][k + 1] * F(lZ[k], matrF[j, k]);
                    }
                    rep += string.Format("<td>{0:f4}", arrY[j].arr[i]);
                }
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
        public double sum, sum2, dev, devStd, sigma, sigmaStd;
        public double[] arrFreq, arrMid;    // частоты и середины интервалов
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
                    double t = StatTables.GetStudDistrInv(n - 2, alpha);
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
        public void DoHistogram(bool useSturgess)
        {
            int k;
            if (useSturgess)
                k = 1 + (int)(3.32 * Math.Log10(arr.Length));
            else
                k = (int)Math.Sqrt(arr.Length);
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

            arrMid = new double[k];
            for (int i = 0; i < arrMid.Length; i++)
                arrMid[i] = min + h * (i + 0.5);
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
        public string GetReport()
        {
            int d = 5;
            string s = string.Format("<P>ВЫБОРОЧНЫЕ ХАРАКТЕРИСТИКИ {0}, {1}<BR>" +
                "Минимум: {1}<SUB>min</SUB> = {2}<BR>" +
                "Максимум: {1}<SUB>max</SUB> = {3}<BR>" +
                "Размах выборки: w = {4}<BR>" +
                "Среднее: {1}<SUB>ср</SUB> = {5}<BR>" +
                "Средний квадрат: {1}<SUP>2</SUP><SUB>ср</SUB> = {6}<BR>" +
                "Дисперсия: s<SUP>2</SUP> = {7}<BR>" +
                "Среднее квадр. откл.: s = {8}<BR>" +
                "Испр. дисперсия: s<SUP>2</SUP><SUB>испр</SUB> = {9}<BR>" +
                "Испр. среднее квадр. откл.: s<SUB>испр</SUB> = {10}<BR>" +
                "Асимметрия: A = {11}<BR>" +
                "Эксцесс: E = {12}<BR>" +
                "Коэффициент вариации: v = {13}<BR></P>",
                name, id, Math.Round(min, d), Math.Round(max, d),
                Math.Round(min - max, d), Math.Round(av, d),
                Math.Round(av2, d), Math.Round(dev, d), Math.Round(sigma, d),
                Math.Round(devStd, d), Math.Round(sigmaStd, d), Math.Round(asym, d),
                Math.Round(exc, d), Math.Round(var, d));
            return s;
        }
        public string CheckNorm(double alpha)
        {
            double n = arr.Length;
            double y = asym * Math.Sqrt((n + 1) * (n + 3) / 6 / (n - 2));
            double beta = 3 * (n * n + 27 * n - 70) * (n + 1) * (n + 3) / (n - 2) / (n + 5) / (n + 7) / (n + 9);
            double w2 = -1 + Math.Sqrt(2 * (beta - 1)), delta = 1 / Math.Sqrt(Math.Log(w2));
            double alp = Math.Sqrt(2 / (w2 - 1));
            double zAs = delta * Math.Log((y / alp) + Math.Sqrt((y / alp) * (y / alp) + 1));

            double c = 6 * (n * n - 5 * n + 2) / (n + 7) / (n + 9) *
                Math.Sqrt(6 * (n + 3) * (n + 5) / n / (n - 2) / (n - 3));
            double f = 6 + 8 / c * (2 / c + Math.Sqrt(1 + 4 / c / c));
            double mExc = 3 - 6 / (n + 1), devExc = 24 / n * (1 - 225 / (15 * n + 224));
            double x = (exc - mExc) / Math.Sqrt(devExc);
            double dExc = (1 - 2 / 9 / f - Math.Pow((1 - 2 / f) / (1 + x * Math.Sqrt(2 / (f - 4))), 1.0 / 3)) / Math.Sqrt(2.0 / 9 / f);

            NormalDistribution dist = new NormalDistribution(0, 1);
            double u = dist.InverseCumulativeDistribution(1 - alpha / 2);
            string s = string.Format("Уровень значимости: α = {0}<br>", alpha);
            s += string.Format("Квантиль стандартного нормального распределения для двустороннего доверительного интервала:<br>" +
                "u = u(1-α/2) = {0:f3}<br>", u);
            s += string.Format("Асимметрия: A = {0:g3}<br>Нормализующее z-преобразование асимметрии: z = {1:g3}<br>", asym, zAs);
            if (Math.Abs(zAs) < u)
                s += "|z| < u =>  нет оснований отвергнуть гипотезу о нормальности<br>";
            else
                s += "|z| ≥ u =>  гипотеза о нормальности отвергается<br>";
            s += string.Format("Эксцесс: E = {0:g3}<br>Нормализующее d-преобразование эксцесса: d = {1:g3}<br>", exc, dExc);
            if (Math.Abs(dExc) < u)
                s += "|d| < u =>  нет оснований отвергнуть гипотезу о нормальности<br>";
            else
                s += "|d| ≥ u =>  гипотеза о нормальности отвергается<br>";
            return s;
        }
        public object Clone()
        {
            return new Sample(name, id, (double[])arr.Clone());
        }
        public void Transform(string t)
        {
            double[] arrT = (double[])arr.Clone();
            switch (t)
            {
                case "x^2":
                    for (int i = 0; i < arrT.Length; i++)
                        arrT[i] = arr[i] * arr[i];
                    break;
                case "x^3":
                    for (int i = 0; i < arrT.Length; i++)
                        arrT[i] = arr[i] * arr[i] * arr[i];
                    break;
                case "1/x":
                    for (int i = 0; i < arrT.Length; i++)
                        arrT[i] = 1 / arr[i];
                    break;
                case "1/x^2":
                    for (int i = 0; i < arrT.Length; i++)
                        arrT[i] = 1 / (arr[i] * arr[i]);
                    break;
                case "sqrt(x)":
                    for (int i = 0; i < arrT.Length; i++)
                        arrT[i] = Math.Sqrt(arr[i]);
                    break;
                case "1/sqrt(x)":
                    for (int i = 0; i < arrT.Length; i++)
                        arrT[i] = 1 / Math.Sqrt(arr[i]);
                    break;
                case "ln(x)":
                    for (int i = 0; i < arrT.Length; i++)
                        arrT[i] = Math.Log(arr[i]);
                    break;
                case "exp(x)":
                    for (int i = 0; i < arrT.Length; i++)
                        arrT[i] = Math.Exp(arr[i]);
                    break;
                case "x":
                    for (int i = 0; i < arrT.Length; i++)
                        arrT[i] = arr[i];
                    break;
                default:
                    throw new Exception();
            }
            for (int i = 0; i < arrT.Length; i++)
                if (double.IsInfinity(arrT[i]) || double.IsNaN(arrT[i]))
                    throw new Exception();
            switch (t)
            {
                case "x^2":
                    idHtml = string.Format("{0}<sup>2</sup>", idHtml);
                    break;
                case "x^3":
                    idHtml = string.Format("{0}<sup>3</sup>", idHtml);
                    break;
                case "1/x":
                    idHtml = string.Format("{0}<sup>-1</sup>", idHtml);
                    break;
                case "1/x^2":
                    idHtml = string.Format("{0}<sup>-2</sup>", idHtml);
                    break;
                case "sqrt(x)":
                    idHtml = string.Format("{0}<sup>1/2</sup>", idHtml);
                    break;
                case "1/sqrt(x)":
                    idHtml = string.Format("{0}<sup>-1/2</sup>", idHtml);
                    break;
                case "ln(x)":
                    idHtml = string.Format("ln({0})", idHtml);
                    break;
                case "exp(x)":
                    idHtml = string.Format("exp({0})", idHtml);
                    break;
            }
            id = string.Format(t.Replace("x", "{0}"), id);
            arr = arrT;
            Calculate();
        }
        public void Norm()
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
    }
    public class Regression
    {
        public double[] arrYMod;
        public double[] arrB;      // коэффициенты регрессии
        public double[,] matrC;    // матр. сист. норм. ур.
        public double[,] matrCInv; // обр. матр. C
        public double[,] matrR;    // матрица коэф. кор.
        public double[,] matrRPart;// матр. част. коэфф. кор.
        public double R;   // коэф. множ. кор.
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
            double minor11 = GetMinor(mR, 0, 0);
            for (int i = 0; i < mR.RowCount; i++)
                for (int j = 0; j < i; j++)
                    matrRPart[i, j] = GetMinor(mR, 1, j) /
                        Math.Sqrt(minor11 * GetMinor(mR, j, j));
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
            string s = string.Format("УРАВНЕНИЕ РЕГРЕССИИ ДЛЯ {0}<BR>" +
                "Матрица системы нормальных уравнений:<BR>{1}<BR>" +
                "Обратная матрица системы нормальных уравнений:<BR>{2}<BR>" +
                "Уравнение регрессии:<BR> {3} = ", sY.name,
                MatrixToHtml(matrC, d), MatrixToHtml(matrCInv, d), sY.idHtml);
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
            double fishRegrTheor = StatTables.GetFishDistrInv(n - 1, n - p - 1, alpha);
            double fishRegr = sY.devStd / devRem;
            s += "ПРОВЕРКА ЗНАЧИМОСТИ УРАВНЕНИЯ РЕГРЕССИИ<br>";
            s += string.Format("Наблюдаемое значение критерия Фишера: F<sub>набл</sub> = {0:g3}<br>" +
                "Теоретическое значение критерия Фишера: F<sub>теор</sub> = F(1-α,n-1,n-p-1) = F({1};{2};{3}) = {4:g3}<br>{5}<br>",
                fishRegr, 1 - alpha, n - 1, n - p - 1, fishRegrTheor,
                fishRegr > fishRegrTheor ? "F<sub>набл</sub> > F<sub>теор</sub> => уравнение регрессии значимо" :
                "F<sub>набл</sub> ≤ F<sub>теор</sub> => уравнение регрессии не значимо");

            // знач. коэф. ур. регр.            
            s += "ПРОВЕРКА ЗНАЧИМОСТИ КОЭФФИЦИЕНТОВ УРАВНЕНИЯ РЕГРЕССИИ<br>";
            double studBTheor = StatTables.GetStudDistrInv(n - p - 1, alpha);
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
            double studRTheor = StatTables.GetStudDistrInv(n - p - 1, alpha);
            double fishRTheor = StatTables.GetFishDistrInv(n - p - 1, p, alpha);
            s += string.Format("Множественный коэффициент корреляции: R = {0:g3}<br>Наблюдаемое значение критерия Стьюдента: " +
                "t<sub>набл</sub> = {1:g3}<br>Теоретическое значение критерия Стьюдента: " +
                "t<sub>теор</sub> = t(1-α;n-p-1) = t({2};{3}) = {4:g3}<br>{5}<br>" +
                "Наблюдаемое значение критерия Фишера: F<sub>набл</sub> = {6:g3}<br>" +
                "Теоретическое значение критерия Фишера: F<sub>теор</sub> = F(1-α;n-p-1;p) = F({7};{8};{9} = {10:g3}<br>{11}<br>",
                R, studR, 1 - alpha, n - p - 1, studRTheor, Math.Abs(studR) > studRTheor ? "|t<sub>набл</sub>| > t<sub>теор</sub> => коэффициент значим" :
            "|t<sub>набл</sub>| ≤ t<sub>теор</sub> => коэффициент не значим", fishR, 1 - alpha, n - p - 1, p, fishRTheor,
             fishR > fishRTheor ? "F<sub>набл</sub> > F<sub>теор</sub> => коэффициент значим" :
            "F<sub>набл</sub> ≤ F<sub>теор</sub> => коэффициент не значим");
            return s;
        }
        public string GetCorrReport()
        {
            int d = 3;
            List<string> l = new List<string>();
            l.Add(sY.name);
            foreach (Sample s in arrSX)
                l.Add(s.name);
            return string.Format("Корреляционная таблица<br>{0}Матрица частных коэффициентов корреляции{1}",
                MatrixToHtml(matrR, d, l.ToArray()),
                 MatrixToHtml(matrRPart, d, l.ToArray()));
        }
        public string CheckCorr(double alpha)
        {
            double tTheor = StatTables.GetStudDistrInv(n - 2, alpha);
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
        public double GetRyx1()
        {
            return matrR[0, 1];
        }
        double GetMinor(Matrix m, int rowIndex, int colIndex)
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
    }
    public class StatTables
    {
        static double epsilon = 0.0000001;
        static StudentsTDistribution distStud = new StudentsTDistribution();
        static FisherSnedecorDistribution distFish = new FisherSnedecorDistribution();
        static public double GetStudDistrInv(int n, double alpha)
        {
            distStud.DegreesOfFreedom = n;
            return GetX(1 - alpha, distStud);
        }
        static public double GetFishDistrInv(int n1, int n2, double alpha)
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

    public class HJInitialParams : IInitialParams
    {
        public double xEpsilon, fEpsilon;
        public double[] arrMu, arrSigma, arrY;
        double[][] arrCoeff;
        double[] arrUMin, arrUMax, arrUSMin, arrYOpt, arrYMin, arrYMax, arrAlpha, arrUMid;
        double r, mult;
        string[,] matrFunc;
        public HJInitialParams(double xEps, double fEps, double[][] arrCoeff, string[,] matrFunc,
            double[] arrUMin, double[] arrUMax, double[] arrUSMin, double[] arrYOpt,
            double[] arrYMin, double[] arrYMax, double[] arrAlpha, double r, double mult)
        {
            this.xEpsilon = xEps;
            this.fEpsilon = fEps;
            this.arrCoeff = arrCoeff;
            this.matrFunc = matrFunc;
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
        public double GetFuncValue(double[] arrX)
        {
            int p = arrUMin.Length, n = arrYOpt.Length;
            arrMu = new double[n];
            for (int j = 0; j < n; j++)
            {
                arrMu[j] = arrCoeff[j][0];
                for (int i = 0; i < p; i++)
                    arrMu[j] += arrCoeff[j][i + 1] * (F(arrUMid[i], matrFunc[j, i]) +
                        f(arrUMid[i], matrFunc[j, i]) * (arrX[i] - arrUMid[i]));
            }
            arrSigma = new double[n];
            for (int j = 0; j < n; j++)
            {
                for (int i = 0; i < p; i++)
                    arrSigma[j] += (arrCoeff[j][i + 1] * f(arrUMid[i], matrFunc[j, i]) * arrX[p + i]) *
                        (arrCoeff[j][i + 1] * f(arrUMid[i], matrFunc[j, i]) * arrX[p + i]);
                arrSigma[j] = Math.Sqrt(arrSigma[j]);
            }
            arrY = new double[n];
            for (int j = 0; j < n; j++)
            {
                arrY[j] = arrCoeff[j][0];
                for (int i = 0; i < p; i++)
                {
                    arrY[j] += arrCoeff[j][i + 1] * F(arrX[i], matrFunc[j, i]);
                }
            }
            double res = 0, z;
            for (int i = 0; i < p; i++)
            {
                if ((z = arrUSMin[i] - arrX[p + i]) > 0)
                    res += z;
                if ((z = arrX[i] + mult * arrX[p + i] - arrUMax[i]) > 0)
                    res += z;
                if ((z = arrUMin[i] - arrX[i] + mult * arrX[p + i]) > 0)
                    res += z;
            }
            for (int j = 0; j < n; j++)
            {
                if ((z = arrMu[j] + mult * arrSigma[j] - arrYMax[j]) > 0)
                    res += z;
                if ((z = arrYMin[j] - arrMu[j] + mult * arrSigma[j]) > 0)
                    res += z;
            }
            res *= r;            
            for (int j = 0; j < n; j++)
                res += arrAlpha[j] * (arrY[j] - arrYOpt[j]) * (arrY[j] - arrYOpt[j]);
            return res;
        }
        double F(double x, string func)
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
                    return 1 / x / x;
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
        double f(double x, string func)
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
                    return -1 / x / x;
                case "1/x^2":
                    return -2 / x / x / x;
                case "sqrt(x)":
                    return 1 / 2 / Math.Sqrt(x);
                case "1/sqrt(x)":
                    return -1 / 2 / x / Math.Sqrt(x);
                case "ln(x)":
                    return 1 / x;
                case "exp(x)":
                    return Math.Exp(x);
                default:
                    throw new Exception();
            }
        }
        public double GetDerivative(double[] arrX, int index)
        {
            double xOld = arrX[index];
            double f1 = GetFuncValue(arrX);
            double eps = 0.001;
            arrX[index] += eps;
            double f2 = GetFuncValue(arrX);
            arrX[index] = xOld;
            return (f2 - f1) / eps;
        }
        public string ToHtml(int d)
        {
            return string.Format("<P>ПАРАМЕТРЫ<BR>" +
                "Количество переменных: {0}<BR>" +
                "Точность X: {1}<BR>" +
                "Точность F: {2}<BR>",
                arrUMin.Length * 2, Math.Round(xEpsilon, d), Math.Round(fEpsilon, d));
        }
    }
    public class HJIteration : IIteration
    {
        public double[] arrX, arrE; // arrE - направление
        public double[] arrXDelta;
        public double mult;         // mult - множитель в поиске по обр.
        public double fRes;
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
        public void CalcResult(HJInitialParams ip)
        {
            fRes = ip.GetFuncValue(arrX);
        }
        public string ToHtml(IInitialParams initParams, int d)
        {
            string res = "";
            for (int i = 0; i < arrX.Length / 2; i++)
            {
                string s = "; ";
                if (i == arrX.Length / 2 - 1)
                    s = "";
                res += string.Format("{0:f4}±{1:f4}{2}",
                    arrX[i], arrX[i + arrX.Length / 2], s);
            }
            return string.Format("F = {0:f4}, ({1})", fRes, res);
            /*return string.Format("<P>" +
                "Текущее решение:<BR>{1}<BR>" +
                "Величины шагов:<BR>{2}<BR>" +
                "Направление поиска по образцу:<BR>{3}<BR>" +
                "Значение множителя в поиске по образцу:<BR>{4}</P>",
                "", Html.ArrayToHtml(arrX, d),
                Html.ArrayToHtml(arrXDelta, d), Html.ArrayToHtml(arrE, d),
                Math.Round(mult, d));*/
        }
        public override string ToString()
        {
            return "F = " + fRes.ToString();
        }
    }
    public class HJOptimizer : IOptimizer
    {
        HJInitialParams ip;
        HJIteration it;
        public void Initialize(IInitialParams initParams)
        {
            ip = (HJInitialParams)initParams;
        }
        public IIteration DoIteration(IIteration prevIter)
        {
            it = (HJIteration)prevIter;
            double f = ip.GetFuncValue(it.arrX);
            while (it.arrE == null)
            {
                f = ip.GetFuncValue(it.arrX);
                double[] arrENext, arrXNext;
                double fNext = Research(it.arrX, out arrENext, out arrXNext);
                if (Math.Abs(fNext - f) < ip.fEpsilon)
                {
                    for (int i = 0; i < it.arrXDelta.Length; i++)
                        it.arrXDelta[i] /= 2;
                    for (int i = 0; i < it.arrXDelta.Length; i++)
                        if (it.arrXDelta[i] < ip.xEpsilon)
                        {
                            it.CalcResult(ip);
                            return null;
                        }
                }
                else
                    it.arrE = arrENext;
            }
            double[] arrXSmp = new double[it.arrX.Length];
            for (int i = 0; i < arrXSmp.Length; i++)
                arrXSmp[i] = it.arrX[i] + it.arrE[i] * it.mult;
            double fSmp = ip.GetFuncValue(arrXSmp);
            if (Math.Abs(fSmp - f) < ip.fEpsilon || fSmp > f)
            {
                it.fRes = f;
                return new HJIteration(it.arrX, it.arrXDelta, null, 1);
            }
            double[] arrXRes, arrERes;
            double fRes = Research(arrXSmp, out arrERes, out arrXRes);
            HJIteration itNext;
            if (Math.Abs(fRes - f) < ip.fEpsilon)
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
        double Research(double[] arrX, out double[] arrE, out double[] arrXNext)
        {
            arrE = new double[it.arrX.Length];
            arrXNext = (double[])arrX.Clone();
            for (int i = 0; i < arrE.Length; i++)
            {
                double f = ip.GetFuncValue(arrXNext);
                double xOld = arrXNext[i];
                arrXNext[i] -= it.arrXDelta[i];
                double f1 = ip.GetFuncValue(arrXNext);
                arrXNext[i] = xOld + it.arrXDelta[i];
                double f2 = ip.GetFuncValue(arrXNext);
                arrXNext[i] = xOld;
                if (f1 < f && f1 <= f2)
                    arrE[i] = -it.arrXDelta[i];
                else if (f2 < f && f2 < f1)
                    arrE[i] = it.arrXDelta[i];
                else
                    arrE[i] = 0;
                arrXNext[i] += arrE[i];
            }
            return ip.GetFuncValue(arrXNext);
        }
        /*double PowellOptimization(double lambda1, double lambdaDelta,
            double fEpsilon, double lambdaEpsilon, int maxIter)
        {
            int iterCount = 0;        
            step2: double lambda2 = lambda1 + lambdaDelta;
            double f1 = GetFuncValue(lambda1), f2 = GetFuncValue(lambda2), lambda3;
            if (f1 > f2)
                lambda3 = lambda1 + 2 * lambdaDelta;
            else
                lambda3 = lambda1 - lambdaDelta;

            List<XY> listLF = new List<XY>();
            listLF.Add(new XY(lambda1, f1));
            listLF.Add(new XY(lambda2, f2));
            listLF.Add(new XY(lambda3, GetFuncValue(lambda3)));

        step6: listLF.Sort(new XYComparerByY());
            XY xyMin = listLF[0];
            listLF.Sort();

            double denom = 2 * ((listLF[1].x - listLF[2].x) * listLF[0].y +
                (listLF[2].x - listLF[0].x) * listLF[1].y +
                (listLF[0].x - listLF[1].x) * listLF[2].y);
            if (denom == 0)
            {
                lambda1 = xyMin.x;
                if (iterCount++ > maxIter)
                    throw new Exception();
                goto step2;
            }
            double numer = (listLF[1].x * listLF[1].x - listLF[2].x * listLF[2].x) * listLF[0].y +
                (listLF[2].x * listLF[2].x - listLF[0].x * listLF[0].x) * listLF[1].y +
                (listLF[0].x * listLF[0].x - listLF[1].x * listLF[1].x) * listLF[2].y;
            XY xyOpt = new XY(numer / denom, GetFuncValue(numer / denom));
            listLF.Add(xyOpt);
            listLF.Sort();
            if (Math.Abs(xyMin.y - xyOpt.y) < fEpsilon &&
                Math.Abs(xyMin.x - xyOpt.x) < lambdaEpsilon)
                return xyOpt.x;

            XY xyNext = xyOpt;
            if (xyMin.y < xyOpt.y)
                xyNext = xyMin;

            int index = listLF.BinarySearch(xyNext);
            if (index > 0 && index < 2)
            {
                if (index == 2)
                    listLF.RemoveAt(0);
                else
                    listLF.RemoveAt(3);
                if (iterCount++ > maxIter)
                    throw new Exception();
                goto step6;
            }

            lambda1 = xyNext.x;
            if (iterCount++ > maxIter)
                throw new Exception();
            goto step2;
        }
        double GetFuncValue(double lambda)
        {
            return ip.GetFuncValue(GetXNext(lambda));
        }
        double[] GetXNext(double lambda)
        {
            double[] arrXNext = new double[it.arrX.Length];
            for (int i = 0; i < arrXNext.Length; i++)
                arrXNext[i] = it.arrX[i] +
                    it.arrE[i] * it.mult * it.arrXDelta[i] * lambda;
            return arrXNext;
        }*/
    }
    public struct XY : IComparable<XY>
    {
        public double x, y;
        public XY(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
        public int CompareTo(XY other)
        {
            return x.CompareTo(other.x);
        }
    }
    public class XYComparerByY : IComparer<XY>
    {
        public int Compare(XY x, XY y)
        {
            return x.y.CompareTo(y.y);
        }
    }
    public static class Html
    {
        public static string ArrayToHtml(double[] arr, int d)
        {
            string s = "<TABLE BORDER = 3>";
            if (arr == null)
                s += "<TR><TD>Не определено</TR></TD>";
            else
                for (int i = 0; i < arr.Length; i++)
                    s += string.Format("<TR><TD>{0}</TD></TR>",
                        Math.Round(arr[i], d));
            return s + "</TABLE>";
        }
        public static string MatrixToHtml(double[,] matr, int d)
        {
            string s = "<TABLE BORDER = 3>";
            for (int i = 0; i < matr.GetLength(0); i++)
            {
                s += "<TR>";
                for (int j = 0; j < matr.GetLength(1); j++)
                    s += string.Format("<TD>{0}</TD>",
                        Math.Round(matr[i, j], d));
                s += "</TR>";
            }
            return s + "</TABLE>";
        }
        public static string MatrixToHtml(double[][] matr, int d)
        {
            string s = "<TABLE BORDER = 3>";
            for (int i = 0; i < matr.Length; i++)
            {
                s += "<TR>";
                for (int j = 0; j < matr[i].Length; j++)
                    s += string.Format("<TD>{0}</TD>",
                        Math.Round(matr[i][j], d));
                s += "</TR>";
            }
            return s + "</TABLE>";
        }
        public static string MatrixTranToHtml(double[][] matr, int d)
        {
            string s = "<TABLE BORDER = 3>";
            for (int i = 0; i < matr[0].Length; i++)
            {
                s += "<TR>";
                for (int j = 0; j < matr.Length; j++)
                    s += string.Format("<TD>{0}</TD>",
                        Math.Round(matr[j][i], d));
                s += "</TR>";
            }
            return s + "</TABLE>";
        }
    }
    public interface IOptimizer
    {
        void Initialize(IInitialParams initParams);
        IIteration DoIteration(IIteration prevIter);
    }
    public interface IInitialParams
    {
        double GetFuncValue(double[] arrX);
        double GetDerivative(double[] arrX, int index);
        string ToHtml(int d);
    }
    public interface IIteration
    {
        string ToHtml(IInitialParams initParams, int d);
    }
}
