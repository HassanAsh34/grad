using System.Security.Claims;
using System.Text;
using grad.Application.Auth.Services;
using grad.Application.LessonFeatures.Services;
using Grad.Application.AdminFeatures.Interfaces;
using Grad.Application.AdminFeatures.Services;
using Grad.Application.Auth.Interfaces;
using Grad.Application.Auth.Services;
using Grad.Application.Common.Interfaces;
using Grad.Application.Common.Services;
using Grad.Application.ExerciseFeatures.Interfaces;
using Grad.Application.ExerciseFeatures.Services;
using Grad.Application.LessonFeatures.Interfaces;
using Grad.Application.StudentFeatures.Interfaces;
using Grad.Application.StudentFeatures.Services;
using Grad.Application.SubjectFeatures.Interfaces;
using Grad.Application.SubjectFeatures.Services;
using Grad.Application.SubmissionFeatures.Interfaces;
using Grad.Application.SubmissionFeatures.Services;
using Grad.Infrastructure.Repository;
using Grad.Application.TeacherFeatures.Interfaces;
using Grad.Application.TeacherFeatures.Services;
using Grad.Application.Users.Interfaces;
using Grad.Application.Users.Services;
using Grad.API.Middleware;
using Grad.Infrastructure.Cache;
using Grad.Infrastructure.EmailServices;
using Grad.Infrastructure.FilesServices;
using Grad.Infrastructure.Persistence;
using Grad.Infrastructure.Persistence.Configurations;
using Grad.Infrastructure.Repository;
using Grad.Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using Serilog;
using StackExchange.Redis;
using Grad.Application.ParentFeatures.Interfaces;
using Grad.Application.ParentFeatures.Services;
using Grad.Application.QAFeature.Interfaces;
using Grad.Application.QAFeature.Services;



namespace Grad.API
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// ================= SERILOG =================
			builder.Host.UseSerilog((context, config) => config
				.ReadFrom.Configuration(context.Configuration)
				.Enrich.FromLogContext()
				.WriteTo.Console()
				.WriteTo.File(
					path: "Logs/log-.txt",
					rollingInterval: RollingInterval.Day,
					retainedFileCountLimit: 30,
					outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
				)
			);

			// Add services to the container.
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
				try
				{
					builder.Services.AddSingleton<IConnectionMultiplexer>(
						ConnectionMultiplexer.Connect(conn));
				}
				catch (RedisConnectionException ex)
				{
					Console.WriteLine($"Redis unavailable, falling back to NoOp: {ex.Message}");
					builder.Services.AddSingleton<IConnectionMultiplexer>(
						NoOpConnectionMultiplexer.Instance);
				}
			}
			else
			{
				builder.Services.AddSingleton<IConnectionMultiplexer>(
					NoOpConnectionMultiplexer.Instance);
			}

			// 2. Setup Unit of Work & Common Services
			builder.Services.AddScoped<IUowServices, UowServices>();
			builder.Services.AddScoped<ICloudinaryServices, CloudinaryServices>();
			builder.Services.Configure<CloudinarySettings>(
							builder.Configuration.GetSection("CloudinarySettings")
						);
			builder.Services.AddScoped<IEmailServices, EmailService>();
			builder.Services.AddScoped<ITokenServices, TokenServices>();
			builder.Services.AddScoped<IRedisServices, RedisServices>();
			builder.Services.AddHostedService<SeedHostedService>();
			builder.Services.AddMemoryCache();

			// 3. Setup Repositories (Infrastructure layer)
			builder.Services.AddScoped<IRepository, Repository>();
			builder.Services.AddScoped<IAdminRepository, AdminRepository>();
			builder.Services.AddScoped<IUserRepository, UserRepository>();
			builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();
			builder.Services.AddScoped<ITeacherRepository, TeacherRepository>();
			builder.Services.AddScoped<IStudentRepository, StudentRepository>();
			builder.Services.AddScoped<IlessonRepository, lessonRepository>();
			builder.Services.AddScoped<IExerciseRepository, ExerciseRepository>();
			builder.Services.AddScoped<IAuthRepository, AuthRepository>();
			builder.Services.AddScoped<ISubmissionRepository, SubmissionRepository>();
			builder.Services.AddScoped<IParentRepository, ParentRepository>();
			builder.Services.AddScoped<IInqueryRepository, InqueryRepository>();

			// 4. Setup Services (Application layer)
			builder.Services.AddScoped<IUserServices, UserServices>();
			builder.Services.AddScoped<ISubjectServices, SubjectServices>();
			builder.Services.AddScoped<ILessonServices, LessonServices>();
			builder.Services.AddScoped<IExerciseServices, ExerciseServices>();
			builder.Services.AddScoped<ITeacherServices, TeacherServices>();
			builder.Services.AddScoped<IStudentServices, StudentServices>();
			builder.Services.AddScoped<IAdminServices, AdminServices>();
			builder.Services.AddScoped<IAuthServices, AuthServices>();
			builder.Services.AddScoped<ISubmissionServices, SubmissionServices>();
			builder.Services.AddScoped<IPerquisiteServices, PerquisiteServices>();
			builder.Services.AddScoped<IParentServices, ParentServices>();
			builder.Services.AddScoped<ICummunicationServices, CommunicationServices>();

			var app = builder.Build();

			// Configure the HTTP request pipeline.
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

			// Global exception handling (catches + logs all unhandled errors)
			app.UseMiddleware<ExceptionHandlingMiddleware>();

			// Serilog HTTP request logging (logs method, path, status code, elapsed time)
			app.UseSerilogRequestLogging();

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
				Log.Information("Grad API starting up...");
				app.Run();
			}
			catch (Exception ex)
			{
				Log.Fatal(ex, "HOST CRASH — Application terminated unexpectedly");
				throw;
			}
			finally
			{
				Log.CloseAndFlush();
			}
		}
	}
}


