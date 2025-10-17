using Azure.Data.Tables;
using PostAI.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Services ---
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

var tableService = new TableServiceClient(
    builder.Configuration.GetConnectionString("AzureTableStorage") ??
    "UseDevelopmentStorage=true");

var usageTable = tableService.GetTableClient("UserUsage");
usageTable.CreateIfNotExists();
builder.Services.AddSingleton(usageTable);

// Custom services
builder.Services.AddSingleton<OpenAIService>();
builder.Services.AddSingleton<UsageTrackerService>();
builder.Services.AddSingleton<StorageService>();
builder.Services.AddSingleton<InstagramService>();
builder.Services.AddHttpClient<InstagramPublisherService>();


var app = builder.Build();

app.MapOpenApi();
app.UseSwaggerUI();

PostAI.Api.Endpoints.HealthEndpoints.MapEndpoints(app);
PostAI.Api.Endpoints.CaptionEndpoints.MapEndpoints(app);
PostAI.Api.Endpoints.ImageEndpoints.MapEndpoints(app);
PostAI.Api.Endpoints.PromoEndpoints.MapEndpoints(app);
PostAI.Api.Endpoints.PostsEndpoints.MapEndpoints(app);
PostAI.Api.Endpoints.InstagramEndpoints.MapEndpoints(app);
app.MapInstagramPublishEndpoints();



app.Run();
