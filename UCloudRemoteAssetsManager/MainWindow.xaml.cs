using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using UcloudAgent;

namespace UCloudRemoteAssetsManager
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        BucketData bucketData;
        FolderTree cloudFileTree;
        FolderTree localFileTree;
        public MainWindow()
        {
            if (!File.Exists(UAgent.workPath + "\\filemgr-win64.exe") )
            {
                MessageBox.Show("未找到 filemgr-win64.exe \n将文件放到 " + UAgent.workPath + " 目录下");
                Application.Current.Shutdown();
            }
            cloudFileTree = new FolderTree();
            localFileTree = new FolderTree();
            localFileTree.root = new FolderTreeNode("addRoot");
            InitializeComponent();
            // 绑定log更新
            uilog.onGetNewLog = (newLog) => {
                //logBox.Text = newLog;
                logBtn.Header = newLog;
            };
            //读档
            bucketData = savedata.getInstance().bucketdata;
            //refreshProject();

            //退出时存档
            Application.Current.Exit += (object sender, ExitEventArgs e) => { savedata.getInstance().bucketdata.serialization(); };
            //保存实例给state
            State.mainWindow = this;
        }
        
        public bool refreshProject()
        {
            if (State.currentBucket == null)
            {
                return false;
            }
            uilog.log("尝试载入bucket : " + State.currentBucket.bucketName);
            if (refreshCloudTreeView())
            {
                Title = "UCloudUploadToolForCcc -" + State.currentBucket.bucketName;
                uilog.log("载入bucket成功 : " + State.currentBucket.bucketName);
                localTreeView_cleanBtn_Click(null, null); // 清空本地树
                return true;
            }
            uilog.log("载入bucket失败 : " + State.currentBucket.bucketName);
            return false;
        }
        bool refreshCloudTreeView()
        {
            //string receiveFileList = UAgent.exec(cmdPreset.getFileList());
            string receiveFileList = UAgent.exeGetFileList();
            var ufileInfoList = UFileInfo.importByReceiveString(receiveFileList);
            if (ufileInfoList == null)
            {
                uilog.error("不存在该bucket");
                return false;
            }
            cloudFileTree = new FolderTree(ufileInfoList);
            cloudTreeView.ItemsSource = null;
            cloudTreeView.ItemsSource = cloudFileTree.root.children;
            return true;
        }
        /// <summary>
        /// 重置所有信息
        /// </summary>
        void reset()
        {
            uilog.log("重置数据");
            State.currentBucket = null;
            localTreeView_cleanBtn_Click(null, null); // 清空本地树
            cleanCloudTreeView();   // 清空线上树（表面上）
            Title = "UCloudUploadToolForCcc";
        }
        #region bottomMenu
        private void logBtn_Click(object sender, RoutedEventArgs e)
        {
            LogWindowSpace.logWindow logWindow = new LogWindowSpace.logWindow();
            logWindow.Show();
        }
        #endregion

        #region titleMenu
        private void testBtn_Click(object sender, RoutedEventArgs e)
        {
            //UAgent.exec("--action getfilelist --bucket wp-wx");
            //var res = UAgent.exec(cmdPreset.getFileList());
            cloudFileTree.root.isChecked = true;
        }

        private void deployBucketBtn_Click(object sender, RoutedEventArgs e)
        {
            BucketDeployWindowSpace.BucketDeployWindow bucketDeployWindow = new BucketDeployWindowSpace.BucketDeployWindow();
            bucketDeployWindow.ShowDialog();
        }
        private void aboutBtn_Click(object sender, RoutedEventArgs e)
        {
            AboutWindowSpace.AboutWindow aboutWindow = new AboutWindowSpace.AboutWindow();
            aboutWindow.ShowDialog();
        }
        #endregion

        #region localTree
        /// <summary>
        /// 将文件或文件夹拖入以添加文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void localTreeView_Drop(object sender, DragEventArgs e)
        {
            if (State.currentBucket == null) return;
            Array paths = null;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                paths = ((System.Array)e.Data.GetData(DataFormats.FileDrop));
            }
            if (paths != null)
            {
                List<string> files = new List<string>();
                var parentPath = Directory.GetParent(paths.GetValue(0).ToString()).ToString();
                foreach (string s in paths)
                {
                    if (Directory.Exists(s))
                    {
                        files.AddRange(Directory.GetFiles(s, "*", SearchOption.AllDirectories));
                    }
                    else
                    {
                        files.Add(s);
                    }
                }
                var tFileTree = new FolderTree(files, parentPath);
                localFileTree.root.children.AddRange(tFileTree.root.children);
                foreach (var i in tFileTree.root.children)
                {
                    i.parent = localFileTree.root;
                }
                var l = new List<FolderTreeNode>();
                foreach (var i in localFileTree.root.getAllChildren(ref l, true, true))
                {
                    i.tree = localFileTree;
                    i.root = localFileTree.root;
                }
                localTreeView.ItemsSource = null;
                localTreeView.ItemsSource = localFileTree.root.children;
            }
        }
        /// <summary>
        /// 上传
        /// </summary>
        private void uploadBtn_Click(object sender, RoutedEventArgs e)
        {
            if (State.currentBucket == null) return;
            var l = new List<FolderTreeNode>();
            foreach (var i in localFileTree.root.getAllChildren(ref l))
            {
                if (i == null) continue;
                if (!i.isFile) continue;
                uilog.log("上传文件 : " + i.path);
                var res = UAgent.exec(cmdPreset.uploadFile(i.path, i.fallPath));
                Console.WriteLine(res);
            }
            refreshCloudTreeView();
        }
        /// <summary>
        /// 删除
        /// </summary>
        private void localTreeView_deleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (State.currentBucket == null) return;
            var l = new List<FolderTreeNode>();
            localFileTree.root.getAllChildren(ref l, true);
            foreach (var i in l)
            {
                if (i == null) continue;        // 应该不存在
                if (i.parent==null) continue;   // 自己是root
                if (i.isChecked == true)        // 过滤中间态的文件夹
                {
                    if (i.parent != null)
                    {
                        i.parent.children.Remove(i);
                        i.parent = null;
                    }
                    i.children.RemoveAll((p) => { return true; });
                }
            }
            localTreeView.ItemsSource = null;
            localTreeView.ItemsSource = localFileTree.root.children;
            localFileTree.root.freshCheckState();
        }
        /// <summary>
        /// 清空
        /// </summary>
        private void localTreeView_cleanBtn_Click(object sender, RoutedEventArgs e)
        {
            if (State.currentBucket == null) return;
            var l = new List<FolderTreeNode>();
            localFileTree.root.getAllChildren(ref l, true, true);   // 和删除的唯一不同是包括了未选中的文件
            foreach (var i in l)
            {
                if (i == null) continue;        // 应该不存在
                if (i.parent == null) continue;   // 自己是root
                if (i.parent != null)
                {
                    i.parent.children.Remove(i);
                    i.parent = null;
                }
                i.children.RemoveAll((p) => { return true; });
            }
            localTreeView.ItemsSource = null;
            localTreeView.ItemsSource = localFileTree.root.children;
            localFileTree.root.freshCheckState();
        }
        /// <summary>
        /// 全选
        /// </summary>
        private void localTreeView_selectAllBtn_Click(object sender, RoutedEventArgs e)
        {
            if (State.currentBucket == null) return;
            localFileTree.root.isChecked = true;
        }
        /// <summary>
        /// 全不选
        /// </summary>
        private void localTreeView_UnselectAllBtn_Click(object sender, RoutedEventArgs e)
        {
            if (State.currentBucket == null) return;
            localFileTree.root.isChecked = false;
        }
        /// <summary>
        /// 反选
        /// </summary>
        private void localTreeView_InverseSelectBtn_Click(object sender, RoutedEventArgs e)
        {
            if (State.currentBucket == null) return;
            var l = new List<FolderTreeNode>();
            localFileTree.root.getAllChildren(ref l, false, true);
            foreach (var i in l)
            {
                i.isChecked = !i.isChecked;
            }
            localFileTree.root.freshCheckState();
        }

        #endregion

        #region cloudTree
        /// <summary>
        /// 删除
        /// </summary>
        private void cloudTreeView_deleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (State.currentBucket == null) return;
            var l = new List<FolderTreeNode>();
            cloudFileTree.root.getAllChildren(ref l);
            foreach (var i in l)
            {
                if (i == null) continue;
                if (!i.isFile) continue;
                uilog.log("删除文件 : " + i.path);
                var res = UAgent.exec(cmdPreset.deleteFile(i.path));
                Console.WriteLine(res);
            }
            refreshCloudTreeView();
        }

        /// <summary>
        /// 全选
        /// </summary>
        private void cloudTreeView_selectAllBtn_Click(object sender, RoutedEventArgs e)
        {
            if (State.currentBucket == null) return;
            cloudFileTree.root.isChecked = true;
        }
        /// <summary>
        /// 全不选
        /// </summary>
        private void cloudTreeView_UnselectAllBtn_Click(object sender, RoutedEventArgs e)
        {
            if (State.currentBucket == null) return;
            cloudFileTree.root.isChecked = false;
        }
        /// <summary>
        /// 反选
        /// </summary>
        private void cloudTreeView_InverseSelectBtn_Click(object sender, RoutedEventArgs e)
        {
            if (State.currentBucket == null) return;
            var l = new List<FolderTreeNode>();
            cloudFileTree.root.getAllChildren(ref l, false, true);
            foreach (var i in l)
            {
                i.isChecked = !i.isChecked;
            }
            cloudFileTree.root.freshCheckState();
        }

        private void cleanCloudTreeView()
        {
            if (State.currentBucket == null) return;
            var l = new List<FolderTreeNode>();
            cloudFileTree.root.getAllChildren(ref l, true, true);   // 和删除的唯一不同是包括了未选中的文件
            foreach (var i in l)
            {
                if (i == null) continue;        // 应该不存在
                if (i.parent == null) continue;   // 自己是root
                if (i.parent != null)
                {
                    i.parent.children.Remove(i);
                    i.parent = null;
                }
                i.children.RemoveAll((p) => { return true; });
            }
            cloudTreeView.ItemsSource = null;
        }
        #endregion

        
    }
}
