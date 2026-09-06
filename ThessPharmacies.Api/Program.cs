using Microsoft.EntityFrameworkCore;
using ThessPharmacies.Api.Data;
using ThessPharmacies.Parser;
using ThessPharmacies.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddScoped<FsthPdfParser>();
builder.Services.AddScoped<DutyImportService>();

builder.Services.AddDbContext<ThessPharmaciesDbContext>(
    options =>
        options.UseNpgsql(
            builder.Configuration.GetConnectionString(
                "DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();