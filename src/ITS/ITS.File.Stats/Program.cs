using ITS.Core;
using ITS.File.Stats;
using ITS.Interfaces;
using ITS.SQL;
using ITS.Storage;
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

        services.AddHostedService<StatCalculatorWorker>();

        var sqlConfig = hostContext.Configuration.GetSection(SectionNameConsts.SqlOptionsSectionName)
            .Get<SqlOptions>();
        services.AddScoped<IWorkTaskRepository, WorkTaskRepository>(_ =>
            new WorkTaskRepository(sqlConfig.ConnectionString));

        var storageConfig = hostContext.Configuration.GetSection(SectionNameConsts.StorageOptionsSectionName)
            .Get<StorageOptions>();
        services.AddScoped<IWorkStatsRepository, WorkStatsStorageRepository>(_ =>
            new WorkStatsStorageRepository(storageConfig.FileName));

        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Type} {Message:lj} {NewLine}{Exception}")
            .CreateLogger();
    })
    .Build();

await host.RunAsync();