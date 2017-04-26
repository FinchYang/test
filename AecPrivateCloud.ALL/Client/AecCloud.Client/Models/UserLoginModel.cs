namespace AecCloud.Client.Models
{
    public  class UserLoginModel : ModelBase
    {
        /// <summary>
        /// 是否显示登录结果
        /// </summary>
        private bool _isShowDescription;
        public bool IsShowDescription
        {
            get { return _isShowDescription; }
            set { this.SetProperty(ref this._isShowDescription, value); }
        }

        /// <summary>
        /// 登录结果描述
        /// </summary>
        private string _loginDescription;
        public string LoginDescription
        {
            get { return _loginDescription; }
            set { this.SetProperty(ref this._loginDescription, value); }
        }
    }
}
