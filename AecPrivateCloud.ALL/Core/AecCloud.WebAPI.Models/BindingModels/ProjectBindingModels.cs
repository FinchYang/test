using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.BaseCore;
using AecCloud.WebAPI.Models.DataAnnotations;

//http://stackoverflow.com/questions/13425320/what-parameters-does-the-stringlength-attribute-errormessage-take

namespace AecCloud.WebAPI.Models
{
    public class ProjectCreateModel
    {
         [Required(ErrorMessage = "{0}必填.")]
      //  [Required]
        [Display(Name = "项目名称")]
        [RegularExpression(Utility.ProjectNamePattern, ErrorMessage = "{0}的格式不正确")]
        public string Name { get; set; }
        [Required]
        [Display(Name = "建设单位")]
        public string OwnerUnit { get; set; }

     //   [Required]
        [Display(Name = "项管单位")]
        public string PmUnit { get; set; }
        [Required]
        [Display(Name = "监理单位")]
        public string PropSupervisorUnit { get; set; }
       // [Required]
        [Display(Name = "勘察单位")]
        public string InvestigateUnit { get; set; }

        [Required]
        [Display(Name = "设计单位")]
        public string PropDesignUnit { get; set; }

        [Required]
        [Display(Name = "建设规模")]
        public string ConstructionScale { get; set; }

        [Required]
        [Display(Name = "合同金额")]
        public string ContractAmount { get; set; }

        [Required(ErrorMessage = "{0}必填.")]
        [Display(Name = "项目编号")]
        public string Number { get; set; }
        [Display(Name = "项目描述")]
        public string Description { get; set; }
        [Display(Name = "起始时间")]
        [NeedSetDate(ErrorMessage="必须设置{0}")]
        public DateTime StartDateUtc { get; set; }
        [Display(Name = "结束时间")]
        [NeedSetDate(ErrorMessage = "必须设置{0}")]
        public DateTime EndDateUtc { get; set; }
        [Display(Name = "封面")]
        [MaxLength(Utility.MaxImageLength, ErrorMessage = "{0}图片大小不能大于200K")]
        public byte[] Cover { get; set; }
        [Display(Name = "模板ID")]
        [Required]
        [Range(1, Int64.MaxValue, ErrorMessage="{0}必须是正整数")]
        public long TemplateId { get; set; }

        [Display(Name = "公司ID")]
        [Required]
        [Range(1, Int64.MaxValue, ErrorMessage = "{0}必须是正整数")]
      
        public long CompanyId { get; set; }

        [Display(Name = "项目类别ID")]
        [Required]
        [Range(1, Int64.MaxValue, ErrorMessage = "{0}必须是正整数")]
        public long ProjectClassId { get; set; }

        
        [Display(Name = "项目等级ID")]
        [Required]
        [Range(1, Int64.MaxValue, ErrorMessage = "{0}必须是正整数")]
        public long ProjectLevelId { get; set; }


        [Display(Name = "地区ID")]
        [Required]
        [Range(1, Int64.MaxValue, ErrorMessage = "{0}必须是正整数")]
        public long AreaId { get; set; }

        //public long CloudId { get; set; }
        //[Required]
        [Display(Name = "参与方ID")]
        //[Range(1, int.MaxValue, ErrorMessage = "{0}必须是正整数")]
        public long ProjectPartyId { get; set; }
        public string Area { get; set; }
        public string Company { get; set; }
        public string ProjectLevel { get; set; }

        public string ProjectClass { set; get; }
        [Required(ErrorMessage="必须填写一个单体！")]
        public string FirstModel { get; set; }

        public string ModelList { get; set; }
    }

