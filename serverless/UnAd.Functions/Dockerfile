FROM public.ecr.aws/lambda/dotnet:8 AS base

FROM mcr.microsoft.com/dotnet/sdk:8.0 as build
WORKDIR /src
COPY UnAd.Functions.csproj .
RUN dotnet restore UnAd.Functions.csproj

WORKDIR "/src/UnAd.Functions"
COPY . .
RUN dotnet build "UnAd.Functions.csproj" --configuration Release --output /app/build

FROM build AS publish
RUN dotnet publish "UnAd.Functions.csproj" \
    --configuration Release \ 
    --runtime linux-x64 \
    --self-contained false \ 
    --output /app/publish \
    -p:PublishReadyToRun=true  

FROM base AS final
WORKDIR /var/task
COPY --from=publish /app/publish .
