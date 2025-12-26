using Google.Cloud.Storage.V1;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ETF_Uploader.Services
{
    public class GcpService
    {
        private readonly string _bucketName;
        private readonly StorageClient _storageClient;

        public GcpService(string jsonKeyPath, string bucketName)
        {
            _bucketName = bucketName;

            if (!File.Exists(jsonKeyPath))
            {
                // 如果找不到檔案，直接丟出中文錯誤，並顯示它到底在找哪個路徑
                throw new FileNotFoundException($"找不到金鑰檔案！\n程式預期路徑是：{jsonKeyPath}");
            }

            // 設定權限環境變數
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", jsonKeyPath);
            _storageClient = StorageClient.Create();
        }

        /// <summary>
        /// 上傳單一檔案到 GCS
        /// </summary>
        public void UploadFile(string localFilePath)
        {
            if (!File.Exists(localFilePath))
                throw new FileNotFoundException($"找不到要上傳的檔案：{localFilePath}");

            string fileName = Path.GetFileName(localFilePath);

            using (var fileStream = File.OpenRead(localFilePath))
            {
                _storageClient.UploadObject(_bucketName, fileName, null, fileStream);
            }
        }

        /// <summary>
        /// (非同步版) 上傳單一檔案到 GCS
        /// </summary>
        public async Task UploadFileAsync(string localFilePath)
        {
            if (!File.Exists(localFilePath))
                throw new FileNotFoundException($"找不到要上傳的檔案：{localFilePath}");

            string fileName = Path.GetFileName(localFilePath);

            using (var fileStream = File.OpenRead(localFilePath))
            {
                // 這裡改用 UploadObjectAsync，並加上 await
                await _storageClient.UploadObjectAsync(_bucketName, fileName, null, fileStream);
            }
        }
    }
}
