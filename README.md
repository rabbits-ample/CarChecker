# CarChecker_Real

Middleware service that connects a License Plate Reader (LPR) system to an automated SMS alerting pipeline. When a vehicle is detected, CarChecker_Real looks up the plate, checks its registration and activation status, and — if the vehicle is registered but not active — sends the owner a reminder text via the Textel API.

## How It Works

1. A vehicle drives past the **License Plate Reader (LPR)**.
2. The LPR pushes a read notification to this middleware.
3. The middleware looks up the plate against **Paylock**, which returns the vehicle, its registration state, active state, and the owner's phone number.
4. If the vehicle is **registered but not active**, the middleware sends a canned reminder message via the **Textel** API.
5. The owner receives a text reminding them to activate their vehicle.

```
Vehicle Owner → LPR → CarChecker_Real → Paylock (lookup)
                                       → Textel (SMS) → Vehicle Owner
```

*(See `/docs/sequence-diagram.png` for the full sequence diagram.)*

## Tech Stack

- ASP.NET Core Web API (.NET 9)
  
## Integrations & Contacts

## Getting Started

### Prerequisites

- .NET 9 SDK
- Docker & Docker Compose
- Access credentials for Paylock and Textel (see Configuration below)


### Configuration

Credentials are supplied via Docker secret files (not environment variables or hardcoded config) and read through `AddKeyPerFile`. Place credential files in a local `secrets/` folder (excluded from git via `.gitignore`):

```
secrets/
├── Paylock__ClientId
├── Paylock__ClientSecret
├── Textel__ClientId
└── Textel__ClientSecret
```


The test suite includes bUnit component tests and Selenium end-to-end tests. Tests do not depend on external API calls — all HTTP dependencies are mocked.

## Project Structure

```
CarChecker_Real/
├── Components/       # Blazor components
├── Controllers/       # e.g. HitController — receives LPR hits
├── Models/             # Car, Token, etc.
├── Services/          # PlateLookupService, TokenService, TextelService
├── secrets/            # local-only, gitignored
└── Test/                # test project
```

## Status

🚧 In development.
