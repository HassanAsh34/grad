using System.Security.Claims;
using System.Text;
using global::grad.Infrastructure.Repository;
using grad.Application.admin.Interfaces;
using grad.Application.admin.Services;
using grad.Application.Auth.Interfaces;
using grad.Application.Auth.Services;
using grad.Application.Common.Interfaces;
using grad.Application.lesson.Interfaces;
using grad.Application.lesson.Services;
using grad.Application.parent.Interfaces;
using grad.Application.parent.Services;
using grad.Application.student.Interfaces;
using grad.Application.student.Services;
using grad.Application.subject.Interfaces;
using grad.Application.subject.Services;
using grad.Application.teacher.Interfaces;
using grad.Application.teacher.Services;
using grad.Application.Users.Interfaces;
using grad.Application.Users.Services;
using grad.Infrastructure.Cache;
using grad.Infrastructure.EmailServices;
using grad.Infrastructure.FilesServices;
using grad.Infrastructure.Persistence;
using grad.Infrastructure.Persistence.Configurations;
using grad.Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using StackExchange.Redis;

namespace grad
{
	public class Program
	{
		public static void Main(string[] args)
		{
			BsonSerializer.RegisterSerializer(
				new GuidSerializer(GuidRepresentation.Standard));

			var builder = WebApplication.CreateBuilder(args);

			// ================= LOGGING =================
			builder.Logging.ClearProviders();
			builder.Logging.AddConsole();
			builder.Logging.AddDebug();

			// ================= SERVICES =================
			builder.Services.AddControllers();
			builder.Services.AddEndpointsApiExplorer();
			bool hosted = builder.Configuration.GetValue<bool>("Deployed");
			builder.Services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new() { Title = "Grad API", Version = "v1" });

				//if deployed using docker
				//if(hosted)
				//	c.AddServer(new OpenApiServer
				//	{
				//		Url = "https://localhost:5001", // your HTTPS Docker mapped port
				//		Description = "Docker container endpoint"
				//	});

				c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
				{
					Name = "Authorization",
					Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
					Scheme = "Bearer",
					BearerFormat = "JWT",
					In = Microsoft.OpenApi.Models.ParameterLocation.Header,
					Description = "Bearer {your JWT token}"
				});

