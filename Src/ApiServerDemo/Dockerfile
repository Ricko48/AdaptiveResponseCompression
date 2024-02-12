#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ApiServerDemo/ApiServerDemo.csproj", "ApiServerDemo/"]
COPY ["AdaptiveResponseCompression/AdaptiveResponseCompression.Server/AdaptiveResponseCompression.Server.csproj", "AdaptiveResponseCompression/AdaptiveResponseCompression.Server/"]
COPY ["AdaptiveResponseCompression/AdaptiveResponseCompression.Common/AdaptiveResponseCompression.Common.csproj", "AdaptiveResponseCompression/AdaptiveResponseCompression.Common/"]
RUN dotnet restore "./ApiServerDemo/ApiServerDemo.csproj"
COPY . .
WORKDIR "/src/ApiServerDemo"
RUN dotnet build "./ApiServerDemo.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ApiServerDemo.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ApiServerDemo.dll"]