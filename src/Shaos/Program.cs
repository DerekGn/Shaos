using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using Shaos.Data;
using Shaos.Hosting;
using Shaos.Json;
using Shaos.Repository;
using Shaos.Services;
using Shaos.Services.Options;

namespace Shaos
{
    public static class Program
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

            builder.Services.AddDbContext<ShaosDbContext>(options =>
                options.UseSqlite(connectionString,
                _ => _.MigrationsAssembly(typeof(ShaosDbContext).Assembly.GetName().Name)));

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<IdentityUser>(
                options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services
                .AddAuthentication()
                .AddBearerToken(IdentityConstants.BearerScheme);

            builder.Services.AddDbContext<ShaosDbContext>(options =>
                options.UseSqlite(connectionString));

            builder.Services.AddApiVersioning(_ =>
            {
                _.DefaultApiVersion = new ApiVersion(1, 0);
                _.AssumeDefaultVersionWhenUnspecified = true;
                _.ReportApiVersions = true;
                _.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddApiExplorer(_ =>
            {
                _.GroupNameFormat = "'v'VVV";
                _.SubstituteApiVersionInUrl = true;
            });

            builder.Services.AddRazorPages();
            builder.Services.AddControllers()
                .AddJsonOptions(_ =>
                {
                    JsonSerializerOptionsDefault.Configure(_.JsonSerializerOptions);
                });

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

                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Shaos.Api.Model.xml"));
            });

            // Application defined services
            builder.Services.AddScoped<IPlugInService, PlugInService>();

            builder.Services.AddSingleton<IAssemblyCache, AssemblyCache>();
            builder.Services.AddSingleton<ICodeFileValidationService, CodeFileValidationService>();
            builder.Services.AddSingleton<ICompilerService, CSharpCompilerService>();
            builder.Services.AddSingleton<IFileStoreService, FileStoreService>();
            builder.Services.AddSingleton<IPlugInRuntime, PlugInRuntime>();
            builder.Services.AddSingleton<ISystemService, SystemService>();

            builder.Services.AddHostedService<MonitorBackgroundWorker>();

            builder.Services.Configure<FileStoreOptions>(builder.Configuration.GetSection(nameof(FileStoreOptions)));

            var app = builder.Build();
            var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

            app.MapIdentityApi<IdentityUser>();

            app.MapControllers();

            app.UseSwagger();
            app.UseSwaggerUI(_ =>
            {
                foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions.Reverse())
                {
                    _.SwaggerEndpoint($"{description.GroupName}/swagger.json", description.GroupName);
                }
            });

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