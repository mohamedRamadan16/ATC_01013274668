using EventHorizon.DataAccess.Data;
using EventHorizon.DataAccess.Persistence;
using EventHorizon.DataAccess.Repository;
using EventHorizon.DataAccess.Repository.IRepository;
using EventHorizon.Models.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace EventHorizon
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
            builder.Services.AddSwaggerGen();

            builder.Services.AddAutoMapper(typeof(MappingConfig));

            builder.Services.AddScoped<ISeeder, Seeder>();
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<IEventRepository, EventRepository>();
            builder.Services.AddScoped<IUserEventRepository, UserEventRepository>();



            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
            });


            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddAuthentication(options =>
            {
                // Check With JWT Token in header
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                // [Authorize]
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; // return unauthorized instead of notfound
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>   // Verified key
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    //ValidIssuer = builder.Configuration["JWT:IssuerURL"],
                    //ValidAudience = builder.Configuration["JWT:AudienceURL"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecretKey"]))
                };

            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("EventHorizonPolicy", policy =>
                {
                    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                });

            });

            builder.Services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("bearerAuth", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "bearerAuth"}
                        },

                        []
                    }       

                });
            });


            var app = builder.Build();

            /// seeding when start the application
            var scope = app.Services.CreateScope();
            var seeder = scope.ServiceProvider.GetRequiredService<ISeeder>();
            seeder.Seed();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseStaticFiles();

            app.UseHttpsRedirection();

            app.UseCors("EventHorizonPolicy");

            app.UseAuthentication();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
