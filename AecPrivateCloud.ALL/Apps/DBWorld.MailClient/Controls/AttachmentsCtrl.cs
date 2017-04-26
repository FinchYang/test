using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Windows.Forms;

namespace TestDragFile
{
    public partial class AttachmentsCtrl : UserControl
    {
        private readonly List<string> _attachments = new List<string>();

        public AttachmentsCtrl()
        {
            InitializeComponent();
        }

        public void AddAttachmentPath(string filePath)
        {
            if (!String.IsNullOrEmpty(filePath))
            {
                var child = new AttachmentCtrl
                {
                    Path = filePath,
                    Text = Path.GetFileName(filePath)
                };

                child.DeleteSelf += Child_OnDeleteSelf;
                _attachments.Add(filePath);
                this.flowLayoutPanel.Controls.Add(child);
            }
        }

        public IEnumerable<string> GetAttachmentsPath()
        {
            return _attachments;
        }

        public void ClearAttachmentsPath()
        {
            _attachments.Clear();
            this.flowLayoutPanel.Controls.Clear();
        }

        private void Child_OnDeleteSelf(object sender, EventArgs e)
        {
            var child = sender as AttachmentCtrl;
            if (child != null)
            {
                _attachments.Remove(child.Path);
                this.flowLayoutPanel.Controls.Remove(child);
            }
        }
    }
}
