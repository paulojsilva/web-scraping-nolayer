FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["WebScrapingNoLayer.csproj", ""]
RUN dotnet restore "./WebScrapingNoLayer.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "WebScrapingNoLayer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WebScrapingNoLayer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS http://*:$PORT
ENV ASPNETCORE_ENVIRONMENT DYNO
ENTRYPOINT ["dotnet", "WebScrapingNoLayer.dll"]