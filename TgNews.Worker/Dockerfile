FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["TgNews/TgNews.csproj", "TgNews/"]
RUN dotnet restore "TgNews/TgNews.sln"
COPY . .
WORKDIR "/src/TgForwarder"
RUN dotnet build "TgNews.sln" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TgNews.sln" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TgNews.Worker.dll", "--server"]