    public class ProjectEditModel
    {
        [Required]
        [Display(Name = "项目ID")]
        [Range(1, int.MaxValue, ErrorMessage = "{0}必须是正整数")]
        public long Id { get; set; }
        //[Required(ErrorMessage="必须提供{0}")]
        [Display(Name = "项目名称")]
        public string Name { get; set; }
        [Display(Name = "项目编号")]
        public string Number { get; set; }
        [Display(Name="封面")]
        //[MaxLength(204800, ErrorMessage="{0}图片大小不能大于200K")]
        public byte[] Cover { get; set; }
        [Display(Name = "封面")]
        public string Cover64 { get; set; }
        [Display(Name="项目描述")]
        public string Description { get; set; }
        [Display(Name="起始时间")]
        [NeedSetDate(ErrorMessage = "必须设置{0}")]
        public DateTime StartDateUtc { get; set; }
        [Display(Name="结束时间")]
        [NeedSetDate(ErrorMessage = "必须设置{0}")]
        public DateTime EndDateUtc { get; set; }
        [Display(Name="项目模板")]
        //[Required]
        //[Range(1, int.MaxValue, ErrorMessage = "{0}必须是正整数")]
        public long TemplateId { get; set; }
        [Display(Name = "项目状态")]
        public long StatusId { get; set; }
        [Display(Name = "建设单位")]
        public string OwnerUnit { get; set; }
        [Display(Name="设计单位")]
        public string DesignUnit { get; set; }
        [Display(Name = "施工单位")]
        public string ConstructionUnit { get; set; }
        [Display(Name = "监理单位")]
        public string SupervisionUnit { get; set; }
        [Display(Name = "勘察单位")]
        public string InvestigateUnit { get; set; }
        [Display(Name = "项管单位")]
        public string PmUnit { get; set; }

        [Display(Name = "公司ID")]
        [Required]
        [Range(1, Int64.MaxValue, ErrorMessage = "{0}必须是正整数")]

        public long CompanyId { get; set; }
        public string Company { get; set; }
        public string ProjectClass { get; set; }
        public string Level { get; set; }
        public string Area { get; set; }
        public string ContractAmount { get; set; }
        public string ConstructionScale { get; set; }
        
    }

