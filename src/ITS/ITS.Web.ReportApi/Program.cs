using ITS.Core;
using ITS.Interfaces;
using ITS.SQL;
using ITS.Web.ReportApi.Authentication;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOptions<AppOptions>()
    .Bind(builder.Configuration.GetSection(SectionNameConsts.AppOptionsSectionName))
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

var sqlOptions = builder.Configuration.GetSection(SectionNameConsts.SqlOptionsSectionName)
    .Get<SqlOptions>();
builder.Services.AddTransient<IUserRepository, ItsUserRepository>(_ =>
    new ItsUserRepository(sqlOptions.ConnectionString));
builder.Services.AddTransient<IWorkTaskRepository, WorkTaskRepository>(_ =>
    new WorkTaskRepository(sqlOptions.ConnectionString));
builder.Services.AddTransient<IWorkTaskCommentRepository, WorkTaskCommentRepository>(_ =>
    new WorkTaskCommentRepository(sqlOptions.ConnectionString));

builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(conf =>
{
    conf.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "Api key to access the ITS api's",
        Type = SecuritySchemeType.ApiKey,
        Name = AuthOptions.ApiKeyHeaderName,
        In = ParameterLocation.Header,
        Scheme = "ApiKeyScheme"
    });
    var scheme = new OpenApiSecurityScheme
    {
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "ApiKey"
        },
        In = ParameterLocation.Header
    };
    var requirement = new OpenApiSecurityRequirement
    {
        { scheme, new List<string>() }
    };
    conf.AddSecurityRequirement(requirement);
});

builder.Services.AddScoped<ApiKeyAuthFilter>();

const string allowOrigins = "_projectConf24_AllowedOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: allowOrigins,
        policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyMethod()
                .WithExposedHeaders(AuthOptions.ApiKeyHeaderName);
        });
});
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthorization();
app.UseCors(allowOrigins);
app.UseEndpoints(endpoints =>
{
    endpoints.MapHealthChecks("/health").AllowAnonymous();
    endpoints.MapControllers();
});
app.Run();