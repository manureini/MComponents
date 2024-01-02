using MComponents.ExampleApp.Components;
using MComponents.ExampleApp.Data;
using MComponents.ExampleApp.Service;
using MComponents.Files;
using MComponents.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MComponents.ExampleApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddSingleton<WeatherForecastService>();
            builder.Services.AddSingleton<IFileUploadService, FileUploadService>();

            builder.Services.AddMComponents(options =>
            {
                options.RegisterResourceLocalizer = true;
                options.RegisterStringLocalizer = true;
                options.RegisterNavigation = true;
                options.RegisterTimezoneService = true;
            });

            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                options.SupportedUICultures = MComponentSettings.AllSupportedCultures;
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                //app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.UseRequestLocalization();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    } 
}
