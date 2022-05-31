#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["ksqldB.RestApi.Client.WorkerService/ksqldB.RestApi.Client.WorkerService.csproj", "ksqldB.RestApi.Client.WorkerService/"]
RUN dotnet restore "ksqldB.RestApi.Client.WorkerService/ksqldB.RestApi.Client.WorkerService.csproj"
COPY . .
WORKDIR "/src/ksqldB.RestApi.Client.WorkerService"
RUN dotnet build "ksqldB.RestApi.Client.WorkerService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ksqldB.RestApi.Client.WorkerService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ksqldB.RestApi.Client.WorkerService.dll"]