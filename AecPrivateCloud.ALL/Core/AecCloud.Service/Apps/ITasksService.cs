using System.Collections.Generic;
using AecCloud.Core.Domain;

namespace AecCloud.Service.Apps
{
    public interface ITasksService
    {
        void InsertTasks(Tasks app);
        IList<Tasks> GetTasksByUser(string userid, string vaultguid); 
        void DeleteTasks(Tasks app); 
    }
}