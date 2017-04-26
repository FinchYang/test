namespace Notification.Client
{
    partial class NoticeSet
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NoticeSet));
            this.treeViewConcern = new System.Windows.Forms.TreeView();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxmfnotificationserver = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeViewConcern
            // 
            this.treeViewConcern.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewConcern.Location = new System.Drawing.Point(0, 0);
            this.treeViewConcern.Name = "treeViewConcern";
            this.treeViewConcern.Size = new System.Drawing.Size(476, 598);
            this.treeViewConcern.TabIndex = 0;
            this.treeViewConcern.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.treeViewConcern_AfterCheck);
            this.treeViewConcern.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewConcern_AfterSelect);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.treeViewConcern);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.label3);
            this.splitContainer1.Panel2.Controls.Add(this.textBoxmfnotificationserver);
            this.splitContainer1.Size = new System.Drawing.Size(476, 649);
            this.splitContainer1.SplitterDistance = 598;
            this.splitContainer1.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 17);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(121, 15);
            this.label3.TabIndex = 12;
            this.label3.Text = "通知Web服务设置";
            // 
            // textBoxmfnotificationserver
            // 
            this.textBoxmfnotificationserver.Location = new System.Drawing.Point(137, 12);
            this.textBoxmfnotificationserver.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textBoxmfnotificationserver.Name = "textBoxmfnotificationserver";
            this.textBoxmfnotificationserver.Size = new System.Drawing.Size(322, 25);
            this.textBoxmfnotificationserver.TabIndex = 11;
            this.textBoxmfnotificationserver.TextChanged += new System.EventHandler(this.textBoxmfnotificationserver_TextChanged);
            // 
            // NoticeSet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(476, 649);
            this.Controls.Add(this.splitContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NoticeSet";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "设置关注信息和Web服务";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.NoticeSet_FormClosing);
            this.Load += new System.EventHandler(this.NoticeSet_Load);
            this.SizeChanged += new System.EventHandler(this.NoticeSet_SizeChanged);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.TreeView treeViewConcern;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label label3;
        public System.Windows.Forms.TextBox textBoxmfnotificationserver;

    }
}