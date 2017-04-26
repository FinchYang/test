using System.Xml.Serialization;

namespace DBWorld.Config.Models
{
    [XmlRootAttribute("VaultConfig")]
    public class VaultConfigModel
    {
        /// <summary>
        /// 用户id
        /// </summary>
        [XmlElementAttribute("UserId", IsNullable = false)] 
        public long UserId { get; set; }

        [XmlArrayAttribute("VaultsName")]
        public string[] VaultsName { get; set; }
    }
}
