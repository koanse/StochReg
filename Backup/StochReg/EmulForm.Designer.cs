namespace StochReg
{
    partial class EmulForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.cbTEmul = new System.Windows.Forms.ComboBox();
            this.cbTReg = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tbP = new System.Windows.Forms.TextBox();
            this.tbMult = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tbDU = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tbDF = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tbUInit = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.tbDUInit = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.tbSInit = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.tbDSInit = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.tbIter = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.tbR = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.tbC = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.dgvU = new System.Windows.Forms.DataGridView();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvY = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvU)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvY)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(142, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Технология для эмуляции:";
            // 
            // cbTEmul
            // 
            this.cbTEmul.FormattingEnabled = true;
            this.cbTEmul.Location = new System.Drawing.Point(198, 6);
            this.cbTEmul.Name = "cbTEmul";
            this.cbTEmul.Size = new System.Drawing.Size(271, 21);
            this.cbTEmul.TabIndex = 1;
            this.cbTEmul.SelectedIndexChanged += new System.EventHandler(this.cbTEmul_SelectedIndexChanged);
            // 
            // cbTReg
            // 
            this.cbTReg.FormattingEnabled = true;
            this.cbTReg.Location = new System.Drawing.Point(198, 33);
            this.cbTReg.Name = "cbTReg";
            this.cbTReg.Size = new System.Drawing.Size(271, 21);
            this.cbTReg.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 36);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(172, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Технология для идентификации:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 63);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(381, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Вероятность выбора оптимального значения технологического фактора:";
            // 
            // tbP
            // 
            this.tbP.Location = new System.Drawing.Point(15, 79);
            this.tbP.Name = "tbP";
            this.tbP.Size = new System.Drawing.Size(454, 20);
            this.tbP.TabIndex = 5;
            // 
            // tbMult
            // 
            this.tbMult.Location = new System.Drawing.Point(15, 118);
            this.tbMult.Name = "tbMult";
            this.tbMult.Size = new System.Drawing.Size(454, 20);
            this.tbMult.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 102);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(350, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Множитель, определяющий вероятность выполнения ограничений:";
            // 
            // tbDU
            // 
            this.tbDU.Location = new System.Drawing.Point(15, 157);
            this.tbDU.Name = "tbDU";
            this.tbDU.Size = new System.Drawing.Size(454, 20);
            this.tbDU.TabIndex = 9;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 141);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(217, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "Точность по технологическим факторам:";
            // 
            // tbDF
            // 
            this.tbDF.Location = new System.Drawing.Point(15, 196);
            this.tbDF.Name = "tbDF";
            this.tbDF.Size = new System.Drawing.Size(454, 20);
            this.tbDF.TabIndex = 11;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 180);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(188, 13);
            this.label6.TabIndex = 10;
            this.label6.Text = "Точность критерия оптимальности:";
            // 
            // tbUInit
            // 
            this.tbUInit.Location = new System.Drawing.Point(15, 235);
            this.tbUInit.Name = "tbUInit";
            this.tbUInit.Size = new System.Drawing.Size(454, 20);
            this.tbUInit.TabIndex = 13;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 219);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(333, 13);
            this.label7.TabIndex = 12;
            this.label7.Text = "Начальное значение мат. ожиданий технологических факторов:";
            // 
            // tbDUInit
            // 
            this.tbDUInit.Location = new System.Drawing.Point(15, 274);
            this.tbDUInit.Name = "tbDUInit";
            this.tbDUInit.Size = new System.Drawing.Size(454, 20);
            this.tbDUInit.TabIndex = 15;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(12, 258);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(276, 13);
            this.label8.TabIndex = 14;
            this.label8.Text = "Шаг по мат. ожиданиям технологическим факторам:";
            // 
            // tbSInit
            // 
            this.tbSInit.Location = new System.Drawing.Point(15, 313);
            this.tbSInit.Name = "tbSInit";
            this.tbSInit.Size = new System.Drawing.Size(454, 20);
            this.tbSInit.TabIndex = 17;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(12, 297);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(338, 13);
            this.label9.TabIndex = 16;
            this.label9.Text = "Начальное значение среднекв. откл. технологических факторов:";
            // 
            // tbDSInit
            // 
            this.tbDSInit.Location = new System.Drawing.Point(15, 352);
            this.tbDSInit.Name = "tbDSInit";
            this.tbDSInit.Size = new System.Drawing.Size(454, 20);
            this.tbDSInit.TabIndex = 19;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(12, 336);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(268, 13);
            this.label10.TabIndex = 18;
            this.label10.Text = "Шаг по среднекв. откл. технологических факторов:";
            // 
            // tbIter
            // 
            this.tbIter.Location = new System.Drawing.Point(15, 391);
            this.tbIter.Name = "tbIter";
            this.tbIter.Size = new System.Drawing.Size(454, 20);
            this.tbIter.TabIndex = 21;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(12, 375);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(377, 13);
            this.label11.TabIndex = 20;
            this.label11.Text = "Макс. число итераций оптимизации для заданного множителя штрафов:";
            // 
            // tbR
            // 
            this.tbR.Location = new System.Drawing.Point(15, 430);
            this.tbR.Name = "tbR";
            this.tbR.Size = new System.Drawing.Size(454, 20);
            this.tbR.TabIndex = 25;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(12, 414);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(223, 13);
            this.label13.TabIndex = 24;
            this.label13.Text = "Начальное значение множителя штрафов:";
            // 
            // tbC
            // 
            this.tbC.Location = new System.Drawing.Point(15, 469);
            this.tbC.Name = "tbC";
            this.tbC.Size = new System.Drawing.Size(454, 20);
            this.tbC.TabIndex = 27;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(12, 453);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(230, 13);
            this.label14.TabIndex = 26;
            this.label14.Text = "Константа изменения множителя штрафов:";
            // 
            // dgvU
            // 
            this.dgvU.AllowUserToAddRows = false;
            this.dgvU.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvU.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvU.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1,
            this.Column2,
            this.Column3,
            this.Column4});
            this.dgvU.Location = new System.Drawing.Point(475, 12);
            this.dgvU.Name = "dgvU";
            this.dgvU.RowHeadersVisible = false;
            this.dgvU.Size = new System.Drawing.Size(416, 220);
            this.dgvU.TabIndex = 28;
            // 
            // Column1
            // 
            this.Column1.HeaderText = "Фактор";
            this.Column1.Name = "Column1";
            // 
            // Column2
            // 
            this.Column2.HeaderText = "Мин. знач.";
            this.Column2.Name = "Column2";
            // 
            // Column3
            // 
            this.Column3.HeaderText = "Макс. знач.";
            this.Column3.Name = "Column3";
            // 
            // Column4
            // 
            this.Column4.HeaderText = "Мин. СКО";
            this.Column4.Name = "Column4";
            // 
            // dgvY
            // 
            this.dgvY.AllowUserToAddRows = false;
            this.dgvY.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvY.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvY.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.dataGridViewTextBoxColumn2,
            this.dataGridViewTextBoxColumn3,
            this.dataGridViewTextBoxColumn4,
            this.Column5});
            this.dgvY.Location = new System.Drawing.Point(475, 238);
            this.dgvY.Name = "dgvY";
            this.dgvY.RowHeadersVisible = false;
            this.dgvY.Size = new System.Drawing.Size(416, 236);
            this.dgvY.TabIndex = 29;
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.HeaderText = "Показатель";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.HeaderText = "Мин. знач.";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.HeaderText = "Макс. знач.";
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            // 
            // dataGridViewTextBoxColumn4
            // 
            this.dataGridViewTextBoxColumn4.HeaderText = "Вес";
            this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            // 
            // Column5
            // 
            this.Column5.HeaderText = "Опт. знач.";
            this.Column5.Name = "Column5";
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(735, 480);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 30;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(816, 480);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 31;
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // EmulForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(903, 515);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.dgvY);
            this.Controls.Add(this.dgvU);
            this.Controls.Add(this.tbC);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.tbR);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.tbIter);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.tbDSInit);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.tbSInit);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.tbDUInit);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.tbUInit);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.tbDF);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.tbDU);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.tbMult);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tbP);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cbTReg);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cbTEmul);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EmulForm";
            this.ShowIcon = false;
            this.Text = "Эмуляция многоступенчатой технологии";
            ((System.ComponentModel.ISupportInitialize)(this.dgvU)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvY)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column3;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column4;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column5;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        public System.Windows.Forms.ComboBox cbTEmul;
        public System.Windows.Forms.ComboBox cbTReg;
        public System.Windows.Forms.TextBox tbP;
        public System.Windows.Forms.TextBox tbMult;
        public System.Windows.Forms.TextBox tbDU;
        public System.Windows.Forms.TextBox tbDF;
        public System.Windows.Forms.TextBox tbUInit;
        public System.Windows.Forms.TextBox tbDUInit;
        public System.Windows.Forms.TextBox tbSInit;
        public System.Windows.Forms.TextBox tbDSInit;
        public System.Windows.Forms.TextBox tbIter;
        public System.Windows.Forms.TextBox tbR;
        public System.Windows.Forms.TextBox tbC;
        public System.Windows.Forms.DataGridView dgvU;
        public System.Windows.Forms.DataGridView dgvY;
    }
}