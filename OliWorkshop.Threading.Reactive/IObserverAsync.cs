using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OliWorkshop.Threading.Reactive
{
    public interface IObserverAsync<Target>
    {
        Task NextAsync(Task<Target> target);

        void Complete(Task completion);
    }
}
