namespace DatabaseMigration
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
            ((System.ComponentModel.ISupportInitialize)(this.numCommandTimeout)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDataBatchSize)).BeginInit();
            this.SuspendLayout();
            // 
            // lblCommandTimeout
            // 
            this.lblCommandTimeout.AutoSize = true;
            this.lblCommandTimeout.Location = new System.Drawing.Point(12, 9);
            this.lblCommandTimeout.Name = "lblCommandTimeout";
            this.lblCommandTimeout.Size = new System.Drawing.Size(101, 12);
            this.lblCommandTimeout.TabIndex = 0;
            this.lblCommandTimeout.Text = "Command timeout:";
            // 
            // numCommandTimeout
            // 
            this.numCommandTimeout.Location = new System.Drawing.Point(119, 7);
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
            this.label1.Location = new System.Drawing.Point(216, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 12;
            this.label1.Text = "(second)";
            // 
            // numDataBatchSize
            // 
            this.numDataBatchSize.Location = new System.Drawing.Point(119, 36);
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
            this.label2.Location = new System.Drawing.Point(12, 38);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(101, 12);
            this.label2.TabIndex = 13;
            this.label2.Text = "Data batch size:";
            // 
            // frmSetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(442, 297);
            this.Controls.Add(this.numDataBatchSize);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnConfirm);
            this.Controls.Add(this.numCommandTimeout);
            this.Controls.Add(this.lblCommandTimeout);
            this.Name = "frmSetting";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.frmSetting_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numCommandTimeout)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDataBatchSize)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblCommandTimeout;
        private System.Windows.Forms.NumericUpDown numCommandTimeout;
        private System.Windows.Forms.Button btnConfirm;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numDataBatchSize;
        private System.Windows.Forms.Label label2;
    }
}