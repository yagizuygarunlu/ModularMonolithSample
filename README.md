# 🏗️ ModularMonolithSample - Enterprise Event Management System (2025 Edition)

[![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/)
[![Architecture](https://img.shields.io/badge/Architecture-Modular%20Monolith-blue.svg)](https://microservices.io/patterns/monolithic.html)
[![Clean Architecture](https://img.shields.io/badge/Clean-Architecture-green.svg)](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
[![CQRS](https://img.shields.io/badge/Pattern-CQRS-orange.svg)](https://martinfowler.com/bliki/CQRS.html)
[![Build Status](https://img.shields.io/badge/build-passing-brightgreen.svg)]()
[![Tests](https://img.shields.io/badge/tests-147%20passing-success.svg)]()
[![Year](https://img.shields.io/badge/Edition-2025-gold.svg)]()

> **A comprehensive enterprise-grade event management system demonstrating cutting-edge .NET 9 architecture patterns, built with modular monolith approach for scalability and maintainability - Updated for 2025!**

## 🎯 **Project Overview**

This project showcases a **production-ready event management system** built using **Modular Monolith architecture** with **Clean Architecture principles**. It demonstrates how to structure large-scale .NET applications while maintaining code quality, testability, and enterprise-level features using the latest .NET 9 patterns and 2025 best practices.

## 🏛️ **Architecture & Design Patterns**

### **Core Architecture**
- **🏗️ Modular Monolith** - Organized into bounded contexts
- **🧅 Clean Architecture** - Clear separation of concerns
- **⚡ CQRS** with MediatR for command/query separation
- **🎯 Domain-Driven Design** - Rich domain models with business logic
- **📡 Event-Driven Architecture** - Domain events for loose coupling

### **Modern .NET 9 Features (2025)**
- **🚀 Minimal APIs** - Modern endpoint mapping
- **🏥 Enhanced Health Checks** - Comprehensive system monitoring
- **📊 OpenTelemetry Integration** - Modern observability
- **🎛️ Feature Flags System** - Runtime feature management
- **📈 Real-time Metrics** - Performance monitoring
- **🔧 Improved JSON Handling** - Modern serialization options

### **Key Design Patterns**
- **🔧 Repository Pattern** - Data access abstraction
- **🎭 Mediator Pattern** - Decoupled communication
- **🏭 Factory Pattern** - Object creation management
- **🎁 Result Pattern** - Error handling without exceptions
- **🧱 Builder Pattern** - Complex object construction

## 📦 **Bounded Contexts (Modules)**

### **1. 🎪 Event Management**
```csharp
// Rich domain models with business logic
public class Event : AggregateRoot
{
    public void Create(string name, DateTime startDate, int capacity, decimal price)
    {
        // Business logic validation
        // Domain event publishing
    }
}
```

### **2. 👥 Attendee Management**
- Registration workflows
- Attendee validation rules
- Email/phone verification

### **3. 🎫 Ticket Management**
- Ticket issuance
- Status tracking
- Automatic ticket generation

### **4. 💬 Feedback System**
- Post-event feedback collection
- Rating system
- Comment validation

## 🔧 **Enterprise Building Blocks**

### **🎯 MediatR Behaviors Pipeline**
```csharp
Request → Validation → Logging → Performance → Caching → Transaction → Handler
```

| Behavior | Purpose | Features |
|----------|---------|----------|
| **ValidationBehavior** | Input validation | FluentValidation integration |
| **LoggingBehavior** | Request/response logging | Structured logging with Serilog |
| **PerformanceBehavior** | Performance monitoring | Configurable thresholds |
| **CachingBehavior** | Response caching | Memory cache with TTL |
| **RetryBehavior** | Resilience patterns | Exponential backoff |
| **AuditBehavior** | User action tracking | IP, User, Timestamp logging |
| **TransactionBehavior** | Data consistency | Automatic transaction management |

### **🛡️ Global Exception Handling**
- **RFC 7807** compliant error responses
- **Custom exception types** (Domain, Validation, NotFound, Conflict)
- **Global middleware** for unhandled exceptions
- **Structured error logging**

### **📊 API Response Wrappers**
```csharp
// Consistent API responses
{
  "success": true,
  "data": { /* result */ },
  "message": "Success",
  "traceId": "12345-67890"
}
```

### **🎯 Result Pattern Implementation**
```csharp
public Result<Event> CreateEvent(CreateEventCommand command)
{
    // Business logic
    if (validation.Failed)
        return Result<Event>.Failure("Validation failed");
    
    return Result<Event>.Success(createdEvent);
}
```

## 🚀 **Modern .NET 9 Features (2025 Edition)**

### **⚡ Minimal APIs**
```csharp
// Modern endpoint mapping with typed results
app.MapPost("/api/v1/events", async (CreateEventCommand command, IMediator mediator) =>
{
    var result = await mediator.Send(command);
    return result.IsSuccess 
        ? TypedResults.Created($"/api/v1/events/{result.Value}", result.Value)
        : TypedResults.BadRequest(result.Error);
})
.WithName("CreateEvent")
.WithOpenApi();
```

### **🏥 Enhanced Health Checks**
```csharp
// Comprehensive health monitoring
builder.Services.AddHealthChecks()
    .AddCheck<ModuleHealthCheck>("event-module")
    .AddCheck<DatabaseHealthCheck>("database");
```

### **🎛️ Feature Flags System**
```csharp
// Runtime feature management
if (await featureFlagService.IsEnabledAsync("enable-event-creation"))
{
    // Feature-specific logic
}
```

### **📊 OpenTelemetry Observability**
```csharp
// Modern metrics and tracing
services.AddModernObservability()
    .AddMetrics()
    .AddTracing()
    .AddLogging();
```

## 🧪 **Testing Strategy**

### **📊 Test Coverage**
- **147 Unit Tests** - 100% business logic coverage
- **Integration Tests** - End-to-end scenarios
- **Behavior Tests** - Cross-cutting concerns

### **🔬 Testing Tools**
- **xUnit** - Test framework
- **NSubstitute** - Mocking framework
- **Shouldly** - Assertion library
- **FluentValidation.TestExtensions** - Validation testing

### **📋 Test Categories**
```
├── 🎪 Event Module Tests (31 tests)
├── 👥 Attendee Module Tests (46 tests)
├── 🎫 Ticket Module Tests (30 tests)
├── 💬 Feedback Module Tests (40 tests)
└── 🔗 Integration Tests
```

## 🚀 **Getting Started**

### **Prerequisites**
- **.NET 9.0 SDK**
- **Visual Studio 2022** or **VS Code**
- **SQL Server** (LocalDB supported)

### **Quick Start**
```bash
# Clone the repository
git clone https://github.com/yourusername/ModularMonolithSample.git

# Navigate to project
cd ModularMonolithSample

# Restore packages
dotnet restore

# Run tests
dotnet test

# Start application
dotnet run --project src/API/ModularMonolithSample.API
```

### **🐳 Docker Support**
```bash
# Build and run with Docker
docker-compose up --build
```

## 📁 **Project Structure**
```
ModularMonolithSample/
├── 📁 src/
│   ├── 📁 API/                          # Web API layer
│   │   ├── 📁 Extensions/               # Modern endpoint mappings
│   │   └── 📄 Program.cs                # .NET 9 minimal hosting
│   ├── 📁 BuildingBlocks/               # Shared enterprise components
│   │   ├── 🎭 Behaviors/                # MediatR behaviors
│   │   ├── 🛡️ Exceptions/               # Custom exceptions
│   │   ├── 📊 Models/                   # Response models
│   │   ├── 🏥 HealthChecks/             # Health monitoring
│   │   ├── 📈 Observability/            # OpenTelemetry
│   │   ├── 🎛️ FeatureFlags/             # Feature management
│   │   └── 🔧 Extensions/               # DI extensions
│   └── 📁 Modules/                      # Bounded contexts
│       ├── 🎪 Event/                    # Event management
│       ├── 👥 Attendee/                 # Attendee management
│       ├── 🎫 Ticket/                   # Ticket management
│       └── 💬 Feedback/                 # Feedback system
└── 📁 tests/                           # Comprehensive test suite
```

## 🛠️ **Technology Stack**

### **Core Technologies**
- **.NET 9.0** - Latest framework features
- **ASP.NET Core** - Web framework
- **Entity Framework Core** - ORM
- **MediatR** - CQRS implementation
- **FluentValidation** - Input validation

### **Modern 2025 Additions**
- **Minimal APIs** - Simplified endpoint definition
- **OpenTelemetry** - Observability and monitoring
- **Feature Flags** - Runtime feature management
- **Enhanced Health Checks** - System monitoring
- **Improved JSON** - Modern serialization

### **Quality & Testing**
- **xUnit** - Unit testing
- **NSubstitute** - Mocking
- **Shouldly** - Assertions
- **Serilog** - Structured logging

### **Infrastructure**
- **SQL Server** - Database
- **Docker** - Containerization
- **Swagger** - API documentation

## 🎯 **Key Features Demonstrated**

### **🏗️ Architecture Patterns**
- **Modular Monolith** with clear module boundaries
- **Clean Architecture** with dependency inversion
- **CQRS** separation with MediatR
- **Domain Events** for decoupled communication
- **Repository Pattern** for data access

### **🔧 Enterprise Features**
- **Global Exception Handling** with RFC 7807 compliance
- **API Response Wrappers** for consistent responses
- **Advanced MediatR Behaviors** (7 different behaviors)
- **Comprehensive Logging** with structured data
- **Performance Monitoring** with configurable thresholds

### **🚀 Modern .NET 9 Features**
- **Minimal APIs** for simplified endpoint definition
- **Enhanced Health Checks** with detailed reporting
- **Feature Flags** with environment/role-based controls
- **OpenTelemetry Integration** for observability
- **Improved JSON Handling** with modern options

### **🧪 Testing Excellence**
- **147 Unit Tests** with high coverage
- **Integration Tests** for end-to-end scenarios
- **Behavior Testing** for cross-cutting concerns
- **Mock-based Testing** with NSubstitute

## 📊 **API Endpoints**

### **🎪 Event Management**
```http
POST   /api/v1/events          # Create event
GET    /api/v1/events/{id}     # Get event
```

### **👥 Attendee Management**
```http
POST   /api/v1/attendees/register  # Register attendee
```

### **🎫 Ticket Management**
```http
POST   /api/v1/tickets/issue   # Issue ticket
```

### **💬 Feedback System**
```http
POST   /api/v1/feedback        # Submit feedback
```

### **🔧 System Endpoints**
```http
GET    /health                 # Health checks
GET    /health/live            # Liveness probe
GET    /health/ready           # Readiness probe
GET    /api/version            # API version info
GET    /api/system-info        # System information
```

## 🎛️ **Configuration**

### **Feature Flags**
```json
{
  "FeatureFlags": {
    "Flags": {
      "enable-event-creation": {
        "Enabled": true,
        "AllowedEnvironments": ["Development", "Production"]
      },
      "enable-advanced-analytics": {
        "Enabled": true,
        "RolloutPercentage": 50.0
      }
    }
  }
}
```

### **Performance Settings**
```json
{
  "BehaviorSettings": {
    "Performance": {
      "SlowRequestThresholdMs": 2000,
      "EnableDetailedMetrics": true
    }
  }
}
```

## 🌟 **What Makes This Project Special**

### **🏆 Enterprise-Grade Quality**
- **Production-ready** architecture patterns
- **Comprehensive error handling** with proper HTTP status codes
- **Extensive logging** with structured data
- **Performance monitoring** with configurable thresholds
- **Health checks** for operational monitoring

### **📚 Educational Value**
- **Real-world patterns** used in enterprise applications
- **Clean code principles** throughout the codebase
- **Comprehensive documentation** with examples
- **Test-driven development** with high coverage
- **Modern .NET practices** and patterns

### **🔧 Technical Excellence**
- **SOLID principles** applied consistently
- **Domain-driven design** with rich models
- **Event-driven architecture** for loose coupling
- **Dependency injection** with proper lifetime management
- **Configuration-based** feature management

## 🚀 **Recent Updates (2025 Edition)**

### **✨ New Features**
- **Minimal APIs** implementation
- **Enhanced Health Checks** with detailed reporting
- **Feature Flags System** with runtime management
- **OpenTelemetry Integration** for observability
- **Modern JSON Configuration** with .NET 9 features

### **🔧 Improvements**
- **Updated to .NET 9.0** with latest features
- **Enhanced Swagger Documentation** with better descriptions
- **Improved Error Handling** with more detailed responses
- **Better Configuration Management** with feature flags
- **Modern Logging** with structured output

### **📊 Metrics**
- **147 Unit Tests** (all passing)
- **4 Modules** with clear boundaries
- **7 MediatR Behaviors** for cross-cutting concerns
- **5 Custom Exceptions** for proper error handling
- **Multiple Health Checks** for system monitoring

## 🤝 **Contributing**

We welcome contributions! Please see our [Contributing Guidelines](CONTRIBUTING.md) for details.

## 📄 **License**

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 **Acknowledgments**

- **Clean Architecture** by Robert C. Martin
- **Domain-Driven Design** by Eric Evans
- **CQRS** pattern by Greg Young
- **.NET Team** for the amazing framework
- **Community contributors** for inspiration and feedback

---

⭐ **If you find this project helpful, please give it a star!** ⭐

📧 **Questions?** Open an issue or reach out to the maintainers.

🔗 **Share** this project with others who might benefit from modern .NET architecture patterns!

---

*Built with ❤️ using .NET 9.0 and modern architecture patterns - 2025 Edition* 