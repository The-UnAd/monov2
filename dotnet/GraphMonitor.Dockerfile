FROM public.ecr.aws/lambda/dotnet:8 AS base

FROM mcr.microsoft.com/dotnet/sdk:8.0-bullseye-slim as build
WORKDIR /src
COPY ["GraphMonitor/GraphMonitor/GraphMonitor.csproj", "GraphMonitor/"]
RUN dotnet restore "GraphMonitor/GraphMonitor.csproj"

WORKDIR "/src/GraphMonitor"
COPY ./GraphMonitor/GraphMonitor .
RUN dotnet build "GraphMonitor.csproj" --configuration Release --output /app/build

FROM build AS publish
RUN dotnet publish "GraphMonitor.csproj" \
    --configuration Release \
    --runtime linux-x64 \
    --self-contained false \
    --output /app/publish \
    -p:PublishReadyToRun=true

FROM base AS final
WORKDIR /var/task
COPY --from=publish /app/publish .