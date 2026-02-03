using System.Security.Claims;
using System.Text;
using grad.Data;
using grad.Interfaces;
using grad.Repositories;
using grad.Services;
using MailerSend.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using StackExchange.Redis;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

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

			// ================= DI =================
			builder.Services.AddScoped<IRepository, Repository>();
			builder.Services.AddScoped<IUserServices, UserServices>();
			builder.Services.AddScoped<IRedisServices, RedisServices>();
			builder.Services.AddScoped<ITokenServices, TokenServices>();
			builder.Services.AddScoped<IParentServices, ParentServices>();
			builder.Services.AddScoped<IAdminServices, AdminServices>();
			builder.Services.AddScoped<ISubjectServices, SubjectServices>();
			builder.Services.AddScoped<IUowServices, UowServices>();
			builder.Services.AddScoped<ITeacherServices,TeacherServices>();
			builder.Services.AddScoped<IStudentServices, StudentServices>();
			builder.Services.AddScoped<ILessonServices, LessonServices>();
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
						app.Logger.LogError(exception, "🔥 UNHANDLED EXCEPTION");

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
			app.MapControllers();

			// ================= RUN =================
			try
			{
				app.Run();
			}
			catch (Exception ex)
			{
				Console.WriteLine("🔥 HOST CRASH");
				Console.WriteLine(ex);
				throw;
			}
		}
	}
}





//using System.Security.Claims;
//using System.Text;
//using grad.Data;
//using grad.Interfaces;
//using grad.Model;
//using grad.Repositories;
//using grad.Services;
//using MailerSend.AspNetCore;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.AspNetCore.Diagnostics;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.FileProviders;
//using Microsoft.Extensions.Options;
//using Microsoft.IdentityModel.Tokens;
//using MongoDB.Driver;
//using StackExchange.Redis;


//namespace grad
//{
//	public class Program
//	{
//		public static void Main(string[] args)
//		{
//			var builder = WebApplication.CreateBuilder(args);

//			// Add services to the container.

//			builder.Services.AddControllers();
//			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//			builder.Services.AddEndpointsApiExplorer();
//			builder.Services.AddScoped<IRepository, Repository>();
//			builder.Services.AddScoped<IUserServices, UserServices>();
//			builder.Services.AddScoped<IRedisServices, RedisServices>();
//			builder.Services.AddScoped<ITokenServices, TokenServices>();
//			builder.Services.AddScoped<IParentServices, ParentServices>();
//			builder.Services.AddScoped<IAdminServices, AdminServices>();
//			builder.Services.AddScoped<ISubjectServices, SubjectServices>();
//			builder.Services.AddScoped<IUowServices, UowServices>();
//			builder.Services.AddHostedService<SeedHostedService>();

//			//firestore
//			//builder.Services.AddSingleton<FireStoreContext>();
//			//builder.Services.AddScoped<FireStoreRepository>();

//			//mongo db configuration
//			builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection(nameof(MongoDBSettings)));

//			builder.Services.AddSingleton<IMongoClient>(sp =>
//			{
//				var settings = sp.GetRequiredService<IOptions<MongoDBSettings>>().Value;
//				return new MongoClient(settings.ConnectionString);
//			});

//			builder.Services.AddSingleton<MongoDBContext>();

//			//builder.Services.AddScoped<ITeacherServices, TeacherServices>();
//			builder.Services.AddHttpClient<IEmailServices, EmailServices>();
//			builder.Services.Configure<MailerSendOptions>(builder.Configuration.GetSection("MailerSend"));
//			builder.Services.AddCors(options =>
//			{
//				options.AddPolicy("DefaultCors", policy => //dont forget to change the allowed ports in app.UseCors
//				{
//					policy
//						//.WithOrigins(
//						//	"http://localhost:3000",
//						//	"https://localhost:3000")//TODO: change this to the actual frontend url
//						.AllowAnyOrigin()
//						.AllowAnyHeader()
//						.AllowAnyMethod();
//						//.AllowCredentials();
//				});
//			});
//			bool Hosted = builder.Configuration.GetValue<bool>("Deployed");

//			builder.Services.AddDbContext<Db_Context>(options =>
//			{
//				if (Hosted == true)
//					options.UseSqlServer(builder.Configuration.GetConnectionString("DeployedConnection"));
//				else
//					options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
//			});
//			builder.Services.AddAuthentication(options =>
//			{
//				options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//				options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//			}).
//			AddJwtBearer(options =>
//			{
//				var jwtKey = builder.Configuration["JWT:Key"] ?? throw new InvalidOperationException("JWT:Key is not configured.");
//				options.TokenValidationParameters = new TokenValidationParameters
//				{
//					ValidateIssuerSigningKey = true,
//					IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
//					ValidateIssuer = false,
//					ValidateAudience = false,
//					RoleClaimType = ClaimTypes.Role,
//					NameClaimType = ClaimTypes.NameIdentifier
//				};
//				options.Events = new JwtBearerEvents
//				{
//					OnMessageReceived = context =>
//					{
//						if (!context.Request.Cookies["Reset_Token"].IsNullOrEmpty())
//							context.Token = context.Request.Cookies["Reset_Token"];
//						else
//							context.Token = context.Request.Cookies["access_token"];
//						//else if (!context.Request.Cookies["access_token"].IsNullOrEmpty())
//						//	context.Token = context.Request.Cookies["access_token"];
//						//else
//						//	context.Token = context.Request.Cookies["refresh_token"];---
//						return Task.CompletedTask;
//					}
//				};
//			});

