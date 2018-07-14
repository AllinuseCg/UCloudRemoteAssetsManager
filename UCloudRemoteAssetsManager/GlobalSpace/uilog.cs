using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UCloudRemoteAssetsManager
{
    class uilog
    {
        static uint logNo = 0;
        public static Action<string> onGetNewLog;
        static List<string> logList = new List<string>();
        public static void log(string newLog)
        {
            var log = "[Log " + logNo + " ]: " + newLog;
            logNo++;
            Console.WriteLine(log);
            logList.Add(log);
            onGetNewLog(log);
        }

        public static void error(string newLog)
        {
            var log = "[error " + logNo + " ]: " + newLog;
            logNo++;
            Console.WriteLine(log);
            logList.Add(log);
            onGetNewLog(log);
        }

        /// <summary>
        /// 返回最新的 c 个log
        /// </summary>
        public static List<string> getLastLog(int c = 10)
        {
            if (c > logList.Count) c = logList.Count;
            List<string> resault = new List<string>();
            if (c > 0)
            {
                resault = new ArraySegment<string>(logList.ToArray(), logList.Count - c, c).ToList();
                return resault;
            }
            else
            {
                return logList;
            }
          
        }
    }
}