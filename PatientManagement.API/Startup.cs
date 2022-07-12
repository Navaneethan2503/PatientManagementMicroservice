using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PatientManagement.Domain.Aggregates.PatientAggregate;
using PatientManagement.Domain.Interfaces;
using PatientManagement.Infrastructure.Data.Contexts;
using PatientManagement.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientManagement.API
{
    public class Startup
    {
        private readonly IWebHostEnvironment env;
        public Startup(IConfiguration configuration , IWebHostEnvironment env)
        {
            Configuration = configuration;
            this.env = env;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration.GetConnectionString("DefaultConnectionString");
            services.AddControllers();
            //services.AddDbContext<PatientManagementContext>(options => options.UseSqlServer(connectionString));
            
            if (env.IsDevelopment())
            {
                services.AddDbContext<PatientManagementContext>(setup => setup.UseSqlServer(connectionString));
            }
            else if (env.EnvironmentName == "Staging")
            {
                services.AddDbContext<PatientManagementContext>(setup => setup.UseInMemoryDatabase("PatientDB"));
            }
            else if (env.EnvironmentName == "Production")
            {

            }
           
            services.AddScoped<IRepository<Appointment>, Repository<Appointment>>();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo()
                {
                    Title = "Patient Management API",
                    Description = "In This Api , There are Endpoints for appointment Booking , Resheduling , Deleting and getting appointments .",
                });
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "Jwt",
                    In = ParameterLocation.Header,
                    Description = "Jwt token for authorized user"
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {new OpenApiSecurityScheme(){Reference=new OpenApiReference{ Id="Bearer", Type=ReferenceType.SecurityScheme}}, new string[]{ } }
                });
            });

            var issuer = Configuration["JWT:issuer"];
            var audience = Configuration["JWT:audience"];
            var key = Configuration["JWT:key"];
            byte[] keyBytes = Encoding.ASCII.GetBytes(key);
            SecurityKey securityKey = new SymmetricSecurityKey(keyBytes);
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = securityKey
                };
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors(options =>
            {
                options.AllowAnyOrigin();   
                options.AllowAnyMethod();  
                options.AllowAnyHeader();  
            });
            app.UseSwagger();
            app.UseSwaggerUI(options => options.SwaggerEndpoint("v1/swagger.json", "Patient Management API"));
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
