# Crypto Watchlist API (Backend)

A robust, secure C# .NET 8 Web API that serves as the backbone for the Live Cryptocurrency Watchlist. This backend handles user authentication, data validation, and aggregates real-time and historical cryptocurrency data securely via the CoinGecko API.

---

## 🚀 Features

* **Secure Authentication:** Endpoints for user registration and login.
* **Input Validation:** Enforces secure data rules (e.g., passwords must be between 8 and 16 characters long).
* **Authenticated API Aggregation:** Securely communicates with the CoinGecko Demo API using developer keys.
* **Database Integration:** Built with Entity Framework Core, connecting seamlessly to a Supabase PostgreSQL instance.
* **Safe Error Handling:** Smart fallbacks ensure the API remains resilient even if external endpoints hit rate limits.

---

## 🛠️ Tech Stack

* **Framework:** .NET 8 Core Web API
* **Database Provider:** Entity Framework Core (EF Core)
* **Database Cloud Host:** Supabase (PostgreSQL)
* **External Data Provider:** CoinGecko API v3

---

## 🔐 Environment Variables & Configuration

To protect sensitive keys and credentials, this project utilizes environment variables. **Never hardcode passwords or API keys in the source files.**

Ensure the following variables are configured in your hosting environment (e.g., Render Dashboard) or your local `appsettings.json` (do not commit this file to GitHub if it contains active keys):

| Variable Key | Description |
| :--- | :--- |
| `COINGECKO_API_KEY` | Your registered CoinGecko developer/demo key. |
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string pointing to your Supabase instance. |

---

## 📡 API Endpoints

### Authentication
* `POST /api/watchlist/register` - Registers a new user. 
    * *Validation:* Enforces unique usernames and passwords between 8 and 16 characters.
* `POST /api/watchlist/login` - Authenticates a user and returns their identifier profile.

### Watchlist Management
* `GET /api/watchlist/{userId}/portfolio` - Retrieves the live status, images, and current valuations of all tracked assets.
* `POST /api/watchlist/coins` - Validates and appends a cryptocurrency asset into the database tracking system.
* `GET /api/watchlist/historical/{coinId}` - Fetches sequential historical pricing trends used to compile interactive frontend charts.

---

## 💻 Local Setup Instructions

### Prerequisites
* [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
* An active Supabase project (for the database)
* A CoinGecko Demo account API Key

### Running Locally

1. **Clone the repository:**
   ```bash
   git clone <your-backend-repo-url>
   cd <your-backend-repo-folder>

2. **Configure your local secrets:**
Set up your temporary local environment variables or update your appsettings.Development.json:

JSON
{
  "ConnectionStrings": {
    "DefaultConnection": "YOUR_SUPABASE_CONNECTION_STRING"
  },
  "COINGECKO_API_KEY": "YOUR_COINGECKO_API_KEY"
}

3. **Restore dependencies and build:**

Bash

dotnet restore
dotnet build
Run the API application:

Bash

dotnet run
The application will boot up locally (typically available at https://localhost:7036 or similar depending on your launch profiles).

🛡️ Production & Deployment Notes
This service is optimized for hosting on Render.

Cold Starts: Since this runs on a free web service instance, it may go to sleep after periods of inactivity. The first API request after a period of rest might experience a 50-60 second delay while the container spins back up.

Security Architecture: External pricing components actively extract credentials securely via Environment.GetEnvironmentVariable("COINGECKO_API_KEY"), protecting our developer network footprint from scraping vulnerabilities.