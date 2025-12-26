using System;
using System.IO;

namespace ETF_Uploader
{
    public static class LogHelper
    {
        // 設定 Log 存檔路徑 (封裝在這裡，主程式不用管路徑)
        //private const string LogFolderPath = @"C:\Users\2500771\Desktop\ETF\k8s log";

        /// <summary>
        /// 寫入 Log 的公開方法
        /// </summary>
        /// <param name="message">要記錄的訊息</param>
        public static void Write(string message)
        {
            try
            {
                // 1. 抓取目前 .exe 所在的資料夾 (跟 Form1 抓的一樣)
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;

                string logFolderPath = Path.Combine(baseDir, "k8s_log");
                // 1. 如果資料夾不存在，就建立
                if (!Directory.Exists(logFolderPath))
                {
                    Directory.CreateDirectory(logFolderPath);
                }

                // 2. 設定檔名 (每天一個檔案，例如: Log_20251223.txt)
                string fileName = $"Log_{DateTime.Now:yyyyMMdd}.txt";
                string fullPath = Path.Combine(logFolderPath, fileName);

                // 3. 組合訊息內容 (時間 + 訊息)
                string logContent = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\r\n";

                // 4. 寫入檔案 (Append 代表用「附加」的方式)
                File.AppendAllText(fullPath, logContent);
            }
            catch
            {
                // 寫 Log 失敗不做處理，避免影響主程式
            }
        }
    }
}
