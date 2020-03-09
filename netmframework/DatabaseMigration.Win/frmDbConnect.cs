using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Profile;
using DatabaseMigration.Core;
using DatabaseMigration.Profile;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseMigration.Win
{
    public partial class frmDbConnect : Form
    {
        public DatabaseType DatabaseType { get; set; }
        public string ProflieName { get; set; }

        public ConnectionInfo ConnectionInfo { get; set; }

        private bool requriePassword = false;
        private bool isAdd = true;

        public frmDbConnect(DatabaseType dbType)
        {
            this.isAdd = true;
            this.DatabaseType = dbType;
            this.Init();
        }

        public frmDbConnect(DatabaseType dbType, string profileName, bool requriePassword = false)
        {
            this.isAdd = false;
            this.requriePassword = requriePassword;
            this.DatabaseType = dbType;
            this.ProflieName = profileName;
            this.Init();
        }

        private void Init()
        {
            InitializeComponent();
            this.cboAuthentication.Text = AuthenticationType.Password.ToString();

            if (string.IsNullOrEmpty(this.ProflieName))
            {
                this.txtProfileName.Text = "";
            }
            else
            {
                this.txtProfileName.Text = this.ProflieName.ToString();
                this.LoadProfile();
            }
            this.SetControlStatus();
        }

        private void SetControlStatus()
        {
            if (this.DatabaseType == DatabaseType.MySql)
            {
                this.lblPort.Visible = this.txtPort.Visible = true;
                this.txtPort.Text = "3306";
            }

            if (this.DatabaseType != DatabaseType.SqlServer)
            {
                this.cboAuthentication.Enabled = false;
            }
        }

        private void LoadProfile()
        {
            ConnectionInfo connectionInfo = ConnectionInfoProfileManager.GetConnectionInfo(this.DatabaseType, this.ProflieName);

            this.txtServerName.Text = connectionInfo.Server;
            this.txtPort.Text = connectionInfo.Port;
            this.cboAuthentication.Text = connectionInfo.IntegratedSecurity ? AuthenticationType.Windows.ToString() : AuthenticationType.Password.ToString();
            this.txtUserId.Text = connectionInfo.UserId;
            this.txtPassword.Text = connectionInfo.Password;
            this.cboDatabase.Text = connectionInfo.Database;

            if (connectionInfo.IntegratedSecurity)
            {
                this.cboAuthentication.Text = AuthenticationType.Windows.ToString();
            }
            else
            {
                if (!string.IsNullOrEmpty(connectionInfo.Password))
                {
                    this.chkRememberPassword.Checked = true;
                }
            }
        }

        private async void btnTest_Click(object sender, EventArgs e)
        {
            ConnectionInfo connectionInfo = this.GetConnectionInfo();
            DbInterpreter dbInterpreter = DbInterpreterHelper.GetDbInterpreter(this.DatabaseType, connectionInfo, new DbInterpreterOption());

            try
            {
                using (DbConnection dbConnection = dbInterpreter.GetDbConnector().CreateConnection())
                {
                    dbConnection.Open();

                    MessageBox.Show("Success.");

                    if (string.IsNullOrEmpty(this.cboDatabase.Text.Trim()))
                    {
                        this.cboDatabase.Items.Clear();
                        List<Database> databaseses = await dbInterpreter.GetDatabasesAsync();
                        databaseses.ForEach(item =>
                        {
                            this.cboDatabase.Items.Add(item.Name);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed:" + ex.Message);
            }
        }

        private ConnectionInfo GetConnectionInfo()
        {
            return new ConnectionInfo()
            {
                Server = this.txtServerName.Text.Trim(),
                Port = this.txtPort.Text.Trim(),
                IntegratedSecurity = this.cboAuthentication.Text != AuthenticationType.Password.ToString(),
                UserId = this.txtUserId.Text.Trim(),
                Password = this.txtPassword.Text.Trim(),
                Database = this.cboDatabase.Text
            };
        }

        private void frmDbConnect_Load(object sender, EventArgs e)
        {
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            string profileName = this.txtProfileName.Text.Trim();
            this.ConnectionInfo = this.GetConnectionInfo();

            List<ConnectionInfoProfile> profiles = ConnectionInfoProfileManager.GetProfiles(this.DatabaseType);

            if (!string.IsNullOrEmpty(profileName) && profiles.Any(item => item.Name == profileName))
            {
                string msg = $"The profile name \"{profileName}\" has been existed";
                if (this.isAdd)
                {
                    DialogResult dialogResult = MessageBox.Show(msg + ", are you sure to override it.", "Confirm", MessageBoxButtons.YesNo);

                    if (dialogResult != DialogResult.Yes)
                    {
                        this.DialogResult = DialogResult.None;
                        return;
                    }
                }
                else if (!this.isAdd && this.ProflieName != profileName)
                {
                    MessageBox.Show(msg + ", please edit that.");
                    return;
                }
            }

            ConnectionInfoProfile profile = new ConnectionInfoProfile() { Name = profileName, DbType = this.DatabaseType, ConnectionInfo = this.ConnectionInfo, RememberPassword = this.chkRememberPassword.Checked };
            this.ProflieName = ConnectionInfoProfileManager.Save(profile);
            this.DialogResult = DialogResult.OK;
        }

        private void cboAuthentication_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool isWindowsAuth = this.cboAuthentication.Text == AuthenticationType.Windows.ToString();

            this.txtUserId.Enabled = !isWindowsAuth;
            this.txtPassword.Enabled = !isWindowsAuth;
            this.chkRememberPassword.Enabled = !isWindowsAuth;

            if (!isWindowsAuth)
            {
                this.txtUserId.Text = this.txtPassword.Text = "";
                this.chkRememberPassword.Checked = false;
            }
        }

        private void frmDbConnect_Activated(object sender, EventArgs e)
        {
            if (this.requriePassword)
            {
                this.txtPassword.Focus();
            }
        }
    }

    public enum AuthenticationType
    {
        Windows,
        Password
    }
}
