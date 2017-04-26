using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using AecCloud.WebAPI.Models;
using AecCloud.WebAPI.Models.DataAnnotations;
using log4net;
using MfNotification.Core.NotifyObject;
using Microsoft.AspNet.SignalR;

namespace DBWorld.AecCloud.Web.hubs
{
    public class CscecPushHub : Hub
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public class SignalrClient
        {
            public string UserName { set; get; }
              public string ConnectId { set; get; }
        }
        private static List<SignalrClient> _scList=new List<SignalrClient>();
        public void Login (string username,string cid)
        {
            var found = false;
            foreach (SignalrClient signalrClient in _scList)
            {
                if (cid == signalrClient.ConnectId)
                {
                    found = true;
                }
            }
            if (!found)
            {
                _scList.Add(new SignalrClient{ConnectId=cid,UserName=username});
                Log.Info(string.Format("CscecPushHub {0},{1},{2}  Login,Context.ConnectionId={3}", username, cid, _scList.Count, Context.ConnectionId));
            }
        }
        public void PushMsg(MfTask mt)
        {
            Log.Info(string.Format("msg {0},总用户列表用户数 {1},本次通知用户数{2}", mt.Name, _scList.Count,mt.UserNameLists.Count));
            foreach (string userName in mt.UserNameLists)
            {
                foreach (SignalrClient signalrClient in _scList)
                {
                    if (signalrClient.UserName == userName)
                    {
                        Log.Info(string.Format("befor send {0},user , {1},{2}", mt.Name, userName, signalrClient.ConnectId));
                        Clients.Client(signalrClient.ConnectId).NewMsg(mt);
                      //  break;
                    }
                }
            }
        }
        public void CheckMfilesConnect(string username)
        {
            Log.Info(string.Format("CheckMfilesConnect,username= {0}, {1},={2},", username,  Context.ConnectionId, _scList.Count));
            var found = false;
            foreach (SignalrClient signalrClient in _scList)
            {
                if (signalrClient.UserName == username)
                {
                    //Log.Info(string.Format("CheckMfilesConnect {0},user , {1},", username, signalrClient.ConnectId));
                    //Clients.Client(signalrClient.ConnectId).CheckMfilesConnect(guid);
                    found = true;
                    break;
                }
            }
            //if (!found)
            //{
            Clients.Client(Context.ConnectionId).haha(found.ToString(), "请运行通知中心Notice，待Notice自动创建项目后进入项目库");
                //Log.Info(string.Format("111 {0},guid = {1},cid={2},", username, guid, Context.ConnectionId));
              //  Clients.Caller.haha("false", "Caller");
                //Log.Info(string.Format("222 {0},guid = {1},cid={2},", username, guid, Context.ConnectionId));
                //Clients.All.haha("true", " Clients.All请运行通知中心Notice，待Notice自动创建项目后进入项目库");
                //Log.Info(string.Format("CheckMfilesConnect,after notice client username= {0},guid = {1},cid={2},", username, guid, Context.ConnectionId));
             //   Clients.Caller.warning("true", "Caller warning");
                //Log.Info(string.Format("333 {0},guid = {1},cid={2},", username, guid, Context.ConnectionId));
                //Clients.All.warning("true", "All warning");
                //Log.Info(string.Format("444 {0},guid = {1},cid={2},", username, guid, Context.ConnectionId));
             //   Clients.Client(Context.ConnectionId).warning("false", "ConnectionId warning");
                //Log.Info(string.Format("555 {0},guid = {1},cid={2},", username, guid, Context.ConnectionId));
          //  }
        }
        public void PushNewApp(VaultAppModel mcc)
        {
            Clients.All.NewApp(mcc);
        }
        public void PushNewProject(MfilesClientConfig mcc)
        {
            Clients.All.NewProject(mcc);
        }
        public void PushNoticeUpdatePackage(UpdateInfo updateInfo)
        {
            Clients.All.NoticeUpdate(updateInfo);
        }
    }
}