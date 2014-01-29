using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;

namespace Eowin.AzureServiceBusRelay.Server.Tests
{
    public class RequestExecutor<TInput, TOutput>
    {
        class Request
        {
            public TInput Value { get; private set; }
            public TaskCompletionSource<TOutput> TaskCompletionSource { get; private set; }
            public Request(TInput t)
            {
                Value = t;
                TaskCompletionSource = new TaskCompletionSource<TOutput>();
            }
        }

        private readonly BlockingCollection<Request> _queue = new BlockingCollection<Request>(new ConcurrentQueue<Request>());

        public Task<TOutput> Post(TInput request)
        {
            var r = new Request(request);
            _queue.Add(r);
            return r.TaskCompletionSource.Task;
        }

        public void RunSync(Func<TInput, TOutput> f)
        {
            var wi = _queue.Take();
            var r = f(wi.Value);
            wi.TaskCompletionSource.SetResult(r);
        }
    }
}

