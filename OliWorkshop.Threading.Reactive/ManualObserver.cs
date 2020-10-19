using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OliWorkshop.Threading.Reactive
{
    /// <summary>
    /// Manual Observer is artifacto to observe a abritary sequences of changes
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ManualObserveble<T> : IObservable<T>
    {
        TaskCompletionSource<byte> TaskCompletionSource;

        List<IObserverAsync<T>> Observers = new List<IObserverAsync<T>>();

        public ManualObserveble()
        {
            TaskCompletionSource = new TaskCompletionSource<byte>();
        }

        /// <summary>
        /// Create a binder observer with this observable
        /// </summary>
        /// <param name="observer"></param>
        public void Bind(IObserverAsync<T> observer)
        {
            Observers.Add(observer);
            observer.Complete(TaskCompletionSource.Task);
        }

        /// <summary>
        /// Update a new change to inform the last value
        /// </summary>
        /// <param name="handler"></param>
        public void Update(Func<Task<T>> handler)
        {
            var task = handler.Invoke();
            Observers.ForEach( o => o.NextAsync(task));
        }

        /// <summary>
        /// Update a new change to inform the last value
        /// </summary>
        /// <param name="element"></param>
        public void Update(Task<T> element)
        {
            Observers.ForEach(o => o.NextAsync(element));
        }

        /// <summary>
        /// Update a new change to inform the last value and wait for all observers
        /// </summary>
        /// <param name="handler"></param>
        /// <returns></returns>
        public Task UpdateAsync(Func<Task<T>> handler)
        {
            var task = handler.Invoke();
            return Task.WhenAll(Observers.Select(o => o.NextAsync(task)));
        }

        /// <summary>
        /// Update a new change to inform the last value and wait for all observers
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public Task UpdateAsync(Task<T> element)
        {
            return Task.WhenAll(Observers.Select(o => o.NextAsync(element)));
        }

        /// <summary>
        /// Notify all every observer that the flow is complete
        /// </summary>
        public void Complete()
        {
            TaskCompletionSource.SetResult(1);
        }
    }
}