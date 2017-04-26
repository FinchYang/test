
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Documents;

namespace DBWorld.DesignCloud.Models
{
    public class UserGroupModel : ModelBase
    {
        /// <summary>
        /// 组id
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 组名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 组的别名
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// 组成员
        /// </summary>
        public ObservableCollection<UserModel> Members { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
