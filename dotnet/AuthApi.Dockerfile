FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src
COPY Precision.Apis.Auth.Cognito.sln ./
COPY UnAd.Redis/*.csproj ./UnAd.Redis/
COPY Precision.Apis.Auth.Cognito/*.csproj ./Precision.Apis.Auth.Cognito/

RUN dotnet restore
COPY . .
WORKDIR /src/Precision.Apis.Auth.Cognito
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish Precision.Apis.Auth.Cognito.csproj -c Release -o /src/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /src/publish .
ENTRYPOINT ["dotnet", "Precision.Apis.Auth.Cognito.dll"]



