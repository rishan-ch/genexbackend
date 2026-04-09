using CloudinaryDotNet;
using GeneX_Backend.Modules.SMS.Services;
using GeneX_Backend.Infrastructure.CloudinaryService;
using GeneX_Backend.Infrastructure.Data;
using GeneX_Backend.Infrastructure.Notification;
using GeneX_Backend.Modules.Banner.Interface;
using GeneX_Backend.Modules.Banner.Service;
using GeneX_Backend.Modules.BillingInfo.Interface;
using GeneX_Backend.Modules.BillingInfo.Service;
using GeneX_Backend.Modules.Cart.Interface;
using GeneX_Backend.Modules.Cart.Services;
using GeneX_Backend.Modules.Category.Interface;
using GeneX_Backend.Modules.Category.Services;
using GeneX_Backend.Modules.Coupon.Interface;
using GeneX_Backend.Modules.Coupon.Service;
using GeneX_Backend.Modules.Dashbaord;
using GeneX_Backend.Modules.Discount.Interface;
using GeneX_Backend.Modules.Email;
using GeneX_Backend.Modules.Notification.Service;
using GeneX_Backend.Modules.Orders.Interfaces;
using GeneX_Backend.Modules.Orders.Services;
using GeneX_Backend.Modules.Products.Interfaces;
using GeneX_Backend.Modules.Products.Services;
using GeneX_Backend.Modules.Review.Interface;
using GeneX_Backend.Modules.SMS.Interface;
using GeneX_Backend.Modules.Users.Entities;
using GeneX_Backend.Modules.Users.Interfaces;
using GeneX_Backend.Modules.Users.Services;
using GeneX_Backend.Modules.WishList.Interface;
using GeneX_Backend.Modules.WishList.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// 1. Port Configuration (Vital for Render)
// This tells Kestrel to listen on the port Render provides, or 10000 by default.
var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// 2. Cloudinary Setup
// It will now pull from Render Environment Variables instead of a local .env file.
// 2. Cloudinary Setup
var cloudName = builder.Configuration["CLOUDINARY_CLOUDNAME"];
var apiKey = builder.Configuration["CLOUDINARY_APIKEY"];
var apiSecret = builder.Configuration["CLOUDINARY_APISECRET"];

// Create the account object
var cloudinaryAccount = new Account(cloudName, apiKey, apiSecret);

// Create the Cloudinary instance
var cloudinary = new Cloudinary(cloudinaryAccount);

// REGISTER IT HERE - This is the line your app was missing!
builder.Services.AddSingleton(cloudinary);

// CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("https://genex-frontend.vercel.app") // Removed trailing slash for better matching
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid token."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] { }
        }
    });
});

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection"));
});

// Identity
builder.Services.AddIdentity<UserEntity, RoleEntity>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// DI Registrations
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ISubCategoryService, SubCategoryService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ICouponService, CouponService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IDiscountService, DiscountService>();
builder.Services.AddScoped<IBillingInfoService, BillingInfoService>();
builder.Services.AddScoped<IBannerService, BannerService>();
builder.Services.AddScoped<IWishlistService, WishlistService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<ISparrowSmsService, SparrowSmsService>();
builder.Services.AddSignalR();
builder.Services.AddScoped<IJWTService, JWTService>();
builder.Services.AddScoped<CloudinaryService>();

// Authentication
var jwtConfig = builder.Configuration.GetSection("JwtConfig");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtConfig["Issuer"],
        ValidAudience = jwtConfig["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig["Key"] ?? "fallback_secret_key_for_dev")),
        NameClaimType = "sub"
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            if (!string.IsNullOrEmpty(accessToken) && context.Request.Path.StartsWithSegments("/notificationHub"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngularApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificationHub>("/notificationHub");

// Automatic Seeding
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<RoleEntity>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserEntity>>();
    
    // Seed Roles
    var roles = new[] { "SuperAdmin", "Admin", "Customer" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new RoleEntity { Name = role });
        }
    }

    // Seed Users
    var usersToSeed = new[]
    {
        new { Email = "sohil.shrestha@pratibuddha.com.np", UserName = "sohil.shrestha", Password = "Sohil@123", Role = "SuperAdmin" },
        new { Email = "shrawan.tamrakar@pratibuddha.com.np", UserName = "shrawan.tamrakar", Password = "Shrawan@123", Role = "SuperAdmin" },
        new { Email = "info@pratibuddha.com.np", UserName = "Info@pratibuddha", Password = "Pratibuddha@123", Role = "Admin" }
    };

    foreach (var u in usersToSeed)
    {
        if (await userManager.FindByEmailAsync(u.Email) == null)
        {
            var user = new UserEntity { 
                Email = u.Email, 
                UserName = u.UserName, 
                FirstName = "Admin", 
                Address = "Nepal", 
                EmailConfirmed = true, 
                isDeleted = false 
            };
            var result = await userManager.CreateAsync(user, u.Password);
            if (result.Succeeded) await userManager.AddToRoleAsync(user, u.Role);
        }
    }
}

app.Run();
