using System.Collections.Generic;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.Extensions.Scheduler;

public class IntegrationTestFrameworkExecutor : XunitTestFrameworkExecutor
{
    public IntegrationTestFrameworkExecutor(AssemblyName assemblyName, ISourceInformationProvider sourceInformationProvider,
        IMessageSink diagnosticMessageSink) : base(assemblyName, sourceInformationProvider, diagnosticMessageSink)
    {
    }

    protected override async void RunTestCases(IEnumerable<IXunitTestCase> testCases, IMessageSink executionMessageSink,
        ITestFrameworkExecutionOptions executionOptions)
    {
        using (var assemblyRunner = new IntegrationTestAssemblyRunner(TestAssembly, testCases, DiagnosticMessageSink,
                   executionMessageSink, executionOptions))
        {
            await assemblyRunner.RunAsync();
        }
    }
}