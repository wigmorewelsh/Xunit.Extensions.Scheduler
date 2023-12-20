using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.Extensions.Scheduler;

public class IntegrationTestAssemblyRunner : XunitTestAssemblyRunner
{
    IntegrationTestScheduler scheduler = new IntegrationTestScheduler(Environment.ProcessorCount);
    private static readonly AsyncLimiter Limiter = new AsyncLimiter(Environment.ProcessorCount);

    public IntegrationTestAssemblyRunner(ITestAssembly testAssembly, IEnumerable<IXunitTestCase> testCases,
        IMessageSink diagnosticMessageSink, IMessageSink executionMessageSink,
        ITestFrameworkExecutionOptions executionOptions) : base(testAssembly, testCases, diagnosticMessageSink,
        executionMessageSink, executionOptions)
    {
    }


    protected override async Task<RunSummary> RunTestCollectionAsync(IMessageBus messageBus,
        ITestCollection testCollection, IEnumerable<IXunitTestCase> testCases,
        CancellationTokenSource cancellationTokenSource)
    {
        using var _ = await Limiter.LockAsync(cancellationTokenSource.Token);
        return await new IntegrationTestCollectionRunner(testCollection, testCases, DiagnosticMessageSink, messageBus,
            TestCaseOrderer, new ExceptionAggregator(Aggregator), cancellationTokenSource, scheduler).RunAsync();
    }


    protected override async Task<RunSummary> RunTestCollectionsAsync(IMessageBus messageBus,
        CancellationTokenSource cancellationTokenSource)
    {
        Task<RunSummary> TaskRunner(Func<Task<RunSummary>> code) => Task.Run(code, cancellationTokenSource.Token);

        List<Task<RunSummary>>? parallel = null;
        List<Func<Task<RunSummary>>>? nonParallel = null;
        var summaries = new List<RunSummary>();

        foreach (var collection in OrderTestCollections())
        {
            Func<Task<RunSummary>> task = () =>
                RunTestCollectionAsync(messageBus, collection.Item1, collection.Item2, cancellationTokenSource);

            // attr is null here from our new unit test, but I'm not sure if that's expected or there's a cheaper approach here
            // Current approach is trying to avoid any changes to the abstractions at all
            var attr = collection.Item1.CollectionDefinition?.GetCustomAttributes(typeof(CollectionDefinitionAttribute))
                .SingleOrDefault();
            if (attr?.GetNamedArgument<bool>(nameof(CollectionDefinitionAttribute.DisableParallelization)) == true)
            {
                (nonParallel ??= new List<Func<Task<RunSummary>>>()).Add(task);
            }
            else
            {
                (parallel ??= new List<Task<RunSummary>>()).Add(TaskRunner(task));
            }
        }

        if (parallel?.Count > 0)
        {
            foreach (var task in parallel)
            {
                try
                {
                    summaries.Add(await task);
                }
                catch (TaskCanceledException)
                {
                }
            }
        }

        if (nonParallel?.Count > 0)
        {
            foreach (var task in nonParallel)
            {
                try
                {
                    summaries.Add(await TaskRunner(task));
                    if (cancellationTokenSource.IsCancellationRequested)
                        break;
                }
                catch (TaskCanceledException)
                {
                }
            }
        }

        return new RunSummary()
        {
            Total = summaries.Sum(s => s.Total),
            Failed = summaries.Sum(s => s.Failed),
            Skipped = summaries.Sum(s => s.Skipped)
        };
    }
}