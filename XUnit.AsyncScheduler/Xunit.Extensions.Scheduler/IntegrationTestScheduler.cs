using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace Xunit.Extensions.Scheduler;

public class IntegrationTestScheduler : IAsyncDisposable
{
    private readonly Channel<UnitOfWork> loading = Channel.CreateUnbounded<UnitOfWork>();
    private readonly List<Task> workers = new();
    private readonly CancellationTokenSource tokenSource;

    public IntegrationTestScheduler(int parallel)
    {
            tokenSource = new CancellationTokenSource();
            for (int i = 0; i < parallel; i++)
            {
                workers.Add(Task.Run(() => ConsumeLoop(tokenSource.Token))); 
            }
        }

    private async Task ConsumeLoop(CancellationToken cancellationToken)
    {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var next = await loading.Reader.ReadAsync(cancellationToken);
                    await next.Run();
                }
            }
            catch (Exception err)
            {
                return;
            }
        }

    public async ValueTask DisposeAsync()
    {
            tokenSource.Cancel();
            await Task.WhenAll(workers);
        }

    private class UnitOfWork
    {
        private TaskCompletionSource<RunSummary> ResultThing = new();
        private Func<Task<RunSummary>> Func { get; }
        public Task<RunSummary> Result => ResultThing.Task;

        public UnitOfWork(Func<Task<RunSummary>> func)
        {
                Func = func;
            }

        public async Task Run()
        {
                try
                {
                    var res = await Func();
                    ResultThing.SetResult(res);
                }
                catch (Exception err)
                {
                    ResultThing.SetException(err);
                }
            }
    }
        
    public Task<RunSummary> Run(Func<Task<RunSummary>> func)
    {
            var uow = new UnitOfWork(func);
            loading.Writer.WriteAsync(uow);

            return uow.Result;
        }
}