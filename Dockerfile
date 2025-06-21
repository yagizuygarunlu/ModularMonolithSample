# ModularMonolithSample - 2025 Edition
# Multi-stage Docker build for .NET 9 with optimizations

###########################################
# Build Stage
###########################################
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Set build configuration
ARG BUILD_CONFIGURATION=Release

# Copy solution and project files for better layer caching
COPY ["ModularMonolithSample.sln", "."]
COPY ["Directory.Build.props", "."]

# Copy all project files
COPY ["src/API/ModularMonolithSample.API/ModularMonolithSample.API.csproj", "src/API/ModularMonolithSample.API/"]
COPY ["src/BuildingBlocks/ModularMonolithSample.BuildingBlocks/ModularMonolithSample.BuildingBlocks.csproj", "src/BuildingBlocks/ModularMonolithSample.BuildingBlocks/"]

# Copy module project files
COPY ["src/Modules/Event/ModularMonolithSample.Event.Application/ModularMonolithSample.Event.Application.csproj", "src/Modules/Event/ModularMonolithSample.Event.Application/"]
COPY ["src/Modules/Event/ModularMonolithSample.Event.Domain/ModularMonolithSample.Event.Domain.csproj", "src/Modules/Event/ModularMonolithSample.Event.Domain/"]
COPY ["src/Modules/Event/ModularMonolithSample.Event.Infrastructure/ModularMonolithSample.Event.Infrastructure.csproj", "src/Modules/Event/ModularMonolithSample.Event.Infrastructure/"]

COPY ["src/Modules/Attendee/ModularMonolithSample.Attendee.Application/ModularMonolithSample.Attendee.Application.csproj", "src/Modules/Attendee/ModularMonolithSample.Attendee.Application/"]
COPY ["src/Modules/Attendee/ModularMonolithSample.Attendee.Domain/ModularMonolithSample.Attendee.Domain.csproj", "src/Modules/Attendee/ModularMonolithSample.Attendee.Domain/"]
COPY ["src/Modules/Attendee/ModularMonolithSample.Attendee.Infrastructure/ModularMonolithSample.Attendee.Infrastructure.csproj", "src/Modules/Attendee/ModularMonolithSample.Attendee.Infrastructure/"]

COPY ["src/Modules/Ticket/ModularMonolithSample.Ticket.Application/ModularMonolithSample.Ticket.Application.csproj", "src/Modules/Ticket/ModularMonolithSample.Ticket.Application/"]
COPY ["src/Modules/Ticket/ModularMonolithSample.Ticket.Domain/ModularMonolithSample.Ticket.Domain.csproj", "src/Modules/Ticket/ModularMonolithSample.Ticket.Domain/"]
COPY ["src/Modules/Ticket/ModularMonolithSample.Ticket.Infrastructure/ModularMonolithSample.Ticket.Infrastructure.csproj", "src/Modules/Ticket/ModularMonolithSample.Ticket.Infrastructure/"]

COPY ["src/Modules/Feedback/ModularMonolithSample.Feedback.Application/ModularMonolithSample.Feedback.Application.csproj", "src/Modules/Feedback/ModularMonolithSample.Feedback.Application/"]
COPY ["src/Modules/Feedback/ModularMonolithSample.Feedback.Domain/ModularMonolithSample.Feedback.Domain.csproj", "src/Modules/Feedback/ModularMonolithSample.Feedback.Domain/"]
COPY ["src/Modules/Feedback/ModularMonolithSample.Feedback.Infrastructure/ModularMonolithSample.Feedback.Infrastructure.csproj", "src/Modules/Feedback/ModularMonolithSample.Feedback.Infrastructure/"]

# Restore dependencies with optimizations
RUN dotnet restore "src/API/ModularMonolithSample.API/ModularMonolithSample.API.csproj" \
    --runtime linux-x64 \
    --verbosity minimal

# Copy the rest of the source code
COPY . .

# Build the application
WORKDIR "/src/src/API/ModularMonolithSample.API"
RUN dotnet build "ModularMonolithSample.API.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/build \
    --no-restore \
    --verbosity minimal

###########################################
# Publish Stage
###########################################
FROM build AS publish
ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "ModularMonolithSample.API.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    --no-restore \
    --no-build \
    --runtime linux-x64 \
    --self-contained false \
    --verbosity minimal

###########################################
# Runtime Stage
###########################################
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

# Set up non-root user for security
RUN groupadd -r appgroup && useradd -r -g appgroup appuser

# Install additional packages if needed
RUN apt-get update && apt-get install -y \
    curl \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

# Copy published application
COPY --from=publish /app/publish .

# Set ownership and permissions
RUN chown -R appuser:appgroup /app
USER appuser

# Configure environment
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_HTTP_PORTS=8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:8080/health/live || exit 1

# Expose port
EXPOSE 8080

# Labels for better container management
LABEL maintainer="ModularMonolithSample Team" \
      version="1.0.0" \
      description="Event Management API - 2025 Edition" \
      architecture="Modular Monolith" \
      framework=".NET 9.0" \
      year="2025"

# Entry point
ENTRYPOINT ["dotnet", "ModularMonolithSample.API.dll"] 