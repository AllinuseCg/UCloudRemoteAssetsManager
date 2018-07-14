using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UcloudAgent
{
    static public class cmdPreset
    {
        /// <summary>
        /// 因为缓冲区不够所以不能这样写了
        /// </summary>
        /// <returns></returns>
        public static string getFileList(string limit = null, string marker = null)
        {
            if (UAgent.workBucket == null) return null;
            string result = "--action getfilelist --bucket " + UAgent.workBucket.bucketName;
            if (limit != null)
            {
                result+= " --limit " +limit;
            }
            if (marker != null)
            {
                result += " --marker " + marker;
            }
            return result;
        }

        public static string uploadFile(string key, string fallPath)
        {
            if (UAgent.workBucket == null) return null;
            if (fallPath == null || fallPath == "") return null;
            if (key == null || key == "") return null;
            return "--action mput --bucket " + UAgent.workBucket.bucketName + " --key " + key + " --file " + fallPath;
        }

        public static string deleteFile(string key)
        {
            if (UAgent.workBucket == null) return null;
            if (key == null || key == "") return null;
            return "--action delete --bucket " + UAgent.workBucket.bucketName + " --key " + key;
        }
       
    }
}
