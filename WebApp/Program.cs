using Microsoft.EntityFrameworkCore;
using SQL;
using SQL.DatabaseSeed;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("WebAppDbContext");
builder.Services.AddDbContext<WebAppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddTransient<Seed>();
builder.Services.AddControllers();
builder.Services.AddCors();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
{
    var context = serviceScope.ServiceProvider.GetRequiredService<WebAppDbContext>();
    var databaseSeed = serviceScope.ServiceProvider.GetRequiredService<Seed>();

    context.Database.EnsureDeleted();
    context.Database.EnsureCreated();
    databaseSeed.ConvertCSVtoModels();
   
        
}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors(policy =>
{
    policy.AllowAnyHeader();
    policy.AllowCredentials();
    policy.AllowAnyMethod();
    policy.WithOrigins("http://localhost:4200");

});
app.UseHttpsRedirection();

app.MapControllers();
app.Run();

