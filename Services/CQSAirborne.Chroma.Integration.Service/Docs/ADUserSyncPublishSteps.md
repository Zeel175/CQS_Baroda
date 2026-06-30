# AD User Sync Hangfire Publish Steps

Use these steps to publish `CQSAirborne.Chroma.Integration.Service` as a separate IIS site for Hangfire. The API site can continue to expose Swagger at its existing URL, and the Hangfire dashboard will be opened from the new Hangfire site.

## 1. Keep API and Hangfire as separate IIS sites

Recommended IIS layout:

| Purpose | IIS site | Physical path | URL |
| --- | --- | --- | --- |
| Main API and Swagger | `CQS-API` | API publish folder | `https://api-server/swagger` |
| Hangfire dashboard and worker | `CQS-Hangfire` | Chroma Integration publish folder | `https://hangfire-server/scheduler` |

Do not publish the Hangfire worker files into the API site. Publish `CQSAirborne.Services.API` to the API site and publish `CQSAirborne.Chroma.Integration.Service` to the Hangfire site.

## 2. Update Hangfire configuration before publishing

In `Services/CQSAirborne.Chroma.Integration.Service/appsettings.json`, verify these values:

- `ConnectionStrings:DefaultConnection` points to the application database where `adm_Employee` exists.
- `ConnectionStrings:HangfireConnection` points to the Hangfire database used by the Hangfire server.
- `ADUserSync:CronExpression` controls the schedule. `*/5 * * * *` runs every five minutes.
- `ADUserSync:LdapPaths` contains each AD OU that should be scanned.
- `ADUserSync:DefaultPlantId`, `ADUserSync:DefaultOrgRole`, and `ADUserSync:DefaultCreatedBy` contain fallback values for inserted users.
- `ADUserSync:DepartmentMappings` contains any AD department names that must be normalized before saving.

## 3. Publish the Hangfire site

From the repository root, run:

```powershell
dotnet publish Services/CQSAirborne.Chroma.Integration.Service/CQSAirborne.Chroma.Integration.Service.csproj -c Release -o C:\Publish\CQS-Hangfire
```

Copy the generated files from `C:\Publish\CQS-Hangfire` to the Hangfire server, for example:

```text
C:\inetpub\wwwroot\CQS-Hangfire
```

## 4. Create the separate IIS site on the Hangfire server

1. Install the .NET 8 Hosting Bundle on the Hangfire server.
2. Open IIS Manager.
3. Create a new site named `CQS-Hangfire`.
4. Set **Physical path** to the copied publish folder, for example `C:\inetpub\wwwroot\CQS-Hangfire`.
5. Configure a binding for the Hangfire host name or port, for example `https://hangfire-server` or `http://hangfire-server:8081`.
6. Set the application pool to **No Managed Code**.
7. Run the application pool under a domain/service account that has access to Active Directory and SQL Server.
8. Grant that account read/write permission to any configured log folder such as `ADUserSync:LogPath`.
9. Start the site and browse to `https://hangfire-server/scheduler`.

## 5. Keep Swagger on the API site

Publish the API project to the API IIS site as usual:

```powershell
dotnet publish Services/CQSAirborne.Services.API/CQSAirborne.Services.API.csproj -c Release -o C:\Publish\CQS-API
```

Swagger remains on the API site at:

```text
https://api-server/swagger
```

The API project no longer needs to host `/scheduler`; `/scheduler` belongs to the separate Hangfire site.

## 6. Verify the Hangfire deployment

1. Open the Hangfire dashboard at `https://hangfire-server/scheduler`.
2. Confirm recurring job `ad-user-sync-job` exists.
3. Add a test user in one configured AD OU.
4. Wait for the next schedule or trigger the job manually from Hangfire.
5. Confirm a row is created in `adm_Employee` with `ADID` and `UserName` set to the AD `samAccountName`.
6. Check `Logs/ad_user_sync_users.txt` and `Logs/ad_user_sync_error.txt` in the publish folder, or the configured `ADUserSync:LogPath`.
