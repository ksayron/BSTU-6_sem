# Lab 9 - Unit Testing (PersonalKnowledgeBot)

## What was selected for testing

This lab uses real code from:

- `D:\Projects\PKM_TGBOT\PersonalKnowledgeBot\src\PKB.Domain`
- `D:\Projects\PKM_TGBOT\PersonalKnowledgeBot\src\PKB.Application`

Tested components:

- `PKB.Domain.Entities.KnowledgeItem`
- `PKB.Application.Services.KnowledgeItemService`
- `PKB.Application.Services.DigestService`

These components were selected because they contain meaningful business behavior:

- creation/state-transition logic (`KnowledgeItem`)
- orchestration and validation-like decision branches (`KnowledgeItemService`)
- aggregation logic for weekly statistics (`DigestService`)

## Test scope

Total tests: **15**

- Happy path tests
- Invalid/unsupported input tests
- Edge cases
- Duplicate detection behavior
- Exception expectation (`ArgumentException` for unsupported item type)
- Dependency isolation checks (ensuring save/add/publish/enqueue are or are not called)

## Test doubles used

To keep tests unit-level and independent from infrastructure:

- `FakeKnowledgeItemRepository` (stub + call tracking)
- `FakeTagRepository` (stub + call tracking)
- `FakeUnitOfWork` (call tracking)
- `FakeMediator` (captures published notifications)
- `FakeBackgroundJobClient` (captures enqueued jobs)

No real DB, network, file storage, Telegram transport, or API controllers are involved.

## Project structure

- `PKB.Lab9.UnitTests.csproj`
- `Domain/KnowledgeItemTests.cs`
- `Application/KnowledgeItemServiceTests.cs`
- `Application/DigestServiceTests.cs`
- `TestDoubles/Fakes.cs`
- `README.md`

## How to run tests

From `D:\BSTU\6sem\TPO\lab9`:

```powershell
dotnet test
```

If your environment has restricted ACLs for local `obj/bin` folders, run with temp output paths:

```powershell
dotnet test --no-restore `
  -p:MSBuildProjectExtensionsPath=C:\Users\user\AppData\Local\Temp\pkb-lab9\obj\ `
  -p:BaseIntermediateOutputPath=C:\Users\user\AppData\Local\Temp\pkb-lab9\obj\ `
  -p:BaseOutputPath=C:\Users\user\AppData\Local\Temp\pkb-lab9\bin\
```

## How to run coverage

```powershell
dotnet test --collect:"XPlat Code Coverage"
```

Coverage file will be generated under:

- `D:\BSTU\6sem\TPO\lab9\TestResults\...\coverage.cobertura.xml`

ACL-restricted environment variant:

```powershell
dotnet test --no-restore --collect:"XPlat Code Coverage" `
  --results-directory C:\Users\user\AppData\Local\Temp\pkb-lab9\TestResults `
  -p:MSBuildProjectExtensionsPath=C:\Users\user\AppData\Local\Temp\pkb-lab9\obj\ `
  -p:BaseIntermediateOutputPath=C:\Users\user\AppData\Local\Temp\pkb-lab9\obj\ `
  -p:BaseOutputPath=C:\Users\user\AppData\Local\Temp\pkb-lab9\bin\
```

## Current limitations

- The lab project references compiled assemblies (`PKB.Application.dll`, `PKB.Domain.dll`) from the real project build output.  
  In this sandbox environment, `ProjectReference` restore/build to external folder trees is restricted by filesystem ACLs.
- Tests focus on domain and application services, not on API controllers, Telegram transport handlers, or infrastructure wiring.
