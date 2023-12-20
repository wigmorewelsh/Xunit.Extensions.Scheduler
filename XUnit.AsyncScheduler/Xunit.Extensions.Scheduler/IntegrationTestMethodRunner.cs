using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.Extensions.Scheduler;

public class IntegrationTestMethodRunner : XunitTestMethodRunner
{
    readonly object[] constructorArguments;
    private readonly IntegrationTestScheduler integrationTestScheduler;
    readonly IMessageSink diagnosticMessageSink;

    public IntegrationTestMethodRunner(ITestMethod testMethod, IReflectionTypeInfo @class, IReflectionMethodInfo method,
        IEnumerable<IXunitTestCase> testCases, IMessageSink diagnosticMessageSink, IMessageBus messageBus,
        ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource,
        object[] constructorArguments, IntegrationTestScheduler integrationTestScheduler) : base(testMethod, @class, method,
        testCases, diagnosticMessageSink, messageBus, aggregator, cancellationTokenSource, constructorArguments)
    {
        this.constructorArguments = constructorArguments;
        this.integrationTestScheduler = integrationTestScheduler;
        this.diagnosticMessageSink = diagnosticMessageSink;
    }

    protected override Task<RunSummary> RunTestCaseAsync(IXunitTestCase testCase)
        => integrationTestScheduler.Run(() => testCase.RunAsync(diagnosticMessageSink, MessageBus, constructorArguments,
            new ExceptionAggregator(Aggregator), CancellationTokenSource));
}