using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using Shaos.Data;
using Shaos.Repository;
using Shaos.Services;
using System.Reflection;

namespace Shaos
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateBootstrapLogger();

            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddSerilog();

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(connectionString));

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<IdentityUser>(
                options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services
                .AddAuthentication()
                .AddBearerToken(IdentityConstants.BearerScheme);

            builder.Services.AddDbContext<ShaosDbContext>(options =>
                options.UseSqlite(connectionString));

            builder.Services.AddRazorPages();
            builder.Services.AddControllers();

            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Shaos API",
                    Description = "",
                    //TermsOfService = new Uri("https://example.com/terms"),
                    //Contact = new OpenApiContact
                    //{
                    //    Name = "Example Contact",
                    //    Url = new Uri("https://example.com/contact")
                    //},
                    //License = new OpenApiLicense
                    //{
                    //    Name = "Example License",
                    //    Url = new Uri("https://example.com/license")
                    //}
                });

                options.EnableAnnotations();

                var rootDocFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var modelDocFile = $"Shaos.Api.Model.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, rootDocFile));
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, modelDocFile));
            });

            builder.Services.AddSingleton<IPlugInService, PlugInService>();

            var app = builder.Build();

            app.MapIdentityApi<IdentityUser>();

            app.MapControllers();

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "AspNetCoreApi v1"));

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }
    }
}