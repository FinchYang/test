using System;

namespace AecCloud.Client.Models
{
    public class DiskCloudModel : ModelBase
    {

        private string _searchString;
        private Uri _webBrowserSource;

        /// <summary>
        /// 搜索的关键字
        /// </summary>
        public string SearchString
        {
            get { return _searchString; }
            set { SetProperty(ref _searchString, value); }
        }

        /// <summary>
        /// WebBrowser的数据源
        /// </summary>
        public Uri WebBrowserSoure
        {
            get { return _webBrowserSource; }
            set { SetProperty(ref _webBrowserSource, value); }
        }
    }
}
