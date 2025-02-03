using BusinessLogicLayer.Interfaces;
using BusinessLogicLayer.Services;
using DataAccessLayer.Context;
using DataAccessLayer.Interfaces;
using DataAccessLayer.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configure DbContext with the connection string
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register services and repositories for DI
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IFundoonoteRepository, FundoonoteRepository>();

// Register the services 
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFundoonoteService, FundoonoteService>();
builder.Services.AddScoped<AuthService>(); 

// Add controllers for API
builder.Services.AddControllers();

// Enable Swagger for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Enable Swagger middleware for API
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.MapControllers();

app.Run();
