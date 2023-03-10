using System.IO.Compression;
using ITS.Core;
using ITS.Interfaces;
using ITS.SQL;
using ITS.Storage;
using ITS.Web.Base;
using ITS.Web.Options;
using ITS.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.ResponseCompression;
using Polly;
using Polly.Contrib.WaitAndRetry;

var builder = WebApplication.CreateBuilder(args);

//configuring options
builder.Services.AddOptions<AppOptions>()
    .Bind(builder.Configuration.GetSection(SectionNameConsts.AppOptionsSectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddOptions<StorageOptions>()
    .Bind(builder.Configuration.GetSection(SectionNameConsts.StorageOptionsSectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddOptions<ApiOptions>()
    .Bind(builder.Configuration.GetSection(SectionNameConsts.ApiOptionsSectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddOptions<SqlOptions>()
    .Bind(builder.Configuration.GetSection(SectionNameConsts.SqlOptionsSectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddOptions<AuthOptions>()
    .Bind(builder.Configuration.GetSection(SectionNameConsts.AuthOptionsSectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

//adding interface mappings for services connecting to SQL
var sqlOptions = builder.Configuration.GetSection(SectionNameConsts.SqlOptionsSectionName)
    .Get<SqlOptions>();
builder.Services.AddTransient<IUserRepository, ItsUserRepository>(_ =>
    new ItsUserRepository(sqlOptions.ConnectionString));
builder.Services.AddTransient<ICategoryRepository, CategoryRepository>(_ =>
    new CategoryRepository(sqlOptions.ConnectionString));
builder.Services.AddTransient<ITagRepository, TagRepository>(_ =>
    new TagRepository(sqlOptions.ConnectionString));
builder.Services.AddTransient<IWorkTaskRepository, WorkTaskRepository>(_ =>
    new WorkTaskRepository(sqlOptions.ConnectionString));
builder.Services.AddTransient<IProfileSettingsService, ProfileSettingsService>(_ =>
    new ProfileSettingsService(sqlOptions.ConnectionString));
builder.Services.AddTransient<IWorkTaskCommentRepository, WorkTaskCommentRepository>(_ =>
    new WorkTaskCommentRepository(sqlOptions.ConnectionString));
var storageOptions = builder.Configuration.GetSection(SectionNameConsts.StorageOptionsSectionName)
    .Get<StorageOptions>();
builder.Services.AddTransient<IWorkStatsRepository, WorkStatsStorageRepository>(_ =>
    new WorkStatsStorageRepository(storageOptions.FileName));
builder.Services.AddScoped<IUserDataContext, UserDataContext>();

//adding system services
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<ITempDataProvider, CookieTempDataProvider>();
builder.Services.AddResponseCompression(options => options.Providers.Add<GzipCompressionProvider>());
builder.Services.Configure<GzipCompressionProviderOptions>(compressionOptions =>
    compressionOptions.Level = CompressionLevel.Optimal);
builder.Services.AddHealthChecks();

//http services with retry policies
builder.Services.AddHttpClient<ReportApiHttpService>()
    .AddTransientHttpErrorPolicy(policyBuilder =>
        policyBuilder.WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 5)));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => options.LoginPath = new PathString("/User/Login"));
builder.Services.AddRazorPages().AddRazorPagesOptions(options =>
    options.Conventions.AddPageRoute("/Info/Index", ""));

builder.Services.AddApplicationInsightsTelemetry();

var app = builder.Build();

if (!app.Environment.IsDevelopment()) app.UseExceptionHandler("/Error");

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapHealthChecks("/health").AllowAnonymous();
    endpoints.MapRazorPages();
});
app.Run();