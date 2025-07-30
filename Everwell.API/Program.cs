// ============================================================================
// EVERWELL HEALTHCARE BACKEND API - MAIN APPLICATION ENTRY POINT
// ============================================================================
// This is the main configuration file for the Everwell Healthcare API
// It sets up all services, middleware, authentication, and database connections
// for the gender health management system

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

// ============================================================================
// SERVICE CONTAINER CONFIGURATION
// ============================================================================
// Configure all services that will be injected throughout the application

// Configure timezone for Vietnam (UTC+7) - important for appointment scheduling
TimeZoneInfo.ClearCachedData();
var utcPlus7 = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

// Configure MVC controllers with JSON serialization options
// - IgnoreCycles: Prevents infinite loops when serializing related entities
// - WriteIndented: Makes JSON responses more readable for debugging
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });
// Configure API documentation and caching
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGenWithAuth(); // Custom extension that adds JWT authentication to Swagger
builder.Services.AddMemoryCache(); // In-memory caching for performance optimization

// ============================================================================
// DATABASE CONFIGURATION
// ============================================================================
// Configure PostgreSQL database connection with retry policy for reliability
builder.Services.AddDbContext<EverwellDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("SupabaseConnection"),
    npgsqlOptionsAction: sqlOptions =>
    {
        // Retry policy: Automatically retry failed database operations
        // This handles temporary network issues or database unavailability
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorCodesToAdd: null);
    });
});

// ============================================================================
// REPOSITORY PATTERN & DATA ACCESS LAYER
// ============================================================================
// Configure Unit of Work pattern for database transactions
builder.Services.AddScoped<IUnitOfWork<EverwellDbContext>, UnitOfWork<EverwellDbContext>>();
// Generic repository for common CRUD operations
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
// HTTP context access for getting current user information
builder.Services.AddHttpContextAccessor();
// AutoMapper for object-to-object mapping (Entity to DTO conversions)
builder.Services.AddAutoMapper(typeof(UserMapper), typeof(ChatMapper));

// ============================================================================
// BUSINESS LOGIC SERVICES REGISTRATION
// ============================================================================
// Register all business services with dependency injection container
// These services contain the core business logic for each domain

// Core system services
builder.Services.AddScoped<INotificationService, NotificationService>(); // Push notifications and alerts
builder.Services.AddScoped<IUserService, UserService>(); // User management (CRUD, profiles)
builder.Services.AddScoped<IAuthService, AuthService>(); // Authentication and authorization

// Healthcare domain services
builder.Services.AddScoped<IAppointmentService, AppointmentService>(); // Appointment booking and management
builder.Services.AddScoped<IFeedbackService, FeedbackService>(); // Customer feedback and reviews
builder.Services.AddScoped<IPostService, PostService>(); // Educational content management
builder.Services.AddScoped<IQuestionService, QuestionService>(); // Q&A system for consultations
builder.Services.AddScoped<ISTITestingService, STITestingService>(); // STI testing package management
builder.Services.AddScoped<ITestResultService, TestResultService>(); // Medical test results processing
builder.Services.AddScoped<IMenstrualCycleTrackingService, MenstrualCycleTrackingService>(); // Cycle tracking
builder.Services.AddScoped<IMenstrualCycleNotificationService, MenstrualCycleNotificationService>(); // Cycle notifications

// Analytics and reporting services
builder.Services.AddScoped<IDashboardService, DashboardService>(); // Dashboard data aggregation
builder.Services.AddScoped<IStatisticalReportService, StatisticalReportService>(); // Business intelligence reports

// External integration services
builder.Services.AddScoped<IEmailService, EmailService>(); // Email notifications
builder.Services.AddScoped<IPaymentService, PaymentService>(); // VNPay payment integration
builder.Services.AddScoped<ITokenService, TokenService>(); // JWT token management
builder.Services.AddScoped<TokenProvider>(); // Token generation utility
builder.Services.AddScoped<ICalendarService, CalendarService>(); // Calendar integration
builder.Services.AddScoped<IDailyService, DailyService>(); // Daily.co video meeting integration
builder.Services.AddScoped<IChatService, ChatService>(); // Real-time chat functionality

// AI services
builder.Services.AddHttpClient<IAiChatService, EnhancedGeminiChatService>(); // Google Gemini AI integration

