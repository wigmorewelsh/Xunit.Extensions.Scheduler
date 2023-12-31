using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.Extensions.Scheduler;

public class IntegrationTestClassRunner : XunitTestClassRunner
{
    private readonly IntegrationTestScheduler integrationTestScheduler;

    public IntegrationTestClassRunner(ITestClass testClass, IReflectionTypeInfo @class,
        IEnumerable<IXunitTestCase> testCases, IMessageSink diagnosticMessageSink, IMessageBus messageBus,
        ITestCaseOrderer testCaseOrderer, ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource, IDictionary<Type, object> collectionFixtureMappings,
        IntegrationTestScheduler integrationTestScheduler) : base(testClass, @class, testCases, diagnosticMessageSink,
        messageBus, testCaseOrderer, aggregator, cancellationTokenSource, collectionFixtureMappings)
    {
        this.integrationTestScheduler = integrationTestScheduler;
    }

    protected override Task<RunSummary> RunTestMethodAsync(ITestMethod testMethod, IReflectionMethodInfo method,
        IEnumerable<IXunitTestCase> testCases, object[] constructorArguments)
        => new IntegrationTestMethodRunner(testMethod, Class, method, testCases, DiagnosticMessageSink, MessageBus,
                new ExceptionAggregator(Aggregator), CancellationTokenSource, constructorArguments,
                integrationTestScheduler)
            .RunAsync();
}