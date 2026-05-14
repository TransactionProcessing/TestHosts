# TestHosts

## Mobile Wallet test API

The `TestHosts` web application now includes a GSMA-style mobile wallet test API under `mobilemoney/v1_0` with:

- OAuth2 client-credentials token issuance via `POST /oauth/token`
- account creation, lookup, status, balance, and transaction history endpoints
- idempotent transaction and reversal creation using the `X-Idempotency-Key` header
- asynchronous transaction and reversal completion via a background processor
- webhook subscriptions plus per-request callback URLs for event delivery
- audit entries for token, account, transaction, reversal, and webhook operations
- SQL Server or in-memory persistence using the `MobileWalletReadModel` connection string

Default seeded OAuth2 client credentials are configured in `appsettings*.json` under `MobileWallet:OAuth`.
