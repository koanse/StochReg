using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.OleDb;

namespace StochReg
{
    public partial class TechForm : Form
    {
        public List<Stage> lStage = new List<Stage>();
        public TechForm()
        {
            InitializeComponent();
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

        private void btnAdd_Click(object sender, EventArgs e)
        {
            //try
            {
                if (openFileDialog1.ShowDialog() != DialogResult.OK)
                    return;
                string s = "provider = Microsoft.Jet.OLEDB.4.0;" +
                        "data source = " + openFileDialog1.FileName + ";" +
                        "extended properties = Excel 8.0;";
                OleDbConnection conn = new OleDbConnection(s);
                conn.Open();
                DataTable t = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                string[] arr = new string[t.Rows.Count];
                for (int i = 0; i < t.Rows.Count; i++)
                    arr[i] = t.Rows[i]["TABLE_NAME"].ToString();
                SheetForm sf = new SheetForm(arr);
                if (sf.ShowDialog() != DialogResult.OK)
                    return;
                s = string.Format("SELECT * FROM [{0}]", sf.lb.SelectedItem.ToString());
                OleDbDataAdapter da = new OleDbDataAdapter(s, conn);
                t = new DataTable();
                da.Fill(t);
                conn.Close();
                arr = new string[t.Columns.Count];
                for (int i = 0; i < t.Columns.Count; i++)
                    arr[i] = t.Columns[i].Caption;
                StageForm stf = new StageForm(arr);
                if (stf.ShowDialog() != DialogResult.OK)
                    return;

                List<string> lVar = new List<string>();
                for (int i = 0; i < stf.lbX.Items.Count; i++)
                    lVar.Add(stf.lbX.Items[i].ToString());
                for (int i = 0; i < stf.lbU.Items.Count; i++)
                    lVar.Add(stf.lbU.Items[i].ToString());
                for (int i = 0; i < stf.lbY.Items.Count; i++)
                    lVar.Add(stf.lbY.Items[i].ToString());
                List<int> lIndex = new List<int>();
                double tmp;
                for (int i = 0; i < t.Rows.Count; i++)
                {
                    int j;
                    for (j = 0; j < lVar.Count; j++)
                        if (!double.TryParse(t.Rows[i][lVar[j]].ToString(), out tmp))
                            break;
                    if (j == lVar.Count)
                        lIndex.Add(i);
                }
                Variable[] arrX = new Variable[stf.lbX.Items.Count];
                for (int i = 0; i < arrX.Length; i++)
                {
                    string name = stf.lbX.Items[i].ToString();
                    double[] arrValue = new double[lIndex.Count];
                    for (int j = 0; j < arrValue.Length; j++)
                        arrValue[j] = double.Parse(t.Rows[lIndex[j]][name].ToString());
                    arrX[i] = new Variable()
                    {
                        name = name,
                        id = string.Format("x{0}", i + 1),
                        arr = arrValue
                    };
                    arrX[i].Norm();
                }
                Variable[] arrU = new Variable[stf.lbU.Items.Count];
                for (int i = 0; i < arrU.Length; i++)
                {
                    string name = stf.lbU.Items[i].ToString();
                    double[] arrValue = new double[lIndex.Count];
                    for (int j = 0; j < arrValue.Length; j++)
                        arrValue[j] = double.Parse(t.Rows[lIndex[j]][name].ToString());
                    arrU[i] = new Variable()
                    {
                        name = name,
                        id = string.Format("u{0}", i + 1),
                        arr = arrValue
                    };
                    arrU[i].Norm();
                }
                Variable[] arrY = new Variable[stf.lbY.Items.Count];
                for (int i = 0; i < arrY.Length; i++)
                {
                    string name = stf.lbY.Items[i].ToString();
                    double[] arrValue = new double[lIndex.Count];
                    for (int j = 0; j < arrValue.Length; j++)
                        arrValue[j] = double.Parse(t.Rows[lIndex[j]][name].ToString());
                    arrY[i] = new Variable()
                    {
                        name = name,
                        id = string.Format("y{0}", i + 1),
                        arr = arrValue
                    };
                    arrY[i].Norm();
                }

                lStage.Add(new Stage()
                {
                    name = stf.tbName.Text,
                    arrX = arrX,
                    arrU = arrU,
                    arrY = arrY
                });
                lbStage.Items.Add(stf.tbName.Text);
            }
            //catch
            {
                //MessageBox.Show("Ошибка создания передела");
            }
        }

        private void btnDel_Click(object sender, EventArgs e)
        {
            try
            {
                int i = lbStage.SelectedIndex;
                lbStage.Items.RemoveAt(i);
                lStage.RemoveAt(i);
            }
            catch { }
        }
    }
}
