using ITS.Core;
using ITS.File.Stats;
using ITS.Interfaces;
using ITS.SQL;
using ITS.Storage;
using ITS.Storage.Azure;
using Serilog;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging((_, builder) => builder.AddConsole())
    .ConfigureServices((hostContext, services) =>
    {
        services.Configure<HostOptions>(options =>
        {
            options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
            options.ShutdownTimeout = TimeSpan.FromSeconds(30);
        });

        services.Configure<SqlOptions>(hostContext.Configuration
            .GetSection(SectionNameConsts.SqlOptionsSectionName));
        services.Configure<StorageOptions>(hostContext.Configuration
            .GetSection(SectionNameConsts.StorageOptionsSectionName));
        services.Configure<AzureStorageOptions>(hostContext.Configuration
            .GetSection(SectionNameConsts.AzureStorageSectionName));

        services.AddHostedService<StatCalculatorWorker>();

        var sqlConfig = hostContext.Configuration.GetSection(SectionNameConsts.SqlOptionsSectionName)
            .Get<SqlOptions>();
        services.AddScoped<IWorkTaskRepository, WorkTaskRepository>(_ =>
            new WorkTaskRepository(sqlConfig.ConnectionString));

        var storageConfig = hostContext.Configuration.GetSection(SectionNameConsts.AzureStorageSectionName)
            .Get<AzureStorageOptions>();
        services.AddScoped<IWorkStatsRepository, BlobWorkStatsRepository>(_ =>
            new BlobWorkStatsRepository(storageConfig.ConnectionString, storageConfig.Container,
                storageConfig.FileName));

        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Type} {Message:lj} {NewLine}{Exception}")
            .CreateLogger();
    })
    .Build();

await host.RunAsync();