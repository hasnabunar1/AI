
using AiAgents.FashionAgent.Application.Runners;
using AiAgents.FashionAgent.Application.Services;
using AiAgents.FashionAgent.Infrastructure;
using AiAgents.FashionAgent.ML;
using AiAgents.FashionAgent.Web.BackgroundWorkers;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ===== DATABASE =====
builder.Services.AddDbContext<FashionAgentDbContext>(options =>
    options.UseSqlite("Data Source=fashionagent.db"));

// ===== ML CLASSIFIER =====
builder.Services.AddSingleton<IFashionTrendClassifier, MlNetFashionClassifier>();

// ===== SERVICES =====
builder.Services.AddScoped<QueueService>();
builder.Services.AddScoped<ScoringService>();
builder.Services.AddScoped<TrainingService>();
builder.Services.AddScoped<DatabaseSeeder>();
builder.Services.AddScoped<AiAgents.FashionAgent.Infrastructure.Services.LearningService>();

// ===== AGENT RUNNERS =====
builder.Services.AddScoped<FashionScoringAgentRunner>();
builder.Services.AddScoped<FashionRetrainAgentRunner>();

// ===== BACKGROUND WORKERS =====
builder.Services.AddHostedService<ScoringBackgroundWorker>();
builder.Services.AddHostedService<RetrainBackgroundWorker>();

// ===== MVC =====
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ===== INITIALIZE DATABASE =====
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<FashionAgentDbContext>();
    await context.Database.EnsureCreatedAsync();

    // Seed from CSV
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    var csvPath = Path.Combine(app.Environment.ContentRootPath, "Data", "Winter_Fashion_Trends_Dataset.csv");

    if (File.Exists(csvPath))
    {
        await seeder.SeedFromCsvAsync(csvPath, CancellationToken.None);
    }

    // Initial training if no model exists
    var hasModel = await context.ModelVersions.AnyAsync();
    if (!hasModel)
    {
        var classifier = scope.ServiceProvider.GetRequiredService<IFashionTrendClassifier>();
        var trainingService = scope.ServiceProvider.GetRequiredService<TrainingService>();

        Console.WriteLine("Training initial model...");
        var model = await trainingService.TrainModelAsync(activate: true, CancellationToken.None);
        Console.WriteLine($"Model trained!  Accuracy: {model.Accuracy:P2}");
    }
    else
    {
        // Load existing model
        var activeModel = await context.ModelVersions.FirstOrDefaultAsync(x => x.IsActive);
        if (activeModel != null)
        {
            var classifier = scope.ServiceProvider.GetRequiredService<IFashionTrendClassifier>();
            await classifier.LoadModelAsync(activeModel.ModelPath, CancellationToken.None);
        }
    }
}

// ===== MIDDLEWARE =====
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Recommendation}/{action=Index}/{id?}");

Console.WriteLine("========================================");
Console.WriteLine("   FASHION TREND AI AGENT STARTED!");
Console.WriteLine("========================================");
Console.WriteLine("Agent is now processing items...");

app.Run();