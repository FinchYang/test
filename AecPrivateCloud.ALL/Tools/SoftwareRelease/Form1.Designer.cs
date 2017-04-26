namespace SoftwareRelease
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
            this.buttonnotice = new System.Windows.Forms.Button();
            this.textBoxweburl = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.richTextBoxlog = new System.Windows.Forms.RichTextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxwebpath = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxversion = new System.Windows.Forms.TextBox();
            this.buttonapp = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxapppath = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBoxappver = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textBoxguid = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // buttonnotice
            // 
            this.buttonnotice.Location = new System.Drawing.Point(471, 54);
            this.buttonnotice.Name = "buttonnotice";
            this.buttonnotice.Size = new System.Drawing.Size(189, 23);
            this.buttonnotice.TabIndex = 0;
            this.buttonnotice.Text = "Notice 升级发布";
            this.buttonnotice.UseVisualStyleBackColor = true;
            this.buttonnotice.Click += new System.EventHandler(this.buttonnotice_Click);
            // 
            // textBoxweburl
            // 
            this.textBoxweburl.Location = new System.Drawing.Point(72, 54);
            this.textBoxweburl.Name = "textBoxweburl";
            this.textBoxweburl.Size = new System.Drawing.Size(380, 21);
            this.textBoxweburl.TabIndex = 1;
            this.textBoxweburl.Text = "http://gc.cscec82.com:8000/";
            this.textBoxweburl.TextChanged += new System.EventHandler(this.textBoxweburl_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 57);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "web url";
            // 
            // richTextBoxlog
            // 
            this.richTextBoxlog.Location = new System.Drawing.Point(14, 210);
            this.richTextBoxlog.Name = "richTextBoxlog";
            this.richTextBoxlog.Size = new System.Drawing.Size(646, 192);
            this.richTextBoxlog.TabIndex = 3;
            this.richTextBoxlog.Text = "";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 84);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 5;
            this.label2.Text = "web path";
            // 
            // textBoxwebpath
            // 
            this.textBoxwebpath.Location = new System.Drawing.Point(72, 81);
            this.textBoxwebpath.Name = "textBoxwebpath";
            this.textBoxwebpath.Size = new System.Drawing.Size(380, 21);
            this.textBoxwebpath.TabIndex = 4;
            this.textBoxwebpath.Text = "D:\\PrivateCloud";
            this.textBoxwebpath.TextChanged += new System.EventHandler(this.textBoxwebpath_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 30);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 7;
            this.label3.Text = "notice ver";
            // 
            // textBoxversion
            // 
            this.textBoxversion.Location = new System.Drawing.Point(72, 27);
            this.textBoxversion.Name = "textBoxversion";
            this.textBoxversion.Size = new System.Drawing.Size(380, 21);
            this.textBoxversion.TabIndex = 6;
            this.textBoxversion.Text = "1.0.0.23";
            this.textBoxversion.TextChanged += new System.EventHandler(this.textBoxversion_TextChanged);
            // 
            // buttonapp
            // 
            this.buttonapp.Location = new System.Drawing.Point(471, 106);
            this.buttonapp.Name = "buttonapp";
            this.buttonapp.Size = new System.Drawing.Size(189, 23);
            this.buttonapp.TabIndex = 8;
            this.buttonapp.Text = "App 升级发布";
            this.buttonapp.UseVisualStyleBackColor = true;
            this.buttonapp.Click += new System.EventHandler(this.buttonapp_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 111);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 12);
            this.label4.TabIndex = 10;
            this.label4.Text = "app path";
            // 
            // textBoxapppath
            // 
            this.textBoxapppath.Location = new System.Drawing.Point(72, 108);
            this.textBoxapppath.Name = "textBoxapppath";
            this.textBoxapppath.Size = new System.Drawing.Size(380, 21);
            this.textBoxapppath.TabIndex = 9;
            this.textBoxapppath.Text = "C:\\privatecloud\\0installersource\\vaultapps";
            this.textBoxapppath.TextChanged += new System.EventHandler(this.textBoxapppath_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 138);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(47, 12);
            this.label5.TabIndex = 12;
            this.label5.Text = "app ver";
            // 
            // textBoxappver
            // 
            this.textBoxappver.Location = new System.Drawing.Point(72, 135);
            this.textBoxappver.Name = "textBoxappver";
            this.textBoxappver.Size = new System.Drawing.Size(380, 21);
            this.textBoxappver.TabIndex = 11;
            this.textBoxappver.Text = "55";
            this.textBoxappver.TextChanged += new System.EventHandler(this.textBoxappver_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 165);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 12);
            this.label6.TabIndex = 14;
            this.label6.Text = "app guid";
            // 
            // textBoxguid
            // 
            this.textBoxguid.Location = new System.Drawing.Point(72, 162);
            this.textBoxguid.Name = "textBoxguid";
            this.textBoxguid.Size = new System.Drawing.Size(380, 21);
            this.textBoxguid.TabIndex = 13;
            this.textBoxguid.Text = "F101258B-FD65-4199-B22F-240B507C0DCC";
            this.textBoxguid.TextChanged += new System.EventHandler(this.textBoxguid_TextChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(672, 414);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.textBoxguid);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBoxappver);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBoxapppath);
            this.Controls.Add(this.buttonapp);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBoxversion);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxwebpath);
            this.Controls.Add(this.richTextBoxlog);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxweburl);
            this.Controls.Add(this.buttonnotice);
            this.Name = "Form1";
            this.Text = "Software Release Tool";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonnotice;
        private System.Windows.Forms.TextBox textBoxweburl;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RichTextBox richTextBoxlog;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxwebpath;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxversion;
        private System.Windows.Forms.Button buttonapp;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBoxapppath;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBoxappver;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBoxguid;
    }
}

