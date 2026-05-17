# Stage 1 - runtime base
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

RUN apt-get update && \
    apt-get install -y ca-certificates && \
    rm -rf /var/lib/apt/lists/*

# Stage 2 - build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["CatalogAPI/CatalogAPI.csproj", "CatalogAPI/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
COPY ["Core/Core.csproj", "Core/"]

RUN dotnet restore "CatalogAPI/CatalogAPI.csproj"

COPY . .
WORKDIR "/src/CatalogAPI"
RUN dotnet publish "CatalogAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3 - final
FROM base AS final
WORKDIR /app

COPY --from=datadog/dd-lib-dotnet-init:latest /datadog-init/monitoring-home/ /dd_tracer/dotnet/

ENV CORECLR_ENABLE_PROFILING=1
ENV CORECLR_PROFILER={846F5F1C-F9AE-4B07-969E-05C26BC060D8}
ENV CORECLR_PROFILER_PATH=/dd_tracer/dotnet/Datadog.Trace.ClrProfiler.Native.so
ENV DD_DOTNET_TRACER_HOME=/dd_tracer/dotnet

ENV DD_TRACE_ENABLED=true
ENV DD_LOGS_INJECTION=true
ENV DD_RUNTIME_METRICS_ENABLED=true

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "CatalogAPI.dll"]