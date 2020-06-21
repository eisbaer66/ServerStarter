#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.


FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["ServerStarter/Server/ServerStarter.Server.csproj", "ServerStarter/Server/"]
COPY ["ServerStarter/Shared/ServerStarter.Shared.csproj", "ServerStarter/Shared/"]
COPY ["ServerStarter/Client/ServerStarter.Client.csproj", "ServerStarter/Client/"]
COPY ["Zarlo.Stats/Zarlo.Stats.csproj", "Zarlo.Stats/"]
RUN dotnet restore "ServerStarter/Server/ServerStarter.Server.csproj"
COPY . .
WORKDIR "/src/ServerStarter/Server"
RUN dotnet build "ServerStarter.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ServerStarter.Server.csproj" -c Release -o /app/publish

FROM base AS final
ENV DOTNET_USE_POLLING_FILE_WATCHER 1
ENV ASPNETCORE_URLS http://+:80
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ServerStarter.Server.dll"]