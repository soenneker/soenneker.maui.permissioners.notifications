using Android.Runtime;
using Microsoft.Testing.Extensions;
using Microsoft.Testing.Platform.Builder;
using Microsoft.Testing.Platform.Extensions;
using Microsoft.Testing.Platform.Extensions.Messages;

[assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]

namespace Soenneker.Maui.Permissioners.Notifications.DeviceTests;

[Instrumentation(Name = "com.soenneker.notifications.devicetests.TestInstrumentation")]
public class TestInstrumentation : Instrumentation
{
    protected TestInstrumentation(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership) { }

    public override void OnCreate(Bundle? arguments)
    {
        base.OnCreate(arguments);
        Start();
    }

    public override async void OnStart()
    {
        base.OnStart();

        var consumer = new ResultConsumer(this);
        var bundle = new Bundle();
        try
        {
            var writeablePath = Application.Context.GetExternalFilesDir(null)?.AbsolutePath ?? Path.GetTempPath();
            var resultsPath = Path.Combine(writeablePath, "TestResults");
            var builder = await TestApplication.CreateBuilderAsync([
                "--results-directory", resultsPath,
                "--report-trx"
            ]);
            builder.AddMSTest(() => [GetType().Assembly]);
            builder.AddTrxReportProvider();
            builder.TestHost.AddDataConsumer(_ => consumer);

            using ITestApplication app = await builder.BuildAsync();
            await app.RunAsync();

            bundle.PutInt("passed", consumer.Passed);
            bundle.PutInt("failed", consumer.Failed);
            bundle.PutInt("skipped", consumer.Skipped);
            bundle.PutString("resultsPath", consumer.TrxReportPath);
            Finish(Result.Ok, bundle);
        }
        catch (Exception ex)
        {
            bundle.PutString("error", ex.ToString());
            Finish(Result.Canceled, bundle);
        }
    }

}
