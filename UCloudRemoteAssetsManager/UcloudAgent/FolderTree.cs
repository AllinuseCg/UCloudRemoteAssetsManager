using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UcloudAgent
{
    class FolderTree
    {
        public FolderTreeNode root;
        public FolderTree() { }
        public FolderTree(List<UFileInfo> uData) { initWithUcloudData(uData); }
        
        public void initWithUcloudData(List<UFileInfo> uData)
        {
            root = new FolderTreeNode("root");
            foreach (UFileInfo i in uData)
            {
                var f = new FolderTreeFile() { tree = this, root = root, fallPath = null };
                f.ufileInfo = i;
                f.Key = i.Key;
            }
            root.isChecked = false;
        }

        public FolderTree(List<string> paths, string parentPath) { initWithFileArray(paths, parentPath); }

        public void initWithFileArray(List<string> paths, string parentPath)
        {
            root = new FolderTreeNode("root");
            foreach (string i in paths)
            {
                var f = new FolderTreeFile() { tree = this, root = root, fallPath = i };
                f.ufileInfo = null;
                f.Key = i.Substring(parentPath.Length);
            }
            root.isChecked = true;
        }
    }

    /// <summary>
    /// 路径节点
    /// </summary>
    class FolderTreeNode : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public FolderTreeNode(string _field) { field = _field; children = new List<FolderTreeNode>(); }
        public FolderTreeNode() { children = new List<FolderTreeNode>(); }

        public virtual bool isFile { get { return false; } }
        public FolderTreeNode self { get { return this; } }
        /// <summary>
        /// 所属的tree
        /// </summary>
        public FolderTree tree;
        public FolderTreeNode root;
        public FolderTreeNode parent { get; set; }
        public List<FolderTreeNode> children { get; set; }

        public bool? _isCheckde = true;
        /// <summary>
        /// 让 freshCheckState() 只执行一次
        /// </summary>
        static int checkDepth = 0;
        /// <summary>
        /// 用于有复选框的TreeView，实现INotifyPropertyChanged接口也是为复选框服务的
        /// </summary>
        public bool? isChecked
        {
            get { return _isCheckde; }
            set
            {
                bool? newValue = value;
                switch (value)
                {
                    case true:
                        newValue = true;
                        break;
                    case false:
                        newValue = false;
                        break;
                    case null:
                        newValue = false;
                        break;
                }
                checkDepth++;
                foreach (var i in children)
                {
                    i.isChecked = newValue;
                }
                _isCheckde = newValue;
                if (checkDepth == 1)
                {
                    if (root != null)
                    {
                        root.freshCheckState();
                    }
                    else
                    {
                        freshCheckState();
                    }
                }
                checkDepth--;
            }
            
        }
        /// <summary>
        /// 刷新所有的_isCheckde状态
        /// </summary>
        public bool? freshCheckState()
        {
            if (children.Count == 0)
            {
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("IsChecked"));
                }
                return _isCheckde;
            }
            int trueCount = 0;
            int falseCount = 0;
            for (int j = 0; j < children.Count; j++)
            {
                switch (children[j].freshCheckState())
                {
                    case true:
                        trueCount++;
                        break;
                    case false:
                        falseCount++;
                        break;
                    case null:
                        return null;
                }
            }
            bool? resault = null;
            if (trueCount == 0 && falseCount != 0) resault = false;
            else if (trueCount != 0 && falseCount == 0) resault = true;
            _isCheckde = resault;
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("IsChecked"));
            }
            return resault;
        }
        /// <summary>
        /// 这个对象所代表的路径上的一段，或文件名
        /// </summary>
        public virtual string field { get; private set; }

        /// <summary>
        /// 这个存相对路径
        /// </summary>
        public string[] pathArray;
        /// <summary>
        /// 对本地文件存完整路径，线上文件留空
        /// </summary>
        public string fallPath;

        /// <summary>
        /// 本地获取相对路径，线上获取key
        /// </summary>
        public string path {
            get
            {
                if (pathArray == null)
                {
                    return "";
                }
                return string.Join("/", pathArray);
            }
        }

        public FolderTreeNode getChild(string name)
        {
            var r = children.Find((i) => { return i.field == name; });
            return r;
        }
        
        public override string ToString()
        {
            return path;
        }

        public List<FolderTreeNode> getAllChildren(ref List<FolderTreeNode> list,bool inclodeFolder = false, bool inclodeUnexpanded = false)
        {
            if (children.Count == 0)
            {
                if (inclodeUnexpanded || isChecked != false)
                {
                    list.Add(this);
                }
            }
            else
            {
                if (inclodeFolder)
                {
                    if (inclodeUnexpanded || isChecked != false)
                    {
                        list.Add(this);
                    }
                }
                foreach (var i in children)
                {
                    i.getAllChildren(ref list, inclodeFolder, inclodeUnexpanded);
                }
            }
            return list;
        }
    }

    /// <summary>
    /// 文件节点(叶节点)
    /// </summary>
    class FolderTreeFile : FolderTreeNode
    {
        public override bool isFile { get { return true; } }
        public override string field { get { return fileName; } }
        public string Key
        {
            set
            {
                var ps = value.Split(@"/\".ToCharArray(),StringSplitOptions.RemoveEmptyEntries);
                pathArray = ps;

                var t1 = root;
                for (int i = 0; i < ps.Length - 1; i++)
                {
                    var t2 = t1.getChild(ps[i]);
                    if (t2 == null)
                    {
                        t2 = new FolderTreeNode(ps[i]) { root = root, tree = tree, parent = t1 };
                        t1.children.Add(t2);
                    }
                    t2.pathArray = new ArraySegment<string>(ps, 0, i + 1).ToArray();
                    t1 = t2;
                }
                parent = t1;
                t1.children.Add(this);
                fileName = ps[ps.Length - 1];
            }
            get
            {
                return ufileInfo.Key;
            }
            
        }
        /// <summary>
        /// 线上文件的详细信息的引用，本地文件则为null
        /// </summary>
        public UFileInfo ufileInfo;
        public string fileName
        {
            get { return string.Join(".", fileNameArray); }
            set
            {
                var ps = value.Split('.');
                fileNameArray = ps;
                if (ps.Length == 0)
                {
                    fileMidCode = "error";
                    fileExName = "error";
                    return;
                }
                else if (ps.Length == 1)
                {
                    fileMidCode = "";
                    fileExName = "";
                }
                else if (ps.Length == 2)
                {
                    fileMidCode = "";
                    fileExName = ps[1];
                }
                else if (ps.Length >= 3)
                {
                    fileMidCode = string.Join(".", new ArraySegment<string>(ps, 1, ps.Length - 2));
                    fileExName = ps[ps.Length - 1];
                }
            }
        }
        /// <summary>
        /// 文件名以'.'分段的数组
        /// </summary>
        public string[] fileNameArray { get; private set; }
        /// <summary>
        /// 文件名第一段
        /// </summary>
        public string filePureName { get { return fileNameArray[0]; }  }
        /// <summary>
        /// 拓展名，没有为空串
        /// </summary>
        public string fileExName { get; private set; }
        /// <summary>
        /// 除了文件名和拓展名中间的部分
        /// </summary>
        public string fileMidCode { get; private set; }
       
    }
}

