#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["SqlServer/Connect.SqlServer.csproj", "SqlServer/"]
COPY ["../../Kafka.DotNet.SqlServer/Kafka.DotNet.SqlServer.csproj", "../../Kafka.DotNet.SqlServer/"]
COPY ["../../Kafka.DotNet.ksqlDb/Kafka.DotNet.ksqlDB.csproj", "../../Kafka.DotNet.ksqlDb/"]
RUN dotnet restore "SqlServer/Connect.SqlServer.csproj"
COPY . .
WORKDIR "/src/SqlServer"
RUN dotnet build "Connect.SqlServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Connect.SqlServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Connect.SqlServer.dll"]
