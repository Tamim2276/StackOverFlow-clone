# StackOverflow Lite — ASP.NET Core Web API

A simplified Q&A REST API built with **Clean Architecture**, **CQRS via MediatR**, **Entity Framework Core + PostgreSQL**, **Redis caching**, and **JWT authentication**.

---

## Project Structure

```
StackOverflowLite/
├── StackOverflowLite.Domain/          # Entities, enums — no dependencies
├── StackOverflowLite.Application/     # CQRS handlers, interfaces, DTOs
├── StackOverflowLite.Infrastructure/  # JWT service, Redis cache
├── StackOverflowLite.Persistence/     # EF Core DbContext, configurations, migrations
└── StackOverflowLite.Api/             # Controllers, middleware, Program.cs
```

---

## Quick Start

### 1. Start dependencies
```bash
docker-compose up -d
```
This starts PostgreSQL (5432) and Redis (6379).

### 2. Configure `appsettings.Development.json`
Update the connection strings and JWT secret if needed.

### 3. Run migrations & start API
```bash
cd StackOverflowLite.Api
dotnet run
```
Migrations are applied automatically on startup.  
Swagger UI is available at **http://localhost:5000** (root).

### 4. Manual migration (optional)
```bash
dotnet ef migrations add InitialCreate \
  --project StackOverflowLite.Persistence \
  --startup-project StackOverflowLite.Api \
  --output-dir Migrations

dotnet ef database update \
  --project StackOverflowLite.Persistence \
  --startup-project StackOverflowLite.Api
```

---

## API Endpoints

### Auth
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/auth/register` | ❌ | Register and receive JWT |
| POST | `/api/auth/login` | ❌ | Login and receive JWT |
| GET | `/api/auth/profile` | ✅ | Get own profile |

### Questions
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/questions` | ❌ | List questions (pagination, tag filter, search) |
| GET | `/api/questions/{id}` | ❌ | Get question with all answers |
| POST | `/api/questions` | ✅ | Create a question |
| PUT | `/api/questions/{id}` | ✅ | Update own question |
| DELETE | `/api/questions/{id}` | ✅ | Delete own question |

**Query params for GET /api/questions:** `page`, `pageSize`, `tag`, `search`

### Answers
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/questions/{questionId}/answers` | ✅ | Post an answer |
| PUT | `/api/questions/{questionId}/answers/{answerId}` | ✅ | Edit own answer |
| DELETE | `/api/questions/{questionId}/answers/{answerId}` | ✅ | Delete own answer |
| POST | `/api/questions/{questionId}/answers/{answerId}/accept` | ✅ | Accept answer (question author only) |

### Votes
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/votes` | ✅ | Upvote or downvote; calling again toggles off |

**Request body:**
```json
{
  "target": "Question",    // or "Answer"
  "targetId": "guid",
  "voteType": "Upvote"     // or "Downvote"
}
```

### Tags
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/tags` | ❌ | List all tags sorted by usage |

---

## Architecture Notes

- **Domain** — pure C# entities with no external dependencies.
- **Application** — MediatR `IRequest`/`IRequestHandler` for every use case. Interfaces (`IApplicationDbContext`, `IJwtService`, `ICacheService`, `ICurrentUserService`) defined here, implemented in outer layers.
- **Infrastructure** — JWT token generation (`JwtService`), Redis distributed caching (`RedisCacheService` using `StackExchange.Redis`).
- **Persistence** — EF Core with PostgreSQL via Npgsql. Fluent API configurations in `Configurations/`. Auto-migration on startup.
- **API** — Thin controllers delegating entirely to MediatR. Global exception middleware. Swagger with Bearer auth support.

### Key behaviours
- View counts are incremented atomically in Redis (fire-and-forget) for fast read paths.
- Voting toggles: same vote type removes the vote; opposite type switches direction. Users cannot vote on their own content.
- Only the question author can accept an answer; accepting a new one automatically un-accepts the previous.
- Tag names are normalised to lowercase and deduplicated.
# StackOverFlow-clone
