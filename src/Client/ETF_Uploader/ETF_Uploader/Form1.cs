using Google.Cloud.Storage.V1;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ETF_Uploader
{
    public partial class Form1 : Form
    {
        // 設定 Bucket 與 金鑰
        private const string BucketName = "etf-data-adroit-cortex-482002-k4";
        private const string JsonKeyPath = @"C:\Users\2500771\Downloads\adroit-cortex-482002-k4-5661a09d1270.json";
        // 設定下載的目標資料夾
        private const string DownloadFolderPath = @"C:\Users\2500771\Desktop\ETF\k8s output";

        public Form1()
        {
            InitializeComponent();

            // --- 綁定瀏覽按鈕事件 ---
            btnBrowseYesterday.Click += (s, e) => SelectFile(txtYesterday);
            btnBrowseToday.Click += (s, e) => SelectFile(txtToday);

            // --- 綁定拖曳 (Drag & Drop) 事件 ---
            // 讓這兩個文字框都使用同一套拖曳邏輯
            SetupDragDrop(txtYesterday);
            SetupDragDrop(txtToday);
        }

        // --- 1. 拖曳功能設定區 ---
        private void SetupDragDrop(TextBox txt)
        {
            // 當物件拖進來時觸發
            txt.DragEnter += (s, e) =>
            {
                // 檢查拖進來的是不是「檔案」
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    e.Effect = DragDropEffects.Copy; // 游標變成複製圖示
                }
                else
                {
                    e.Effect = DragDropEffects.None; // 禁止圖示
                }
            };

            // 當放開滑鼠時觸發
            txt.DragDrop += (s, e) =>
            {
                // 取得拖入的檔案路徑陣列
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (files != null && files.Length > 0)
                {
                    string filePath = files[0]; // 只取第一個檔案

                    // 簡單檢查副檔名
                    if (Path.GetExtension(filePath).ToLower() == ".xlsx" ||
                        Path.GetExtension(filePath).ToLower() == ".csv")
                    {
                        txt.Text = filePath; // 把路徑填入文字框
                    }
                    else
                    {
                        MessageBox.Show("請拖入 .xlsx 或 .csv 檔案！", "格式錯誤");
                    }
                }
            };
        }

        // --- 2. 瀏覽檔案功能 ---
        private void SelectFile(TextBox txt)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Excel Files|*.xlsx|All Files|*.*";
            ofd.Title = "請選擇 ETF 持股檔案";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                txt.Text = ofd.FileName;
            }
        }



        private void UploadSingleFile(StorageClient storage, string localPath)
        {
            using (var fileStream = File.OpenRead(localPath))
            {
                string fileName = Path.GetFileName(localPath);
                storage.UploadObject(BucketName, fileName, null, fileStream);
            }
        }

        // --- 3. 比較按鈕 (上傳並觸發) ---
        private async void btnCompare_Click_1(object sender, EventArgs e)
        {


            // --- 1. 基本檢查 ---
            if (string.IsNullOrWhiteSpace(txtYesterday.Text) || string.IsNullOrWhiteSpace(txtToday.Text))
            {
                MessageBox.Show("請確認檔案路徑是否正確！", "提醒");
                return;
            }

            // 鎖定按鈕避免重複點擊
            btnCompare.Enabled = false;
            btnCompare.Text = "處理中...";

            DateTimeOffset operationStartTime = DateTimeOffset.UtcNow;

            LogHelper.Write("=== 使用者按下比較按鈕，流程開始 ===");
            LogHelper.Write($"昨日檔案: {txtYesterday.Text}");
            LogHelper.Write($"今日檔案: {txtToday.Text}");

            try
            {
                // --- 2. 上傳檔案到雲端 ---
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", JsonKeyPath);
                var storage = StorageClient.Create();

                UploadSingleFile(storage, txtYesterday.Text);
                UploadSingleFile(storage, txtToday.Text);

                LogHelper.Write(" Excel 檔案上傳至 GCS 成功");
                // --- 3. 觸發 K8s 任務 ---
                string yamlPath = @"C:\Users\2500771\Desktop\ETF\ETF\ETF Compare\k8s\job.yaml"; // 確認 YAML 路徑

                LogHelper.Write("正在觸發 Kubernetes Job...");
                // 刪除舊任務 (忽略找不到的錯誤)
                RunCommand("kubectl delete job etf-analysis-job --ignore-not-found");

                // 啟動新任務
                RunCommand($"kubectl apply -f \"{yamlPath}\"");
                LogHelper.Write(" K8s Job 已啟動");
                // --- 4. 等待與下載 (Polling 核心) ---
                // 最多等待 60 秒，每 3 秒檢查一次
                int maxRetries = 20;
                bool isSuccess = false;

                for (int i = 0; i < maxRetries; i++)
                {
                    // 更新按鈕文字，讓使用者知道還在跑
                    btnCompare.Text = $"雲端分析中 ({i * 3}s)...";

                    // 等待 3 秒
                    await System.Threading.Tasks.Task.Delay(3000);

                    // 嘗試去抓「比 operationStartTime 還要新」的報告
                    // 如果抓到了，這個函式會回傳 true
                    if (TryDownloadReport(operationStartTime))
                    {
                        isSuccess = true;
                        break; // 成功了！跳出迴圈
                    }
                }

                if (!isSuccess)
                {
                    string msg = "分析超時 (60秒)。可能 K8s 發生錯誤，或執行時間過長。";
                    LogHelper.Write($"❌ {msg}");
                    MessageBox.Show(msg + "\n請稍後手動檢查雲端。", "逾時");
                }
            }
            catch (Exception ex)
            {
                LogHelper.Write($"發生嚴重錯誤: {ex.Message}");
                MessageBox.Show($"執行過程發生錯誤: {ex.Message}", "錯誤");
            }
            finally
            {
                // 恢復按鈕狀態
                btnCompare.Enabled = true;
                btnCompare.Text = "比較";
                LogHelper.Write("=== 流程結束 ===");
            }
        }

        private void btnBrowseYesterday_Click(object sender, EventArgs e)
        {

        }
        // 輔助函式：用來在背景執行 CMD 指令
        private void RunCommand(string command)
        {
            System.Diagnostics.ProcessStartInfo processInfo = new System.Diagnostics.ProcessStartInfo("cmd.exe", "/c " + command);
            processInfo.CreateNoWindow = true;         // 不顯示黑視窗
            processInfo.UseShellExecute = false;       // 不使用作業系統外殼啟動
            processInfo.RedirectStandardOutput = true; // 攔截輸出訊息
            processInfo.RedirectStandardError = true;  // 攔截錯誤訊息

            var process = System.Diagnostics.Process.Start(processInfo);
            process.WaitForExit(); // 等待指令執行完畢

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            // 如果指令執行失敗 (ExitCode 不為 0)，拋出錯誤
            if (process.ExitCode != 0)
            {
                throw new Exception($"指令錯誤: {error}\n(輸出: {output})");
            }
        }

        // 專用函式：找出最新報告並下載
        private bool TryDownloadReport(DateTimeOffset minTime)
        {
            try
            {
                if (!Directory.Exists(DownloadFolderPath)) Directory.CreateDirectory(DownloadFolderPath);

                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", JsonKeyPath);
                var storage = StorageClient.Create();

                // 找檔案
                var objects = storage.ListObjects(BucketName, "Reports/");
                var latestObject = objects
                    .Where(o => o.Name.EndsWith(".txt"))
                    .OrderByDescending(o => o.UpdatedDateTimeOffset)
                    .FirstOrDefault();

                // 如果連檔案都沒有，或是檔案是「舊的」(比按鈕按下的時間還早)
                // 就回傳 false，讓主程式繼續等
                if (latestObject == null || latestObject.UpdatedDateTimeOffset < minTime)
                {
                    return false;
                }

                // --- 找到了！而且是新的！ ---

                string localFileName = Path.Combine(DownloadFolderPath, Path.GetFileName(latestObject.Name));
                if (File.Exists(localFileName)) File.Delete(localFileName);

                using (var fileStream = File.Create(localFileName))
                {
                    storage.DownloadObject(BucketName, latestObject.Name, fileStream);
                }

                LogHelper.Write($"報告下載成功: {localFileName}");

                DialogResult result = MessageBox.Show(
                    $"報告下載成功！\n(耗時約 {(DateTimeOffset.UtcNow - minTime).TotalSeconds:N0} 秒)\n\n請問是否要打開資料夾？",
                    "下載完成", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start("explorer.exe", DownloadFolderPath);
                }
                else
                {
                    MessageBox.Show($"檔案已儲存於以下路徑：\n{localFileName}", "路徑資訊", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                return true; // 任務完成
            }
            catch (Exception ex)
            {
                // 如果中間網路斷掉或出錯，記錄下來，但回傳 false 讓它下次重試
                LogHelper.Write($"下載嘗試失敗: {ex.Message}");
                return false;
            }
        }

    }
}