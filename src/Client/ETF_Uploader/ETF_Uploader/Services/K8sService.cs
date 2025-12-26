using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETF_Uploader.Services
{
    public class K8sService
    {
        private readonly string _jobName;

        public K8sService(string jobName)
        {
            _jobName = jobName;
        }

        /// <summary>
        /// 執行 kubectl apply 來啟動 Job
        /// </summary>
        public string ApplyJob(string yamlPath)
        {
            if (!File.Exists(yamlPath))
                throw new FileNotFoundException($"找不到 YAML 檔案：{yamlPath}");

            return RunCommand($"kubectl apply -f \"{yamlPath}\"");
        }

        /// <summary>
        /// 執行 kubectl delete 來刪除舊 Job
        /// </summary>
        public string DeleteJob()
        {
            return RunCommand($"kubectl delete job {_jobName} --ignore-not-found");
        }

        /// <summary>
        /// 私有的指令執行器
        /// </summary>
        private string RunCommand(string command)
        {
            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = "/c " + command;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            // 如果有錯誤且不是警告，回傳錯誤訊息
            if (process.ExitCode != 0 && !string.IsNullOrWhiteSpace(error))
            {
                return $"[Error] {error}";
            }

            return output;
        }
    }
}
