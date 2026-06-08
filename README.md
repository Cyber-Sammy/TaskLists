# TaskListsApp

TaskListsApp is a test Web API for managing task lists, sharing access to lists, and managing task items inside lists.

## Tech stack

- .NET 10 Web API
- MongoDB
- Serilog
- Scalar/OpenAPI
- TypeScript API provider

## Project structure

- `src/TaskLists.Api` - HTTP transport layer, controllers, middleware, request/response models.
- `src/TaskLists.Application` - use cases, service contracts, validators, repository contracts, application models.
- `src/TaskLists.Domain` - domain entities, domain factory, mutators, domain rules.
- `src/TaskLists.Infrastructure.Mongo` - MongoDB documents, mappings, repositories, indexes, configuration.
- `src/TaskLists.SharedKernel` - shared primitives such as `Result`.
- `client/api-provider` - TypeScript API provider without UI components.
- `tests/TaskLists.Tests` - test project.

## Run MongoDB locally

```powershell
docker compose up -d mongo
```

Default connection settings:

```text
mongodb://tasklists_admin:tasklists_password@localhost:27017
Database: tasklists
```

They are configured in `src/TaskLists.Api/appsettings.Development.json`.

## Run API

```powershell
dotnet run --project src/TaskLists.Api/TaskLists.Api.csproj
```

In Development, OpenAPI is available at:

```text
/openapi/v1.json
```

Scalar API reference is available at:

```text
/scalar/v1
```

## Current user

The API has no authentication by task requirements. Current user is passed with request header:

```text
X-User-Id: <user-guid>
```

## Development users API

`/api/users` is a development helper for creating and reading users during manual testing.

It is controlled by:

```json
{
  "ApiFeatures": {
    "UsersApiEnabled": true
  }
}
```

The base `appsettings.json` disables it by default. `appsettings.Development.json` enables it.

## TypeScript API provider

Build:

```powershell
cd client/api-provider
npm install
npm run build
```

Usage example:

```ts
import { TaskListsApiProvider } from '@task-lists/api-provider';

const api = new TaskListsApiProvider({
  baseUrl: 'https://localhost:7001',
  currentUserId: '00000000-0000-0000-0000-000000000000',
  correlationIdFactory: () => crypto.randomUUID(),
});

const lists = await api.getTaskLists({
  page: 1,
  pageSize: 20,
  sortBy: 'CreatedAt',
  sortDirection: 'Descending',
});
```

The provider contains:

- typed request/response models;
- common `ApiClient`;
- `ApiError` and `NetworkError`;
- request configuration for base URL, current user header, default headers, custom fetch and correlation id;
- convenient methods for task lists, members, task items and development users API.

## Verification

```powershell
dotnet test TaskListsApp.slnx
cd client/api-provider
npm run build
```
