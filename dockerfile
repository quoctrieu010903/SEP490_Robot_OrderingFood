# Stage 1: Base - Lớp runtime tối ưu cho ASP.NET Core
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5235
USER app

# Stage 2: Build - Lớp chứa SDK để build ứng dụng
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Đảm bảo NuGet cache được clear và cấu hình đúng
RUN dotnet nuget locals all --clear

# Tối ưu hóa caching bằng cách restore dependencies trước
COPY ["SEP490_Robot_Ordering_Food.sln", "."]
COPY ["SEP490_Robot_FoodOrdering.API/SEP490_Robot_FoodOrdering.API.csproj", "SEP490_Robot_FoodOrdering.API/"]
COPY ["SEP490_Robot_FoodOrdering.Application/SEP490_Robot_FoodOrdering.Application.csproj", "SEP490_Robot_FoodOrdering.Application/"]
COPY ["SEP490_Robot_FoodOrdering.Domain/SEP490_Robot_FoodOrdering.Domain.csproj", "SEP490_Robot_FoodOrdering.Domain/"]
COPY ["SEP490_Robot_FoodOrdering.Infrastructure/SEP490_Robot_FoodOrdering.Infrastructure.csproj", "SEP490_Robot_FoodOrdering.Infrastructure/"]
COPY ["SEP490_Robot_FoodOrdering.Core/SEP490_Robot_FoodOrdering.Core.csproj", "SEP490_Robot_FoodOrdering.Core/"]

# Restore với verbose để debug nếu cần
RUN dotnet restore "SEP490_Robot_Ordering_Food.sln" --verbosity normal

# Copy toàn bộ source code và build
COPY . .
WORKDIR "/src"

# Build solution (bao gồm restore tự động)
RUN dotnet build "SEP490_Robot_Ordering_Food.sln" -c $BUILD_CONFIGURATION

# Stage 3: Publish - Chỉ publish project API
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "SEP490_Robot_FoodOrdering.API/SEP490_Robot_FoodOrdering.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Stage 4: Final - Lớp cuối cùng, nhẹ nhất
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SEP490_Robot_FoodOrdering.API.dll"]