namespace StochReg
{
    partial class StageForm
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
            this.lbData = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lbX = new System.Windows.Forms.ListBox();
            this.label3 = new System.Windows.Forms.Label();
            this.lbU = new System.Windows.Forms.ListBox();
            this.label4 = new System.Windows.Forms.Label();
            this.lbY = new System.Windows.Forms.ListBox();
            this.btnX = new System.Windows.Forms.Button();
            this.btnU = new System.Windows.Forms.Button();
            this.btnY = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.tbName = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // lbData
            // 
            this.lbData.FormattingEnabled = true;
            this.lbData.Location = new System.Drawing.Point(13, 72);
            this.lbData.Name = "lbData";
            this.lbData.Size = new System.Drawing.Size(489, 160);
            this.lbData.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 56);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Данные";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 271);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(39, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Входы";
            // 
            // lbX
            // 
            this.lbX.FormattingEnabled = true;
            this.lbX.Location = new System.Drawing.Point(13, 287);
            this.lbX.Name = "lbX";
            this.lbX.Size = new System.Drawing.Size(159, 160);
            this.lbX.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(178, 271);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(69, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Управления";
            // 
            // lbU
            // 
            this.lbU.FormattingEnabled = true;
            this.lbU.Location = new System.Drawing.Point(178, 287);
            this.lbU.Name = "lbU";
            this.lbU.Size = new System.Drawing.Size(159, 160);
            this.lbU.TabIndex = 4;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(343, 271);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(47, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Выходы";
            // 
            // lbY
            // 
            this.lbY.FormattingEnabled = true;
            this.lbY.Location = new System.Drawing.Point(343, 287);
            this.lbY.Name = "lbY";
            this.lbY.Size = new System.Drawing.Size(159, 160);
            this.lbY.TabIndex = 6;
            // 
            // btnX
            // 
            this.btnX.Location = new System.Drawing.Point(58, 238);
            this.btnX.Name = "btnX";
            this.btnX.Size = new System.Drawing.Size(75, 23);
            this.btnX.TabIndex = 8;
            this.btnX.Text = "Добавить";
            this.btnX.UseVisualStyleBackColor = true;
            this.btnX.Click += new System.EventHandler(this.btnX_Click);
            // 
            // btnU
            // 
            this.btnU.Location = new System.Drawing.Point(216, 238);
            this.btnU.Name = "btnU";
            this.btnU.Size = new System.Drawing.Size(75, 23);
            this.btnU.TabIndex = 9;
            this.btnU.Text = "Добавить";
            this.btnU.UseVisualStyleBackColor = true;
            this.btnU.Click += new System.EventHandler(this.btnU_Click);
            // 
            // btnY
            // 
            this.btnY.Location = new System.Drawing.Point(381, 238);
            this.btnY.Name = "btnY";
            this.btnY.Size = new System.Drawing.Size(75, 23);
            this.btnY.TabIndex = 10;
            this.btnY.Text = "Добавить";
            this.btnY.UseVisualStyleBackColor = true;
            this.btnY.Click += new System.EventHandler(this.btnY_Click);
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(347, 453);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 11;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(428, 453);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 12;
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(13, 453);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(75, 23);
            this.btnClear.TabIndex = 13;
            this.btnClear.Text = "Очистить";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(10, 9);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(57, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "Название";
            // 
            // tbName
            // 
            this.tbName.Location = new System.Drawing.Point(12, 25);
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(490, 20);
            this.tbName.TabIndex = 0;
            // 
            // StageForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(514, 488);
            this.Controls.Add(this.tbName);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.btnY);
            this.Controls.Add(this.btnU);
            this.Controls.Add(this.btnX);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.lbY);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lbU);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lbX);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lbData);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "StageForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Добавление передела";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lbData;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnX;
        private System.Windows.Forms.Button btnU;
        private System.Windows.Forms.Button btnY;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnClear;
        public System.Windows.Forms.ListBox lbX;
        public System.Windows.Forms.ListBox lbU;
        public System.Windows.Forms.ListBox lbY;
        private System.Windows.Forms.Label label5;
        public System.Windows.Forms.TextBox tbName;
    }
}

