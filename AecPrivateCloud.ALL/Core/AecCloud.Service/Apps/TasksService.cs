//using System;
//using System.Collections.Generic;
//using System.Linq;
//using AecCloud.Core;
//using AecCloud.Core.Domain;

//namespace AecCloud.Service.Apps
//{
//    public class TasksService : ITasksService
//    {
//        private readonly IRepository<Tasks> _appRepo;
//        public TasksService(IRepository<Tasks> appRepo)
//        {
//            _appRepo = appRepo;
//        }
//        public void InsertTasks(Tasks tasks)
//        {
//            if (tasks == null) throw new ArgumentNullException("tasks");
//            _appRepo.Insert(tasks);
//        }

//        public IList<Tasks> GetTasksByUser(string userid, string vaultguid)
//        {
//            if (String.IsNullOrWhiteSpace(userid)) throw new ArgumentException("userid");
//            if (String.IsNullOrWhiteSpace(vaultguid)) throw new ArgumentException("vaultguid");
//            return _appRepo.Table.Where(c => c.Userid == userid && c.Vaultguid== vaultguid).ToList();
//        }


//        public void DeleteTasks(Tasks tasks)
//        {
//            if (tasks == null) throw new ArgumentNullException("tasks");
//            _appRepo.Delete(tasks);
//        }
//    }
//}