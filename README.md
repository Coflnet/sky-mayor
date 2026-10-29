## Sky Mayor Service

Service to store the voting and result of the mayor elections.

## Start

-   docker compose up
-   go get
-   go run ./cmd/sky-mayor

### Configuration

See .env.dist

## Internal PKI rollout

Stage the `CASSANDRA:TLS:{CA_CERT_PATH,CERT_PATH,KEY_PATH,SERVER_NAME}` files and
settings before deploying Core 0.7.5; its legacy PFX fallback no longer accepts
untrusted certificates. The current legacy CQL server identity is `sky-client`;
use the actual CQL CA and shared-root bundle, independently of internode trust.
The workload needs a private writable ephemeral `HOME` for public intermediate
certificates. Credential loading and the existing updater remain unchanged.

`HTTPS:TLS` uses the same three PEM file paths for a server-purpose fullchain/key.
It adds native HTTPS 8443 and preserves HTTP 8080. Client certificates are
requested and validated when present; `REQUIRE_CLIENT_CERTIFICATE` defaults to
false for migration and can require clients in a later phase. New handshakes
reload certificates and trust without a restart.

A bounded CQL `SELECT key FROM system.local` runs before HTTP startup and on
`/health/ready`. Preserve the existing business readiness probe during initial
promotion, then enable the new database readiness path after verifying the exact
candidate. The one-minute connection guard must be attached before promotion.
