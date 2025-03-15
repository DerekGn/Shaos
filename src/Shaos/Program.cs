/*
* MIT License
*
* Copyright (c) 2025 Derek Goslin https://github.com/DerekGn
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/

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
using Shaos.Services.IO;
using Shaos.Services.Runtime;
using Shaos.Services.Store;
using Shaos.Services.System;
using Shaos.Services.Validation;

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
            builder.Services.AddScoped<IStore, Store>();

            builder.Services.AddSingleton<ICodeFileValidationService, CodeFileValidationService>();
            builder.Services.AddSingleton<IFileStoreService, FileStoreService>();
            builder.Services.AddSingleton<IPlugInValidationService, PlugInValidationService>();
            builder.Services.AddSingleton<IRuntimeService, RuntimeService>();
            builder.Services.AddSingleton<ISystemService, SystemService>();

            builder.Services.AddHostedService<MonitorBackgroundWorker>();

            builder.Services.AddMemoryCache();

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