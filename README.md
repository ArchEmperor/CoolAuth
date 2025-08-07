#CoolAuth
CoolAuth — a clean, production-ready authentication template for ASP.NET Core 9.
Implements JWT access tokens, refresh tokens, magic links, session caching in Redis, and persistence in PostgreSQL.
Perfect starting point for secure APIs and microservices.

#Highlights
ASP.NET Core 9 authentication boilerplate.

Access & Refresh tokens with rotation support.

Magic link passwordless login.

Redis for fast session lookup and alias keys.

PostgreSQL for persistent user/session storage.

Clean architecture with DTOs, AutoMapper, and service separation.

#Requirements
.NET 9 SDK

PostgreSQL (≥ 12)

Redis (≥ 6)

Docker (optional, recommended for local dev)

#Quick Start
Clone repository
git clone https://github.com/your-org/coolauth.git
cd coolauth
Copy environment file .env.example to .env and configure
cp .env.example .env
Edit .env with your Postgres, Redis, and JWT settings.





