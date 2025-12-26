using Google.Cloud.Storage.V1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
    }
}
