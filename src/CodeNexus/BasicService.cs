using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace CodeNexus
{
    public abstract class BasicService : IService
    {
        private Task _serviceWorker;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        public ServiceState State { get; private set; }

        protected abstract void _serviceTask(CancellationToken cancellationToken);

        public void Start()
        {
            switch ( this.State)
            {
                case ServiceState.Shiny:
                case ServiceState.Stopped:
                    this.State = ServiceState.Starting;
                    _serviceWorker = Task.Factory.StartNew(() => _serviceTask(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
                    _serviceWorker.ContinueWith((t) =>
                                                    {
                                                        Debugger.Launch();
                                                        this.State = ServiceState.Stopped;
                                                        Console.WriteLine(t.Exception.Message);
                                                    }, TaskContinuationOptions.OnlyOnFaulted);
                    this.State = ServiceState.Running;
                    break;
                case ServiceState.Starting:
                    // wait till state is started, then return
                    break;
                case ServiceState.Stopping:
                    // wait till state is stopped, then call self
                    break;
                case ServiceState.Running:
                    // already running, continue quietly
                    break;
            }
        }

        public void Stop()
        {
            switch (this.State)
            {
                case ServiceState.Shiny:
                case ServiceState.Stopped:
                    throw new Exception("Service not running.");
                case ServiceState.Starting:
                    // wait till state is started, then call self
                    break;
                case ServiceState.Stopping:
                    // wait till state is stopped, then return
                    break;
                case ServiceState.Running:
                    _cancellationTokenSource.Cancel();
                    _serviceWorker.Wait();
                    break;
            }
        }
    }
}
