# Modular Monolith Event Management System

A .NET 9 Web API project demonstrating the Modular Monolith architecture pattern for an Event Management System.

## Architecture

The system is built using a Modular Monolith architecture, where each module is self-contained but still part of the same application. The modules are:

- Event Management
- Attendee Management
- Ticket Management
- Feedback Management

Each module follows Clean Architecture principles with:
- Domain Layer: Entities, Value Objects, and Repository Interfaces
- Application Layer: Use Cases (Commands/Queries) using CQRS with MediatR
- Infrastructure Layer: Repository Implementations and Database Context

## Technology Stack

- .NET 9
- ASP.NET Core Web API
- Entity Framework Core (In-Memory Database)
- MediatR for CQRS
- Clean Architecture
- Modular Monolith Pattern

## Getting Started

### Prerequisites

- .NET 9 SDK
- Visual Studio 2022 or VS Code

### Running the Application

1. Clone the repository:
```bash
git clone https://github.com/yourusername/ModularMonolithSample.git
```

2. Navigate to the project directory:
```bash
cd ModularMonolithSample
```

3. Run the application:
```bash
dotnet run --project src/API/ModularMonolithSample.API
```

4. The API will be available at `https://localhost:5001` and `http://localhost:5000`

### API Endpoints

- `POST /api/events` - Create a new event
- More endpoints coming soon...

## Project Structure

```
ModularMonolithSample/
├── src/
│   ├── Modules/
│   │   ├── Event/
│   │   │   ├── Domain/
│   │   │   ├── Application/
│   │   │   └── Infrastructure/
│   │   ├── Attendee/
│   │   ├── Ticket/
│   │   └── Feedback/
│   ├── BuildingBlocks/
│   │   ├── Common/
│   │   └── Infrastructure/
│   └── API/
└── tests/
    └── Modules/
```

## Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details. 