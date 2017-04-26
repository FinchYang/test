namespace miscellaneous
{
    partial class Form1
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
            this.textBoxuser = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxpass = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxmfserver = new System.Windows.Forms.TextBox();
            this.buttonenableusers = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.buttoncreateprojects = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxsqlserver = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // textBoxuser
            // 
            this.textBoxuser.Location = new System.Drawing.Point(461, 29);
            this.textBoxuser.Name = "textBoxuser";
            this.textBoxuser.Size = new System.Drawing.Size(100, 21);
            this.textBoxuser.TabIndex = 0;
            this.textBoxuser.Text = "admin";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(326, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "user";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(583, 29);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "pass";
            // 
            // textBoxpass
            // 
            this.textBoxpass.Location = new System.Drawing.Point(718, 29);
            this.textBoxpass.Name = "textBoxpass";
            this.textBoxpass.Size = new System.Drawing.Size(100, 21);
            this.textBoxpass.TabIndex = 2;
            this.textBoxpass.Text = "cadsimula@123A";
            this.textBoxpass.UseSystemPasswordChar = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(46, 29);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(83, 12);
            this.label3.TabIndex = 5;
            this.label3.Text = "mfiles server";
            // 
            // textBoxmfserver
            // 
            this.textBoxmfserver.Location = new System.Drawing.Point(181, 29);
            this.textBoxmfserver.Name = "textBoxmfserver";
            this.textBoxmfserver.Size = new System.Drawing.Size(100, 21);
            this.textBoxmfserver.TabIndex = 4;
            this.textBoxmfserver.Text = "gc.cscec82.com";
            // 
            // buttonenableusers
            // 
            this.buttonenableusers.Location = new System.Drawing.Point(902, 29);
            this.buttonenableusers.Name = "buttonenableusers";
            this.buttonenableusers.Size = new System.Drawing.Size(75, 23);
            this.buttonenableusers.TabIndex = 6;
            this.buttonenableusers.Text = "enable  users";
            this.buttonenableusers.UseVisualStyleBackColor = true;
            this.buttonenableusers.Click += new System.EventHandler(this.buttonenableusers_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(13, 187);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(1043, 315);
            this.richTextBox1.TabIndex = 7;
            this.richTextBox1.Text = "";
            // 
            // buttoncreateprojects
            // 
            this.buttoncreateprojects.Location = new System.Drawing.Point(755, 135);
            this.buttoncreateprojects.Name = "buttoncreateprojects";
            this.buttoncreateprojects.Size = new System.Drawing.Size(222, 23);
            this.buttoncreateprojects.TabIndex = 8;
            this.buttoncreateprojects.Text = "create projects in advance";
            this.buttoncreateprojects.UseVisualStyleBackColor = true;
            this.buttoncreateprojects.Click += new System.EventHandler(this.buttoncreateprojects_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(46, 74);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(59, 12);
            this.label4.TabIndex = 10;
            this.label4.Text = "sqlserver";
            // 
            // textBoxsqlserver
            // 
            this.textBoxsqlserver.Location = new System.Drawing.Point(181, 74);
            this.textBoxsqlserver.Name = "textBoxsqlserver";
            this.textBoxsqlserver.Size = new System.Drawing.Size(845, 21);
            this.textBoxsqlserver.TabIndex = 9;
            this.textBoxsqlserver.Text = "Data Source=localhost\\sqlexpress;Initial Catalog=AecPrivateCloudZC;user id=sa;pas" +
    "sword=cadsimula123A;MultipleActiveResultSets=true";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1068, 514);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBoxsqlserver);
            this.Controls.Add(this.buttoncreateprojects);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.buttonenableusers);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBoxmfserver);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxpass);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxuser);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxuser;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxpass;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxmfserver;
        private System.Windows.Forms.Button buttonenableusers;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button buttoncreateprojects;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBoxsqlserver;
    }
}

