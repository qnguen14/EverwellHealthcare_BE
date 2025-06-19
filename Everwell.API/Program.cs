using Everwell.BLL.Services.Implements;
using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Everwell.BLL.Infrastructure;
using Everwell.API.Extensions;
using Everwell.API.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Everwell.DAL.Repositories.Implements;
using Everwell.DAL.Repositories.Interfaces;
using Everwell.DAL.Mappers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
TimeZoneInfo.ClearCachedData();
var utcPlus7 = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGenWithAuth();
builder.Services.AddMemoryCache();

builder.Services.AddDbContext<EverwellDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("SupabaseConnection"),
    npgsqlOptionsAction: sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorCodesToAdd: null);
    });
});

builder.Services.AddScoped<IUnitOfWork<EverwellDbContext>, UnitOfWork<EverwellDbContext>>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddHttpContextAccessor();
builder.Services.AddAutoMapper(typeof(UserMapper));

builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
// builder.Services.AddScoped<IServiceService, ServiceService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<ISTITestingService, STITestingService>();
builder.Services.AddScoped<ITestResultService, TestResultService>();
builder.Services.AddScoped<IMenstrualCycleTrackingService, MenstrualCycleTrackingService>();
builder.Services.AddScoped<IMenstrualCycleNotificationService, MenstrualCycleNotificationService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IUnitOfWork<EverwellDbContext>, UnitOfWork<EverwellDbContext>>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<TokenProvider>();
builder.Services.AddHostedService<Everwell.BLL.Services.BackgroundServices.MenstrualCycleNotificationService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = true; // Set to true in productionx
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"])),
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ClockSkew = TimeSpan.Zero, // Disable the default clock skew of 5 minutes
            NameClaimType = JwtRegisteredClaimNames.Sub, // Use the NameIdentifier claim type for user ID
            RoleClaimType = ClaimTypes.Role, // Use the Role claim type for roles
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context
                    .Request.Headers["Authorization"]
                    .ToString()
                    .Replace("Bearer ", "");
                Console.WriteLine($"Received token: {token}");
                return Task.CompletedTask;
            },
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy =>
        policy.RequireRole("Admin"));
    options.AddPolicy("RequireManagerRole", policy =>
        policy.RequireRole("Manager"));
    options.AddPolicy("RequireConsultant", policy =>
        policy.RequireRole("Consultant"));
    options.AddPolicy("RequireStaffRole", policy =>
        policy.RequireRole("Staff"));
    options.AddPolicy("RequireCustomerRole", policy =>
        policy.RequireRole("Customer"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
        options.SwaggerEndpoint("/swagger/v2.5/swagger.json", "Everwell.API v2.5"));
}

app.UseCors(options =>
{
    options.SetIsOriginAllowed(origin =>
            origin.StartsWith("http://localhost:") || origin.StartsWith("https://localhost:"))
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseMiddleware<TokenBlacklistMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();
