FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/API/ModularMonolithSample.API/ModularMonolithSample.API.csproj", "src/API/ModularMonolithSample.API/"]
COPY ["src/BuildingBlocks/ModularMonolithSample.BuildingBlocks/ModularMonolithSample.BuildingBlocks.csproj", "src/BuildingBlocks/ModularMonolithSample.BuildingBlocks/"]
COPY ["src/Modules/Event/ModularMonolithSample.Event.Domain/ModularMonolithSample.Event.Domain.csproj", "src/Modules/Event/ModularMonolithSample.Event.Domain/"]
COPY ["src/Modules/Event/ModularMonolithSample.Event.Application/ModularMonolithSample.Event.Application.csproj", "src/Modules/Event/ModularMonolithSample.Event.Application/"]
COPY ["src/Modules/Event/ModularMonolithSample.Event.Infrastructure/ModularMonolithSample.Event.Infrastructure.csproj", "src/Modules/Event/ModularMonolithSample.Event.Infrastructure/"]
COPY ["src/Modules/Attendee/ModularMonolithSample.Attendee.Domain/ModularMonolithSample.Attendee.Domain.csproj", "src/Modules/Attendee/ModularMonolithSample.Attendee.Domain/"]
COPY ["src/Modules/Attendee/ModularMonolithSample.Attendee.Application/ModularMonolithSample.Attendee.Application.csproj", "src/Modules/Attendee/ModularMonolithSample.Attendee.Application/"]
COPY ["src/Modules/Attendee/ModularMonolithSample.Attendee.Infrastructure/ModularMonolithSample.Attendee.Infrastructure.csproj", "src/Modules/Attendee/ModularMonolithSample.Attendee.Infrastructure/"]
COPY ["src/Modules/Ticket/ModularMonolithSample.Ticket.Domain/ModularMonolithSample.Ticket.Domain.csproj", "src/Modules/Ticket/ModularMonolithSample.Ticket.Domain/"]
COPY ["src/Modules/Ticket/ModularMonolithSample.Ticket.Application/ModularMonolithSample.Ticket.Application.csproj", "src/Modules/Ticket/ModularMonolithSample.Ticket.Application/"]
COPY ["src/Modules/Ticket/ModularMonolithSample.Ticket.Infrastructure/ModularMonolithSample.Ticket.Infrastructure.csproj", "src/Modules/Ticket/ModularMonolithSample.Ticket.Infrastructure/"]
COPY ["src/Modules/Feedback/ModularMonolithSample.Feedback.Domain/ModularMonolithSample.Feedback.Domain.csproj", "src/Modules/Feedback/ModularMonolithSample.Feedback.Domain/"]
COPY ["src/Modules/Feedback/ModularMonolithSample.Feedback.Application/ModularMonolithSample.Feedback.Application.csproj", "src/Modules/Feedback/ModularMonolithSample.Feedback.Application/"]
COPY ["src/Modules/Feedback/ModularMonolithSample.Feedback.Infrastructure/ModularMonolithSample.Feedback.Infrastructure.csproj", "src/Modules/Feedback/ModularMonolithSample.Feedback.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/API/ModularMonolithSample.API/ModularMonolithSample.API.csproj"

# Copy everything else
COPY . .

# Build the application
WORKDIR "/src/src/API/ModularMonolithSample.API"
RUN dotnet build "ModularMonolithSample.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ModularMonolithSample.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ModularMonolithSample.API.dll"] 