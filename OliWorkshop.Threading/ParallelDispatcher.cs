using System;
using System.Collections.Generic;
using System.Text;

namespace OliWorkshop.Threading
{
    /// <summary>
    /// Delegate to represent a event handler that is listening an event
    /// </summary>
    /// <param name="payload"></param>
    public delegate void EventListener(object payload);

    /// <summary>
    /// Delegate to represent a event handler that is listening an event
    /// </summary>
    /// <param name="payload"></param>
    public delegate void EventListener<T>(T payload);

    /// <summary>
    /// The parallel dispatcher is artifact to handle event in parallel threads
    /// </summary>
    public class ParallelDispatcher
    {
        /// <summary>
        /// This class require a thread manager instance to
        /// work with event in background
        /// </summary>
        /// <param name="manager"></param>
        public ParallelDispatcher(ThreadManager manager)
        {
            Manager = manager ?? throw new ArgumentNullException(nameof(manager));
        }

        ThreadManager Manager { get; }

        Dictionary<string, List<EventListener>> Listeners { get; set; }

        /// <summary>
        /// Add a new EventListener 
        /// </summary>
        /// <param name="identifierEvent"></param>
        /// <param name="action"></param>
        public void Listen(string identifierEvent, EventListener action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (Listeners.ContainsKey(identifierEvent))
            {
                Listeners[identifierEvent].Add(action);
            }
            else
            {
                Listeners.Add(identifierEvent, new List<EventListener> { action });
            }
        }

        /// <summary>
        /// Add a new EventListener 
        /// </summary>
        /// <param name="identifierEvent"></param>
        /// <param name="action"></param>
        public void Listen<TEvent>(EventListener<TEvent> actionTyped)
        {
            if (actionTyped is null)
            {
                throw new ArgumentNullException(nameof(actionTyped));
            }

            string identifierEvent = typeof(TEvent).FullName;

            // create a type alias to set compatibility
            var action =  actionTyped as EventListener;

            if (Listeners.ContainsKey(identifierEvent))
            {
                Listeners[identifierEvent].Add(action);
            }
            else
            {
                Listeners.Add(identifierEvent, new List<EventListener> { action });
            }
        }

        /// <summary>
        /// Dispacther method to execute event listeners
        /// </summary>
        /// <param name="identifierEvent"></param>
        /// <param name="payload"></param>
        public void Dispacth(string identifierEvent, object payload)
        {
            if(Listeners.ContainsKey(identifierEvent))
            {
               // execute all listeners
               Listeners[identifierEvent].ForEach(current => {

                   // run in background by queue
                   Manager.EnqueueAction(delegate { current.Invoke(payload);  });
               });
            }
            else
            {
                // the evetn not found
                throw new EventNotFoundException();
            }
        }

        /// <summary>
        /// Dispacth event by type fullname and that same insatnce is the payload
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="payload"></param>
        public void Dispacth<T>(T payload)
        {
            // dispacth by type string fullname
            Dispacth(typeof(T).FullName, payload);
        }
    }
}
