using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UcloudAgent;

namespace UCloudRemoteAssetsManager
{
    class State
    {
        public static BucketInfo lastBucket { get; private set; }
        public static BucketInfo currentBucket {
            get { return UAgent.workBucket; }
            set {
                lastBucket = UAgent.workBucket;
                UAgent.workBucket = value;
                uilog.log("当前bucket变更为 : " + value.bucketName);
                BucketConfig.saveConfig();//将config写入文件
            }
        }
        public static MainWindow mainWindow;
    }

    static class ApplicationInfomation
    {
        public static string Version { get { return "beta 1.3"; } }
        public static string Copyright { get { return "Allinuse"; } }
    }
    

     
}
