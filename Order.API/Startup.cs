using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MassTransit;
using Order.API.Models.Context;
using Microsoft.EntityFrameworkCore;
using Order.API.Consumers;
using Shared;

namespace Order.API
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
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("SQLServer")));

            services.AddMassTransit(configure =>
            {
                configure.AddConsumer<PaymentCompletedEventConsumer>();
                configure.AddConsumer<PaymentFailedEventConsumer>();
                configure.AddConsumer<StockNotReservedEventConsumer>();
                configure.UsingRabbitMq((context, configurator) =>
                {
                    configurator.Host(Configuration.GetConnectionString("RabbitMQ"));
                    configurator.ReceiveEndpoint(RabbitMQSettings.Order_PaymentCompletedEventQueue, e => e.ConfigureConsumer<PaymentCompletedEventConsumer>(context));
                    configurator.ReceiveEndpoint(RabbitMQSettings.Order_PaymentFailedEventQueue, e => e.ConfigureConsumer<PaymentFailedEventConsumer>(context));
                    configurator.ReceiveEndpoint(RabbitMQSettings.Order_StockNotReservedEventQueue, e => e.ConfigureConsumer<StockNotReservedEventConsumer>(context));
                });
            });

            services.AddMassTransitHostedService();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Order.API", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order.API v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
