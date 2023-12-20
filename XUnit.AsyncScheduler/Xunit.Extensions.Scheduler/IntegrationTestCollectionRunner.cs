using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.Extensions.Scheduler;

public class IntegrationTestCollectionRunner : XunitTestCollectionRunner
{
    private readonly IntegrationTestScheduler integrationTestScheduler;

    public IntegrationTestCollectionRunner(ITestCollection testCollection, IEnumerable<IXunitTestCase> testCases,
        IMessageSink diagnosticMessageSink, IMessageBus messageBus, ITestCaseOrderer testCaseOrderer,
        ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource,
        IntegrationTestScheduler integrationTestScheduler) : base(testCollection, testCases, diagnosticMessageSink, messageBus,
        testCaseOrderer, aggregator, cancellationTokenSource)
    {
        this.integrationTestScheduler = integrationTestScheduler;
    }

    protected override Task<RunSummary> RunTestClassAsync(ITestClass testClass, IReflectionTypeInfo @class,
        IEnumerable<IXunitTestCase> testCases)
        => new IntegrationTestClassRunner(testClass, @class, testCases, DiagnosticMessageSink, MessageBus, TestCaseOrderer,
            new ExceptionAggregator(Aggregator), CancellationTokenSource, CollectionFixtureMappings,
            integrationTestScheduler).RunAsync();
}