    public class ProjectTransferModel
    {
        /// <summary>
        /// ProjectId
        /// </summary>
        [Display(Name="项目ID")]
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "{0}必须是正整数")]
        public int Id { get; set; }
        /// <summary>
        /// 移交给的用户ID
        /// </summary>
        [Display(Name="移交用户")]
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "{0}必须是正整数")]
        public int ToUserId { get; set; }
    }
    /// <summary>
    /// 众包邀请
    /// </summary>
    public class ContractInvitationModel
    {
        /// <summary>
        /// 招标项目ID
        /// </summary>
        public int BidProjId { get; set; }
        /// <summary>
        /// 发起招标人的用户ID
        /// </summary>
        public long BidOwnerUserId { get; set; }
        /// <summary>
        /// 发起招标人的Email
        /// </summary>
        public string BidOwnerEmail { get; set; }
        /// <summary>
        /// 中标人的Email
        /// </summary>
        public string BidWinnerEmail { get; set; }
        
    }
    /// <summary>
    /// 通过Email邀请项目成员的模型绑定类
    /// </summary>
    public class ProjectInvitationEmailModel
    {
        public long InviterId { get; set; }
        [Required(ErrorMessage = "{0}必填.")]
        [Display(Name = "被邀请人邮箱")]
        [RegularExpression(Utility.EmailPattern, ErrorMessage = "{0}的格式不正确")]
        public string InviteeEmail { get; set; }
        [Display(Name="邀请信息")]
        public string InvitationMessage { get; set; }
        [Display(Name="项目ID")]
        [Required(ErrorMessage = "必须指定{0}")]
        [Range(1, int.MaxValue, ErrorMessage = "{0}必须是正整数")]
        public long ProjectId { get; set; }
        [Display(Name="邀请参与方ID")]
        //[Required(ErrorMessage = "必须指定{0}。")]
        //[Range(1, int.MaxValue, ErrorMessage = "{0}必须是正整数")]
        public long InviteePartId { get; set; }
        [Display(Name = "项目库中的用户ID")]
        //[Required(ErrorMessage = "必须指定{0}")]
        //[Range(1, int.MaxValue, ErrorMessage = "{0}必须是正整数")]
        public int MFUserId { get; set; }

        public int BidProjId { get; set; }

        public override string ToString()
        {
            return InviteeEmail + " # " + ProjectId + " # " + InviteePartId;
        }

    }

    /// <summary>
    /// 通过用户ID邀请项目成员的模型绑定类
    /// </summary>
    public class ProjectInvitationUserModel
    {
        [Display(Name="被邀请人ID")]
        [Required(ErrorMessage = "必须指定{0}。")]
        [Range(1, int.MaxValue, ErrorMessage = "{0}必须是正整数")]
        public int InviteeId { get; set; }
        [Display(Name = "邀请信息")]
        [MinLength(3, ErrorMessage="{0}不能少于{1}个字符")]
        public string InvitationMessage { get; set; }
        [Display(Name = "项目ID")]
        [Required(ErrorMessage = "必须指定{0}。")]
        [Range(1, int.MaxValue, ErrorMessage = "{0}必须是正整数")]
        public int ProjectId { get; set; }
        [Display(Name = "邀请参与方ID")]
        //[Required(ErrorMessage = "必须指定{0}。")]
        //[Range(1, int.MaxValue, ErrorMessage = "{0}必须是正整数")]
        public int InviteePartId { get; set; }
        [Display(Name = "项目中的UserID")]
        [Required(ErrorMessage = "必须指定{0}")]
        [Range(1, int.MaxValue, ErrorMessage = "{0}必须是正整数")]
        public int MFUserId { get; set; }

    }

    public class ProjectInvitationConfirmEmailModel
    {
        [Display(Name="确认消息")]
        [MinLength(3)]
        public string ConfirmMessage { get; set; }
        [Display(Name = "项目ID")]
        [Required(ErrorMessage = "必须指定{0}。")]
        [Range(1, int.MaxValue, ErrorMessage = "{0}必须是正整数")]
        public long ProjectId { get; set; }
        [Display(Name = "邀请者ID")]
        [Required(ErrorMessage = "必须指定{0}。")]
        [Range(1, int.MaxValue, ErrorMessage = "{0}必须是正整数")]
        public long InviterId { get; set; }
        /// <summary>
        /// 邀请用的Email，可以为空；若为空，认为按照UserId邀请
        /// </summary>
        [Display(Name = "被邀请者的Email")]
        public string InviteeEmail { get; set; }
        [Display(Name = "被邀请加入的参与方")]
        //[Required(ErrorMessage = "必须指定{0}。")]
        //[Range(1, int.MaxValue, ErrorMessage = "{0}必须是正整数")]
        public long InviteePartId { get; set; }
    }

    public class ProjectInvitationConfirmAcceptModel
    {
        [Display(Name = "项目ID")]
        [Required(ErrorMessage = "必须指定{0}。")]
        [Range(1, int.MaxValue, ErrorMessage = "{0}必须是正整数")]
        public int ProjectId { get; set; }
        [Display(Name = "被邀请者ID")]
        [Required(ErrorMessage = "必须指定{0}。")]
        [Range(1, int.MaxValue, ErrorMessage = "{0}必须是正整数")]
        public int InviteeId { get; set; }
        [Display(Name = "被邀请加入的参与方")]
        //[Required(ErrorMessage = "必须指定{0}。")]
        //[Range(1, int.MaxValue, ErrorMessage = "{0}必须是正整数")]
        public int InviteePartId { get; set; }

        [Display(Name = "角色")]
        public string Role { get; set; }

    }
    public class ProjectMemberModel
    {
        [Display(Name = "要删除的用户名")]
        //[Required(ErrorMessage = "必须指定{0}。")]
        public string UserName { get; set; }
        [Display(Name = "要删除的联系人ID")]
        [Required(ErrorMessage = "必须指定{0}。")]
        [Range(1, int.MaxValue, ErrorMessage = "{0}必须是正整数")]
        public int ContactId { get; set; }
        [Display(Name = "项目ID")]
        [Required(ErrorMessage = "必须指定{0}。")]
        [Range(1, int.MaxValue, ErrorMessage = "{0}必须是正整数")]
        public int ProjectId { get; set; }
        /// <summary>
        /// 当前项目库的用户ID，删除发起人
        /// </summary>
        [Display(Name = "当前项目库的用户ID")]
        public int MFUserId { get; set; }
    }

    public class ProjectCreateUserGroupModel
    {
        [Display(Name = "项目ID")]
        [Required(ErrorMessage = "必须指定{0}。")]
        [Range(1, int.MaxValue, ErrorMessage = "{0}必须是正整数")]
        public int ProjectId { get; set; }
        [Display(Name = "用户组名称")]
        [Required(ErrorMessage = "必须指定{0}。")]
        public string GroupName { get; set; }

    }

    public class ProjectRemoveUserGroupModel
    {
        [Display(Name = "项目ID")]
        [Required(ErrorMessage = "必须指定{0}。")]
        [Range(1, int.MaxValue, ErrorMessage = "{0}必须是正整数")]
        public int ProjectId { get; set; }
        [Display(Name = "用户组ID")]
        [Required(ErrorMessage = "必须指定{0}。")]
        [Range(1, int.MaxValue, ErrorMessage = "{0}必须是正整数")]
        public int GroupId { get; set; }
    }

    public class ProjectRemoveUserFromPartyModel
    {
        [Display(Name = "项目ID")]
        [Required(ErrorMessage = "必须指定{0}。")]
        [Range(1, int.MaxValue, ErrorMessage = "{0}必须是正整数")]
        public int ProjectId { get; set; }
        [Display(Name = "参与方ID")]
        [Required(ErrorMessage = "必须指定{0}。")]
        [Range(1, int.MaxValue, ErrorMessage = "{0}必须是正整数")]
        public int PartyId { get; set; }
        [Display(Name = "用户组ID")]
        [Required(ErrorMessage = "必须指定{0}。")]
        [Range(1, int.MaxValue, ErrorMessage = "{0}必须是正整数")]
        public string UserName { get; set; }
        [Display(Name = "项目中的UserID")]
        [Required(ErrorMessage = "必须指定{0}")]
        [Range(1, int.MaxValue, ErrorMessage = "{0}必须是正整数")]
        public int MFUserId { get; set; }
    }

    public class ProjectRemoveUserFromGroupModel
    {
        [Display(Name = "项目ID")]
        [Required(ErrorMessage = "必须指定{0}。")]
        [Range(1, int.MaxValue, ErrorMessage = "{0}必须是正整数")]
        public int ProjectId { get; set; }
        [Display(Name = "用户组ID")]
        [Required(ErrorMessage = "必须指定{0}。")]
        [Range(1, int.MaxValue, ErrorMessage = "{0}必须是正整数")]
        public int GroupId { get; set; }
        [Display(Name="用户")]
        [Required(ErrorMessage = "必须指定{0}。")]
        public string UserName { get; set; }
    }

    public class ProjectAddUserToGroupModel
    {
        [Display(Name = "项目ID")]
        [Required(ErrorMessage = "必须指定项目ID。")]
        [Range(1, int.MaxValue, ErrorMessage = "{0}必须是正整数")]
        public int ProjectId { get; set; }
        [Display(Name = "用户组ID")]
        [Required(ErrorMessage = "必须指定用户组ID。")]
        [Range(1, int.MaxValue, ErrorMessage = "{0}必须是正整数")]
        public int GroupId { get; set; }
        [Required(ErrorMessage = "必须指定用户。")]
        public string UserName { get; set; }
    }
}
