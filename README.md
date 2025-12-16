# 📊 Portfolio Insights – Personal Finance Dashboard

This project is a demonstration of a .NET 8 microservices-based personal finance and analytics platform. It builds on the architectural patterns and infrastructure explored in my earlier E-Commerce Microservices project, applying them to a number‑driven, analytics‑focused domain.

The application allows users to manage investment portfolios, retrieve simulated market prices, compute analytics asynchronously, and receive notifications based on computed results. All services are containerized and orchestrated via Docker Compose, showcasing a realistic, end‑to‑end distributed system.

Building this application helped solidify my understanding of:

- Event‑driven microservices
- gRPC and HTTP communication
- CQRS and layered architectures
- Message‑based analytics pipelines
- Vertical Slice and Clean Architectures
- Containerized orchestration with Docker
- Implementation of Cross-cutting concerns such as validations, logging, health checks and custom exception handling

## 🧩 Solution Overview

The solution consists of eight projects, all located at the same directory level:

1. Market Data Service – Simulated market prices and price updates
2. Portfolio Service – User portfolios and asset management
3. Analytics Service – Asynchronous analytics computation
4. Notification Service – User notifications based on analytics events

5. BuildingBlocks – Shared abstractions, behaviors, and utilities
6. BuildingBlocks.Messaging – Event contracts and MassTransit configuration

7. YARP API Gateway – Centralized routing layer

8. Web Client App – Razor Pages frontend

All services, data stores, the message broker, the API Gateway, and the client application run in isolated Docker containers and communicate through a single Docker Compose environment.

## 🚀 Features

- Microservices Architecture – Independently deployable services with clear responsibilities
- Event‑Driven Workflows – Asynchronous communication via RabbitMQ and MassTransit
- Synchronous Communication – gRPC used for high‑performance service‑to‑service calls
- API Gateway – YARP reverse proxy as a unified entry point
- CQRS‑Inspired Design – Commands, queries, and handlers shared via Building Blocks
- Multi‑Database Strategy – Each service owns its own datastore
- Containerization – Full Docker and Docker Compose orchestration

## 🛠 Tech Stack

### ✅ Backend

- ASP.NET Core 8 (Minimal APIs)
- Carter for endpoint definition
- gRPC for synchronous inter‑service communication
- MassTransit with RabbitMQ
- CQRS abstractions (ICommand / IQuery / Handlers)

### ✅ Databases & Storage

- SQLite (Market Data, Notification Services)
- PostgreSQL + Marten (Portfolio Service – NoSQL document database)
- SQL Server (Analytics Service)
- Redis (Market Data Service caching)
- In‑Memory Stores (Market Data simulation & logs)

### ✅ Infrastructure & Tools

- Docker & Docker Compose
- YARP Reverse Proxy
- Refit (Web Client API consumption)

🗂 Project Structure


## 🗂 Project Structure
```plaintext
Portfolio-Insights/
│
├── MarketData.API/              # Simulated market prices (SQLite, Redis, In-Memory)
├── Portfolio.API/               # User portfolios (PostgreSQL + Marten)
│
├── Analytics.API/               # Analytics endpoints (SQL Server)
├── Analytics.Application/       # Application layer
├── Analytics.Domain/            # Domain models and logic
├── Analytics.Infrastructure/    # Data access and integrations
│
├── Notification.API/            # User notifications (SQLite)
│
├── BuildingBlocks/              # Shared CQRS, behaviors, exceptions
├── BuildingBlocks.Messaging/    # Events, DTOs, MassTransit config
│
├── ApiGateway/                  # YARP reverse proxy
│
├── WebApp/                      # Razor Pages client
│
├── docker-compose.yml
├── docker-compose.override.yml
└── README.md
```


## 🧩 Service Responsibilities & Communication

### ✅ Market Data Service

- Provides simulated asset prices
- Exposes gRPC contracts to retrieve prices
- Publishes MarketPricesUpdatedEvent via RabbitMQ
- Uses SQLite, Redis, and in‑memory stores

#### Endpoints

- POST /simulate – Simulate market price update
- GET /logs – Retrieve recent simulation logs
- GET /lastupdate – Retrieve last update timestamp
- GET /health - Perform health checks

### ✅ Portfolio Service

- Manages user portfolios and assets
- Stores data using Marten (Document Store over PostgreSQL)
- Consumes Market Data prices via gRPC
- Publishes portfolio‑related events

#### Endpoints

- POST /create-or-update-portfolio
- POST /add-asset
- DELETE /remove-asset
- GET /get-portfolio
- GET /get-portfolio-valuation
- GET /health - Perform health checks
  
#### Published Events

- PortfolioUpdatedEvent

### ✅ Analytics Service

- Multi‑layered architecture: API, Application, Domain, Infrastructure
- Consumes events from Market Data and Portfolio Services
- Computes portfolio metrics asynchronously
- Stores analytics snapshots in SQL Server
- Publishes analytics results

#### Endpoints

- POST /refresh-analytics
- GET /analytics/by-user
- GET /analytics/history/by-user
- GET /health - Perform health checks
  
#### Consumed Events

- MarketPricesUpdatedEvent
- PortfolioUpdatedEvent

#### Published Events

- AnalyticsComputedEvent

### ✅ Notification Service

- Listens for analytics results
- Stores user notifications
- Uses SQLite for persistence

#### Endpoints

- GET /notifications/by-user
- POST /mark-as-read
- GET /health - Perform health checks
  
#### Consumed Events

- AnalyticsComputedEvent

### 🧩 Events

- MarketPricesUpdatedEvent
- PortfolioUpdatedEvent
- AnalyticsComputedEvent

All events are defined in BuildingBlocks.Messaging and transported via RabbitMQ using MassTransit.

### 🌐 API Gateway (YARP)

The YARP API Gateway serves as the single entry point for the client application, routing requests to the appropriate backend services and simplifying client‑side integration.

### 🖥 Web Client Application

- ASP.NET Core Razor Pages
- Uses Refit for strongly‑typed HTTP clients

Displays:
- Portfolio management UI
- Analytics dashboards
- Notification lists
- Market simulation controls

## ⚙ Getting Started

Clone the Repository
```bash
git clone https://github.com/tigerbluejay/microservices-infrastructure-personal-finance-dashboard.git
cd Portfolio-Insights
```
Run with Docker Compose

Ensure Docker Desktop is running, then execute:

```bash
docker-compose -f docker-compose.yml -f docker-compose.override.yml up -d
```

This will start:

- All microservices
- Databases
- Redis
- RabbitMQ
- API Gateway
- Web Client

### Access the Services
Service	URL	Description
- https://localhost:8082

## 💡 Development Notes

- All endpoints are implemented using Minimal APIs with Carter
- Each service owns its data store
- Communication patterns are intentionally mixed (gRPC + messaging)

The project prioritizes architectural clarity over production‑ready UI

This project is intended as a portfolio and learning showcase, demonstrating how modern .NET microservices can be composed, orchestrated, and evolved in a realistic distributed system.

