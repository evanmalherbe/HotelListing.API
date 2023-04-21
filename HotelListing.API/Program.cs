using HotelListing.API.Core.Configurations;
using HotelListing.API.Core.Contracts;
using HotelListing.API.Data;
using HotelListing.API.Core.MiddleWare;
using HotelListing.API.Core.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Serilog;
using System.Security.Cryptography.Xml;
using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var connectionString = builder.Configuration.GetConnectionString("HotelListingDbConnectionString");
builder.Services.AddDbContext<HotelListingDbContext>(options =>
{
	options.UseSqlServer(connectionString);
});

builder.Services.AddIdentityCore<ApiUser>()
	.AddRoles<IdentityRole>()
	.AddTokenProvider<DataProtectorTokenProvider<ApiUser>>("HotelListingApi")
	.AddEntityFrameworkStores<HotelListingDbContext>()
	.AddDefaultTokenProviders();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
	options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
	{ Title = "Hotel Listing API", Version = "v1" });
	options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
	{
		Description = @"JWT Authorization header using the Bearer scheme. 
									Enter 'Bearer' [space] and then your token in the text input below. 
									Example: 'Bearer 12345abcdef'",
		Name = "Authorization",
		In = ParameterLocation.Header,
		Type = SecuritySchemeType.ApiKey,
		Scheme = JwtBearerDefaults.AuthenticationScheme
	});
	options.AddSecurityRequirement(new OpenApiSecurityRequirement
	{
		{
			new OpenApiSecurityScheme
			{
				Reference = new OpenApiReference
				{
					Type = ReferenceType.SecurityScheme,
					Id = JwtBearerDefaults.AuthenticationScheme
				},
				Scheme = "0auth2",
				Name = JwtBearerDefaults.AuthenticationScheme,
				In = ParameterLocation.Header
			},
			new List<string>()
		}
	});
});

builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAll", 
		b => b.AllowAnyHeader()
			.AllowAnyOrigin()
			.AllowAnyMethod());
});

builder.Services.AddApiVersioning(options =>
{
	options.AssumeDefaultVersionWhenUnspecified = true;
	options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
	options.ReportApiVersions = true;
	options.ApiVersionReader = ApiVersionReader.Combine(
		new QueryStringApiVersionReader("api-version"),
		new HeaderApiVersionReader("X-Version"),
		new MediaTypeApiVersionReader("ver")
		);
});

builder.Services.AddVersionedApiExplorer(
	options =>
	{
		options.GroupNameFormat = "'v'VVV";
		options.SubstituteApiVersionInUrl = true;
	});

builder.Host.UseSerilog((ctx, lc) => lc.WriteTo.Console().ReadFrom.Configuration(ctx.Configuration));

builder.Services.AddAutoMapper(typeof(AutoMapperConfig));

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<ICountriesRepository, CountriesRepository>();
builder.Services.AddScoped<IHotelsRepository, HotelsRepository>();
builder.Services.AddScoped<IAuthManager, AuthManager>();

builder.Services.AddAuthentication(options =>
{
	// "Bearer"
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuerSigningKey = true,
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateLifetime = true,
		ClockSkew = TimeSpan.Zero,
		ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
		ValidAudience = builder.Configuration["JwtSettings:Audience"],
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]))
	};
});

builder.Services.AddResponseCaching(options =>
{
	options.MaximumBodySize = 1024;
	options.UseCaseSensitivePaths = true;
});

// AspNetCore.HealthChecks.SqlServer
// EF Core - Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore

builder.Services.AddHealthChecks()
	.AddCheck<CustomHealthCheck>("Custom Health Check", 
	failureStatus: HealthStatus.Degraded, 
	tags: new[] {"custom"})
	.AddSqlServer(connectionString, tags: new[] { "database"})
	.AddDbContextCheck<HotelListingDbContext>(tags: new[] {"database"});

builder.Services.AddControllers().AddOData(options =>
{
	options.Select().Filter().OrderBy();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionMiddleware>();

app.MapHealthChecks("/healthcheck", new HealthCheckOptions
{
	Predicate = healthcheck => healthcheck.Tags.Contains("custom"),
	ResultStatusCodes =
	{
		[HealthStatus.Healthy] = StatusCodes.Status200OK,
		[HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable,
		[HealthStatus.Degraded] = StatusCodes.Status200OK
	},
	ResponseWriter = WriteResponse
}) ;

app.MapHealthChecks("/databasehealthcheck", new HealthCheckOptions
{
	Predicate = healthcheck => healthcheck.Tags.Contains("database"),
	ResultStatusCodes =
	{
		[HealthStatus.Healthy] = StatusCodes.Status200OK,
		[HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable,
		[HealthStatus.Degraded] = StatusCodes.Status200OK
	},
	ResponseWriter = WriteResponse
}) ;

app.MapHealthChecks("/healthz", new HealthCheckOptions
{ 
	ResultStatusCodes =
	{
		[HealthStatus.Healthy] = StatusCodes.Status200OK,
		[HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable,
		[HealthStatus.Degraded] = StatusCodes.Status200OK
	},
	ResponseWriter = WriteResponse
});

static Task WriteResponse(HttpContext context, HealthReport healthReport)
{
	context.Response.ContentType = "application/json; charset=utf-8";
	JsonWriterOptions options = new JsonWriterOptions { Indented = true };

	using MemoryStream memorystream = new MemoryStream();
	using (Utf8JsonWriter jsonWriter = new Utf8JsonWriter(memorystream, options))
	{
		jsonWriter.WriteStartObject();
		jsonWriter.WriteString("status", healthReport.Status.ToString());
		jsonWriter.WriteStartObject("results");

		foreach (var healthReportEntry in healthReport.Entries)
		{
			jsonWriter.WriteStartObject(healthReportEntry.Key);
			jsonWriter.WriteString("status", healthReportEntry.Value.Status.ToString());
			jsonWriter.WriteString("description", healthReportEntry.Value.Description);
			jsonWriter.WriteStartObject("data");

			foreach (var item in healthReportEntry.Value.Data)
			{
				jsonWriter.WritePropertyName(item.Key);
				JsonSerializer.Serialize(jsonWriter, item.Value, item.Value?.GetType() ?? typeof(object));
			}

			jsonWriter.WriteEndObject();
			jsonWriter.WriteEndObject();
		}

		jsonWriter.WriteEndObject();
		jsonWriter.WriteEndObject();
	}

	return context.Response.WriteAsync(Encoding.UTF8.GetString(memorystream.ToArray()));
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseResponseCaching();

app.Use(async (context, next) =>
{
	context.Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue()
	{
		Public = true,
		MaxAge = TimeSpan.FromSeconds(10)
	};
	context.Response.Headers[HeaderNames.Vary] = new string[] { "Accept-Encoding" };
	await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

class CustomHealthCheck : IHealthCheck
{
	public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
	{
		var isHealthy = true;

		if (isHealthy)
		{
			return Task.FromResult(HealthCheckResult.Healthy("All systems are looking good."));
		}
		return Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus, "System unhealthy"));
	}
}