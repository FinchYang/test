using System;
using System.Windows.Forms;
using DBWorld.MailCore;
using DBWorld.MailCore.Common;

namespace DBWorld.MailClient
{
    public partial class SignEditor : Form
    {
        private readonly MFilesAPI.Vault _vault;
        private readonly MailConfig _config;

        public SignEditor(MFilesAPI.Vault vault,MailConfig config)
        {
            InitializeComponent();
            _vault = vault;
            _config = config;
            Load += OnLoad;
        }

        private void OnLoad(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(_config.Signature))
            {
                var sign = new MailSignature
                {
                    Date = DateTime.Now.ToString("yyyy-MM-dd"), 
                    Address = _config.Email
                };
                _config.Signature = sign.FormatSign();
            }
            editor.DocumentText = _config.Signature;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(editor.BodyText) )
            {
                _config.Signature = null;
            }
            else
            {
                _config.Signature = editor.BodyHtml;  
            }

            MailCore.MF.MfMailConfig.SetMailConfig(_vault, _config);
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void editor_WbNavigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            if (e.Url.IsFile)
            {
                e.Cancel = true;
            }
        }
    }
}
