using ASProducts.Api.Data;
using ASProducts.Api.Data.Seeds;
using ASProducts.Api.Mappings;
using ASProducts.Api.Orchestrators;
using ASProducts.Api.Repositories;
using ASProducts.Api.Services;

using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options => {
    options.AddPolicy("dev", p => p.WithOrigins("http://localhost:5173")
        .AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
});
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ProductsContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ASProducts.ApiConnection")));

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<ProductProfile>();
}, typeof(Program).Assembly);

builder.Services.AddScoped<IProductRepository, ProductRepository>();

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IImageStorageService, ImageStorageService>();

builder.Services.AddScoped<IProductOrchestrator, ProductOrchestrator>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var _context = services.GetRequiredService<ProductsContext>();

        logger.LogInformation("Applying pending migrations...");
        await _context.Database.MigrateAsync();

        logger.LogInformation("Loading initial data seed...");
        await SeedData.Initialize(_context);

        logger.LogInformation("Database initialized successfully.");
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "Critical error while migrating or populating the database. The application will stop.");
        throw;
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseCors("dev");

app.UseAuthorization();
app.MapControllers();

app.Run();
