namespace RemoteControl.HubLibrary;
public static class Extensions
{
    public static IServiceCollection AddRemoteControlServices(this IServiceCollection services)
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
    public static IApplicationBuilder AddRemoteControlServices(this IApplicationBuilder app)
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