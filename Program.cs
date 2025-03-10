using CommerceBack.Common;
using CommerceBack.Middleware;
using CommerceBack.Repository;
using CommerceBack.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using CommerceBack;
using CommerceBack.Context;
using CommerceBack.Services.Base;
using CommerceBack.Services.Base.CreateService;
using CommerceBack.Services.Base.DeleteService;
using CommerceBack.Services.Base.ReadService;
using CommerceBack.Services.Base.UpdateService;
using CommerceBack.UnitOfWork;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

	c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	{
		Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
		Type = SecuritySchemeType.Http,
		Scheme = "bearer",
		BearerFormat = "JWT",
	});

	c.AddSecurityRequirement(new OpenApiSecurityRequirement
	{
		{ new OpenApiSecurityScheme
			{
				Reference = new OpenApiReference
				{
					Type = ReferenceType.SecurityScheme,
					Id = "Bearer"
				}
			}, []
		}
	});
});

builder.Services.AddDbContext<MyDbContext>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

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
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException()))
	};
});

builder.Services.AddSingleton<Jwt>(_ =>
{
	// You can read configuration values from your app settings here
	var secretKey = builder.Configuration["Jwt:SecretKey"];
	var issuer = builder.Configuration["Jwt:Issuer"];
	var audience = builder.Configuration["Jwt:Audience"];
	var refreshAudience = builder.Configuration["Jwt:RefreshAudience"];
	var refreshSecretKey = builder.Configuration["Jwt:RefreshSecretKey"];

	if (secretKey != null && issuer != null && audience != null && refreshAudience != null && refreshSecretKey != null)
	{
		return new Jwt(secretKey, issuer, audience, refreshAudience, refreshSecretKey);
	}
	throw new InvalidOperationException("invalid properties");
});

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


builder.Services.AddScoped(typeof(ICrudService<>), typeof(CrudService<>));
builder.Services.AddScoped(typeof(ICreateService<>), typeof(CreateService<>));
builder.Services.AddScoped(typeof(IReadService<>), typeof(ReadService<>));
builder.Services.AddScoped(typeof(IUpdateService<>), typeof(UpdateService<>));
builder.Services.AddScoped(typeof(IDeleteService<>), typeof(DeleteService<>));

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ProductService>();

builder.Services.AddScoped<AuthService>();

builder.Services.AddScoped<UserService>();

builder.Services.AddScoped<TokenService>();

builder.Services.AddScoped<CartService>();

builder.Services.AddScoped<ProductCategoryService>();

builder.Services.AddScoped<TokenBlacklistService>();

builder.Services.AddScoped<JwtTokenValidationMiddleware>();

builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddCors(options =>
{
	options.AddPolicy("All", policy => 
		policy.AllowAnyOrigin()
			.AllowAnyMethod()
			.AllowAnyHeader());
	
    // options.AddPolicy("AllowFrontDev",
    //     builder => builder.WithOrigins("http://localhost:5173")
    //                      .AllowAnyMethod()
    //                      .AllowAnyHeader()
				// 		 .AllowCredentials());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("All");

app.UseMiddleware<JwtTokenValidationMiddleware>();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
