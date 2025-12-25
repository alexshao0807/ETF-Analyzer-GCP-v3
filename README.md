# ETF-GKE-Automation - 混合雲架構之自動化分析系統

這是一個結合 **本地端控制 (On-Premise)** 與 **雲端運算 (Cloud-Native)** 的混合架構解決方案。系統由兩大核心組成：基於 **.NET Framework** 的 Windows 用戶端介面，以及運行於 **Google Kubernetes Engine (GKE)** 的 **.NET 8** 分析核心。

本專案展現了跨世代技術框架的整合能力，利用 GitHub Actions 實現雲端邏輯的持續整合 (CI/CD)，同時保留了本地端操作的便利性與直覺性。

##  系統架構設計

本系統採用 **前後端分離 (Separation of Concerns)** 的設計模式：

### 1. 前端控制台 (Client Side)
* **技術框架**：.NET Framework (Windows Forms)
* **功能定位**：提供使用者友善的操作介面 (UI)。
* **職責**：
    * 負責資料的前處理與參數設定。
    * 透過 API 將本地 Excel 數據安全上傳至 Google Cloud Storage (GCS)。
    * 即時監控上傳狀態並提供視覺化回饋。

### 2. 雲端運算核心 (Cloud Core)
* **技術框架**：.NET 8 / Docker / Kubernetes
* **功能定位**：高效能數據處理後端。
* **職責**：
    * 以容器化 (Container) 形式運行於 GKE。
    * 執行複雜的 ETF 交叉比對與權重計算。
    * 自動化生成分析報告並執行雲端環境清理。

##  核心開發哲學

### 雙模並行 (Bimodal IT)
在使用者介面上，選擇成熟穩定的 **.NET Framework** 以確保在 Windows 環境下的最佳相容性與操作體驗；在運算核心上，則擁抱現代化的 **.NET 8** 與 **Linux 容器技術**，以獲取雲端的彈性擴充與高效能。這體現了「適材適用」的架構思維。

### 零維護的自動化 (Maintenance by Automation)
真正的自動化應將「人為干預」降至最低。後端透過 **GitHub Actions** 建立完整 CI/CD 流水線，代碼一旦提交即自動完成建置與部署，確保雲端邏輯永遠保持最新狀態，無需手動維護伺服器。

##  技術亮點

* **跨平台整合**：成功解決 Windows 本地環境與 Linux 雲端容器之間的檔案傳輸與邏輯對接。
* **相容性處理**：針對 .NET 8 在 Linux 容器中處理 Excel 的繪圖限制，透過多階段建置 (Multi-stage build) 預載 `libgdiplus` 函式庫，確保報表生成的精確度。
* **資源最佳化**：利用 Kubernetes Job 特性，僅在分析任務執行時消耗運算資源，任務結束後自動釋放，大幅降低雲端成本。
* **數據生命週期管理**：實作自動化清理機制，分析完成後自動移除雲端原始檔，防止數據污染。

##  技術棧一覽

| 領域 | 技術組件 | 用途 |
| :--- | :--- | :--- |
| **Client UI** | **.NET Framework** | Windows Forms 使用者介面開發 |
| **Backend** | **.NET 8 (Core)** | 高效能數據運算核心 |
| **Infrastructure** | **Docker & Kubernetes (GKE)** | 容器化封裝與雲端編排 |
| **DevOps** | **GitHub Actions** | 自動化 CI/CD 流水線 |
| **Cloud Service** | **Google Cloud Platform** | Artifact Registry, Cloud Storage |
| **Libraries** | **ClosedXML** | 跨平台 Excel 數據處理 |

##  部署與運作流程

1.  **本地操作**：使用者透過 .NET Framework UI 介面選取檔案，一鍵上傳至雲端。
2.  **觸發運算**：GKE 自動拉取最新的 Docker 映像檔 (.NET 8) 啟動 Job。
3.  **邏輯處理**：容器內部執行 ETF 權重比對與趨勢分析。
4.  **結果產出**：分析報告回傳至儲存桶，系統自動清理原始暫存檔。

## 免責聲明

本專案僅用於技術研究、雲端架構驗證與自動化流程展示。投資具有風險，本系統產出之分析結果僅供參考。
