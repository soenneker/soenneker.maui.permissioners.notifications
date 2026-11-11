using Android.Runtime;
using Microsoft.Testing.Extensions;
using Microsoft.Testing.Platform.Builder;
using Microsoft.Testing.Platform.Extensions;
using Microsoft.Testing.Platform.Extensions.Messages;

namespace Soenneker.Maui.Permissioners.Notifications.DeviceTests;

internal sealed class ResultConsumer(Instrumentation instrumentation) : IDataConsumer
{
    int _passed, _failed, _skipped;
    public int Passed => _passed;
    public int Failed => _failed;
    public int Skipped => _skipped;
    public string? TrxReportPath;

    public string Uid => nameof(ResultConsumer);
    public string DisplayName => nameof(ResultConsumer);
    public string Description => "";
    public string Version => "1.0";
    public Task<bool> IsEnabledAsync() => Task.FromResult(true);

    public Type[] DataTypesConsumed => [typeof(TestNodeUpdateMessage), typeof(SessionFileArtifact)];

    public Task ConsumeAsync(IDataProducer dataProducer, IData value, CancellationToken cancellationToken)
    {
        if (value is SessionFileArtifact artifact)
        {
            TrxReportPath = artifact.FileInfo.FullName;
        }
        else if (value is TestNodeUpdateMessage { TestNode: var node })
        {
            var state = node.Properties.SingleOrDefault<TestNodeStateProperty>();
            string? outcome = state switch
            {
                PassedTestNodeStateProperty => "passed",
                FailedTestNodeStateProperty or ErrorTestNodeStateProperty
                    or TimeoutTestNodeStateProperty => "failed",
                SkippedTestNodeStateProperty => "skipped",
                _ => null
            };
            if (outcome is null)
                return Task.CompletedTask;

            _ = outcome switch { "passed" => Interlocked.Increment(ref _passed), "failed" => Interlocked.Increment(ref _failed), _ => Interlocked.Increment(ref _skipped) };

            var id = node.Properties.SingleOrDefault<TestMethodIdentifierProperty>();
            var b = new Bundle();
            b.PutString("test", id is not null ? $"{id.Namespace}.{id.TypeName}.{id.MethodName}" : node.DisplayName);
            b.PutString("outcome", outcome);
            instrumentation.SendStatus(0, b);
        }
        return Task.CompletedTask;
    }
}
