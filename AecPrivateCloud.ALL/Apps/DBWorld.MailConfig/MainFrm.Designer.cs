namespace DBWorld.MailConfig
{
    partial class MainFrm
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
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.label3 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.chkBoxPopSSL = new System.Windows.Forms.CheckBox();
            this.chkBoxSmtpSSL = new System.Windows.Forms.CheckBox();
            this.txtBoxEmail = new System.Windows.Forms.TextBox();
            this.txtBoxPwd = new System.Windows.Forms.TextBox();
            this.txtBoxPopAddr = new System.Windows.Forms.TextBox();
            this.txtBoxSmtpAddr = new System.Windows.Forms.TextBox();
            this.txtBoxPopPort = new System.Windows.Forms.TextBox();
            this.txtBoxSmtpPort = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btnMarkUp = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.txtBoxMarkup = new System.Windows.Forms.TextBox();
            this.lblResult = new System.Windows.Forms.Label();
            this.btnTest = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.txtBoxUserName = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.errorProvider2 = new System.Windows.Forms.ErrorProvider(this.components);
            this.errorProvider3 = new System.Windows.Forms.ErrorProvider(this.components);
            this.errorProvider4 = new System.Windows.Forms.ErrorProvider(this.components);
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider4)).BeginInit();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 31);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(71, 12);
            this.label3.TabIndex = 0;
            this.label3.Text = "POP服务器：";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(271, 31);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 12);
            this.label5.TabIndex = 0;
            this.label5.Text = "端口号：";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(271, 60);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 12);
            this.label6.TabIndex = 1;
            this.label6.Text = "端口号：";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(8, 63);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(77, 12);
            this.label8.TabIndex = 1;
            this.label8.Text = "SMTP服务器：";
            // 
            // chkBoxPopSSL
            // 
            this.chkBoxPopSSL.AutoSize = true;
            this.chkBoxPopSSL.Location = new System.Drawing.Point(211, 29);
            this.chkBoxPopSSL.Name = "chkBoxPopSSL";
            this.chkBoxPopSSL.Size = new System.Drawing.Size(42, 16);
            this.chkBoxPopSSL.TabIndex = 1;
            this.chkBoxPopSSL.Text = "SSL";
            this.chkBoxPopSSL.UseVisualStyleBackColor = true;
            this.chkBoxPopSSL.CheckedChanged += new System.EventHandler(this.chkBoxPopSSL_CheckedChanged);
            // 
            // chkBoxSmtpSSL
            // 
            this.chkBoxSmtpSSL.AutoSize = true;
            this.chkBoxSmtpSSL.Location = new System.Drawing.Point(211, 59);
            this.chkBoxSmtpSSL.Name = "chkBoxSmtpSSL";
            this.chkBoxSmtpSSL.Size = new System.Drawing.Size(42, 16);
            this.chkBoxSmtpSSL.TabIndex = 4;
            this.chkBoxSmtpSSL.Text = "SSL";
            this.chkBoxSmtpSSL.UseVisualStyleBackColor = true;
            this.chkBoxSmtpSSL.CheckedChanged += new System.EventHandler(this.chkBoxSmtpSSL_CheckedChanged);
            // 
            // txtBoxEmail
            // 
            this.txtBoxEmail.Location = new System.Drawing.Point(88, 50);
            this.txtBoxEmail.Name = "txtBoxEmail";
            this.txtBoxEmail.Size = new System.Drawing.Size(292, 21);
            this.txtBoxEmail.TabIndex = 1;
            this.txtBoxEmail.Validating += new System.ComponentModel.CancelEventHandler(this.txtBoxEmail_Validating);
            // 
            // txtBoxPwd
            // 
            this.txtBoxPwd.Location = new System.Drawing.Point(88, 77);
            this.txtBoxPwd.Name = "txtBoxPwd";
            this.txtBoxPwd.Size = new System.Drawing.Size(292, 21);
            this.txtBoxPwd.TabIndex = 2;
            this.txtBoxPwd.UseSystemPasswordChar = true;
            this.txtBoxPwd.Validating += new System.ComponentModel.CancelEventHandler(this.txtBoxPwd_Validating);
            // 
            // txtBoxPopAddr
            // 
            this.txtBoxPopAddr.Location = new System.Drawing.Point(88, 27);
            this.txtBoxPopAddr.Name = "txtBoxPopAddr";
            this.txtBoxPopAddr.Size = new System.Drawing.Size(105, 21);
            this.txtBoxPopAddr.TabIndex = 0;
            // 
            // txtBoxSmtpAddr
            // 
            this.txtBoxSmtpAddr.Location = new System.Drawing.Point(88, 57);
            this.txtBoxSmtpAddr.Name = "txtBoxSmtpAddr";
            this.txtBoxSmtpAddr.Size = new System.Drawing.Size(105, 21);
            this.txtBoxSmtpAddr.TabIndex = 3;
            // 
            // txtBoxPopPort
            // 
            this.txtBoxPopPort.Location = new System.Drawing.Point(328, 27);
            this.txtBoxPopPort.Name = "txtBoxPopPort";
            this.txtBoxPopPort.Size = new System.Drawing.Size(52, 21);
            this.txtBoxPopPort.TabIndex = 2;
            this.txtBoxPopPort.Text = "110";
            this.txtBoxPopPort.Validating += new System.ComponentModel.CancelEventHandler(this.txtBoxPopPort_Validating);
            // 
            // txtBoxSmtpPort
            // 
            this.txtBoxSmtpPort.Location = new System.Drawing.Point(328, 56);
            this.txtBoxSmtpPort.Name = "txtBoxSmtpPort";
            this.txtBoxSmtpPort.Size = new System.Drawing.Size(52, 21);
            this.txtBoxSmtpPort.TabIndex = 5;
            this.txtBoxSmtpPort.Text = "25";
            this.txtBoxSmtpPort.Validating += new System.ComponentModel.CancelEventHandler(this.txtBoxSmtpPort_Validating);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.txtBoxPopPort);
            this.groupBox2.Controls.Add(this.txtBoxSmtpPort);
            this.groupBox2.Controls.Add(this.txtBoxSmtpAddr);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.txtBoxPopAddr);
            this.groupBox2.Controls.Add(this.chkBoxSmtpSSL);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.chkBoxPopSSL);
            this.groupBox2.Location = new System.Drawing.Point(16, 141);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(400, 92);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "服务器信息";
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(260, 343);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 4;
            this.btnOK.Text = "创建(&O)";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.CausesValidation = false;
            this.btnCancel.Location = new System.Drawing.Point(341, 343);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "取消(&C)";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btnMarkUp);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.txtBoxMarkup);
            this.groupBox3.Location = new System.Drawing.Point(16, 248);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(400, 62);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "收发邮件规则*";
            // 
            // btnMarkUp
            // 
            this.btnMarkUp.Location = new System.Drawing.Point(305, 22);
            this.btnMarkUp.Name = "btnMarkUp";
            this.btnMarkUp.Size = new System.Drawing.Size(75, 23);
            this.btnMarkUp.TabIndex = 1;
            this.btnMarkUp.Text = "默认规则";
            this.btnMarkUp.UseVisualStyleBackColor = true;
            this.btnMarkUp.Click += new System.EventHandler(this.btnMarkUp_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(17, 29);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 12);
            this.label4.TabIndex = 0;
            this.label4.Text = "主题包含：";
            // 
            // txtBoxMarkup
            // 
            this.txtBoxMarkup.Location = new System.Drawing.Point(88, 24);
            this.txtBoxMarkup.Name = "txtBoxMarkup";
            this.txtBoxMarkup.Size = new System.Drawing.Size(200, 21);
            this.txtBoxMarkup.TabIndex = 0;
            // 
            // lblResult
            // 
            this.lblResult.AutoSize = true;
            this.lblResult.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.lblResult.Location = new System.Drawing.Point(27, 317);
            this.lblResult.Name = "lblResult";
            this.lblResult.Size = new System.Drawing.Size(0, 12);
            this.lblResult.TabIndex = 18;
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(17, 342);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(84, 23);
            this.btnTest.TabIndex = 3;
            this.btnTest.Text = "测试连接(&T)";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 53);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 12;
            this.label1.Text = "邮件帐号：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(41, 80);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 14;
            this.label2.Text = "密码：";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(30, 26);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(53, 12);
            this.label7.TabIndex = 15;
            this.label7.Text = "用户名：";
            // 
            // txtBoxUserName
            // 
            this.txtBoxUserName.Location = new System.Drawing.Point(89, 23);
            this.txtBoxUserName.Name = "txtBoxUserName";
            this.txtBoxUserName.Size = new System.Drawing.Size(291, 21);
            this.txtBoxUserName.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtBoxUserName);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.txtBoxPwd);
            this.groupBox1.Controls.Add(this.txtBoxEmail);
            this.groupBox1.Location = new System.Drawing.Point(16, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(400, 115);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "用户信息";
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            // 
            // errorProvider2
            // 
            this.errorProvider2.ContainerControl = this;
            // 
            // errorProvider3
            // 
            this.errorProvider3.ContainerControl = this;
            // 
            // errorProvider4
            // 
            this.errorProvider4.ContainerControl = this;
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(156, 347);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(53, 12);
            this.linkLabel1.TabIndex = 19;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "设置指南";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // MainFrm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(430, 377);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.btnTest);
            this.Controls.Add(this.lblResult);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.Name = "MainFrm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "邮箱设置";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainFrm_FormClosing);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider4)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.CheckBox chkBoxPopSSL;
        private System.Windows.Forms.CheckBox chkBoxSmtpSSL;
        private System.Windows.Forms.TextBox txtBoxEmail;
        private System.Windows.Forms.TextBox txtBoxPwd;
        private System.Windows.Forms.TextBox txtBoxPopAddr;
        private System.Windows.Forms.TextBox txtBoxSmtpAddr;
        private System.Windows.Forms.TextBox txtBoxPopPort;
        private System.Windows.Forms.TextBox txtBoxSmtpPort;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox txtBoxMarkup;
        private System.Windows.Forms.Button btnMarkUp;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblResult;
        private System.Windows.Forms.Button btnTest;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtBoxUserName;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ErrorProvider errorProvider1;
        private System.Windows.Forms.ErrorProvider errorProvider2;
        private System.Windows.Forms.ErrorProvider errorProvider3;
        private System.Windows.Forms.ErrorProvider errorProvider4;
        private System.Windows.Forms.LinkLabel linkLabel1;
    }
}