// Background services for automated tasks
builder.Services.AddHostedService<Everwell.BLL.Services.BackgroundServices.MenstrualCycleNotificationService>();
// ============================================================================
// AUTHENTICATION & AUTHORIZATION CONFIGURATION
// ============================================================================
// Configure JWT Bearer authentication for secure API access
// Note: Agora video service has been deprecated in favor of Daily.co

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = true; // Enforce HTTPS in production for security
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // Validate all critical JWT components
            ValidateIssuer = true, // Ensure token comes from trusted issuer
            ValidateAudience = true, // Ensure token is intended for this API
            ValidateLifetime = true, // Check token expiration
            ValidateIssuerSigningKey = true, // Verify token signature
            
            // JWT configuration from appsettings
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"])),
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            
            // Security settings
            ClockSkew = TimeSpan.Zero, // Disable default 5-minute clock skew for precise expiration
            NameClaimType = JwtRegisteredClaimNames.Sub, // Map user ID claim
            RoleClaimType = ClaimTypes.Role, // Map role-based authorization claim
        };

        // JWT event handlers for debugging and logging
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Log received tokens for debugging (remove in production)
                var token = context
                    .Request.Headers["Authorization"]
                    .ToString()
                    .Replace("Bearer ", "");
                Console.WriteLine($"Received token: {token}");
                return Task.CompletedTask;
            },
        };
    });

// Configure role-based authorization policies
// These policies define what roles can access specific endpoints
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy =>
        policy.RequireRole("Admin")); // Full system access
    options.AddPolicy("RequireManagerRole", policy =>
        policy.RequireRole("Manager")); // Management operations
    options.AddPolicy("RequireConsultant", policy =>
        policy.RequireRole("Consultant")); // Healthcare provider access
    options.AddPolicy("RequireStaffRole", policy =>
        policy.RequireRole("Staff")); // Staff operations (test results, content)
    options.AddPolicy("RequireCustomerRole", policy =>
        policy.RequireRole("Customer")); // Patient/customer access
});

// ============================================================================
// APPLICATION PIPELINE CONFIGURATION
// ============================================================================
// Build the application and configure the HTTP request pipeline
var app = builder.Build();

// Configure development-specific middleware
if (app.Environment.IsDevelopment())
{
    // Enable Swagger API documentation in development
    app.UseSwagger();
    app.UseSwaggerUI(options =>
        options.SwaggerEndpoint("/swagger/v2.5/swagger.json", "Everwell.API v2.5"));
}

// ============================================================================
// CORS CONFIGURATION
// ============================================================================
// Configure Cross-Origin Resource Sharing for frontend integration
app.UseCors(options =>
{
    if (app.Environment.IsDevelopment())
    {
        // Development: Allow localhost and Daily.co video meeting domains
        options.SetIsOriginAllowed(origin =>
                origin.StartsWith("http://localhost:") || // Local development servers
                origin.StartsWith("https://localhost:") ||
                origin.EndsWith(".daily.co") || // Daily.co video meeting service
                origin == "https://everwell.daily.co")
            .AllowAnyMethod() // Allow all HTTP methods (GET, POST, PUT, DELETE, etc.)
            .AllowAnyHeader() // Allow all headers
            .AllowCredentials(); // Allow cookies and authentication headers
    }
    else
    {
        // Production: Restrict to specific trusted domains
        var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() 
            ?? new[] { "https://your-production-domain.com", "https://everwell.daily.co" };
            
        // Add Daily.co domains for video meeting functionality
        var dailyOrigins = new[] { "https://everwell.daily.co", "https://app.daily.co" };
        var allOrigins = allowedOrigins.Concat(dailyOrigins).ToArray();
            
        options.WithOrigins(allOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    }
});

// ============================================================================
// MIDDLEWARE PIPELINE
// ============================================================================
// Configure middleware in the correct order (order matters!)

app.UseHttpsRedirection(); // Redirect HTTP to HTTPS for security

// Authentication & Authorization middleware
app.UseAuthentication(); // Validate JWT tokens
app.UseMiddleware<TokenBlacklistMiddleware>(); // Check for blacklisted tokens (logout functionality)
app.UseAuthorization(); // Apply role-based access control

// Map API controllers to handle HTTP requests
app.MapControllers();

// Start the application
app.Run();
