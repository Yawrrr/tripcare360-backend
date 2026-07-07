# Deploying TripCare360 Backend to Azure (Student Subscription)

Target architecture — smallest footprint that runs the whole stack:

```
Namecheap DNS (tripcare360.me)
  api.tripcare360.me   ──A──►  Azure VM public IP
  files.tripcare360.me ──A──►  Azure VM public IP
  mail.tripcare360.me  ──A──►  Azure VM public IP

┌─ Azure VM: Ubuntu 24.04, Standard_B1s (FREE 750 h/month, first 12 months) ─┐
│  Caddy (80/443, auto-HTTPS via Let's Encrypt)                              │
│    ├─► api.…  → webapi container (:8080)                                   │
│    ├─► files.… → minio container (:9000)   [presigned URLs for browser]    │
│    └─► mail.…  → mailpit container (:8025) [HTTP Basic Auth-gated]         │
│  mailpit also reachable at 127.0.0.1:8025 via SSH tunnel, no auth needed   │
└─────────────────────────────────────────────────────────────────────────────┘
        │
        ▼
Azure SQL Database — serverless free offer (100k vCore-seconds/month, 32 GB)

External mock server: already hosted elsewhere; URL injected via .env secret.

CI/CD: push to main → GitHub Actions builds the image on a GitHub runner,
pushes it to GHCR, SSHes into the VM, renders deploy/.env from repo secrets,
pulls the new image and restarts the stack. The VM never compiles anything.
```

Estimated cost during the demo period: **≈ $0/month** (B1s free hours + Azure SQL
free offer + free bandwidth allowance). The only trickle is the static public IP
(~$3–4/month), billed against your $100 credit.

---

## 1. Azure resources (run in Azure Cloud Shell — Bash)

Portal → click the Cloud Shell icon (>_) → Bash. No local install needed.

```bash
# Variables — adjust names/region (southeastasia = Singapore, closest to MY;
# sql server name must be globally unique, lowercase)
RG=tripcare360-rg
LOC=southeastasia
VM=tripcare360-vm
SQLSRV=tripcare360-sql        # change if name is taken
SQLADMIN=tripcaresqladmin
SQLPASS='<PICK-A-STRONG-PASSWORD>'

az group create -n $RG -l $LOC

# ---- VM (B1s = free 750 h/month for the first 12 months of the student sub) ----
az vm create -g $RG -n $VM \
  --image Ubuntu2404 \
  --size Standard_B1s \
  --admin-username azureuser \
  --generate-ssh-keys \
  --public-ip-sku Standard \
  --os-disk-size-gb 30

az vm open-port -g $RG -n $VM --port 80,443

VMIP=$(az vm show -g $RG -n $VM -d --query publicIps -o tsv)
echo "VM public IP: $VMIP"

# ---- Azure SQL (serverless FREE offer: 100k vCore-sec + 32 GB per month) ----
az sql server create -g $RG -n $SQLSRV -l $LOC -u $SQLADMIN -p "$SQLPASS"

az sql db create -g $RG -s $SQLSRV -n TripCare360Db \
  -e GeneralPurpose -f Gen5 -c 2 --compute-model Serverless \
  --use-free-limit --free-limit-exhaustion-behavior AutoPause \
  --backup-storage-redundancy Local

# Allow the VM and your home IP (for running migrations from Visual Studio)
az sql server firewall-rule create -g $RG -s $SQLSRV -n allow-vm \
  --start-ip-address $VMIP --end-ip-address $VMIP
az sql server firewall-rule create -g $RG -s $SQLSRV -n allow-home \
  --start-ip-address <YOUR_HOME_IP> --end-ip-address <YOUR_HOME_IP>   # whatismyip.com
```

> Cloud Shell saved your SSH private key at `~/.ssh/id_rsa`. Download it
> (Cloud Shell → Manage files) or run all later SSH steps from Cloud Shell itself.

Connection string (used in step 4 and 5):

```
Server=tcp:<SQLSRV>.database.windows.net,1433;Initial Catalog=TripCare360Db;User ID=<SQLADMIN>;Password=<SQLPASS>;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

---

## 2. Namecheap DNS

Namecheap dashboard → Domain List → **tripcare360.me** → Advanced DNS → add:

| Type     | Host    | Value            | TTL       |
|----------|---------|------------------|-----------|
| A Record | `api`   | `<VM public IP>` | Automatic |
| A Record | `files` | `<VM public IP>` | Automatic |
| A Record | `mail`  | `<VM public IP>` | Automatic |

Wait until `nslookup api.tripcare360.me` resolves (usually < 5 min). Caddy can
only obtain certificates **after** DNS resolves — don't start the stack before.

---

## 3. Prepare the VM

```bash
ssh azureuser@<VM public IP>
```

```bash
# 3a. Docker
curl -fsSL https://get.docker.com | sudo sh
sudo usermod -aG docker azureuser
exit   # log out and SSH back in so the docker group applies

