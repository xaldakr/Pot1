using Microsoft.EntityFrameworkCore;
using Pot1_API.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<Pot1Context>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Pot1DbConnection")
    ));
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("reactVite", policyBuilder =>
    {
        policyBuilder.WithOrigins("http://localhost:5173");
        policyBuilder.AllowAnyOrigin();
        policyBuilder.AllowAnyMethod();
    });
});

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

app.UseCors("reactVite");

app.Run();
