/******************************
* 协同云邮件模块的元数据配置
*******************************/
var md = md || {};
(function (u, undefined) {

    //邮件
    var mail = {
        typeAlias: '0',
        classAlias: 'ClassProjMail',
        ownerAlias: null,
        propDefs: {
            PropMailSubject: "PropMailSubject", //主题
            PropMailSender: "PropMailSender", //发件人
            PropMailReceiver: "PropMailReceiver", //收件人
            PropMailCc: "PropMailCc", //抄送人
            PropMailCreatedTime: "PropMailCreatedTime", //创建时间
            PropMailFolders: "PropMailFolders", //邮件文件夹
            PropTags: "PropTags", //标签
            PropEmailAttachments: "PropEmailAttachments", //附件
            PropIsRead: "PropIsRead" //已读?
        }
    };

    var mailSettings = { //邮箱设置
        typeAlias: 'ObjMailSettings',
        classAlias: 'ClassMailSettings',
        ownerAlias: null,
        propDefs: {
            PropMailFullname: "PropMailFullname", //姓名
            PropMailBox: "PropMailBox", //邮箱
            PropPassword: "PropPassword", //密码
            PropMailPop: "PropMailPop", //接收邮件服务器（POP）
            PropMailPopPort: "PropMailPopPort", //接收邮件服务器（POP）端口
            PropSSLReceive: "PropSSLReceive", //接收启用SSL
            PropMailSmtp: "PropMailSmtp", //发送邮件服务器（SMTP）
            PropMailSmtpPort: "PropMailSmtpPort", //发送邮件服务器（SMTP）端口
            PropSSLSend: "PropSSLSend", //发送启用SSL
            PropMailContains: "PropMailContains", //主题包含
            PropMailReceiveInterval: "PropMailReceiveInterval", //接收邮件时间间隔(小时)
            PropSignature: "PropSignature"//邮件签名
        }
    };

    var emailAdressBook = {//邮件联系人
        typeAlias: 'ObjEmailAdressBook',
        classAlias: 'ClassEmailAddressBook',
        ownerAlias: null,
        propDefs: {
            Title: "PropAddressBookTitle", //邮件联系人名称
            LinkmanName: "PropLinkmanName", //姓名
            Email: "PropEmail", //邮箱
            InnerUser: "PropInnerUser" //内部用户
        }
    };

    u.mail = mail; //邮件
    u.mailSettings = mailSettings; //邮箱设置
    u.emailAdressBook = emailAdressBook;//邮件联系人
})(md);