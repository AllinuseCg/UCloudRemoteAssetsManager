using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace UcloudAgent
{
    class UFileInfo
    {
        public string BucketName;
        public string Key;
        public string Hash;
        public string MimeType;
        public string Size;
        public string Created;
        public string Modified;
        
        /// <summary>
        /// 将从uCloud返回的字符串转换为数据
        /// </summary>
        public static List<UFileInfo> importByReceiveString(string str)
        {
            //if (str.IndexOf("End with empty DataSet") != -1)  // 2018/04/08 10:41:04.007000 [INFO]End with empty DataSet
            //{
            //    return new List<UFileInfo>();
            //}
            var index = str.LastIndexOf("}");
            if (index == -1)
            {
                MessageBox.Show("bucket中目前没有文件");
                return new List<UFileInfo>();
            }
            if (str.IndexOf("ErrMsg") != -1)
            {
                MessageBox.Show("不存在该bucket");
                return null;
            }
            List<UFileInfo> list = new List<UFileInfo>();
            str = str.Substring(0, index);
            str = str.Trim();
            str = str.Substring(1, str.Length - 2);
            var entries = str.Split(new string[] { "}\n\n{" }, StringSplitOptions.None);
            foreach (string entry in entries)
            {
                var paras = entry.Split(new char[] { '\n', ':' }, StringSplitOptions.RemoveEmptyEntries);
                var t = new UFileInfo()
                {
                    BucketName = paras[1].Trim(),
                    Key = paras[3].Trim(),
                    Hash = paras[5].Trim(),
                    MimeType = paras[7].Trim(),
                    Size = paras[9].Trim(),
                    Created = paras[11].Trim(),
                    Modified = paras[13].Trim(),
                };
                list.Add(t);
            }
            return list;
        }
       

    }
}
