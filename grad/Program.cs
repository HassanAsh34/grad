
using grad.Data;
using grad.Interfaces;
using grad.Repositories;
using grad.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using StackExchange.Redis;
using MailerSend.AspNetCore;
using System.Security.Claims;


namespace grad
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.

			builder.Services.AddControllers();
			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddScoped<IRepository, Repository>();
			builder.Services.AddScoped<IUserServices, UserServices>();
			builder.Services.AddScoped<IRedisServices, RedisServices>();
			builder.Services.AddScoped<ITokenServices, TokenServices>();
			builder.Services.AddScoped<IParentServices, ParentServices>();
			builder.Services.AddScoped<IAdminServices, AdminServices>();
			builder.Services.AddScoped<ISubjectServices, SubjectServices>();
			builder.Services.AddScoped<IUowServices, UowServices>();
			builder.Services.AddSingleton<FireStoreContext>();
			builder.Services.AddScoped<FireStoreRepository>();
			//builder.Services.AddScoped<ITeacherServices, TeacherServices>();
			builder.Services.AddHttpClient<IEmailServices, EmailServices>();
			builder.Services.Configure<MailerSendOptions>(builder.Configuration.GetSection("MailerSend"));
			builder.Services.AddCors(options =>
			{
				options.AddPolicy("DefaultCors", policy => //dont forget to change the allowed ports in app.UseCors
				{
					policy
						//.WithOrigins(
						//	"http://localhost:3000",
						//	"https://localhost:3000")//TODO: change this to the actual frontend url
						.AllowAnyOrigin()
						.AllowAnyHeader()
						.AllowAnyMethod();
						//.AllowCredentials();
				});
			});

			builder.Services.AddDbContext<Db_Context>(options =>
			{
				options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
			});
			builder.Services.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			}).
			AddJwtBearer(options =>
			{
				var jwtKey = builder.Configuration["JWT:Key"] ?? throw new InvalidOperationException("JWT:Key is not configured.");
				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuerSigningKey = true,
					IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
					ValidateIssuer = false,
					ValidateAudience = false,
					RoleClaimType = ClaimTypes.Role,
					NameClaimType = ClaimTypes.NameIdentifier
				};
				options.Events = new JwtBearerEvents
				{
					OnMessageReceived = context =>
					{
						if (!context.Request.Cookies["Reset_Token"].IsNullOrEmpty())
							context.Token = context.Request.Cookies["Reset_Token"];
						else
							context.Token = context.Request.Cookies["access_token"];
						return Task.CompletedTask;
					}
				};
			});
			
			bool enabled = builder.Configuration.GetValue<bool>("Redis:Enabled");
			if (enabled)
			{
				builder.Services.AddSingleton<IConnectionMultiplexer>(Services =>
				{
					var connectionString = builder.Configuration["Redis:ConnectionString"];
					if (string.IsNullOrEmpty(connectionString))
					{
						throw new InvalidOperationException("Redis connection string is not configured. Set Redis:ConnectionString in appsettings.json or environment variables.");
					}
					var configuration = ConfigurationOptions.Parse(connectionString);
					configuration.AbortOnConnectFail = false;
					return ConnectionMultiplexer.Connect(configuration);
				});
			}
			else
			{
				Console.WriteLine("Redis is disabled via configuration.");
				builder.Services.AddSingleton<IConnectionMultiplexer>(NoOpConnectionMultiplexer.Instance);
			}

			builder.Services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new() { Title = "Ecommerce API", Version = "v1" });

				// 🔑 Add JWT Authentication support
				c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
				{
					Name = "Authorization",
					Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
					Scheme = "Bearer",
					BearerFormat = "JWT",
					In = Microsoft.OpenApi.Models.ParameterLocation.Header,
					Description = "Enter 'Bearer' followed by your JWT token.\nExample: Bearer 12345abcdef"
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
						new string[] {}
					}
				});
			});


			var app = builder.Build();
			//uncomment that line for development
			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}
			else
			{
				app.UseExceptionHandler(errorApp =>
				{
					errorApp.Run(async context =>
					{
						context.Response.StatusCode = StatusCodes.Status500InternalServerError;
						context.Response.ContentType = "application/json";
						var payload = System.Text.Json.JsonSerializer.Serialize(new
						{
							ErrCode = 500,
							ErrMessage = "An unexpected error occurred."
						});
						await context.Response.WriteAsync(payload);
					});
				});
			}


			////for deployment only
			////{
			//app.UseSwagger();
			//app.UseSwaggerUI();
			////}
			app.UseHttpsRedirection();

			app.UseCors("DefaultCors");
			app.UseAuthentication();
			app.UseAuthorization();


			app.MapControllers();

			app.Run();
		}
	}
}
