using System.Reflection;
using EntityFramework.Exceptions.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

namespace Blocktrust.Mediator.Server;

using Blocktrust.Common.Resolver;
using Common;
using Resolver;

public class Startup
{
    /// <summary>
    /// Startup
    /// </summary>
    /// <param name="configuration"></param>
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    /// <summary>
    /// Configuration
    /// </summary>
    private IConfiguration Configuration { get; }

    /// <summary>
    /// Configuration of services
    /// </summary>
    /// <param name="services"></param>
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddCors();
        services.AddHttpContextAccessor();
        services.AddDbContext<DataContext>(options =>
        {
            options
                .EnableSensitiveDataLogging(false)
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .UseExceptionProcessor()
                .UseSqlServer(Configuration.GetConnectionString("mediatorDatabase"));
        });
        services.AddControllers();
        services.AddRazorPages();
        // services.AddApplicationInsightsTelemetry();
        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(typeof(SimpleDidDocResolver).Assembly);
        });
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()));
        services.AddHttpClient();
        //TODO unclear if the secreat resolver has to be scoped or singleton. Crashing when singleton currently
        services.AddScoped<ISecretResolver, MediatorSecretResolver>();
        services.AddSingleton<IDidDocResolver, SimpleDidDocResolver>();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Blocktrust.Mediator", Version = "v1" });
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var commentsFileName = Assembly.GetExecutingAssembly().GetName().Name + ".XML";
            var commentsFile = Path.Combine(baseDirectory, commentsFileName);
            c.IncludeXmlComments(commentsFile);
        });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    /// <summary>
    /// Configure
    /// </summary>
    /// <param name="app"></param>
    /// <param name="env"></param>
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Blocktrust.Mediator v1"));
        }

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseHttpsRedirection();
        app.UseCors(x => x
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapRazorPages();
        });
    }
}