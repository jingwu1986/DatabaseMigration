namespace DatabaseMigration
{
    partial class frmMain
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.settingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.convertorBackgroundWorker = new System.ComponentModel.BackgroundWorker();
            this.sourceScriptBackgroundWorker = new System.ComponentModel.BackgroundWorker();
            this.btnSaveMessage = new System.Windows.Forms.Button();
            this.btnCopyMessage = new System.Windows.Forms.Button();
            this.txtMessage = new System.Windows.Forms.RichTextBox();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.btnRemoveTarget = new System.Windows.Forms.Button();
            this.btnRemoveSource = new System.Windows.Forms.Button();
            this.btnConnect = new System.Windows.Forms.Button();
            this.btnConfigTarget = new System.Windows.Forms.Button();
            this.btnConfigSource = new System.Windows.Forms.Button();
            this.btnAddTarget = new System.Windows.Forms.Button();
            this.btnAddSource = new System.Windows.Forms.Button();
            this.cboTargetProfile = new System.Windows.Forms.ComboBox();
            this.cboSourceProfile = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cboTargetDB = new System.Windows.Forms.ComboBox();
            this.cboSourceDB = new System.Windows.Forms.ComboBox();
            this.btnSourceScript = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnExecute = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chkAsync = new System.Windows.Forms.CheckBox();
            this.chkBulkCopy = new System.Windows.Forms.CheckBox();
            this.chkGenerateIdentity = new System.Windows.Forms.CheckBox();
            this.chkPickup = new System.Windows.Forms.CheckBox();
            this.chkScriptData = new System.Windows.Forms.CheckBox();
            this.chkScriptSchema = new System.Windows.Forms.CheckBox();
            this.txtTargetDbOwner = new System.Windows.Forms.TextBox();
            this.lblTargetDbOwner = new System.Windows.Forms.Label();
            this.chkGenerateSourceScripts = new System.Windows.Forms.CheckBox();
            this.chkExecuteOnTarget = new System.Windows.Forms.CheckBox();
            this.lblOutputFolder = new System.Windows.Forms.Label();
            this.btnOutputFolder = new System.Windows.Forms.Button();
            this.txtOutputFolder = new System.Windows.Forms.TextBox();
            this.lblScriptsMode = new System.Windows.Forms.Label();
            this.chkOutputScripts = new System.Windows.Forms.CheckBox();
            this.tvSource = new System.Windows.Forms.TreeView();
            this.dlgOutputFolder = new System.Windows.Forms.FolderBrowserDialog();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(771, 25);
            this.menuStrip1.TabIndex = 4;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.BackColor = System.Drawing.SystemColors.MenuBar;
            this.toolStripMenuItem1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingToolStripMenuItem});
            this.toolStripMenuItem1.Image = global::DatabaseMigration.Properties.Resources.Config;
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(74, 21);
            this.toolStripMenuItem1.Text = "Config";
            // 
            // settingToolStripMenuItem
            // 
            this.settingToolStripMenuItem.Name = "settingToolStripMenuItem";
            this.settingToolStripMenuItem.Size = new System.Drawing.Size(122, 22);
            this.settingToolStripMenuItem.Text = "Settings";
            this.settingToolStripMenuItem.Click += new System.EventHandler(this.settingToolStripMenuItem_Click);
            // 
            // btnSaveMessage
            // 
            this.btnSaveMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveMessage.Image = global::DatabaseMigration.Properties.Resources.Save;
            this.btnSaveMessage.Location = new System.Drawing.Point(732, 42);
            this.btnSaveMessage.Name = "btnSaveMessage";
            this.btnSaveMessage.Size = new System.Drawing.Size(27, 23);
            this.btnSaveMessage.TabIndex = 9;
            this.btnSaveMessage.UseVisualStyleBackColor = true;
            this.btnSaveMessage.Click += new System.EventHandler(this.btnSaveMessage_Click);
            // 
            // btnCopyMessage
            // 
            this.btnCopyMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCopyMessage.Image = global::DatabaseMigration.Properties.Resources.Copy;
            this.btnCopyMessage.Location = new System.Drawing.Point(733, 13);
            this.btnCopyMessage.Name = "btnCopyMessage";
            this.btnCopyMessage.Size = new System.Drawing.Size(27, 23);
            this.btnCopyMessage.TabIndex = 8;
            this.btnCopyMessage.UseVisualStyleBackColor = true;
            this.btnCopyMessage.Click += new System.EventHandler(this.btnCopyMessage_Click);
            // 
            // txtMessage
            // 
            this.txtMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMessage.BackColor = System.Drawing.Color.White;
            this.txtMessage.Location = new System.Drawing.Point(3, 2);
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.ReadOnly = true;
            this.txtMessage.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.txtMessage.Size = new System.Drawing.Size(725, 75);
            this.txtMessage.TabIndex = 0;
            this.txtMessage.Text = "";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(7, 28);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.btnRemoveTarget);
            this.splitContainer1.Panel1.Controls.Add(this.btnRemoveSource);
            this.splitContainer1.Panel1.Controls.Add(this.btnConnect);
            this.splitContainer1.Panel1.Controls.Add(this.btnConfigTarget);
            this.splitContainer1.Panel1.Controls.Add(this.btnConfigSource);
            this.splitContainer1.Panel1.Controls.Add(this.btnAddTarget);
            this.splitContainer1.Panel1.Controls.Add(this.btnAddSource);
            this.splitContainer1.Panel1.Controls.Add(this.cboTargetProfile);
            this.splitContainer1.Panel1.Controls.Add(this.cboSourceProfile);
            this.splitContainer1.Panel1.Controls.Add(this.label2);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.cboTargetDB);
            this.splitContainer1.Panel1.Controls.Add(this.cboSourceDB);
            this.splitContainer1.Panel1.Controls.Add(this.btnSourceScript);
            this.splitContainer1.Panel1.Controls.Add(this.btnCancel);
            this.splitContainer1.Panel1.Controls.Add(this.btnExecute);
            this.splitContainer1.Panel1.Controls.Add(this.groupBox1);
            this.splitContainer1.Panel1.Controls.Add(this.tvSource);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.btnSaveMessage);
            this.splitContainer1.Panel2.Controls.Add(this.txtMessage);
            this.splitContainer1.Panel2.Controls.Add(this.btnCopyMessage);
            this.splitContainer1.Size = new System.Drawing.Size(764, 478);
            this.splitContainer1.SplitterDistance = 400;
            this.splitContainer1.SplitterWidth = 3;
            this.splitContainer1.TabIndex = 21;
            // 
            // btnRemoveTarget
            // 
            this.btnRemoveTarget.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemoveTarget.BackColor = System.Drawing.Color.White;
            this.btnRemoveTarget.FlatAppearance.BorderSize = 0;
            this.btnRemoveTarget.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRemoveTarget.Image = global::DatabaseMigration.Properties.Resources.Remove;
            this.btnRemoveTarget.Location = new System.Drawing.Point(602, 31);
            this.btnRemoveTarget.Name = "btnRemoveTarget";
            this.btnRemoveTarget.Size = new System.Drawing.Size(18, 17);
            this.btnRemoveTarget.TabIndex = 36;
            this.btnRemoveTarget.UseVisualStyleBackColor = false;
            this.btnRemoveTarget.Visible = false;
            this.btnRemoveTarget.Click += new System.EventHandler(this.btnRemoveTarget_Click);
            // 
            // btnRemoveSource
            // 
            this.btnRemoveSource.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemoveSource.BackColor = System.Drawing.Color.White;
            this.btnRemoveSource.FlatAppearance.BorderSize = 0;
            this.btnRemoveSource.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRemoveSource.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnRemoveSource.Image = global::DatabaseMigration.Properties.Resources.Remove;
            this.btnRemoveSource.Location = new System.Drawing.Point(602, 4);
            this.btnRemoveSource.Name = "btnRemoveSource";
            this.btnRemoveSource.Size = new System.Drawing.Size(16, 18);
            this.btnRemoveSource.TabIndex = 34;
            this.btnRemoveSource.UseVisualStyleBackColor = false;
            this.btnRemoveSource.Visible = false;
            this.btnRemoveSource.Click += new System.EventHandler(this.btnRemoveSource_Click);
            // 
            // btnConnect
            // 
            this.btnConnect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnConnect.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnConnect.Location = new System.Drawing.Point(684, 2);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(75, 49);
            this.btnConnect.TabIndex = 35;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // btnConfigTarget
            // 
            this.btnConfigTarget.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnConfigTarget.BackColor = System.Drawing.Color.White;
            this.btnConfigTarget.FlatAppearance.BorderSize = 0;
            this.btnConfigTarget.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConfigTarget.Image = global::DatabaseMigration.Properties.Resources.Config;
            this.btnConfigTarget.Location = new System.Drawing.Point(580, 31);
            this.btnConfigTarget.Name = "btnConfigTarget";
            this.btnConfigTarget.Size = new System.Drawing.Size(16, 17);
            this.btnConfigTarget.TabIndex = 33;
            this.btnConfigTarget.UseVisualStyleBackColor = false;
            this.btnConfigTarget.Visible = false;
            this.btnConfigTarget.Click += new System.EventHandler(this.btnConfigTarget_Click);
            // 
            // btnConfigSource
            // 
            this.btnConfigSource.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnConfigSource.BackColor = System.Drawing.Color.White;
            this.btnConfigSource.FlatAppearance.BorderSize = 0;
            this.btnConfigSource.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConfigSource.Image = global::DatabaseMigration.Properties.Resources.Config;
            this.btnConfigSource.Location = new System.Drawing.Point(579, 4);
            this.btnConfigSource.Name = "btnConfigSource";
            this.btnConfigSource.Size = new System.Drawing.Size(17, 18);
            this.btnConfigSource.TabIndex = 32;
            this.btnConfigSource.UseVisualStyleBackColor = false;
            this.btnConfigSource.Visible = false;
            this.btnConfigSource.Click += new System.EventHandler(this.btnConfigSource_Click);
            // 
            // btnAddTarget
            // 
            this.btnAddTarget.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddTarget.Image = global::DatabaseMigration.Properties.Resources.Add;
            this.btnAddTarget.Location = new System.Drawing.Point(645, 28);
            this.btnAddTarget.Name = "btnAddTarget";
            this.btnAddTarget.Size = new System.Drawing.Size(33, 23);
            this.btnAddTarget.TabIndex = 31;
            this.btnAddTarget.UseVisualStyleBackColor = true;
            this.btnAddTarget.Click += new System.EventHandler(this.btnAddTarget_Click);
            // 
            // btnAddSource
            // 
            this.btnAddSource.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddSource.Image = global::DatabaseMigration.Properties.Resources.Add;
            this.btnAddSource.Location = new System.Drawing.Point(645, 2);
            this.btnAddSource.Name = "btnAddSource";
            this.btnAddSource.Size = new System.Drawing.Size(33, 23);
            this.btnAddSource.TabIndex = 30;
            this.btnAddSource.UseVisualStyleBackColor = true;
            this.btnAddSource.Click += new System.EventHandler(this.btnAddSource_Click);
            // 
            // cboTargetProfile
            // 
            this.cboTargetProfile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboTargetProfile.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cboTargetProfile.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboTargetProfile.FormattingEnabled = true;
            this.cboTargetProfile.Location = new System.Drawing.Point(186, 29);
            this.cboTargetProfile.Name = "cboTargetProfile";
            this.cboTargetProfile.Size = new System.Drawing.Size(453, 22);
            this.cboTargetProfile.TabIndex = 29;
            this.cboTargetProfile.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.cboTargetProfile_DrawItem);
            this.cboTargetProfile.SelectedIndexChanged += new System.EventHandler(this.cboTargetProfile_SelectedIndexChanged);
            // 
            // cboSourceProfile
            // 
            this.cboSourceProfile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboSourceProfile.BackColor = System.Drawing.SystemColors.Window;
            this.cboSourceProfile.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cboSourceProfile.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSourceProfile.FormattingEnabled = true;
            this.cboSourceProfile.Location = new System.Drawing.Point(186, 3);
            this.cboSourceProfile.Name = "cboSourceProfile";
            this.cboSourceProfile.Size = new System.Drawing.Size(453, 22);
            this.cboSourceProfile.TabIndex = 28;
            this.cboSourceProfile.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.cboSourceProfile_DrawItem);
            this.cboSourceProfile.SelectedIndexChanged += new System.EventHandler(this.cboSourceProfile_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 32);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(47, 12);
            this.label2.TabIndex = 27;
            this.label2.Text = "Target:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 12);
            this.label1.TabIndex = 26;
            this.label1.Text = "Source:";
            // 
            // cboTargetDB
            // 
            this.cboTargetDB.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboTargetDB.FormattingEnabled = true;
            this.cboTargetDB.Location = new System.Drawing.Point(70, 29);
            this.cboTargetDB.Name = "cboTargetDB";
            this.cboTargetDB.Size = new System.Drawing.Size(99, 20);
            this.cboTargetDB.TabIndex = 25;
            this.cboTargetDB.SelectedIndexChanged += new System.EventHandler(this.cboTargetDB_SelectedIndexChanged);
            // 
            // cboSourceDB
            // 
            this.cboSourceDB.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSourceDB.FormattingEnabled = true;
            this.cboSourceDB.Location = new System.Drawing.Point(70, 3);
            this.cboSourceDB.Name = "cboSourceDB";
            this.cboSourceDB.Size = new System.Drawing.Size(99, 20);
            this.cboSourceDB.TabIndex = 24;
            this.cboSourceDB.SelectedIndexChanged += new System.EventHandler(this.cboSourceDB_SelectedIndexChanged);
            // 
            // btnSourceScript
            // 
            this.btnSourceScript.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSourceScript.Enabled = false;
            this.btnSourceScript.Location = new System.Drawing.Point(10, 371);
            this.btnSourceScript.Name = "btnSourceScript";
            this.btnSourceScript.Size = new System.Drawing.Size(261, 23);
            this.btnSourceScript.TabIndex = 23;
            this.btnSourceScript.Text = "Generate scripts of source DB ";
            this.btnSourceScript.UseVisualStyleBackColor = true;
            this.btnSourceScript.Click += new System.EventHandler(this.btnSourceScript_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnCancel.Enabled = false;
            this.btnCancel.Location = new System.Drawing.Point(522, 371);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 22;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnExecute
            // 
            this.btnExecute.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnExecute.Enabled = false;
            this.btnExecute.Location = new System.Drawing.Point(425, 371);
            this.btnExecute.Name = "btnExecute";
            this.btnExecute.Size = new System.Drawing.Size(75, 23);
            this.btnExecute.TabIndex = 21;
            this.btnExecute.Text = "Execute";
            this.btnExecute.UseVisualStyleBackColor = true;
            this.btnExecute.Click += new System.EventHandler(this.btnExecute_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.chkAsync);
            this.groupBox1.Controls.Add(this.chkBulkCopy);
            this.groupBox1.Controls.Add(this.chkGenerateIdentity);
            this.groupBox1.Controls.Add(this.chkPickup);
            this.groupBox1.Controls.Add(this.chkScriptData);
            this.groupBox1.Controls.Add(this.chkScriptSchema);
            this.groupBox1.Controls.Add(this.txtTargetDbOwner);
            this.groupBox1.Controls.Add(this.lblTargetDbOwner);
            this.groupBox1.Controls.Add(this.chkGenerateSourceScripts);
            this.groupBox1.Controls.Add(this.chkExecuteOnTarget);
            this.groupBox1.Controls.Add(this.lblOutputFolder);
            this.groupBox1.Controls.Add(this.btnOutputFolder);
            this.groupBox1.Controls.Add(this.txtOutputFolder);
            this.groupBox1.Controls.Add(this.lblScriptsMode);
            this.groupBox1.Controls.Add(this.chkOutputScripts);
            this.groupBox1.Location = new System.Drawing.Point(286, 54);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(469, 311);
            this.groupBox1.TabIndex = 20;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Options";
            // 
            // chkAsync
            // 
            this.chkAsync.AutoSize = true;
            this.chkAsync.Checked = true;
            this.chkAsync.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAsync.Location = new System.Drawing.Point(7, 257);
            this.chkAsync.Name = "chkAsync";
            this.chkAsync.Size = new System.Drawing.Size(180, 16);
            this.chkAsync.TabIndex = 16;
            this.chkAsync.Text = "Use async to transfer data";
            this.chkAsync.UseVisualStyleBackColor = true;
            // 
            // chkBulkCopy
            // 
            this.chkBulkCopy.AutoSize = true;
            this.chkBulkCopy.Location = new System.Drawing.Point(7, 228);
            this.chkBulkCopy.Name = "chkBulkCopy";
            this.chkBulkCopy.Size = new System.Drawing.Size(186, 16);
            this.chkBulkCopy.TabIndex = 15;
            this.chkBulkCopy.Text = "Use BulkCopy to insert data";
            this.chkBulkCopy.UseVisualStyleBackColor = true;
            // 
            // chkGenerateIdentity
            // 
            this.chkGenerateIdentity.AutoSize = true;
            this.chkGenerateIdentity.Location = new System.Drawing.Point(7, 201);
            this.chkGenerateIdentity.Name = "chkGenerateIdentity";
            this.chkGenerateIdentity.Size = new System.Drawing.Size(126, 16);
            this.chkGenerateIdentity.TabIndex = 14;
            this.chkGenerateIdentity.Text = "Generate identity";
            this.chkGenerateIdentity.UseVisualStyleBackColor = true;
            // 
            // chkPickup
            // 
            this.chkPickup.AutoSize = true;
            this.chkPickup.Location = new System.Drawing.Point(7, 173);
            this.chkPickup.Name = "chkPickup";
            this.chkPickup.Size = new System.Drawing.Size(294, 16);
            this.chkPickup.TabIndex = 13;
            this.chkPickup.Text = "Continue from last error of data sync(if any)";
            this.chkPickup.UseVisualStyleBackColor = true;
            // 
            // chkScriptData
            // 
            this.chkScriptData.AutoSize = true;
            this.chkScriptData.Checked = true;
            this.chkScriptData.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkScriptData.Location = new System.Drawing.Point(177, 21);
            this.chkScriptData.Name = "chkScriptData";
            this.chkScriptData.Size = new System.Drawing.Size(48, 16);
            this.chkScriptData.TabIndex = 12;
            this.chkScriptData.Text = "data";
            this.chkScriptData.UseVisualStyleBackColor = true;
            // 
            // chkScriptSchema
            // 
            this.chkScriptSchema.AutoSize = true;
            this.chkScriptSchema.Checked = true;
            this.chkScriptSchema.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkScriptSchema.Location = new System.Drawing.Point(97, 21);
            this.chkScriptSchema.Name = "chkScriptSchema";
            this.chkScriptSchema.Size = new System.Drawing.Size(60, 16);
            this.chkScriptSchema.TabIndex = 11;
            this.chkScriptSchema.Text = "schema";
            this.chkScriptSchema.UseVisualStyleBackColor = true;
            // 
            // txtTargetDbOwner
            // 
            this.txtTargetDbOwner.Location = new System.Drawing.Point(150, 49);
            this.txtTargetDbOwner.Name = "txtTargetDbOwner";
            this.txtTargetDbOwner.Size = new System.Drawing.Size(130, 21);
            this.txtTargetDbOwner.TabIndex = 10;
            // 
            // lblTargetDbOwner
            // 
            this.lblTargetDbOwner.AutoSize = true;
            this.lblTargetDbOwner.Location = new System.Drawing.Point(6, 52);
            this.lblTargetDbOwner.Name = "lblTargetDbOwner";
            this.lblTargetDbOwner.Size = new System.Drawing.Size(137, 12);
            this.lblTargetDbOwner.TabIndex = 9;
            this.lblTargetDbOwner.Text = "Target database owner:";
            // 
            // chkGenerateSourceScripts
            // 
            this.chkGenerateSourceScripts.AutoSize = true;
            this.chkGenerateSourceScripts.Location = new System.Drawing.Point(7, 145);
            this.chkGenerateSourceScripts.Name = "chkGenerateSourceScripts";
            this.chkGenerateSourceScripts.Size = new System.Drawing.Size(282, 16);
            this.chkGenerateSourceScripts.TabIndex = 8;
            this.chkGenerateSourceScripts.Text = "Generate scripts of source database as well";
            this.chkGenerateSourceScripts.UseVisualStyleBackColor = true;
            // 
            // chkExecuteOnTarget
            // 
            this.chkExecuteOnTarget.AutoSize = true;
            this.chkExecuteOnTarget.Checked = true;
            this.chkExecuteOnTarget.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkExecuteOnTarget.Location = new System.Drawing.Point(7, 114);
            this.chkExecuteOnTarget.Name = "chkExecuteOnTarget";
            this.chkExecuteOnTarget.Size = new System.Drawing.Size(228, 16);
            this.chkExecuteOnTarget.TabIndex = 7;
            this.chkExecuteOnTarget.Text = "Execute scripts on target database";
            this.chkExecuteOnTarget.UseVisualStyleBackColor = true;
            // 
            // lblOutputFolder
            // 
            this.lblOutputFolder.AutoSize = true;
            this.lblOutputFolder.Location = new System.Drawing.Point(7, 284);
            this.lblOutputFolder.Name = "lblOutputFolder";
            this.lblOutputFolder.Size = new System.Drawing.Size(89, 12);
            this.lblOutputFolder.TabIndex = 6;
            this.lblOutputFolder.Text = "Output Folder:";
            // 
            // btnOutputFolder
            // 
            this.btnOutputFolder.Location = new System.Drawing.Point(286, 280);
            this.btnOutputFolder.Name = "btnOutputFolder";
            this.btnOutputFolder.Size = new System.Drawing.Size(36, 23);
            this.btnOutputFolder.TabIndex = 4;
            this.btnOutputFolder.Text = "...";
            this.btnOutputFolder.UseVisualStyleBackColor = true;
            this.btnOutputFolder.Click += new System.EventHandler(this.btnOutputFolder_Click);
            // 
            // txtOutputFolder
            // 
            this.txtOutputFolder.Location = new System.Drawing.Point(102, 281);
            this.txtOutputFolder.Name = "txtOutputFolder";
            this.txtOutputFolder.Size = new System.Drawing.Size(178, 21);
            this.txtOutputFolder.TabIndex = 3;
            // 
            // lblScriptsMode
            // 
            this.lblScriptsMode.AutoSize = true;
            this.lblScriptsMode.Location = new System.Drawing.Point(7, 21);
            this.lblScriptsMode.Name = "lblScriptsMode";
            this.lblScriptsMode.Size = new System.Drawing.Size(83, 12);
            this.lblScriptsMode.TabIndex = 1;
            this.lblScriptsMode.Text = "Scripts mode:";
            // 
            // chkOutputScripts
            // 
            this.chkOutputScripts.AutoSize = true;
            this.chkOutputScripts.Location = new System.Drawing.Point(7, 83);
            this.chkOutputScripts.Name = "chkOutputScripts";
            this.chkOutputScripts.Size = new System.Drawing.Size(108, 16);
            this.chkOutputScripts.TabIndex = 0;
            this.chkOutputScripts.Text = "Output scripts";
            this.chkOutputScripts.UseVisualStyleBackColor = true;
            // 
            // tvSource
            // 
            this.tvSource.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.tvSource.CheckBoxes = true;
            this.tvSource.Location = new System.Drawing.Point(9, 57);
            this.tvSource.Name = "tvSource";
            this.tvSource.Size = new System.Drawing.Size(262, 308);
            this.tvSource.TabIndex = 19;
            this.tvSource.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.tvSource_AfterCheck);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(771, 513);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Database Migration";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem settingToolStripMenuItem;
        private System.ComponentModel.BackgroundWorker convertorBackgroundWorker;
        private System.ComponentModel.BackgroundWorker sourceScriptBackgroundWorker;
        private System.Windows.Forms.RichTextBox txtMessage;
        private System.Windows.Forms.Button btnSaveMessage;
        private System.Windows.Forms.Button btnCopyMessage;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button btnRemoveTarget;
        private System.Windows.Forms.Button btnRemoveSource;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnConfigTarget;
        private System.Windows.Forms.Button btnConfigSource;
        private System.Windows.Forms.Button btnAddTarget;
        private System.Windows.Forms.Button btnAddSource;
        private System.Windows.Forms.ComboBox cboTargetProfile;
        private System.Windows.Forms.ComboBox cboSourceProfile;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cboTargetDB;
        private System.Windows.Forms.ComboBox cboSourceDB;
        private System.Windows.Forms.Button btnSourceScript;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnExecute;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox chkAsync;
        private System.Windows.Forms.CheckBox chkBulkCopy;
        private System.Windows.Forms.CheckBox chkGenerateIdentity;
        private System.Windows.Forms.CheckBox chkPickup;
        private System.Windows.Forms.CheckBox chkScriptData;
        private System.Windows.Forms.CheckBox chkScriptSchema;
        private System.Windows.Forms.TextBox txtTargetDbOwner;
        private System.Windows.Forms.Label lblTargetDbOwner;
        private System.Windows.Forms.CheckBox chkGenerateSourceScripts;
        private System.Windows.Forms.CheckBox chkExecuteOnTarget;
        private System.Windows.Forms.Label lblOutputFolder;
        private System.Windows.Forms.Button btnOutputFolder;
        private System.Windows.Forms.TextBox txtOutputFolder;
        private System.Windows.Forms.Label lblScriptsMode;
        private System.Windows.Forms.CheckBox chkOutputScripts;
        private System.Windows.Forms.TreeView tvSource;
        private System.Windows.Forms.FolderBrowserDialog dlgOutputFolder;
    }
}

