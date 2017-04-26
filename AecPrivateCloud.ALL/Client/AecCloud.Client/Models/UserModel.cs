using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;
using AecCloud.WebAPI.Client;
using AecCloud.WebAPI.Models;
using MFilesAPI;

namespace AecCloud.Client.Models
{
    public class UserModel //: ModelBase
    {
        /// <summary>
        /// 用户id
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 用户昵称
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// 用户头像
        /// </summary>
        public byte[] Image { get; set; }

        /// <summary>
        /// 用户email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// 用户密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 用户是否被选中
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        public string MobilePhoneNum { get; set; }

        /// <summary>
        /// 电话号
        /// </summary>
        public string TelephoneNum { get; set; }

        /// <summary>
        /// QQ号
        /// </summary>
        public string QQNum { get; set; }

        /// <summary>
        /// 公司
        /// </summary>
        public string Company { get; set; }

        /// <summary>
        /// 部门
        /// </summary>
        public string Department { get; set; }

        /// <summary>
        /// 职位
        /// </summary>
        public string Position { get; set; }

        /// <summary>
        /// 身份
        /// </summary>
        public string Identity { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 部分用户信息
        /// </summary>
        public UserDto UserWeb { get; set; }

        /// <summary>
        /// 用户APP
        /// </summary>
        public UserCloudModel UserApp { get; set; }

        public TokenModel Token { get; set; }
    }

    public class UserProfile : ViewModels.ViewModelBase
    {
        private UserDto _user;

        private long _userId;
        public long UserId
        {
            get { return _userId; }
            set
            {
                if (_userId == value) return;
                _userId = value;
                OnPropertyChanged("UserId");
            }
        }
        /// <summary>
        /// 用户账户的网络链接
        /// </summary>
        public string Url { get; set; }


        private string _userName;

        public string UserName
        {
            get { return _userName; }
            set
            {
                if (_userName == value) return;
                _userName = value;
                OnPropertyChanged("UserName");
            }
        }

        private bool _loggedIn;

        public bool Loggedin
        {
            get { return _loggedIn; }
            set
            {
                if (_loggedIn == value) return;
                _loggedIn = value;
                OnPropertyChanged("Loggedin");
            }
        }

        private byte[] ToBytes(BitmapImage image)
        {
            byte[] data;
            var encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));
            using (var ms = new MemoryStream())
            {
                encoder.Save(ms);
                data = ms.ToArray();
            }
            return data;
        }

        private byte[] ToBytes(BitmapFrame image)
        {
            byte[] data;
            var encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(image);
            using (var ms = new MemoryStream())
            {
                encoder.Save(ms);
                data = ms.ToArray();
            }
            return data;
        }

        public UserProfile(UserDto user)
        {
            _user = user;
            Welcome = "欢迎使用DBWorld！\r\n请先登录";
            if (user != null)
            {
                var userName = !String.IsNullOrEmpty(user.FullName) ? user.FullName : user.UserName;
                UserId = user.Id;
                UserName = userName;
                Loggedin = true;
                //Welcome = userName;
                Welcome = "DBWorld工程云V1.0";
            }
        }

        private string _welcome;

        public string Welcome
        {
            get { return _welcome; }
            set
            {
                if (_welcome == value) return;
                _welcome = value;
                OnPropertyChanged("Welcome");
            }
        }

        private byte[] _image;

        public byte[] Image
        {
            get
            {
                if (_image == null)
                {
                    var assemblyName = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location);

                    if (_user == null)
                    {
                        var iconPath = String.Format("pack://application:,,,/{0};Component/Image/Bg/new_default.png",
                            assemblyName);
                        var bitmap1 = BitmapFrame.Create(new Uri(iconPath, UriKind.RelativeOrAbsolute));
                        _image = ToBytes(bitmap1);
                    }
                    else
                    {
                        var iconPath2 = String.Format("pack://application:,,,/{0};Component/Image/Bg/new_default.png", assemblyName);
                        if (_user.Image == null || _user.Image.Length == 0)
                        {
                            var bitmap2 = BitmapFrame.Create(new Uri(iconPath2, UriKind.RelativeOrAbsolute));
                            _image = ToBytes(bitmap2);
                        }
                        else
                        {
                            _image = _user.Image;
                        }
                    }
                }
                return _image;
            }
            set
            {
                if (_image == value) return;
                _image = value;
                OnPropertyChanged("Image");
            }
        }
    }
}
