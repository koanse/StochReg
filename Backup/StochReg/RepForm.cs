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
    public partial class RepForm : Form
    {
        public RepForm(string rep, string text)
        {
            InitializeComponent();
            webBrowser1.DocumentText = rep;
            Text = text;
        }
    }
}
