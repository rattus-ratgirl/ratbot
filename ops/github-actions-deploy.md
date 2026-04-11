# GitHub Actions Deploy Notes

## Workflows

- `.github/workflows/ci-format.yml`
    - Runs formatting, build, and tests
    - Triggered on pull requests targeting `master` and pushes to `master`

- `.github/workflows/build-image.yml`
    - Builds the bot container image from `Dockerfile`
    - Pushes to GHCR
    - Triggered on push to `master`, plus manual dispatch
    - Publishes tags:
        - immutable: `sha-<full_commit_sha>`
        - branch: `master`
        - moving aliases for staging candidates: `latest-staging`, `staging`

- `.github/workflows/deploy-vps.yml`
    - Auto deploy mapping:
        - `master` build completion from a push event -> deploy `staging`
    - Manual dispatch supports:
        - `shared`
        - `production`
        - `staging`
    - Uses explicit Compose project names:
        - `ratbot-shared`
        - `ratbot-production`
        - `ratbot-staging`
    - Prevents overlap with per-target concurrency groups

## Required GitHub Secrets and Variables

Recommended names:

- Secret: `VPS_SSH_PRIVATE_KEY`
    - Private key for CI/CD SSH access
    - Key should match `/home/deploy/.ssh/authorized_keys` on VPS
- Secret: `VPS_SSH_KNOWN_HOSTS` (recommended)
    - Output of `ssh-keyscan -H <host>` for strict host key pinning
- Secret: `VPS_HOST`
    - VPS hostname or IP
- Secret: `VPS_PORT` (optional)
    - SSH port, defaults to `22` when empty
- Secret: `VPS_USER` (optional)
    - Defaults to `deploy` when empty

You can store these at repository scope, or per GitHub Environment (`shared`, `staging`, `production`) for stronger
isolation/approval controls.

## VPS-side Prerequisites

- Directories must already exist on the VPS:
    - `/opt/ratbot/shared`
    - `/opt/ratbot/production`
    - `/opt/ratbot/staging`
- The deploy helper syncs non-secret compose/config assets from the repository to the VPS before running Docker Compose.
- The workflow still does not create or upload real `.env` files.
- Each stack directory must therefore already contain a valid server-side `.env` file before deployment can succeed.
- `deploy` user must:
    - Be allowed to SSH
    - Be able to run Docker/Compose (typically via `docker` group)
- Docker Compose plugin must be installed on the VPS.

## GHCR Pull Authentication Model

The deploy workflow does not send GHCR credentials from GitHub Actions to the VPS.  
Expected model: the VPS `deploy` user is already authenticated for pulls.

One-time setup on VPS (as `deploy`):

```bash
docker login ghcr.io
```

This stores credentials for future `docker compose pull`.

For private images, use a GitHub token/PAT with `read:packages` scope for that login.

## Manual Deploy Usage

Use **Actions -> Deploy RatBot to VPS -> Run workflow**:

- `target=shared`: deploy shared stack only
- `target=production`: deploy production bot stack
    - Requires `image_tag`
- `target=staging`: deploy staging bot stack
    - Defaults to `latest-staging` when `image_tag` is omitted
- `image_tag` for production/staging:
    - Tag only: `sha-<commit_sha>` or `latest-staging`
    - Or full image ref: `ghcr.io/<owner>/<repo>:sha-<commit_sha>`

Recommended production flow:

- Let `master` auto-deploy to staging using the immutable `sha-<commit_sha>` image
- Promote that exact tested image to production manually by supplying the same `sha-<commit_sha>`

For bot stacks, the deploy helper also writes the deployed `RATBOT_IMAGE` back to the server-side `.env` so later manual
`docker compose` runs do not drift to an older moving tag.

The deploy helper writes simple rollback state on the VPS per stack:

- `.deploy-state/current-image.txt`
- `.deploy-state/previous-image.txt`
