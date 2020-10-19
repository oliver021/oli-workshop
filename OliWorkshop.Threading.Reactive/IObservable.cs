namespace OliWorkshop.Threading.Reactive
{
    public interface IObservable<Target>
    {
        void Bind(IObserverAsync<Target> observer);
    }
}
