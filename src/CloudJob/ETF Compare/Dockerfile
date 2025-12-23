# --- 第一階段：建置 (Build) ---
# 使用微軟官方的 .NET 8 SDK 來編譯程式
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1. 先複製專案檔就好 (利用 Docker 快取機制加速)
COPY ["*.csproj", "./"]
# 2.還原 NuGet 套件 (包含您剛裝的 Encoding.CodePages)
RUN dotnet restore

# 3. 複製剩下的所有程式碼
COPY . .
# 4. 發布程式 (編譯成 Release 版本)
RUN dotnet publish -c Release -o /app/publish

# --- 第二階段：執行 (Runtime) ---
# 使用輕量級的 .NET 8 Runtime (不含編譯工具，體積小)
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app

# ★★★ 關鍵修正：安裝 libgdiplus ★★★
# Linux 預設沒有 Windows 的繪圖元件 (GDI+)。
# ClosedXML 操作 Excel 時需要用到它，沒裝這行會報錯！
RUN apt-get update && \
    apt-get install -y libgdiplus libc6-dev && \
    apt-get clean

# 從第一階段複製編譯好的檔案過來
COPY --from=build /app/publish .

# 預先建立資料夾，避免程式找不到路徑
RUN mkdir -p /app/data && mkdir -p /app/output

# ★ 設定程式入口
# 請確認您的 "bin/Debug/net8.0" 裡面生成的 dll 叫什麼名字
# 如果您的專案叫 "ETF Compare 2.0"，這裡通常是 "ETF Compare 2.0.dll"
# 建議加上引號，避免檔名有空白造成錯誤
ENTRYPOINT ["dotnet", "ETF Compare.dll"]