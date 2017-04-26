using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AecCloud.Service
{
    public interface IEmailService
    {
        void SendActivateAccountEmail(EmailSendingModel model);

        void SendActivateCode(EmailSendingModel model);

        void SendInviteMemberEmail(EmailSendingModel model);

        void SendForgotPasswordEmail(EmailSendingModel model);


        void SetAccountEmailInfo(string host, string emailAddress, string password, string userName, string displayName);

        void SetInvitationEmailInfo(string host, string emailAddress, string password, string userName, string displayName);

        void SetPasswordEmailInfo(string host, string emailAddress, string password, string userName, string displayName);

    }
}
