# --- 第一階段：建置 (Build) ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# 1. 修正路徑：配合 GitHub Actions 的上下文 (./src)
# 指向你截圖中的正確資料夾位置
COPY ["CloudJob/ETF Compare/ETF Compare.csproj", "CloudJob/ETF Compare/"]

# 2. 還原 NuGet 套件
RUN dotnet restore "CloudJob/ETF Compare/ETF Compare.csproj"

# 3. 複製剩下的所有程式碼 (這會包含 src 底下的所有內容)
COPY . .

# 4. 發布程式 (切換到正確目錄進行編譯)
WORKDIR "/app/CloudJob/ETF Compare"
RUN dotnet publish "ETF Compare.csproj" -c Release -o /app/publish

# --- 第二階段：執行 (Runtime) ---
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app

# ★★★ 保留你的關鍵修正：安裝 libgdiplus ★★★
RUN apt-get update && \
    apt-get install -y libgdiplus libc6-dev && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# 從第一階段複製編譯好的檔案
COPY --from=build /app/publish .

# 預先建立資料夾，確保權限正確
RUN mkdir -p /app/data && mkdir -p /app/output

# ★ 設定程式入口 (使用引號處理空格)
ENTRYPOINT ["dotnet", "ETF Compare.dll"]