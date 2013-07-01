namespace CodeNexus
{
    public interface IService
    {
        ServiceState State { get; }
        void Start();
        void Stop();
    }
}