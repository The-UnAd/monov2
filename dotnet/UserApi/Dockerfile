FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src
COPY UserApi.sln ./
COPY UnAd.Data/*.csproj ./UnAd.Data/
COPY UnAd.Auth.Web/*.csproj ./UnAd.Auth.Web/
COPY UserApi/*.csproj ./UserApi/

RUN dotnet restore
COPY . .
WORKDIR /src/UserApi
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish "UserApi.csproj" -c Release -o /src/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /src/publish .
ENTRYPOINT ["dotnet", "UserApi.dll"]



