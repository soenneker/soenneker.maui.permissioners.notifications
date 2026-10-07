using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Maui.Permissioners.Notifications.Abstract;

namespace Soenneker.Maui.Permissioners.Notifications.Registrars;

/// <summary>Registers the NotificationsPermissioner service.</summary>
public static class NotificationsPermissionerRegistrar
{
    /// <summary>Registers the permissioner with singleton lifetime.</summary>
    public static IServiceCollection AddNotificationsPermissionerAsSingleton(this IServiceCollection services)
    {
        services.TryAddSingleton<INotificationsPermissioner, NotificationsPermissioner>();
        return services;
    }

    /// <summary>Registers the permissioner with scoped lifetime.</summary>
    public static IServiceCollection AddNotificationsPermissionerAsScoped(this IServiceCollection services)
    {
        services.TryAddScoped<INotificationsPermissioner, NotificationsPermissioner>();
        return services;
    }
}

