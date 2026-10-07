using System.Threading;
using System.Threading.Tasks;
using Soenneker.Maui.Permissioners.Notifications.Abstract;
#if ANDROID || IOS
using Microsoft.Maui.ApplicationModel;
#endif
#if IOS
using Foundation;
using UserNotifications;
#endif

namespace Soenneker.Maui.Permissioners.Notifications;

public sealed class NotificationsPermissioner : INotificationsPermissioner
{
    private static readonly SemaphoreSlim _gate = new(1, 1);

    public bool IsSupported =>
#if ANDROID || IOS
        true;
#else
        false;
#endif

    public async ValueTask<bool> Has(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
#if ANDROID
        using var manager = AndroidX.Core.App.NotificationManagerCompat.From(Android.App.Application.Context);
        return manager?.AreNotificationsEnabled() == true;
#elif IOS
        using var settings = await UNUserNotificationCenter.Current.GetNotificationSettingsAsync()
            .WaitAsync(cancellationToken).ConfigureAwait(false);
        return settings.AuthorizationStatus is UNAuthorizationStatus.Authorized or UNAuthorizationStatus.Provisional or UNAuthorizationStatus.Ephemeral;
#else
        await Task.CompletedTask.ConfigureAwait(false);
        return false;
#endif
    }

    public async ValueTask<bool> Request(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!IsSupported)
            return false;
        // Cancellation releases the caller; the gate stays held until the native prompt finishes.
        return await RequestCore(cancellationToken).WaitAsync(cancellationToken).ConfigureAwait(false);
    }

    public ValueTask<bool> RequestIfNotGranted(CancellationToken cancellationToken = default) => Request(cancellationToken);

    private async Task<bool> RequestCore(CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (await Has(cancellationToken).ConfigureAwait(false))
                return true;
#if ANDROID || IOS
            return await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
#if ANDROID
                if (System.OperatingSystem.IsAndroidVersionAtLeast(33))
                    await Permissions.RequestAsync<Permissions.PostNotifications>();
#elif IOS
                var result = await UNUserNotificationCenter.Current.RequestAuthorizationAsync(
                    UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound);
                if (result.Item2 is not null)
                    throw new NSErrorException(result.Item2);
#endif
                return await Has().ConfigureAwait(false);
            }).ConfigureAwait(false);
#else
            return false;
#endif
        }
        finally
        {
            _gate.Release();
        }
    }
}
