# Backend API Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TradeManagementApi.csproj", "."]
RUN dotnet restore "TradeManagementApi.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "TradeManagementApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TradeManagementApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TradeManagementApi.dll"]