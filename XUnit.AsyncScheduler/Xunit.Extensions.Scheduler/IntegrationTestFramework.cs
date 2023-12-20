using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.Extensions.Scheduler;

public class IntegrationTestFramework : XunitTestFramework
{
    public IntegrationTestFramework(IMessageSink messageSink)
        : base(messageSink)
    {
        }

    protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName)
        => new IntegrationTestFrameworkExecutor(assemblyName, SourceInformationProvider, DiagnosticMessageSink);
}