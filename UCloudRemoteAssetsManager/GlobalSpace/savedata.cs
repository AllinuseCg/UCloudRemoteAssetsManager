using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UcloudAgent;

namespace UCloudRemoteAssetsManager
{
    class savedata
    {
        public static string savePath { get { return System.IO.Directory.GetCurrentDirectory() + "\\savedata"; } }

        static savedata _instance;
        public static savedata getInstance()
        {
            if (_instance == null)
            {
                _instance = new savedata();
            }
            return _instance;
        }        
        private savedata() { }

        public BucketData bucketdata { get { return BucketData.getInstance(); } }
    }

    class BucketData
    {
        static BucketData _instance;
        public static BucketData getInstance()
        {
            if (_instance == null)
            {
                _instance = new BucketData();
            }
            return _instance;
        }
        private BucketData() { checkFileExists(); }
        public static string path { get { return savedata.savePath + "\\bucketData.xml"; } }

        XmlDocument doc;
        XmlElement root;
        public Dictionary<string, BucketInfo> bucketMap { get; private set; }
        public void checkFileExists()
        {
            if(File.Exists(path))
            {
                doc = new XmlDocument();
                doc.Load(path);
                root = doc.DocumentElement;
                deserialization();
                uilog.log("读取bucketData");
            }
            else
            {
                if (!Directory.Exists(savedata.savePath)) Directory.CreateDirectory(savedata.savePath);

                doc = new XmlDocument();
                XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "UTF-8", null);    // 声明XML头信息
                doc.AppendChild(dec);   // 添加进doc对象子节点
                root = doc.CreateElement("Root");
                doc.AppendChild(root);
                doc.Save(path);
                bucketMap = new Dictionary<string, BucketInfo>();
                uilog.log("新建bucketData");
            }
        }
        
        public void addBucket(BucketInfo info)
        {
            if (bucketMap == null)
            {
                bucketMap = new Dictionary<string, BucketInfo>();
            }
            BucketInfo temp;
            if (bucketMap.TryGetValue(info.bucketName, out temp))
            {
                bucketMap.Remove(info.bucketName);
            }
            bucketMap.Add(info.bucketName, info);
        }
        public void deleteXml()
        {
            XmlNode bucketDataN = root.SelectSingleNode("bucketData");
            if (bucketDataN != null)
            {
                root.RemoveChild(bucketDataN);
                doc.Save(path);
            }
        }
        /// <summary>
        /// 反序列化，把xml转成list，读档
        /// </summary>
        public void deserialization()
        {
            bucketMap = new Dictionary<string, BucketInfo>();
            XmlNode bucketDataN = root.SelectSingleNode("bucketData");
            if (bucketDataN != null)
            {
                foreach (XmlNode i in bucketDataN)
                {
                    if (i.Name == "bucket")
                    {
                        bucketMap.Add(i.SelectSingleNode("name").InnerText,
                            new BucketInfo(
                                i.SelectSingleNode("name").InnerText,
                                i.SelectSingleNode("public_key").InnerText,
                                i.SelectSingleNode("private_key").InnerText,
                                i.SelectSingleNode("proxy_host").InnerText,
                                i.SelectSingleNode("api_host").InnerText
                            ));
                    }
                }
            }
            XmlNode MemN = root.getChildNode("memory");
            XmlNode lastBucketN = MemN.getChildNode("lastBucket");
            BucketInfo info;
            if (bucketMap.TryGetValue(lastBucketN.InnerText, out info))
            {
                State.currentBucket = info;
            }
        }
        /// <summary>
        /// 序列化，把list转成xml,存档
        /// </summary>
        public void serialization()
        {
            deleteXml();
      
            XmlNode bucketDataN = root.getChildNode("bucketData");
            foreach (var pair in bucketMap)
            {
                var i = pair.Value;
                var e = doc.CreateElement("bucket");
                bucketDataN.AppendChild(e);
                var nameN = doc.CreateElement("name");
                nameN.InnerText = i.bucketName;
                e.AppendChild(nameN);
                var public_keyN = doc.CreateElement("public_key");
                public_keyN.InnerText = i.publicKey;
                e.AppendChild(public_keyN);
                var private_keyN = doc.CreateElement("private_key");
                private_keyN.InnerText = i.privateKey;
                e.AppendChild(private_keyN);
                var proxy_hostN = doc.CreateElement("proxy_host");
                proxy_hostN.InnerText = i.proxyHost;
                e.AppendChild(proxy_hostN);
                var api_hostN = doc.CreateElement("api_host");
                api_hostN.InnerText = i.apiHost;
                e.AppendChild(api_hostN);
            }
            if (State.currentBucket != null)
            {
                XmlNode MemN = root.getChildNode("memory");
                XmlNode lastBucketN = MemN.getChildNode("lastBucket");
                lastBucketN.InnerText = State.currentBucket.bucketName;
            }
            
            doc.Save(path);
        }
        
    }

    /// <summary>
    /// 管理 config.cfg 文件
    /// </summary>
    class BucketConfig
    {
        public static string configPath { get { return System.IO.Directory.GetCurrentDirectory() + "\\filemgr-win64\\config.cfg"; } }
        public static void saveConfig()
        {
            uilog.log("写入config");
            string s = "{" + '\n' +
                "\"public_key\" : \"" + State.currentBucket.publicKey + "\",\n" +
                "\"private_key\" : \"" + State.currentBucket.privateKey + "\",\n" +
                "\"proxy_host\" : \"" + State.currentBucket.proxyHost + "\",\n" +
                "\"api_host\" : \"" + State.currentBucket.apiHost + "\"\n" +
                "}";
            File.WriteAllLines(configPath, new string[] { s });
        }

    }
}
