using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Maui.Permissioners.Notifications.Abstract;

/// <summary>Checks app-level notification authorization on Android and iOS. Android apps must declare android.permission.POST_NOTIFICATIONS. This does not check individual notification channels or register for remote notifications.</summary>
public interface INotificationsPermissioner
{
    /// <summary>Whether this build supports the platform's permission API.</summary>
    bool IsSupported { get; }

    /// <summary>Checks current authorization without displaying UI. Returns false on unsupported platforms.</summary>
    ValueTask<bool> Has(CancellationToken cancellationToken = default);

    /// <summary>Requests authorization when missing and returns the resulting status. Returns false when unsupported or no settings activity is available.</summary>
    /// <remarks>Call from a foreground app. Cancellation stops waiting, not the system UI. Previously denied notifications may require the user to enable them manually in app settings.</remarks>
    ValueTask<bool> Request(CancellationToken cancellationToken = default);

    /// <summary>Checks authorization and requests it only when missing.</summary>
    ValueTask<bool> RequestIfNotGranted(CancellationToken cancellationToken = default);
}

