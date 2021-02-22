using ApiGeo.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ApiGeo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<SolicitudContext>(options => 
                options.UseSqlServer(Configuration.GetConnectionString("ConnDB"))
            );
            services.AddControllers(options => options.EnableEndpointRouting = false);
            services.Configure<Support.AmqpInfo>(Configuration.GetSection("amqp"));
            services.AddSingleton<Support.AmqpService>();
            services.AddHostedService<BackgroundServices.ConsumeRabbitMQHostedService>();
            services.AddOptions();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseMvc();
        }
    }
}
