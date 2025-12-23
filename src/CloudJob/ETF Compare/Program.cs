using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using System.Diagnostics;
using Google.Cloud.Storage.V1; // 新增這行
public class StockItem
{
    public string Symbol { get; set; } //股票代號
    public string Name { get; set; } // 股票名稱
    public decimal Shares { get; set; } //股數
    public string Weight { get; set; }//權重
}

class Program
{
    static void Main()
    {
        // ★ 修改點 1: 註冊編碼提供者 (讓 Linux 容器看得懂 Big5 中文)
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        // [新增: 啟動碼表]
        Stopwatch sw = new Stopwatch();
        sw.Start();
        Console.WriteLine($"[0 ms] 程式啟動...");



        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string folderPath = Path.Combine(baseDir, "data");       // 輸入資料夾
        string outputPath = Path.Combine(baseDir, "output");     // 輸出資料夾
        // 1. 設定路徑
        //string folderPath = @"C:\Users\2500771\Desktop\ETF\etf data";
        // [新增: 設定輸出路徑]
        //string outputPath = @"C:\Users\2500771\Desktop\ETF\etf output";
        // [新增: 自動檢查並建立輸出資料夾，避免程式報錯]

        Console.WriteLine($"[資訊] 讀取資料路徑: {folderPath}");
        Console.WriteLine($"[資訊] 輸出報告路徑: {outputPath}");
        if (!Directory.Exists(folderPath))
        {
            Console.WriteLine($"[錯誤] 找不到資料夾: {folderPath}");
            return;
        }

        // 自動建立輸出資料夾
        if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);

        // 2. 抓取檔案
        var allFiles = Directory.GetFiles(folderPath, "*ETF_Portfolio_Composition_File_*")
                                .OrderBy(f => Path.GetFileName(f))//排序檔案日期
                                .ToList();

        if (allFiles.Count < 2)
        {
            Console.WriteLine("❌ 檔案數量不足兩個。");
            return;
        }

        string yesterdayFile = allFiles[allFiles.Count - 2];//昨日
        string todayFile = allFiles[allFiles.Count - 1];//今日

        var yesterdayData = FlexibleRead(yesterdayFile);
        Console.WriteLine($"[{sw.ElapsedMilliseconds} ms] 昨日資料讀取完畢 (筆數: {yesterdayData.Count})");
        var todayData = FlexibleRead(todayFile);
        Console.WriteLine($"[{sw.ElapsedMilliseconds} ms] 今日資料讀取完畢 (筆數: {yesterdayData.Count})");

        if (yesterdayData.Count == 0 || todayData.Count == 0) return;
        // 這行將「昨日資料」轉換成「哈希表 (Dictionary)」
        var yesterdayDict = yesterdayData.ToDictionary(s => s.Symbol);
        Console.WriteLine($"[{sw.ElapsedMilliseconds} ms] Dictionary 建立完成 (O(1) 準備就緒)");

        // 3. 準備輸出內容 (使用 StringBuilder 收集文字)
        // [新增: 初始化 StringBuilder 用於收集要寫入檔案的文字內容]
        StringBuilder sb = new StringBuilder();
        // [新增: 建立報告檔標頭資訊]
        string headerInfo = $"--- ETF 持股變動分析報告 ---\n" +
                            $"產生時間: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n" +
                            $"(昨日檔案) {Path.GetFileName(yesterdayFile)}\n" +
                            $"(今日檔案) {Path.GetFileName(todayFile)}\n" +
                            new string('-', 85);


        sb.AppendLine(headerInfo);// 寫入 StringBuilder
        // 定義標題列格式
        string tableHeader = $"{"狀態",-6} {"代號",-12} {"名稱",-25} {"今日股數",-15} {"持股權重",-10}";
        sb.AppendLine(tableHeader);// 寫入 StringBuilder
        sb.AppendLine(new string('-', 85));

        // 4. 進行比對並收集內容
        int count = 0;
        foreach (var current in todayData)
        {
            // 這行在比對時，搜尋速度就是 O(1)
            if (yesterdayDict.TryGetValue(current.Symbol, out var prev))
            {
                if (current.Shares > prev.Shares)
                {
                    string row = GetFormattedRow("增加", current);
                    // [新增: 將結果同時存入 StringBuilder 與 顯示在螢幕]
                    sb.AppendLine(row);
                    Console.WriteLine(row); // 同步顯示在螢幕
                    count++;
                }
            }
            else
            {
                string row = GetFormattedRow("新進", current);
                // [新增: 將結果同時存入 StringBuilder 與 顯示在螢幕]
                sb.AppendLine(row);
                Console.WriteLine(row); // 同步顯示在螢幕
                count++;
            }
        }

        sb.AppendLine($"\n分析完成，共計 {count} 筆變動。");

        // 5. 寫入文字檔
        // [新增: 設定存檔名稱，加入時間戳記防止檔名重複]
        string fileName = $"ETF_Analysis_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
        string fullOutputPath = Path.Combine(outputPath, fileName);

