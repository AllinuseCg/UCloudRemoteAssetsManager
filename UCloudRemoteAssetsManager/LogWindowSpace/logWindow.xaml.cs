using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace UCloudRemoteAssetsManager.LogWindowSpace
{
    /// <summary>
    /// log.xaml 的交互逻辑
    /// </summary>
    public partial class logWindow : Window
    {
        public logWindow()
        {
            InitializeComponent();
            logBox.ItemsSource = uilog.getLastLog(50);
            // 绑定log更新
            uilog.onGetNewLog = (newLog) => {
                logBox.ItemsSource = null;
                logBox.ItemsSource = uilog.getLastLog(50);
                onLogChange();
            };
        }


        private void onLogChange()
        {
            scrollView.ScrollToBottom();
        }
    }
}
