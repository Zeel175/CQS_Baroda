using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CQSAirborne.Data.Contract;
using CQSAirborne.Data.Implementation;
using System;

public class DataModule
{
    public static void Register(IConfiguration Configuration, IServiceCollection serviceCollection)
    {
        serviceCollection.AddDbContext<IDataContext, CQSDataContext>(options =>
        {
            options.UseSqlServer(
                Configuration.GetConnectionString("DefaultConnection"),
                sqlOptions =>
                {
                    // ✅ Enable transient error retry logic
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5, // Retry up to 5 times
                        maxRetryDelay: TimeSpan.FromSeconds(10), // Wait 10 seconds between retries
                        errorNumbersToAdd: null // Apply to all transient errors
                    );
                })
            //.UseLazyLoadingProxies()
            ;
        });

        // ✅ Stored procedure context (kept unchanged)
        serviceCollection.AddTransient<IStoredProcedureContext>(
            s => new StoredProcedureContext(Configuration.GetConnectionString("DefaultConnection")));
    }
}