        try
        {
            // [新增: 執行實體存檔動作，指定 UTF8 編碼確保中文顯示正常]
            // 使用 UTF8 編碼確保中文不亂碼
            File.WriteAllText(fullOutputPath, sb.ToString(), Encoding.UTF8);
            // [新增] 自動上傳到 GCS
            // 請將這裡換成你真實的 Bucket 名稱 (不用加 gs://)
            string bucketName = "etf-data-adroit-cortex-482002-k4";
            string cloudFileName = $"Reports/{fileName}"; // 存到 Reports 資料夾下

            UploadToGcs(fullOutputPath, bucketName, cloudFileName);
            Console.WriteLine(new string('=', 50));
            Console.WriteLine($"✅ 成功！分析結果已存至:");
            Console.WriteLine($"👉 {fullOutputPath}");
            Console.WriteLine(new string('=', 50));
        }
        catch (Exception ex)
        {
            // [新增: 異常處理，避免權限不足或路徑錯誤導致程式崩潰]
            Console.WriteLine($"❌ 寫入檔案失敗: {ex.Message}");
        }

        sw.Stop();
        Console.WriteLine($"\n總共耗時: {sw.ElapsedMilliseconds} ms");
        //Console.WriteLine("\n按任意鍵結束...");
        //Console.ReadKey();
    }

    // --- 以下為讀取邏輯 (維持不變，確保資料抓取準確) ---
    // --- 輔助方法 ---

    static string GetFormattedRow(string status, StockItem item)
    {
        return $"{status,-6} {item.Symbol,-12} {item.Name,-25} {item.Shares,12:N0} {item.Weight,10}";
    }

    static List<StockItem> FlexibleRead(string filePath)
    {
        string ext = Path.GetExtension(filePath).ToLower();
        if (ext == ".csv") return ReadCsv(filePath);
        if (ext == ".xlsx") return ReadXlsx(filePath);
        return new List<StockItem>();
    }

    static string CleanSymbol(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "";
        return input.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).Last();
    }

    static decimal ParseShares(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return 0;
        string cleanValue = Regex.Replace(input, @"[^\d.]", "");
        decimal.TryParse(cleanValue, out decimal result);
        return result;
    }

    static List<StockItem> ReadCsv(string filePath)
    {
        var list = new List<StockItem>();
        var lines = File.ReadAllLines(filePath, Encoding.Default);
        bool isHeaderFound = false; //1.初始化旗標，預設為「尚未找到標題
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            // 2. 關鍵邏輯：如果還沒找到標題，就一直檢查這一行有沒有「股票代號」
            if (!isHeaderFound)
            {
                if (line.Contains("股票代號"))
                {
                    isHeaderFound = true; // 3. 找到了！將旗標設為 true
                }
                    continue; // 跳過標題這一行，去讀下一行（即第一筆資料）
            }
            // 4. 只要 isHeaderFound 變成 true，之後每一行都會進入這裡開始抓取資料
            var cols = line.Split(',').Select(c => c.Trim().Replace("\"", "")).ToArray();
            // ... 後續抓取代號與股數的動作 ...
            if (cols.Length >= 4)
            {
                string symbol = CleanSymbol(cols[0]);
                if (!string.IsNullOrEmpty(symbol) && Regex.IsMatch(symbol, @"^\d+$"))
                    list.Add(new StockItem { Symbol = symbol, Name = cols[1], Shares = ParseShares(cols[2]), Weight = cols[3] });
            }
        }
        return list;
    }

    static List<StockItem> ReadXlsx(string filePath)
    {
        var list = new List<StockItem>();
        using (var workbook = new XLWorkbook(filePath))
        {
            var sheet = workbook.Worksheet(1);
            var rows = sheet.RangeUsed().RowsUsed();// 抓取「所有」有資料的行

            bool isHeaderFound = false;//1.初始化旗標，預設為「尚未找到標題
            foreach (var row in rows)
            {
                string cell1 = row.Cell(1).GetValue<string>().Trim();


                // 如果還沒找到標題，就比對第一個儲存格是不是包含「股票代號」
                if (!isHeaderFound)
                {
                    if (cell1.Contains("股票代號")) 
                    { 
                        isHeaderFound = true; 
                    } 
                    continue; 
                }
                // 只要標題出現過了，後面的 Row 不管在第幾行都會被讀進來
                string symbol = CleanSymbol(cell1);
                // ... 後續存入 list 的動作 ...
                if (string.IsNullOrEmpty(symbol) || !Regex.IsMatch(symbol, @"^\d+$")) continue;
                list.Add(new StockItem { Symbol = symbol, Name = row.Cell(2).GetValue<string>().Trim(), Shares = ParseShares(row.Cell(3).GetValue<string>()), Weight = row.Cell(4).GetValue<string>().Trim() });
            }
        }
        return list;
    }
    static void UploadToGcs(string localFilePath, string bucketName, string objectName)
    {
        Console.WriteLine($"[系統] 準備上傳報告至 GCS: gs://{bucketName}/{objectName}");
        try
        {
            // 這一行程式會自動尋找環境變數 GOOGLE_APPLICATION_CREDENTIALS 的憑證
            var storage = StorageClient.Create();

            using (var fileStream = File.OpenRead(localFilePath))
            {
                storage.UploadObject(bucketName, objectName, null, fileStream);
            }
            Console.WriteLine($"[成功] ✅ 檔案已上傳！");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[錯誤] ❌ 上傳失敗: {ex.Message}");
        }
    }
}