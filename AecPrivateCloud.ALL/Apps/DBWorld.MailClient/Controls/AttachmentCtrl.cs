using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace TestDragFile
{
    public partial class AttachmentCtrl : UserControl
    {
        public AttachmentCtrl()
        {
            InitializeComponent();
        }

        [Browsable(true)]
        [CategoryAttribute("自定义属性"), DescriptionAttribute("附件名称"), DefaultValue("附件名称")]
        public override string Text
        {
            get { return lblName.Text; }
            set
            {
                this.lblName.Text = value;
                var font = new Font(lblName.Font.FontFamily, lblName.Font.Size);
                var textSize = TextRenderer.MeasureText(this.lblName.Text, font);
                this.lblName.Width = textSize.Width;
                this.panel2.Width = textSize.Width + 40;
            }
        }

        [Browsable(true), Category("自定义属性"), Description("附件路径"), DefaultValue("附件路径")]
        public string Path { get; set; }


        public delegate void DeleteSelfEventHandler(object sender, EventArgs e);

        public event DeleteSelfEventHandler DeleteSelf;

        protected virtual void OnDeleteSelf(object sender, EventArgs e)
        {
            DeleteSelfEventHandler handler = DeleteSelf;
            if (handler != null) handler(this, e);
        }
    }
}
