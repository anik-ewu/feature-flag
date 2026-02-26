# Feature Flag SaaS Platform

A full-stack Feature Flag management platform designed to allow engineering teams to progressively decouple code deployments from feature releases. Built with a robust **.NET 8 Clean Architecture** backend and a modern **Angular 17 Standalone** frontend.

## 🏗️ Architecture & Tech Stack

### Backend API (`/Backend`)
The backend is structured using **Clean Architecture** (Domain, Application, Infrastructure, Presentation) principles to ensure high maintainability and segregation of concerns.

*   **.NET 8 Web API**: Core framework serving RESTful endpoints.
*   **Entity Framework Core & PostgreSQL**: Relational database persistence layer.
*   **Redis**: In-memory data store using Cache-Aside patterns (with SemaphoreSlim stampede protection) for ultra-fast flag evaluations.
*   **MediatR**: Implementation of the CQRS (Command Query Responsibility Segregation) pattern for decoupling API controllers from business logic.
*   **FluentValidation**: Strictly typed request validation models.
*   **Serilog**: Structured application logging.

### Frontend Dashboard (`/Frontend`)
The administrative dashboard used by product managers and engineers to toggle flags, configure rollout percentages, and manage dynamic user targeting rules.

*   **Angular 17**: Utilizing the latest Standalone Components architecture.
*   **Angular Material**: Extensive use of Material Design components (Dialogs, Data Tables, Sliders, Forms).
*   **RxJS**: Reactive programming for state and API event management.

---

## 🚀 Getting Started

### 1. Database & Infrastructure
The backend connects to **PostgreSQL** (Core relational data) and **Redis** (Feature Flag evaluations caching).
Make sure you have both engines running. For example, using Docker:

```bash
# Run PostgreSQL
docker run --name pg-feature-flags -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=FeatureFlagsDb -p 5432:5432 -d postgres

# Run Redis
docker run --name redis-feature-flags -p 6379:6379 -d redis
```

### 2. Running the Backend (.NET)
The backend API is configured to run on port `5120`.

```bash
cd Backend/FeatureFlags.Api

# (First Time Only) Run Entity Framework Migrations
dotnet ef database update

# Boot the API server
dotnet run
```
> **Note:** Upon the first successful `dotnet run`, the application will execute a Seed script to inject a dummy `Tenant` and `Project` into the database to allow immediate frontend testing without needing to build full Tenant onboarding flows first.

The API Swagger Documentation will be available at: `http://localhost:5120/swagger`

### 3. Running the Frontend (Angular)
The Angular UI runs on standard port `4200` and proxy-connects to the backend via the `environment.ts` configuration.

```bash
cd Frontend

# Install node dependencies
npm install

# Start the Angular development server
ng serve
```

Access the visual dashboard by navigating to `http://localhost:4200` in your browser.

---

## 🛠️ Core Features

1.  **Multi-Tenancy Setup**: (Base logic implemented) Flags are grouped strictly by `Project` which in turn belong to organizations/`Tenant`s.
2.  **Environment Specificity**: Define flags for *Development*, *Staging*, or *Production* independently.
3.  **Percentage Rollouts**: Dial up a feature from 0% to 100% using consistent-hashing to ensure users have a sticky experience.
4.  **Targeting Rules**: Dynamically apply inclusion/exclusion rules based on specific logical operators (e.g. `Equals`, `Contains`, `StartsWith`).