# 3b. Swap — optional safety headroom (CI builds the image, not the VM,
# so 1 GB RAM is enough; swap just protects against memory spikes)
sudo fallocate -l 2G /swapfile
sudo chmod 600 /swapfile
sudo mkswap /swapfile
sudo swapon /swapfile
echo '/swapfile none swap sw 0 0' | sudo tee -a /etc/fstab
```

That's all the VM needs — no git clone, no .NET SDK. GitHub Actions delivers
everything else.

---

## 4. GitHub repo secrets & first deploy

The workflow is [.github/workflows/deploy.yml](../.github/workflows/deploy.yml).
It runs on every push to `main` (or manually via the Actions tab → *Build &
Deploy Backend* → Run workflow).

GitHub repo → Settings → Secrets and variables → Actions → **New repository
secret**, add all of these:

| Secret | Value |
|---|---|
| `VM_HOST` | VM public IP (or `api.tripcare360.me` once DNS resolves) |
| `VM_USER` | `azureuser` |
| `VM_SSH_KEY` | Contents of the **private** key that pairs with the VM (Cloud Shell: `cat ~/.ssh/id_rsa`) — full text including the BEGIN/END lines |
| `SQL_CONNECTION_STRING` | The Azure SQL connection string from step 1 |
| `JWT_SECRET` | Fresh random string — generate with `openssl rand -base64 48`. Do **not** reuse the appsettings.Local.json one |
| `EXTERNAL_SERVICES_BASE_URL` | Public URL of the hosted mock server (this keeps it out of the repo) |
| `MINIO_ACCESS_KEY` | New value, e.g. `tripcareAdmin` |
| `MINIO_SECRET_KEY` | New strong random value |
| `MAILPIT_USER` | Username for the Mailpit UI, e.g. `admin` |
| `MAILPIT_PASSWORD_HASH` | Bcrypt hash for the Mailpit UI password — see step 6 for how to generate it |

The domain is not a secret — it's set as `DOMAIN: tripcare360.me` at the top
of the workflow file.

Then deploy:

```bash
git push origin main     # or trigger manually from the Actions tab
```

The workflow builds the image, pushes it to
`ghcr.io/yawrrr/tripcare360-webapi`, copies `deploy/docker-compose.yml` +
`Caddyfile` to the VM, writes `~/tripcare360/deploy/.env` from the secrets
above, and runs `docker compose up -d`.

> **Why secrets aren't in the Docker image:** anything baked into an image at
> build time can be read by whoever pulls it (`docker history`, layer
> inspection). The image stays generic; secrets only ever exist in GitHub's
> encrypted store and in the `.env` file on the VM (chmod 600).

---

## 5. Database migrations (run manually from Visual Studio)

Package Manager Console, default project `Tripcare360.Infrastructure`:

```
PM> Update-Database -Project Tripcare360.Infrastructure -StartupProject Tripcare360.WebApi -Connection "Server=tcp:<SQLSRV>.database.windows.net,1433;Initial Catalog=TripCare360Db;User ID=<SQLADMIN>;Password=<SQLPASS>;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

> The serverless DB auto-pauses when idle. The first connection after a pause
> can take ~30–60 s or fail once — just retry.

After migrating, restart the API so `DbSeeder` seeds the admin user (on the VM):

```bash
cd ~/tripcare360/deploy && docker compose restart webapi
```

---

## 6. Verify

```bash
# HTTPS + envelope response (expect a 4xx JSON envelope, which proves the API is up)
curl -i https://api.tripcare360.me/api/policy/verify -X POST \
  -H "Content-Type: application/json" -d '{}'

# MinIO reachable (expect 403 XML — that's healthy)
curl -i https://files.tripcare360.me

# Logs
docker compose logs -f webapi
```

## 6a. Mailpit UI (view "sent" emails)

**Public URL** (needs `MAILPIT_USER`/`MAILPIT_PASSWORD_HASH` secrets set and the
`mail` DNS record from step 2): open `https://mail.tripcare360.me`, log in with
the browser's Basic Auth prompt using `MAILPIT_USER` and the plaintext password
you hashed.

To generate the hash for a chosen password (run once, on the VM, using the
`caddy` image already pulled by the stack):

```bash
ssh azureuser@<VM public IP>
docker run --rm caddy:2 caddy hash-password --plaintext 'YourChosenPassword'
```

Copy the output (starts with `$2a$...`) into the `MAILPIT_PASSWORD_HASH` GitHub
secret, then re-run the deploy workflow so Caddy picks it up.

**Alternative — SSH tunnel** (no public exposure, no secrets needed):

```bash
ssh -L 8025:localhost:8025 azureuser@<VM public IP>
# then open http://localhost:8025
```

---

## 7. Point the frontend at it

In `tripcare360-web`, set the API base URL env var to `https://api.tripcare360.me`.
If the frontend gets deployed later (e.g. Vercel on `tripcare360.me`), its origin
is already whitelisted in CORS via `Cors__AllowedOrigins__*` in
`deploy/docker-compose.yml` — add more origins there and `docker compose up -d`.

---

## Updating after code changes

```bash
git push origin main   # GitHub Actions rebuilds and redeploys automatically
```

Watch progress in the repo's **Actions** tab. Manual fallback (build directly
on the VM, needs the repo cloned there and the swapfile from step 3b):

```bash
cd ~/tripcare360/deploy && docker compose up -d --build
```

## Cost guardrails

- **B1s free hours**: 750 h/month covers one VM 24/7 for the first 12 months.
- **Azure SQL free offer**: with `AutoPause` exhaustion behavior it can never bill.
- Check burn: Portal → Cost Management → Cost analysis.
- When the demo is over: `az group delete -n tripcare360-rg` removes everything.

## Secrets checklist

| Secret | Source of truth | Committed? |
|---|---|---|
| SQL admin password | GitHub secret → rendered into VM `.env` | No |
| JWT secret (new, not the Local.json one) | GitHub secret → VM `.env` | No |
| Mock server URL | GitHub secret → VM `.env` | No |
| MinIO credentials | GitHub secret → VM `.env` | No |
| Mailpit basic-auth user + password hash | GitHub secret → VM `.env` | No |
| VM SSH private key | GitHub secret only | No |

To rotate any secret: update it in GitHub → re-run the workflow. The `.env`
on the VM is overwritten on every deploy.
