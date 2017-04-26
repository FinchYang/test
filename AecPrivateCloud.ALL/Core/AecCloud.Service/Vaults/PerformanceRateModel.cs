using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AecCloud.Service.Vaults
{
    public class PerformanceRateModel
    {
        /// <summary>
        /// 所属单位
        /// </summary>
        public string UnitName { set; get; }
        /// <summary>
        /// 公建项目履约率
        /// </summary>
        public float ComFacilityRate { set; get; }
        /// <summary>
        /// 公建项目履约率条数
        /// </summary>
        public int ComFacilityNum { set; get; }
        /// <summary>
        /// 房地产项目履约率
        /// </summary>
        public float RealEstateRate { set; get; }
        /// <summary>
        /// 房地产项目履约率条数
        /// </summary>
        public int RealEstateNum { set; get; }
        /// <summary>
        /// 基础设施履约率
        /// </summary>
        public float InfrastructureRate { set; get; }
        /// <summary>
        /// 基础设施履约率条数
        /// </summary>
        public int InfrastructureNum { set; get; }
        /// <summary>
        /// 融资类项目履约率
        /// </summary>
        public float FinancingRate { set; get; }
        /// <summary>
        /// 融资类项目履约率条数
        /// </summary>
        public int FinancingNum { set; get; }
        /// <summary>
        /// 工期变更大的项目履约率
        /// </summary>
        public float ChangedProjRate { set; get; }
        /// <summary>
        /// 工期变更的履约率条数
        /// </summary>
        public int ChangedProjNum { set; get; }
        /// <summary>
        /// 公司工期履约率
        /// </summary>
        public float UintRate { set; get; }
        /// <summary>
        /// 公司履约率条数
        /// </summary>
        public int UnitRateNum { set; get; }

        public int year { set; get; }
        public int month { set; get; }
    }

    /// <summary>
    /// 单位按类别统计履约率
    /// </summary>
    public class UnitPerformaceModel
    {
        /// <summary>
        /// 序号
        /// </summary>
        public int Sn { set; get; }
        /// <summary>
        /// 项目类别
        /// </summary>
        public string ProjClass { set; get; }
        /// <summary>
        /// 项目信息列表
        /// </summary>
        public List<ProjPerformInfo> ProjPerformInfos { set; get; }
        /// <summary>
        /// 履约率
        /// </summary>
        public double PerformRate { set; get; }
    }
    /// <summary>
    /// 项目履约率情况
    /// </summary>
    public class ProjPerformInfo
    {
        /// <summary>
        /// 序号
        /// </summary>
        public int Sn { set; get; }
        /// <summary>
        /// 项目名称
        /// </summary>
        public string ProjName { set; get; }
        /// <summary>
        /// 项目额
        /// </summary>
        public string ProjValue { set; get; }
        /// <summary>
        /// 开工日期
        /// </summary>
        public string StartDate { set; get; }
        /// <summary>
        /// 竣工日期
        /// </summary>
        public string CompleteDate { set; get; }
        /// <summary>
        /// 变更后的竣工日期
        /// </summary>
        public string ChangedCompeteDate { set; get; }
        /// <summary>
        /// 项目工期
        /// </summary>
        public string ProjPeroid { set; get; }
        /// <summary>
        /// 实际延期天数
        /// </summary>
        public string RealDelayTime { set; get; }
        /// <summary>
        /// 业主已经确认的延期天数
        /// </summary>
        public string ComfirmDelayTime { set; get; }
        /// <summary>
        /// 未获业主确认的延期天数
        /// </summary>
        public string UncomfirmDelayTime { set; get; }
        /// <summary>
        /// 项目工期总成本
        /// </summary>
        public string TotalProjValue { set; get; }
        /// <summary>
        /// 确认的补偿金额
        /// </summary>
        public string ComfirmCompenValue { set; get; }
        /// <summary>
        /// 确认的补偿说明
        /// </summary>
        public string ComfirmCompenExplain { set; get; }
        /// <summary>
        /// 是否预警
        /// </summary>
        public string IsWarning { set; get; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { set; get; }
        /// <summary>
        /// 履约率
        /// </summary>
        public double PerformRate { set; get; }
    }
}
