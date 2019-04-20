﻿namespace DatabaseMigration
{
    partial class frmSetting
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
            this.lblCommandTimeout = new System.Windows.Forms.Label();
            this.numCommandTimeout = new System.Windows.Forms.NumericUpDown();
            this.btnConfirm = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.numDataBatchSize = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabCommon = new System.Windows.Forms.TabPage();
            this.tabMySql = new System.Windows.Forms.TabPage();
            this.lblMySqlCharset = new System.Windows.Forms.Label();
            this.txtMySqlCharset = new System.Windows.Forms.TextBox();
            this.txtMySqlCharsetCollation = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numCommandTimeout)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDataBatchSize)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabCommon.SuspendLayout();
            this.tabMySql.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCommandTimeout
            // 
            this.lblCommandTimeout.AutoSize = true;
            this.lblCommandTimeout.Location = new System.Drawing.Point(6, 12);
            this.lblCommandTimeout.Name = "lblCommandTimeout";
            this.lblCommandTimeout.Size = new System.Drawing.Size(101, 12);
            this.lblCommandTimeout.TabIndex = 0;
            this.lblCommandTimeout.Text = "Command timeout:";
            // 
            // numCommandTimeout
            // 
            this.numCommandTimeout.Location = new System.Drawing.Point(113, 10);
            this.numCommandTimeout.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numCommandTimeout.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numCommandTimeout.Name = "numCommandTimeout";
            this.numCommandTimeout.Size = new System.Drawing.Size(91, 21);
            this.numCommandTimeout.TabIndex = 1;
            this.numCommandTimeout.Value = new decimal(new int[] {
            600,
            0,
            0,
            0});
            // 
            // btnConfirm
            // 
            this.btnConfirm.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnConfirm.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnConfirm.Location = new System.Drawing.Point(148, 262);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(75, 23);
            this.btnConfirm.TabIndex = 10;
            this.btnConfirm.Text = "OK";
            this.btnConfirm.UseVisualStyleBackColor = true;
            this.btnConfirm.Click += new System.EventHandler(this.btnConfirm_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnCancel.Location = new System.Drawing.Point(252, 262);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 11;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(210, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 12;
            this.label1.Text = "(second)";
            // 
            // numDataBatchSize
            // 
            this.numDataBatchSize.Location = new System.Drawing.Point(113, 39);
            this.numDataBatchSize.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numDataBatchSize.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numDataBatchSize.Name = "numDataBatchSize";
            this.numDataBatchSize.Size = new System.Drawing.Size(91, 21);
            this.numDataBatchSize.TabIndex = 14;
            this.numDataBatchSize.Value = new decimal(new int[] {
            500,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(101, 12);
            this.label2.TabIndex = 13;
            this.label2.Text = "Data batch size:";
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabCommon);
            this.tabControl1.Controls.Add(this.tabMySql);
            this.tabControl1.Location = new System.Drawing.Point(1, 2);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(429, 254);
            this.tabControl1.TabIndex = 15;
            // 
            // tabCommon
            // 
            this.tabCommon.BackColor = System.Drawing.SystemColors.Control;
            this.tabCommon.Controls.Add(this.label2);
            this.tabCommon.Controls.Add(this.numDataBatchSize);
            this.tabCommon.Controls.Add(this.lblCommandTimeout);
            this.tabCommon.Controls.Add(this.numCommandTimeout);
            this.tabCommon.Controls.Add(this.label1);
            this.tabCommon.Location = new System.Drawing.Point(4, 22);
            this.tabCommon.Name = "tabCommon";
            this.tabCommon.Padding = new System.Windows.Forms.Padding(3);
            this.tabCommon.Size = new System.Drawing.Size(421, 228);
            this.tabCommon.TabIndex = 0;
            this.tabCommon.Text = "Common";
            // 
            // tabMySql
            // 
            this.tabMySql.BackColor = System.Drawing.SystemColors.Control;
            this.tabMySql.Controls.Add(this.txtMySqlCharsetCollation);
            this.tabMySql.Controls.Add(this.label3);
            this.tabMySql.Controls.Add(this.txtMySqlCharset);
            this.tabMySql.Controls.Add(this.lblMySqlCharset);
            this.tabMySql.Location = new System.Drawing.Point(4, 22);
            this.tabMySql.Name = "tabMySql";
            this.tabMySql.Padding = new System.Windows.Forms.Padding(3);
            this.tabMySql.Size = new System.Drawing.Size(421, 228);
            this.tabMySql.TabIndex = 1;
            this.tabMySql.Text = "MySql";
            // 
            // lblMySqlCharset
            // 
            this.lblMySqlCharset.AutoSize = true;
            this.lblMySqlCharset.Location = new System.Drawing.Point(22, 19);
            this.lblMySqlCharset.Name = "lblMySqlCharset";
            this.lblMySqlCharset.Size = new System.Drawing.Size(53, 12);
            this.lblMySqlCharset.TabIndex = 0;
            this.lblMySqlCharset.Text = "Charset:";
            // 
            // txtMySqlCharset
            // 
            this.txtMySqlCharset.Location = new System.Drawing.Point(141, 19);
            this.txtMySqlCharset.Name = "txtMySqlCharset";
            this.txtMySqlCharset.Size = new System.Drawing.Size(100, 21);
            this.txtMySqlCharset.TabIndex = 1;
            // 
            // txtMySqlCharsetCollation
            // 
            this.txtMySqlCharsetCollation.Location = new System.Drawing.Point(141, 50);
            this.txtMySqlCharsetCollation.Name = "txtMySqlCharsetCollation";
            this.txtMySqlCharsetCollation.Size = new System.Drawing.Size(100, 21);
            this.txtMySqlCharsetCollation.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(22, 53);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(113, 12);
            this.label3.TabIndex = 2;
            this.label3.Text = "Charset Collation:";
            // 
            // frmSetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(442, 297);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnConfirm);
            this.Name = "frmSetting";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.frmSetting_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numCommandTimeout)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDataBatchSize)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabCommon.ResumeLayout(false);
            this.tabCommon.PerformLayout();
            this.tabMySql.ResumeLayout(false);
            this.tabMySql.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblCommandTimeout;
        private System.Windows.Forms.NumericUpDown numCommandTimeout;
        private System.Windows.Forms.Button btnConfirm;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numDataBatchSize;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabCommon;
        private System.Windows.Forms.TabPage tabMySql;
        private System.Windows.Forms.TextBox txtMySqlCharset;
        private System.Windows.Forms.Label lblMySqlCharset;
        private System.Windows.Forms.TextBox txtMySqlCharsetCollation;
        private System.Windows.Forms.Label label3;
    }
}