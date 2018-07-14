using System;
using System.Collections.Generic;
using System.IO;
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
using UCloudRemoteAssetsManager;

namespace UCloudRemoteAssetsManager.BucketDeployWindowSpace
{
    /// <summary>
    /// BucketDeployWindow.xaml 的交互逻辑
    /// </summary>
    public partial class BucketDeployWindow : Window
    {
        BucketData bucketdata;
        public BucketDeployWindow()
        {
            InitializeComponent();
            bucketdata = savedata.getInstance().bucketdata;
            bucketListBox.DataContext = bucketdata.bucketMap.Keys;
        }

        private void bucketListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            object i = bucketListBox.SelectedItem;
            if (i != null)
            {
                var target = bucketdata.bucketMap.getValue(i.ToString());
                textBox_name.Text = target.bucketName;
                textBox_private_key.Text = target.privateKey;
                textBox_public_key.Text = target.publicKey;
                textBox_proxy_host.Text = target.proxyHost;
                textBox_api_host.Text = target.apiHost;
            }
        }

        /// <summary>
        /// 点击确定
        /// </summary>
        private void submit_Click(object sender, RoutedEventArgs e)
        {
            if (bucketListBox.SelectedItem != null)
            {
                var tempBucket = State.currentBucket;
                State.currentBucket = bucketdata.bucketMap.getValue<UcloudAgent.BucketInfo>(bucketListBox.SelectedItem.ToString());
                if (State.mainWindow.refreshProject())
                {
                    this.Close();
                }
                else
                {
                    State.currentBucket = tempBucket;
                }
            }
        }
        /// <summary>
        /// 点击添加
        /// </summary>
        private void addBtn_Click(object sender, RoutedEventArgs e)
        {
            if (bucketdata.bucketMap.ContainsKey("请输入"))
            {
                return;
            }
            bucketdata.bucketMap.Add("请输入", new UcloudAgent.BucketInfo("请输入", "请输入", "请输入"));
            bucketListBox.DataContext = null;
            bucketListBox.DataContext = bucketdata.bucketMap.Keys;
        }
        /// <summary>
        /// 点击删除
        /// </summary>
        private void deleteBtn_Click(object sender, RoutedEventArgs e)
        {
            object i = bucketListBox.SelectedItem;
            if (i != null)
            {
                bucketdata.bucketMap.Remove(i.ToString());
                bucketListBox.DataContext = null;
                bucketListBox.DataContext = bucketdata.bucketMap.Keys;
            }
        }
        /// <summary>
        /// 保存修改
        /// </summary>
        private void saveEditBtn_Click(object sender, RoutedEventArgs e)
        {
            object i = bucketListBox.SelectedItem;
            if (i != null)
            {
                deleteBtn_Click(sender, e);
                bucketdata.bucketMap.Add(textBox_name.Text, new UcloudAgent.BucketInfo(textBox_name.Text, textBox_public_key.Text, textBox_private_key.Text, textBox_proxy_host.Text, textBox_api_host.Text));
                bucketListBox.DataContext = null;
                bucketListBox.DataContext = bucketdata.bucketMap.Keys;
            }
        }
    }
}
