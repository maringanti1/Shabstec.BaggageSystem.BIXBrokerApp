#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:3.1 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["BIXBrokerApp_RabbitMQ/BIXBrokerApp_RabbitMQ.csproj", "BIXBrokerApp_RabbitMQ/"]
RUN dotnet restore "BIXBrokerApp_RabbitMQ/BIXBrokerApp_RabbitMQ.csproj"
COPY . .
WORKDIR "/src/BIXBrokerApp_RabbitMQ"
RUN dotnet build "BIXBrokerApp_RabbitMQ.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BIXBrokerApp_RabbitMQ.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BIXBrokerApp_RabbitMQ.dll"]