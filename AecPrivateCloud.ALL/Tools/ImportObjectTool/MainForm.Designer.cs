namespace SimulaDesign.ImportObjectTool
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.buttonImportExcel = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.labelClass = new System.Windows.Forms.Label();
            this.comboBoxClasses = new System.Windows.Forms.ComboBox();
            this.buttonMapping = new System.Windows.Forms.Button();
            this.buttonImport = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.labelObjType = new System.Windows.Forms.Label();
            this.comboBoxObjType = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonImportExcel
            // 
            this.buttonImportExcel.Location = new System.Drawing.Point(13, 10);
            this.buttonImportExcel.Name = "buttonImportExcel";
            this.buttonImportExcel.Size = new System.Drawing.Size(75, 23);
            this.buttonImportExcel.TabIndex = 24;
            this.buttonImportExcel.Text = "导入Excel";
            this.buttonImportExcel.UseVisualStyleBackColor = true;
            this.buttonImportExcel.Click += new System.EventHandler(this.buttonImportExcel_Click);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 536);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 23;
            this.label2.Text = "导入进度：";
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(82, 530);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(784, 23);
            this.progressBar1.Step = 1;
            this.progressBar1.TabIndex = 22;
            // 
            // labelClass
            // 
            this.labelClass.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.labelClass.AutoSize = true;
            this.labelClass.Location = new System.Drawing.Point(372, 14);
            this.labelClass.Name = "labelClass";
            this.labelClass.Size = new System.Drawing.Size(53, 12);
            this.labelClass.TabIndex = 21;
            this.labelClass.Text = "对象类别";
            // 
            // comboBoxClasses
            // 
            this.comboBoxClasses.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.comboBoxClasses.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxClasses.FormattingEnabled = true;
            this.comboBoxClasses.Location = new System.Drawing.Point(436, 11);
            this.comboBoxClasses.Name = "comboBoxClasses";
            this.comboBoxClasses.Size = new System.Drawing.Size(244, 20);
            this.comboBoxClasses.TabIndex = 20;
            // 
            // buttonMapping
            // 
            this.buttonMapping.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.buttonMapping.Location = new System.Drawing.Point(691, 10);
            this.buttonMapping.Name = "buttonMapping";
            this.buttonMapping.Size = new System.Drawing.Size(75, 23);
            this.buttonMapping.TabIndex = 19;
            this.buttonMapping.Text = "属性映射";
            this.buttonMapping.UseVisualStyleBackColor = true;
            this.buttonMapping.Click += new System.EventHandler(this.buttonMapping_Click);
            // 
            // buttonImport
            // 
            this.buttonImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonImport.Location = new System.Drawing.Point(773, 10);
            this.buttonImport.Name = "buttonImport";
            this.buttonImport.Size = new System.Drawing.Size(75, 23);
            this.buttonImport.TabIndex = 18;
            this.buttonImport.Text = "导入对象";
            this.buttonImport.UseVisualStyleBackColor = true;
            this.buttonImport.Click += new System.EventHandler(this.buttonImport_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView1.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(-43, 41);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowTemplate.Height = 23;
            this.dataGridView1.Size = new System.Drawing.Size(914, 478);
            this.dataGridView1.TabIndex = 17;
            // 
            // labelObjType
            // 
            this.labelObjType.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.labelObjType.AutoSize = true;
            this.labelObjType.Location = new System.Drawing.Point(96, 13);
            this.labelObjType.Name = "labelObjType";
            this.labelObjType.Size = new System.Drawing.Size(53, 12);
            this.labelObjType.TabIndex = 26;
            this.labelObjType.Text = "对象类型";
            // 
            // comboBoxObjType
            // 
            this.comboBoxObjType.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.comboBoxObjType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxObjType.FormattingEnabled = true;
            this.comboBoxObjType.Location = new System.Drawing.Point(160, 10);
            this.comboBoxObjType.Name = "comboBoxObjType";
            this.comboBoxObjType.Size = new System.Drawing.Size(206, 20);
            this.comboBoxObjType.TabIndex = 25;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(878, 562);
            this.Controls.Add(this.labelObjType);
            this.Controls.Add(this.comboBoxObjType);
            this.Controls.Add(this.buttonImportExcel);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.labelClass);
            this.Controls.Add(this.comboBoxClasses);
            this.Controls.Add(this.buttonMapping);
            this.Controls.Add(this.buttonImport);
            this.Controls.Add(this.dataGridView1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "对象导入工具";
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonImportExcel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label labelClass;
        private System.Windows.Forms.ComboBox comboBoxClasses;
        private System.Windows.Forms.Button buttonMapping;
        private System.Windows.Forms.Button buttonImport;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Label labelObjType;
        private System.Windows.Forms.ComboBox comboBoxObjType;
    }
}

