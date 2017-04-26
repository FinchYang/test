using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace DBWorld.DesignCloud.Models
{
    public class ProjectModel : ModelBase, ICloneable
    {
        public const string ProjStatusSetUped = "立项";
        public const string ProjStatusStarted = "启动";
        public const string ProjStatusPaused = "暂停";
        public const string ProjStatusOvered = "结束";
        public const string ProjStatusArchived = "归档";

        /// <summary>
        /// 项目ID
        /// </summary>
        public long ProjId { get; set; }

        /// <summary>
        /// 项目名称
        /// </summary>
        public string ProjName { get; set; }

        /// <summary>
        /// 项目编号
        /// </summary>
        public string ProjNumber { get; set; }

        /// <summary>
        /// 项目描述
        /// </summary>
        public string ProjDescription { get; set; }

        ///// <summary>
        ///// 项目模板名称
        ///// </summary>
        //public TemplateModel ProjTemplate { get; set; }

        public long ProjTemplateId { get; set; }

        /// <summary>
        /// 项目封面
        /// </summary>
        public byte[] ProjCover { get; set; }

        /// <summary>
        /// 是否为默认项目
        /// </summary>
        public bool IsDefault { get; set; }
 
        /// <summary>
        /// 项目进度
        /// </summary>
        public double ProjProgress { get; set; }

        /// <summary>
        /// 项目状态
        /// </summary>
        private string _projStatus;
        public string ProjStatus
        {
            get { return _projStatus; }
            set
            {
                _projStatus = value;
                OnPropertyChanged("ProjStatus");
            }
        }

        ///// <summary>
        ///// 项目团队
        ///// </summary>
        //public List<UserModel> ProjMembers { get; set; }

        /// <summary>
        /// 开始日期
        /// </summary>
        public DateTime ProjStartTime { get; set; }

        /// <summary>
        /// 截止日期
        /// </summary>
        public DateTime ProjEndTime { get; set; }

        /// <summary>
        /// 参入方ID
        /// </summary>
        public long PartyId { get; set; }

        public long CompanyId { get; set; }

        public long AreaId { get; set; }

        public string ProjectClass { get; set; }

        public long LevelId { get; set; }

        /// <summary>
        /// 建设单位
        /// </summary>
        public string OwnerUnit { get; set; }

        /// <summary>
        /// 设计单位
        /// </summary>
        public string DesignUnit { get; set; }
        
        /// <summary>
        /// 施工单位
        /// </summary>
        public string ConstructionUnit { get; set; }
        
        /// <summary>
        /// 监理单位
        /// </summary>
        public string SupervisionUnit { get; set; }

        /// <summary>
        /// 建设规模
        /// </summary>
        public string ConstructScale { get; set; }
        /// <summary>
        /// 合同金额
        /// </summary>
        public string ContractAmount { get; set; }


        public override string ToString()
        {
            return ProjName;
        }

        /// <summary>
        /// 克隆
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            var obj = MemberwiseClone() as ProjectModel;
            //if (obj != null)
            //{
            //    //obj.ProjTemplate = ProjTemplate.Clone() as TemplateModel;
            //    obj.ProjMembers = new List<UserModel>();
            //    if (ProjMembers != null)
            //    {
            //        foreach (var member in ProjMembers)
            //        {
            //            obj.ProjMembers.Add(member.Clone() as UserModel);
            //        }
            //    }
            //}
            
            return obj;
        }
    }
}
