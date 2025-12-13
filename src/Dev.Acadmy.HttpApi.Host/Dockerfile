# ===============================
# Stage 1: Build
# ===============================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# 1. انسخ ملفات الحل والمشاريع فقط أولاً (للاستفادة من كاش Docker)
COPY ["Dev.Acadmy.sln", "."]
COPY ["src/Dev.Acadmy.HttpApi.Host/*.csproj", "src/Dev.Acadmy.HttpApi.Host/"]
COPY ["src/Dev.Acadmy.Application/*.csproj", "src/Dev.Acadmy.Application/"]
COPY ["src/Dev.Acadmy.Domain/*.csproj", "src/Dev.Acadmy.Domain/"]
COPY ["src/Dev.Acadmy.EntityFrameworkCore/*.csproj", "src/Dev.Acadmy.EntityFrameworkCore/"]
# أضف باقي المشاريع التي يعتمد عليها الـ Host

# 2. استعادة الحزم للمشاريع الأساسية
RUN dotnet restore "Dev.Acadmy.sln"

# 3. انسخ باقي كود المصدر
COPY . .

# 4. انتقل لمجلد الـ Host وابنِ/انشر
WORKDIR /src/src/Dev.Acadmy.HttpApi.Host
RUN dotnet build "../Dev.Acadmy.HttpApi.Host/Dev.Acadmy.HttpApi.Host.csproj" -c $BUILD_CONFIGURATION --no-restore
RUN dotnet publish "../Dev.Acadmy.HttpApi.Host/Dev.Acadmy.HttpApi.Host.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    --no-restore \
    --no-build

# ===============================
# Stage 2: Runtime
# ===============================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# 5. انسخ فقط ملفات النشر النهائية
COPY --from=build /app/publish .

# 6. أنشئ مجلد السجلات (لـ Serilog/أي نظام لوجات)
RUN mkdir -p /app/Logs

# 7. حدد نقطة الدخول
ENTRYPOINT ["dotnet", "Dev.Acadmy.HttpApi.Host.dll"]
