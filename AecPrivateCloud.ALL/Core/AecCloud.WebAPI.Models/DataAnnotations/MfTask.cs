﻿using System.Collections.Generic;

namespace AecCloud.WebAPI.Models.DataAnnotations
{
    public class MfilesClientConfig
    {
        public string Guid { set; get; }
        public string Host { set; get; }
        public string Name { set; get; }
    }
    //public enum NotificationTypeEnum
    //{
    //    NewTask = 1, WorkFlowAssigned = 2, Other = 3, NewDoc = 4, UpdateDoc = 5, DelDoc = 6, NewOtherObj = 7, UpdateOtherObj = 8, DelOtherObj = 9, UpdateTask = 10, TaskDone = 11
    //}
    //public class MfTask
    //{
    //    public MfTask()
    //    {
    //        ClientType = 0;//0:mfiles事件请求,1:客户端请求

    //        Id = -1;
    //        Type = -1;
    //        Version = -1;

    //        ClientName = string.Empty;
    //        VaultGuid = string.Empty;
    //        UserId = string.Empty;
    //        IsDeleted = false;
    //        Name = string.Empty;
    //        Desc = string.Empty;
    //        Time = string.Empty;
    //        LastModifiedTime = string.Empty;
    //        NotificationType = NotificationTypeEnum.Other;

    //        Date = string.Empty;
    //        Monitor = string.Empty;
    //        Createby = string.Empty;
    //        Url = string.Empty;
    //        IsNoticed = 0;//0：未通知，1：已通知
    //    }
    //    public int ClientType;
    //    public string ClientName;
    //    public string VaultGuid;
    //    public NotificationTypeEnum NotificationType;
    //    public List<string> UserIds = new List<string>();
    //    public int Id;
    //    public int Type;
    //    public int Version;

    //    public string UserId;
    //    public bool IsDeleted;
    //    public string Name;
    //    public string Desc;
    //    public string Time;
    //    public string LastModifiedTime;

    //    public string Date;
    //    public string Monitor;
    //    public string Createby;
    //    public string Url;
    //    public int IsNoticed;
    //}
}