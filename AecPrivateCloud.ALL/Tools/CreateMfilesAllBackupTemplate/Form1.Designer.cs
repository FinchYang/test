namespace CreateMfilesAllBackupTemplate
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
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.buttoncrreatetemplate = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonrefreshemployee = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxsourcepath = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(54, 10);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(265, 20);
            this.comboBox1.TabIndex = 0;
            // 
            // buttoncrreatetemplate
            // 
            this.buttoncrreatetemplate.Location = new System.Drawing.Point(346, 8);
            this.buttoncrreatetemplate.Name = "buttoncrreatetemplate";
            this.buttoncrreatetemplate.Size = new System.Drawing.Size(217, 23);
            this.buttoncrreatetemplate.TabIndex = 1;
            this.buttoncrreatetemplate.Text = "create full backup template";
            this.buttoncrreatetemplate.UseVisualStyleBackColor = true;
            this.buttoncrreatetemplate.Click += new System.EventHandler(this.button1_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(12, 131);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(776, 274);
            this.richTextBox1.TabIndex = 2;
            this.richTextBox1.Text = "";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(690, 8);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(98, 23);
            this.button2.TabIndex = 3;
            this.button2.Text = "some test";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 12);
            this.label1.TabIndex = 4;
            this.label1.Text = "vault";
            // 
            // buttonrefreshemployee
            // 
            this.buttonrefreshemployee.Location = new System.Drawing.Point(346, 37);
            this.buttonrefreshemployee.Name = "buttonrefreshemployee";
            this.buttonrefreshemployee.Size = new System.Drawing.Size(217, 23);
            this.buttonrefreshemployee.TabIndex = 5;
            this.buttonrefreshemployee.Text = "refresh employees";
            this.buttonrefreshemployee.UseVisualStyleBackColor = true;
            this.buttonrefreshemployee.Click += new System.EventHandler(this.buttonrefreshemployee_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 12);
            this.label2.TabIndex = 6;
            this.label2.Text = "source path";
            // 
            // textBoxsourcepath
            // 
            this.textBoxsourcepath.Location = new System.Drawing.Point(90, 39);
            this.textBoxsourcepath.Name = "textBoxsourcepath";
            this.textBoxsourcepath.Size = new System.Drawing.Size(229, 21);
            this.textBoxsourcepath.TabIndex = 7;
            this.textBoxsourcepath.Text = "F:\\roaming\\AecPrivateCloud.ALL\\Tools";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 417);
            this.Controls.Add(this.textBoxsourcepath);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.buttonrefreshemployee);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.buttoncrreatetemplate);
            this.Controls.Add(this.comboBox1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Button buttoncrreatetemplate;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonrefreshemployee;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxsourcepath;
    }
}

