using AndroidX.Core.App;

namespace Soenneker.Maui.Permissioners.Notifications.DeviceTests;

[TestClass]
public sealed class NotificationsPermissionerTests
{
    [TestMethod]
    public async Task HasMatchesAndroidNotificationSettings()
    {
        var permissioner = new NotificationsPermissioner();
        using var manager = NotificationManagerCompat.From(Android.App.Application.Context);

        Assert.IsTrue(permissioner.IsSupported);
        Assert.IsNotNull(manager);
        Assert.AreEqual(manager.AreNotificationsEnabled(), await permissioner.Has());
    }

    [TestMethod]
    public async Task CanceledRequestDoesNotOpenPermissionPrompt()
    {
        var permissioner = new NotificationsPermissioner();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(async () => await permissioner.Has(cancellation.Token));
        await Assert.ThrowsAsync<OperationCanceledException>(async () => await permissioner.Request(cancellation.Token));
        await Assert.ThrowsAsync<OperationCanceledException>(async () => await permissioner.RequestIfNotGranted(cancellation.Token));
    }

    [TestMethod]
    public async Task AlreadyGrantedRequestReturnsWithoutPrompt()
    {
        var permissioner = new NotificationsPermissioner();
        if (!await permissioner.Has())
            Assert.Inconclusive("Grant notifications to this test app to exercise the already-granted path.");

        Assert.IsTrue(await permissioner.Request());
        Assert.IsTrue(await permissioner.RequestIfNotGranted());
    }
}
