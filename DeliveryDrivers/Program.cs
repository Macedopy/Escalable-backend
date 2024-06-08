using DeliveryDrivers.Data;
using DeliveryDrivers.Infrastructure;
using DeliveryDrivers.Models;
using DeliveryDrivers.RabbitService;
using MotorcycleRental.Data;
using MotorcycleRental.RabbitService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<DeliveryDriverDatabaseSettings>(builder.Configuration.GetSection("Mottu"));
builder.Services.AddSingleton<DriverRepository>();
builder.Services.AddTransient<IDriverRepository, DriverRepository>();
builder.Services.AddTransient<IMotorcycleService, MotorcycleRepository>();
builder.Services.AddScoped<IDriverimageS3, DriverimageS3>();
builder.Services.AddSingleton<IRabbitBusService, RabbitMQService>();

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
