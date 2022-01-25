ARG APP_VERSION_SUFFIX=0

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG APP_VERSION_SUFFIX
WORKDIR /src
COPY . .
RUN ls -la
RUN dotnet restore "TgNews.sln"
RUN dotnet build "TgNews.sln" -c Release -o /app/build /property:Version=1.0.0.${APP_VERSION_SUFFIX}

FROM build AS publish
ARG APP_VERSION_SUFFIX
RUN dotnet publish "TgNews.sln" -c Release -o /app/publish /property:Version=1.0.0.${APP_VERSION_SUFFIX}

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TgNews.Worker.dll", "--server"]
