FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

ARG ConnectionStrings__UserDb
ENV ConnectionStrings__UserDb=$ConnectionStrings__UserDb

WORKDIR /src
COPY DbSeeder.sln ./
COPY UnAd.Redis/*.csproj ./UnAd.Redis/
COPY UnAd.Data/*.csproj ./UnAd.Data/
COPY DbSeeder/*.csproj ./DbSeeder/

RUN dotnet restore
COPY . .
WORKDIR /src/DbSeeder

FROM build AS publish
RUN dotnet publish DbSeeder.csproj -c Release -o /src/publish /p:UseAppHost=true

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app
COPY --from=publish /src/publish .

CMD ./seed db



