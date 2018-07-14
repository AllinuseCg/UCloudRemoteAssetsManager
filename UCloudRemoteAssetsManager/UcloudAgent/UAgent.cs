using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using UCloudRemoteAssetsManager;

namespace UcloudAgent
{
    static class UAgent
    {
        public static string workPath { get { return System.IO.Directory.GetCurrentDirectory() + "\\filemgr-win64"; } }//filemgr-win64是文件夹名
        public static string exePath { get { return workPath + "\\filemgr-win64"; } }//filemgr-win64是exe名

        public static BucketInfo workBucket = null;

        public static string exec(string cmd)
        {
            if (cmd == null || !checkReady())
            {
                return null;
            }
            else
            {
                ProcessStartInfo psi = new ProcessStartInfo(exePath, cmd);
                psi.StandardOutputEncoding = Encoding.UTF8;
                psi.RedirectStandardOutput = true;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                psi.WorkingDirectory = workPath;
                Process proc = Process.Start(psi);
                
                uilog.log("执行命令 : " + cmd);
                proc.WaitForExit();
                string resault = proc.StandardOutput.ReadToEnd();
                uilog.log("执行命令结果 :\n" + resault);
                return resault;
            }
        }
        public static string exec(string cmd, bool window)
        {
            if (cmd == null || !checkReady())
            {
                return null;
            }
            else
            {
                ProcessStartInfo psi = new ProcessStartInfo(exePath, cmd);
                psi.StandardOutputEncoding = Encoding.UTF8;
                psi.RedirectStandardOutput = true;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = window;
                psi.WorkingDirectory = workPath;
                Process proc = Process.Start(psi);

                uilog.log("执行命令 : " + cmd);
                proc.WaitForExit();
                string resault = proc.StandardOutput.ReadToEnd();
                uilog.log("执行命令结果 :\n" + resault);
                return resault;
            }
        }
        /// <summary>
        /// 获取文件列表多段执行，因为缓冲区不够
        /// </summary>
        public static string exeGetFileList()
        {
            //"--action getfilelist --bucket wp-wx --limit 10 --marker 名字 "
            /*
             * 上一个命令会返回最后一个文件的名字，拿名字作marker的参数调用下一个命令
             * 
             */
            string result = "";
            string cmd = cmdPreset.getFileList("12");
            string marker = "";
            var receive = exec(cmd, false);
            var index = receive.IndexOf("next marker");

            while (true)
            {
                if (index == -1)
                {
                    result += receive;  // "End without next marker"可以交给后面处理
                    break;
                }
                else
                {
                    result += receive.Substring(0, index);
                    marker = receive.Substring(index + "next marker:".Length);
                    cmd = cmdPreset.getFileList("12", marker);
                    receive = exec(cmd, true);
                    index = receive.IndexOf("next marker");
                }
            }

            return result;
        }

        static bool checkReady()
        {
            /*  //在程序开始时验证过了就不再验证了
            if (!File.Exists(workPath + "\\filemgr-win64.exe") || !File.Exists(workPath + "\\config.cfg"))
            {
                MessageBox.Show("未找到 filemgr-win64.exe 或 config.cfg \n将文件放到 " + workPath + " 目录下");
                return false;
            }
            */
            if (workBucket == null)
            {
                MessageBox.Show("未设置Bucket");
                uilog.error("未设置Bucket");
                return false;
            }
            return true;
        }

    }
}