				c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
			{
				{
					new Microsoft.OpenApi.Models.OpenApiSecurityScheme
					{
						Reference = new Microsoft.OpenApi.Models.OpenApiReference
						{
							Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
							Id = "Bearer"
						}
					},
					Array.Empty<string>()
				}
			});
			});
			builder.Services.Configure<CloudinarySettings>(
				builder.Configuration.GetSection("CloudinarySettings")
			);
			//builder.Services.AddSingleton<CloudinaryServices>();

			// ================= DI =================
			builder.Services.AddScoped<IRepository, Repository>();
			builder.Services.AddScoped<IUserServices, UserServices>();
			builder.Services.AddScoped<IRedisServices, RedisServices>();
			builder.Services.AddScoped<ITokenServices, TokenServices>();
			builder.Services.AddScoped<IParentServices, ParentServices>();
			builder.Services.AddScoped<IAdminServices, AdminServices>();
			builder.Services.AddScoped<IAuthServices, AuthServices>();
			builder.Services.AddScoped<ISubjectServices, SubjectServices>();
			builder.Services.AddScoped<IUowServices, UowServices>();
			builder.Services.AddScoped<ITeacherServices, TeacherServices>();
			builder.Services.AddScoped<IStudentServices, StudentServices>();
			builder.Services.AddScoped<ILessonServices, LessonServices>();
			builder.Services.AddScoped<ICloudinaryServices, CloudinaryServices>();
			builder.Services.AddHostedService<SeedHostedService>();

			// ================= MongoDB =================
			builder.Services.Configure<MongoDBSettings>(
				builder.Configuration.GetSection(nameof(MongoDBSettings)));

			builder.Services.AddSingleton<IMongoClient>(sp =>
			{
				var settings = sp.GetRequiredService<IOptions<MongoDBSettings>>().Value;
				return new MongoClient(settings.ConnectionString);
			});

			builder.Services.AddSingleton<MongoDBContext>();


			// ================= Email =================
			builder.Services.Configure<SMTPsettings>(
				builder.Configuration.GetSection("SMTPsettings")
			);
			builder.Services.AddScoped<IEmailServices, EmailService>();

			// ================= CORS =================
			builder.Services.AddCors(options =>
			{
				options.AddPolicy("DefaultCors", policy =>
				{
					policy
						.WithOrigins(
							"https://localhost:4200",
							"http://localhost:4200",
							"https://grad.com:4200",
							"https://grad.com"
						)
						//.AllowAnyOrigin()
						.AllowAnyHeader()
						.AllowAnyMethod()
						.AllowCredentials();
				});
			});

			// ================= DATABASE =================
			//docker deployment kestrel config
			//if(hosted)
			//{
			//	builder.WebHost.ConfigureKestrel(options =>
			//	{
			//		options.ListenAnyIP(80);   // HTTP
			//		options.ListenAnyIP(443, listenOptions =>
			//		{
			//			listenOptions.UseHttps("./certs/cert.pfx", "Grad"); // dev cert, optional
			//		});
			//	});
			//}

			builder.Services.AddDbContext<Db_Context>(options =>
			{
				var cs = hosted
					? builder.Configuration.GetConnectionString("DeployedConnection")
					: builder.Configuration.GetConnectionString("DefaultConnection");

				options.UseSqlServer(cs);
			});

			// ================= AUTH =================
			builder.Services
				.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
				.AddJwtBearer(options =>
				{
					var jwtKey = builder.Configuration["JWT:Key"]
						?? throw new InvalidOperationException("JWT:Key missing");

					options.TokenValidationParameters = new TokenValidationParameters
					{
						ValidateIssuerSigningKey = true,
						IssuerSigningKey = new SymmetricSecurityKey(
							Encoding.UTF8.GetBytes(jwtKey)),
						ValidateIssuer = false,
						ValidateAudience = false,
						RoleClaimType = ClaimTypes.Role,
						NameClaimType = ClaimTypes.NameIdentifier
					};

					options.Events = new JwtBearerEvents
					{
						OnMessageReceived = context =>
						{
							if (context.Request.Cookies.TryGetValue("reset_token", out var reset))
								context.Token = reset;
							else if (context.Request.Cookies.TryGetValue("access_token", out var access))
								context.Token = access;
							//else (context.Request.Cookies.TryGetValue("refresh_token", out var refresh))
							//	context.Token = refresh;

							return Task.CompletedTask;
						}
					};
				});

			// ================= REDIS =================
			bool redisEnabled = builder.Configuration.GetValue<bool>("Redis:Enabled");

			if (redisEnabled)
			{
				string key = hosted ? "Hosted" : "Local";
				string conn = builder.Configuration[$"Redis:{key}"]
					?? throw new InvalidOperationException("Redis connection missing");

				builder.Services.AddSingleton<IConnectionMultiplexer>(
					ConnectionMultiplexer.Connect(conn));
			}
			else
			{
				builder.Services.AddSingleton<IConnectionMultiplexer>(
					NoOpConnectionMultiplexer.Instance);
			}

			// ================= BUILD =================
			var app = builder.Build();

			// ================= EXCEPTION HANDLING =================
			app.UseExceptionHandler(errorApp =>
			{
				errorApp.Run(async context =>
				{
					var exception = context.Features
						.Get<IExceptionHandlerFeature>()?
						.Error;

					if (exception != null)
						app.Logger.LogError(exception, "?? UNHANDLED EXCEPTION");

					context.Response.StatusCode = 500;
					context.Response.ContentType = "application/json";

					await context.Response.WriteAsync(
						System.Text.Json.JsonSerializer.Serialize(new
						{
							ErrCode = 500,
							ErrMessage = "Internal server error"
						}));
				});
			});

			// ================= SWAGGER (ALWAYS ENABLED) =================
			if (app.Environment.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			app.UseSwagger();
			app.UseSwaggerUI(c =>
			{
				c.SwaggerEndpoint("/swagger/v1/swagger.json", "Grad API v1");
				c.RoutePrefix = "swagger";
			});

			// ================= STATIC FILES =================
			var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
			Directory.CreateDirectory(uploadPath);

			app.UseStaticFiles(new StaticFileOptions
			{
				FileProvider = new PhysicalFileProvider(uploadPath),
				RequestPath = "/uploads"
			});

			// ================= PIPELINE =================

			//if (!app.Environment.IsDevelopment())
			//{
			//	app.UseHttpsRedirection();
			//}
			app.UseHttpsRedirection();
			app.UseCors("DefaultCors");
			app.UseAuthentication();
			app.UseAuthorization();

			// Move cancellation middleware to wrap controller execution (must be registered before MapControllers)
			app.Use(async (context, next) =>
			{
				try
				{
					await next();
				}
				catch (OperationCanceledException)
				{
					context.Response.StatusCode = 499; // Client Closed Request
					await context.Response.WriteAsync("Request was cancelled.");
				}
			});

			app.MapControllers();

			// ================= RUN =================
			try
			{
				app.Run();
			}
			catch (Exception ex)
			{
				Console.WriteLine("?? HOST CRASH");
				Console.WriteLine(ex);
				throw;
			}
		}
	}
}