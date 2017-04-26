
using System.Windows.Input;

namespace DBWorld.DesignCloud.Views
{
    /// <summary>
    /// ProjectSettingView.xaml 的交互逻辑
    /// </summary>
    public partial class ProjectSettingViewForAllBackup 
    {
        public ProjectSettingViewForAllBackup()
        {
            InitializeComponent();
        }

        private void TitleDock_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
