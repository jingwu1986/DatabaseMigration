namespace DatabaseMigration
{
    partial class frmDbConnect
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
            this.lblServerName = new System.Windows.Forms.Label();
            this.txtServerName = new System.Windows.Forms.TextBox();
            this.lblAuthentication = new System.Windows.Forms.Label();
            this.cboAuthentication = new System.Windows.Forms.ComboBox();
            this.lblPassword = new System.Windows.Forms.Label();
            this.lblUserId = new System.Windows.Forms.Label();
            this.txtUserId = new System.Windows.Forms.TextBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.btnTest = new System.Windows.Forms.Button();
            this.btnConfirm = new System.Windows.Forms.Button();
            this.chkRememberPassword = new System.Windows.Forms.CheckBox();
            this.lblDatabase = new System.Windows.Forms.Label();
            this.cboDatabase = new System.Windows.Forms.ComboBox();
            this.lblPort = new System.Windows.Forms.Label();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtProfileName = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // lblServerName
            // 
            this.lblServerName.AutoSize = true;
            this.lblServerName.Location = new System.Drawing.Point(19, 57);
            this.lblServerName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblServerName.Name = "lblServerName";
            this.lblServerName.Size = new System.Drawing.Size(84, 17);
            this.lblServerName.TabIndex = 0;
            this.lblServerName.Text = "Server name:";
            // 
            // txtServerName
            // 
            this.txtServerName.Location = new System.Drawing.Point(142, 53);
            this.txtServerName.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtServerName.Name = "txtServerName";
            this.txtServerName.Size = new System.Drawing.Size(193, 23);
            this.txtServerName.TabIndex = 1;
            // 
            // lblAuthentication
            // 
            this.lblAuthentication.AutoSize = true;
            this.lblAuthentication.Location = new System.Drawing.Point(19, 99);
            this.lblAuthentication.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblAuthentication.Name = "lblAuthentication";
            this.lblAuthentication.Size = new System.Drawing.Size(93, 17);
            this.lblAuthentication.TabIndex = 2;
            this.lblAuthentication.Text = "Authentication:";
            // 
            // cboAuthentication
            // 
            this.cboAuthentication.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboAuthentication.FormattingEnabled = true;
            this.cboAuthentication.Items.AddRange(new object[] {
            "Windows",
            "Password"});
            this.cboAuthentication.Location = new System.Drawing.Point(142, 95);
            this.cboAuthentication.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cboAuthentication.Name = "cboAuthentication";
            this.cboAuthentication.Size = new System.Drawing.Size(193, 25);
            this.cboAuthentication.TabIndex = 3;
            this.cboAuthentication.SelectedIndexChanged += new System.EventHandler(this.cboAuthentication_SelectedIndexChanged);
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new System.Drawing.Point(19, 182);
            this.lblPassword.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(67, 17);
            this.lblPassword.TabIndex = 4;
            this.lblPassword.Text = "Password:";
            // 
            // lblUserId
            // 
            this.lblUserId.AutoSize = true;
            this.lblUserId.Location = new System.Drawing.Point(19, 139);
            this.lblUserId.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblUserId.Name = "lblUserId";
            this.lblUserId.Size = new System.Drawing.Size(74, 17);
            this.lblUserId.TabIndex = 5;
            this.lblUserId.Text = "User name:";
            // 
            // txtUserId
            // 
            this.txtUserId.Location = new System.Drawing.Point(142, 135);
            this.txtUserId.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtUserId.Name = "txtUserId";
            this.txtUserId.Size = new System.Drawing.Size(193, 23);
            this.txtUserId.TabIndex = 4;
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(142, 177);
            this.txtPassword.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new System.Drawing.Size(193, 23);
            this.txtPassword.TabIndex = 5;
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(143, 299);
            this.btnTest.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(88, 33);
            this.btnTest.TabIndex = 8;
            this.btnTest.Text = "Test";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // btnConfirm
            // 
            this.btnConfirm.Location = new System.Drawing.Point(256, 299);
            this.btnConfirm.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(88, 33);
            this.btnConfirm.TabIndex = 9;
            this.btnConfirm.Text = "OK";
            this.btnConfirm.UseVisualStyleBackColor = true;
            this.btnConfirm.Click += new System.EventHandler(this.btnConfirm_Click);
            // 
            // chkRememberPassword
            // 
            this.chkRememberPassword.AutoSize = true;
            this.chkRememberPassword.Location = new System.Drawing.Point(142, 219);
            this.chkRememberPassword.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chkRememberPassword.Name = "chkRememberPassword";
            this.chkRememberPassword.Size = new System.Drawing.Size(152, 21);
            this.chkRememberPassword.TabIndex = 6;
            this.chkRememberPassword.Text = "Remember password";
            this.chkRememberPassword.UseVisualStyleBackColor = true;
            // 
            // lblDatabase
            // 
            this.lblDatabase.AutoSize = true;
            this.lblDatabase.Location = new System.Drawing.Point(19, 257);
            this.lblDatabase.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDatabase.Name = "lblDatabase";
            this.lblDatabase.Size = new System.Drawing.Size(66, 17);
            this.lblDatabase.TabIndex = 11;
            this.lblDatabase.Text = "Database:";
            // 
            // cboDatabase
            // 
            this.cboDatabase.FormattingEnabled = true;
            this.cboDatabase.Location = new System.Drawing.Point(142, 253);
            this.cboDatabase.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cboDatabase.Name = "cboDatabase";
            this.cboDatabase.Size = new System.Drawing.Size(193, 25);
            this.cboDatabase.TabIndex = 7;
            // 
            // lblPort
            // 
            this.lblPort.AutoSize = true;
            this.lblPort.Location = new System.Drawing.Point(344, 57);
            this.lblPort.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(35, 17);
            this.lblPort.TabIndex = 13;
            this.lblPort.Text = "Port:";
            this.lblPort.Visible = false;
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(386, 53);
            this.txtPort.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(56, 23);
            this.txtPort.TabIndex = 2;
            this.txtPort.Visible = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(19, 18);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(84, 17);
            this.label1.TabIndex = 16;
            this.label1.Text = "Profile name:";
            // 
            // txtProfileName
            // 
            this.txtProfileName.Location = new System.Drawing.Point(142, 14);
            this.txtProfileName.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtProfileName.Name = "txtProfileName";
            this.txtProfileName.Size = new System.Drawing.Size(193, 23);
            this.txtProfileName.TabIndex = 0;
            // 
            // frmDbConnect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(455, 345);
            this.Controls.Add(this.btnTest);
            this.Controls.Add(this.txtProfileName);
            this.Controls.Add(this.btnConfirm);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtPort);
            this.Controls.Add(this.lblPort);
            this.Controls.Add(this.cboDatabase);
            this.Controls.Add(this.lblDatabase);
            this.Controls.Add(this.chkRememberPassword);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.txtUserId);
            this.Controls.Add(this.lblUserId);
            this.Controls.Add(this.lblPassword);
            this.Controls.Add(this.cboAuthentication);
            this.Controls.Add(this.lblAuthentication);
            this.Controls.Add(this.txtServerName);
            this.Controls.Add(this.lblServerName);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmDbConnect";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Database connection";
            this.Activated += new System.EventHandler(this.frmDbConnect_Activated);
            this.Load += new System.EventHandler(this.frmDbConnect_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblServerName;
        private System.Windows.Forms.TextBox txtServerName;
        private System.Windows.Forms.Label lblAuthentication;
        private System.Windows.Forms.ComboBox cboAuthentication;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.Label lblUserId;
        private System.Windows.Forms.TextBox txtUserId;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Button btnTest;
        private System.Windows.Forms.Button btnConfirm;
        private System.Windows.Forms.CheckBox chkRememberPassword;
        private System.Windows.Forms.Label lblDatabase;
        private System.Windows.Forms.ComboBox cboDatabase;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtProfileName;
    }
}