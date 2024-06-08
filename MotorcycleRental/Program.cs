using MotorcycleRental.Data;
using MotorcycleRental.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<MotorcycleDatabaseSettings>(builder.Configuration.GetSection("Mottu"));
builder.Services.AddSingleton<MotorcycleRepository>();
builder.Services.AddSingleton<IMotorcycleService, MotorcycleRepository>();
builder.Logging

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
