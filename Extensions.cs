namespace RemoteControl.HubLibrary;
public static class Extensions
{
    extension (IServiceCollection services)
    {
        public IServiceCollection AddRemoteControlServices()
        {
            services.AddSignalR(options =>
            {
                options.MaximumReceiveMessageSize = 72428800;
            }); //don't use core. core is only bare necessities.
            services.AddCors(options =>
            {
                options.AddPolicy("AllowOrigin", builder =>
                {
                    builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
                });
            });
            return services;
        }
        
    }
    extension (IApplicationBuilder app)
    {
        public IApplicationBuilder AddRemoteControlServices()
        {
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<RemoteControlHub>("/remotecontrolhub", options =>
                {
                    options.TransportMaxBufferSize = 72428800;
                    options.ApplicationMaxBufferSize = 72428800;
                });
            });
            return app;
        }
    }
}