//			bool enabled = builder.Configuration.GetValue<bool>("Redis:Enabled");
//			if (enabled)
//			{
//				string redisHost =  Hosted == true?  "Hosted" : "Local";
//				builder.Services.AddSingleton<IConnectionMultiplexer>(Services =>
//				{
//					var connectionString = builder.Configuration[$"Redis:{redisHost}"];
//					if (string.IsNullOrEmpty(connectionString))
//					{
//						throw new InvalidOperationException("Redis connection string is not configured. Set Redis:ConnectionString in appsettings.json or environment variables.");
//					}
//					var configuration = ConfigurationOptions.Parse(connectionString);
//					configuration.AbortOnConnectFail = false;
//					return ConnectionMultiplexer.Connect(configuration);
//				});
//			}
//			else
//			{
//				Console.WriteLine("Redis is disabled via configuration.");
//				builder.Services.AddSingleton<IConnectionMultiplexer>(NoOpConnectionMultiplexer.Instance);
//			}

//			builder.Services.AddSwaggerGen(c =>
//			{
//				c.SwaggerDoc("v1", new() { Title = "Ecommerce API", Version = "v1" });

//				// 🔑 Add JWT Authentication support
//				c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
//				{
//					Name = "Authorization",
//					Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
//					Scheme = "Bearer",
//					BearerFormat = "JWT",
//					In = Microsoft.OpenApi.Models.ParameterLocation.Header,
//					Description = "Enter 'Bearer' followed by your JWT token.\nExample: Bearer 12345abcdef"
//				});

//				c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
//				{
//					{
//						new Microsoft.OpenApi.Models.OpenApiSecurityScheme
//						{
//							Reference = new Microsoft.OpenApi.Models.OpenApiReference
//							{
//								Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
//								Id = "Bearer"
//							}
//						},
//						new string[] {}
//					}
//				});
//			});


//			var app = builder.Build();
//			//uncomment that line for development
//			// Configure the HTTP request pipeline.
//			//if (app.Environment.IsDevelopment())
//			//{
//			//	app.UseSwagger();
//			//	app.UseSwaggerUI();
//			//}
//			//else
//			//{
//			//	app.UseExceptionHandler(errorApp =>
//			//	{
//			//		errorApp.Run(async context =>
//			//		{
//			//			context.Response.StatusCode = StatusCodes.Status500InternalServerError;
//			//			context.Response.ContentType = "application/json";
//			//			var payload = System.Text.Json.JsonSerializer.Serialize(new
//			//			{
//			//				ErrCode = 500,
//			//				ErrMessage = "An unexpected error occurred."
//			//			});
//			//			await context.Response.WriteAsync(payload);
//			//		});
//			//	});
//			//}

//			app.UseExceptionHandler(errorApp =>
//			{
//				errorApp.Run(async context =>
//				{
//					context.Response.StatusCode = StatusCodes.Status500InternalServerError;
//					context.Response.ContentType = "application/json";
//					var payload = System.Text.Json.JsonSerializer.Serialize(new
//					{
//						ErrCode = 500,
//						ErrMessage = "An unexpected error occurred."
//					});
//					await context.Response.WriteAsync(payload);
//				});
//			});

//			app.UseExceptionHandler(errorApp =>
//			{
//				errorApp.Run(async context =>
//				{
//					var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
//					var exception = exceptionFeature?.Error;

//					Console.WriteLine("🔥 UNHANDLED EXCEPTION");
//					Console.WriteLine(exception?.ToString());

//					context.Response.StatusCode = StatusCodes.Status500InternalServerError;
//					context.Response.ContentType = "application/json";

//					var payload = System.Text.Json.JsonSerializer.Serialize(new
//					{
//						ErrCode = 500,
//						ErrMessage = "An unexpected error occurred---------------------."
//					});
//					await context.Response.WriteAsync(payload);
//				});
//			});

//			app.UseSwagger();
//			app.UseSwaggerUI(c =>
//			{
//				c.SwaggerEndpoint("/swagger/v1/swagger.json", "Grad API v1");
//				c.RoutePrefix = "swagger"; // default
//			});

//			var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");

//			// Create the folder if it doesn't exist
//			if (!Directory.Exists(uploadPath))
//			{
//				Directory.CreateDirectory(uploadPath);
//			}

//			app.UseStaticFiles(new StaticFileOptions
//			{
//				FileProvider = new PhysicalFileProvider(uploadPath),
//				RequestPath = "/uploads"
//			});



//			//app.UseStaticFiles(new StaticFileOptions
//			//{
//			//	FileProvider = new PhysicalFileProvider(
//			//		Path.Combine(Directory.GetCurrentDirectory(), "uploads")
//			//	),
//			//	RequestPath = "/uploads"
//			//});


//			////for deployment only
//			////{
//			//app.UseSwagger();
//			//app.UseSwaggerUI();
//			////}
//			app.UseHttpsRedirection();

//			app.UseCors("DefaultCors");
//			app.UseAuthentication();
//			app.UseAuthorization();


//			app.MapControllers();

//			app.Run();
//		}
//	}
//}
