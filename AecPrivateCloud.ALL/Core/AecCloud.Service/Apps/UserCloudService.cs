//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using AecCloud.Core;
//using AecCloud.Core.Domain;
//using AecCloud.Core.Domain.Vaults;
//using AecCloud.Service.Users;

//namespace AecCloud.Service.Apps
//{
//    public class UserCloudService : IUserCloudService
//    {

//        //private readonly IRepository<UserCloud> _appRepo;
//        private readonly ICloudService _appService;
//        private readonly IUserService _userService;

//        public UserCloudService(ICloudService appService, IUserService userService) //IRepository<UserCloud> appRepo, 
//        {
//            //_appRepo = appRepo;
//            _appService = appService;
//            _userService = userService;
//        }
//        //public IList<UserCloud> GetCloudsByUserId(long userId)
//        //{
//        //    if (userId <= 0) throw new ArgumentException("userId");
//        //    return _appRepo.Table.Where(c => c.UserId == userId).ToList();
//        //}

//        public void AddCloudToUser(long userId, long appId)
//        {
//            if (userId <= 0) throw new ArgumentException("userId");
//            if (appId <= 0) throw new ArgumentException("appId");
//            var ua = _appRepo.Table.FirstOrDefault(c => c.UserId == userId && c.CloudId == appId);
//            if (ua == null)
//            {
//                ua = new UserCloud { UserId = userId, CloudId = appId };
//                _appRepo.Insert(ua);
//            }
//        }


//        public void RemoveCloudFromUser(long userId, long appId)
//        {
//            if (userId <= 0) throw new ArgumentException("userId");
//            if (appId <= 0) throw new ArgumentException("appId");
//            var ua = _appRepo.Table.FirstOrDefault(c => c.UserId == userId && c.CloudId == appId);
//            if (ua != null)
//            {
//                _appRepo.Delete(ua);
//            }
//        }

//        public bool UserHasCloud(long userId, long appId)
//        {
//            if (userId <= 0) throw new ArgumentException("userId");
//            if (appId <= 0) throw new ArgumentException("appId");
//            return _appRepo.Table.Any(c => c.UserId == userId && c.CloudId == appId);
//        }



//        public void AddDefaultClouds(long userId)
//        {
//            var apps = _appService.GetDefaultClouds();
//            foreach (var a in apps)
//            {
//                AddCloudToUser(userId, a.Id);
//            }
//        }



//        public IList<User> GetUsersInCloud(long appId)
//        {
//            var uas = _appRepo.Table.Where(c => c.CloudId == appId);
//            return uas.Select(c => _userService.GetUserById(c.UserId)).ToList();
//        }
//    }
//}
