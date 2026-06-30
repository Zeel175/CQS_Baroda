# AD User Sync Hangfire Publish Steps

The Hangfire dashboard and recurring AD sync job are now registered in the main API as well as the optional `CQSAirborne.Chroma.Integration.Service` worker. If you run the API on `https://localhost:44309`, open `https://localhost:44309/scheduler`; it uses the same host and port as Swagger, only a different path.

## 1. Update configuration before publishing

In `appsettings.json`, verify these values:

- `ConnectionStrings:DefaultConnection` points to the application database where `adm_Employee` exists.
- `ConnectionStrings:HangfireConnection` points to the Hangfire database.
- `ADUserSync:CronExpression` controls the schedule. `*/5 * * * *` runs every five minutes.
- `ADUserSync:LdapPaths` contains each AD OU that should be scanned.
- `ADUserSync:DefaultPlantId`, `ADUserSync:DefaultOrgRole`, and `ADUserSync:DefaultCreatedBy` contain fallback values for inserted users.
- `ADUserSync:DepartmentMappings` contains any AD department names that must be normalized before saving.

## 2. Publish with the main API

Use this option when you want Swagger/API and Hangfire on the same URL and port. From the repository root, run:

```powershell
dotnet publish Services/CQSAirborne.Services.API/CQSAirborne.Services.API.csproj -c Release -o C:\Publish\CQS-API
```

After deployment, use `/swagger` for API documentation and `/scheduler` for Hangfire on the same base URL.

## 3. Optional: publish the separate Hangfire service

Use this only if you want Hangfire on a separate application/port. From the repository root, run:

```powershell
dotnet publish Services/CQSAirborne.Chroma.Integration.Service/CQSAirborne.Chroma.Integration.Service.csproj -c Release -o C:\Publish\CQS-Hangfire
```

## 4. Deploy to IIS

1. Install the .NET Hosting Bundle on the server.
2. Copy the publish folder to the server, for example `C:\inetpub\wwwroot\CQS-Hangfire`.
3. Create an IIS site or application that points to that folder.
4. Set the application pool to **No Managed Code**.
5. Run the application pool under an identity that can access AD and SQL Server.
6. Browse to `/scheduler` to confirm the Hangfire dashboard loads.

## 5. Deploy as a Windows service alternative

If you do not want IIS, install the published executable as a Windows service:

```powershell
sc.exe create CQS-Hangfire binPath= "C:\Publish\CQS-Hangfire\CQSAirborne.Chroma.Integration.Service.exe" start= auto
sc.exe start CQS-Hangfire
```

## 6. Verify the job

1. Open the Hangfire dashboard at `/scheduler`.
2. Confirm recurring job `ad-user-sync-job` exists.
3. Add a test user in one configured AD OU.
4. Wait for the next schedule or trigger the job manually from Hangfire.
5. Confirm a row is created in `adm_Employee` with `ADID` and `UserName` set to the AD `samAccountName`.
6. Check `Logs/ad_user_sync_users.txt` and `Logs/ad_user_sync_error.txt` in the publish folder, or the configured `ADUserSync:LogPath`.
