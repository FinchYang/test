

using System.Collections.Generic;
using MFilesAPI;

namespace DBWorld.AecCloud.Web.Models
{
    public class MfilesResource
    {
        public Vault Vault { get; set; }
        public int Muserid { get; set; }
        public MFilesServerApplication MFilesServerApplication { get; set; }
    }
    public class ErpPmUser
    {
        public ErpPmUser()
        {
            Selected = false;
        }
        public string UserName { get; set; }
        public string Fullname { get; set; }
        public long Id { get; set; }
        public bool Selected { get; set; }
    }
    public class CompanyManager
    {
        public CompanyManager()
        {
            UserGroups = new List<UserGroupDb>();
        }
        public string Name { get; set; }
        public string Code { get; set; }
        public long Id { get; set; }
        public List<UserGroupDb> UserGroups { get; set; }
    }
    public class UserGroupDb
    {
        public UserGroupDb()
        {
            Dbusers = new List<Dbuser>();
        }
        public long Id { get; set; }
        public long CompanyId { get; set; }
        public long UserId { get; set; }
        public long GroupId { get; set; }
        public string GroupName { get; set; }
        public List<Dbuser> Dbusers { get; set; }
    }
    public class Dbuser
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }
    public class Secureclass
    {
        public string Name { get; set; }
        public string Alias { get; set; }
        public int Classid { get; set; }
    }
    public class ChartData
    {
        public string Name { get; set; }
        public int Number { get; set; }
        public int QualifiedNumber { get; set; }
    }
    public class TaskOrNoticeNew
    {
        public string ProjectName{ get; set; }
        public string Name{ get; set; }
        public string Url{ get; set; }
        public string Date{ get; set; }
        public string Content{ get; set; }
        public string Assigner{ get; set; }
    }
    public class SeclectItem
    {
        public int value { set; get; }
        public int text { set; get; }
    }
    public class TimeLimitWarningModel
    {
        public TimeLimitWarningModel()
        {
            OwnerName = string.Empty;
            OwnerContact = string.Empty;
            TimeLimitStatus = string.Empty;
            Name = string.Empty;
            Url = string.Empty;
            Id = string.Empty;
        }
       
        public string Name { get; set; }
        public string Url { get; set; }
        public string Id { get; set; }
        public string OwnerName { get; set; }
        public string OwnerContact { get; set; }
        public string TimeLimitStatus { get; set; }
        public int TimeLimitStatusId { get; set; }
    }
    public class CostWarningModel
    {
        public CostWarningModel()
        {
            OwnerName = string.Empty;
            OwnerContact = string.Empty;
            ActualCost = string.Empty;
            Name = string.Empty;
            Url = string.Empty;
            Id = string.Empty;
            Deviation = string.Empty;
            Cost = string.Empty;
            PlanCost = string.Empty;
        }

        public string Name { get; set; }
        public string Url { get; set; }
        public string Id { get; set; }
        public string OwnerName { get; set; }
        public string OwnerContact { get; set; }
        public string Cost { get; set; }
        public string PlanCost { get; set; }
        public string ActualCost { get; set; }
        public string Deviation { get; set; }
        public long CostId { get; set; }
    }
    public class ThreeControls
    {
        public ThreeControls()
        {
            Company = string.Empty;
            Fundamental = string.Empty;
            PrincipalPart = string.Empty;
            Finish = string.Empty;
            FinishDoubleWarning = true;
            PrincipalPartDoubleWarning = true;
            FundamentalDoubleWarning = true;
        }
        public int Serial { get; set; }
        public string Name { get; set; }
        public string Company { get; set; }
        public string Manager { get; set; }
        public string Fundamental { get; set; }
        public string PrincipalPart { get; set; }
        public string Finish { get; set; }
        public bool FinishDoubleWarning { get; set; }
        public bool FundamentalDoubleWarning { get; set; }
        public bool PrincipalPartDoubleWarning { get; set; }
    }
    public class SecureIssue
    {
        public string ProjectName{ get; set; }
        public string Name{ get; set; }
        public string Type{ get; set; }
        public string Person{ get; set; }
        public string Time{ get; set; }
        public string Measure{ get; set; }
        public string url{ get; set; }
    }
    public class Contrator
    {
        public string Name{ get; set; }
        public string Url{ get; set; }
        public string PropContractedProfession{ get; set; }
        public string PropBusinessLicenseNumber{ get; set; }
        public string PropTaxRegistrationNumber{ get; set; }
        public string PropQualificationCertificateNumber{ get; set; }
        public string PropLevelOfQualification{ get; set; } public string PropSafetyProductionLicenseNumber{ get; set; }
        public string PropRegisteredCapital{ get; set; }
        public string PropTelephoneAndFaxOfLegalRepresentative{ get; set; }
        public string PropDetailedAddress{ get; set; } public string PropDeputiesAndTelephones{ get; set; }
        public string PropIsQualified{ get; set; }
        public string PropLevel{ get; set; }
        public string PropComment{ get; set; }
    }
}