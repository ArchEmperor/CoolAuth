using CoolAuth.Utils;

namespace CoolAuth.Extensions;

public static class ConfigExtensions
{
    public static void MapEnvironmentVariables(this IApplicationBuilder appBuilder, bool includeSystemVariables = true)
    {
        appBuilder.ApplicationServices.GetRequiredService<EnvMapper>()
            .WithSystemVariables(includeSystemVariables)
            .MapVariables();
    }

    public static void AddEnvironmentVariablesMapper(this IServiceCollection services)
    {
        services.AddSingleton<EnvMapper>();
    }
}