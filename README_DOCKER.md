Docker setup for MediKartX API

Quickstart (dev):

1. Create a `.env` file in the repo root with your secrets. Example:

```
JWT_KEY=your_jwt_key_here
JWT_ISSUER=mediKart
JWT_AUDIENCE=mediKartClients
```

2. Start with docker-compose:

```bash
docker compose up --build
```

This will:
- Start a SQL Server container (listening on host 1433)
- Build and run the `MediKartX.API` container and expose it at https://127.0.0.1:5127

Notes:
- The compose file uses a development SA password (`Your_strong!Passw0rd`). Replace it before using in any shared environment.
- The service expects the following environment variables (set via `.env` or your CI): `JWT_KEY`, `JWT_ISSUER`, `JWT_AUDIENCE`.
- The API reads DB connection from `DB_CONNECTION` environment variable (compose sets it to point to the `db` service).

Useful commands:

- Rebuild and start in foreground:

```bash
docker compose up --build
```

- Start detached:

```bash
docker compose up -d --build
```

- Stop and remove containers:

```bash
docker compose down
```

- View API container logs:

```bash
docker compose logs -f api
```

- Run `dotnet ef database update` inside the api container (if you choose to apply migrations there):

```bash
# get a shell in the running container
docker compose exec api bash
# inside container
dotnet ef database update --project ../MediKartX.Infrastructure -s MediKartX.API
```

Security:
- Do NOT store production secrets in the compose file. Use a secret manager or CI/CD secrets.
- For production deployment use a managed SQL instance and secure credentials.
