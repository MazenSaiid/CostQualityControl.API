using System.Text;
using CostQualityControl.API.Authorization;
using CostQualityControl.API.Data;
using CostQualityControl.API.Models;
using CostQualityControl.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"]!;
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
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

var resources = new[] { "Products", "Ingredients", "Invoices", "Production", "Reports", "Users", "Permissions" };
var actions = new[] { "view", "write", "delete" };
builder.Services.AddAuthorization(options =>
{
    foreach (var resource in resources)
        foreach (var action in actions)
            options.AddPolicy($"{resource}:{action}", policy =>
                policy.RequireAuthenticatedUser()
                      .AddRequirements(new PermissionRequirement(resource, action)));
});

builder.Services.AddMemoryCache();

// CORS — allow Angular frontend
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:4300", "http://localhost:4200"];
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader().AllowAnyMethod().AllowCredentials());
});

// Services
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IIngredientService, IngredientService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IProductionService, ProductionService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();
builder.Services.AddScoped<IFinancialService, FinancialService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CostQC API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Enter: Bearer {token}",
        Name = "Authorization", In = ParameterLocation.Header, Type = SecuritySchemeType.ApiKey, Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {{
        new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
        Array.Empty<string>()
    }});
});

var app = builder.Build();

// Seed roles and admin user on startup
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    await db.Database.MigrateAsync();

    string[] roles = ["Admin", "Manager", "ProductionOperator", "QualityInspector", "Viewer"];
    foreach (var role in roles)
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));

    if (await userManager.FindByNameAsync("admin") is null)
    {
        var admin = new AppUser { UserName = "admin", Email = "admin@costqc.com", FullName = "System Administrator" };
        await userManager.CreateAsync(admin, "Admin@12345");
        await userManager.AddToRoleAsync(admin, "Admin");
    }

    // Seed default permissions
    var resources2 = new[] { "Products", "Ingredients", "Invoices", "Production", "Reports", "Users", "Permissions" };
    var defaultPerms = new Dictionary<string, (bool v, bool w, bool d)[]>
    {
        ["Admin"] = resources2.Select(_ => (true, true, true)).ToArray(),
        ["Manager"] = new[] {
            (true, true, true),   // Products
            (true, true, true),   // Ingredients
            (true, true, false),  // Invoices
            (true, true, false),  // Production
            (true, false, false), // Reports
            (false, false, false),// Users
            (false, false, false) // Permissions
        },
        ["ProductionOperator"] = new[] {
            (true, false, false),  // Products
            (true, false, false),  // Ingredients
            (false, false, false), // Invoices
            (true, true, false),   // Production
            (true, false, false),  // Reports
            (false, false, false), // Users
            (false, false, false)  // Permissions
        },
        ["QualityInspector"] = new[] {
            (true, false, false),  // Products
            (true, false, false),  // Ingredients
            (false, false, false), // Invoices
            (true, false, false),  // Production
            (true, false, false),  // Reports
            (false, false, false), // Users
            (false, false, false)  // Permissions
        },
        ["Viewer"] = new[] {
            (true, false, false),  // Products
            (true, false, false),  // Ingredients
            (false, false, false), // Invoices
            (true, false, false),  // Production
            (true, false, false),  // Reports
            (false, false, false), // Users
            (false, false, false)  // Permissions
        },
    };

    foreach (var (roleName, perms) in defaultPerms)
    {
        for (int i = 0; i < resources2.Length; i++)
        {
            var resource = resources2[i];
            var exists = await db.RolePermissions.AnyAsync(p => p.RoleName == roleName && p.Resource == resource);
            if (!exists)
            {
                db.RolePermissions.Add(new CostQualityControl.API.Models.RolePermission
                {
                    RoleName = roleName,
                    Resource = resource,
                    CanView = perms[i].v,
                    CanWrite = perms[i].w,
                    CanDelete = perms[i].d
                });
            }
        }
    }
    await db.SaveChangesAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
