using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace OliWorkshop.Threading.Reactive
{
    /// <summary>
    /// Delegate that represent the subscriber agent
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public delegate Task Subscriber(BroadcastMessage message);

    /// <summary>
    /// This artifact allow create a reactive flow with channel, event and flow structure
    /// </summary>
    public class BroadcastConcurrent
    {
        /// <summary>
        /// The process task that allow block the thread where you has created the broadcasting
        /// concurrent service
        /// </summary>
        public Task Process => Messsger.Completion;

        /// <summary>
        /// return true if the cancellation is requested
        /// </summary>
        public bool IsCancellationRequested => TokenSource.IsCancellationRequested;

        /// <summary>
        /// Basic Broadcast constrcutor
        /// </summary>
        public BroadcastConcurrent(int MaxConcurrency, int MaxMessages = int.MaxValue)
        {
            // create messeger engine
            Messsger = new ActionBlock<BroadcastMessage>(BlockHandler, new ExecutionDataflowBlockOptions { 
                CancellationToken = TokenSource.Token,
                MaxDegreeOfParallelism = MaxConcurrency,
                BoundedCapacity = MaxMessages,
                EnsureOrdered = false
            });
        }

        /// <summary>
        /// The global token cancellation
        /// </summary>
        CancellationTokenSource TokenSource = new CancellationTokenSource();

        /// <summary>
        /// The engine artifact to manage the concurrent broadcasting flow
        /// </summary>
        ActionBlock<BroadcastMessage> Messsger { get; }

        /// <summary>
        /// Event Listeners record to create execution in background
        /// </summary>
        Dictionary<string, List<Subscriber>> Subscribers { get; } = new Dictionary<string, List<Subscriber>>();

        /// <summary>
        /// Post stream bytes as message
        /// </summary>
        public void PostMessage(string channel, byte[] message)
        {
            Messsger.Post(new BroadcastMessage(channel, message));
        }

        /// <summary>
        /// Post stream bytes as message
        /// </summary>
        public void PostMessage(string channel, Stream message)
        {
            Messsger.Post(new BroadcastMessage(channel, message));
        }

        /// <summary>
        /// Create a new subscriptor to a channel
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="subscriber"></param>
        public void Subscribe(string channel, Subscriber subscriber)
        {
            if (subscriber is null)
            {
                throw new ArgumentNullException(nameof(subscriber));
            }

            /// add new subscriptor or create a new list subscriptor
            if (Subscribers.ContainsKey(channel))
            {
                Subscribers[channel].Add(subscriber);
            }
            else
            {
                Subscribers.Add(channel, new List<Subscriber> { subscriber });
            }
        }

        /// <summary>
        /// Request the cancellation
        /// </summary>
        public void Close()
        {
            Messsger.Complete();
        }

        /// <summary>
        /// Request the cancellation
        /// </summary>
        public void Cancel()
        {
            TokenSource.Cancel();
        }

        /// <summary>
        /// Basic method to control the concurrent flow of broadcasting
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private Task BlockHandler(BroadcastMessage message)
        {
            // check the cancellation requested fro global token
            try
            {
                // emit the exception for cancellation
                TokenSource.Token.ThrowIfCancellationRequested();

                // fetch the subscriptors
                if (Subscribers.ContainsKey(message.Channel))
                {
                    // every block task contains the completion of subscribers tasks
                    return Task.WhenAll(
                        // map every subscriptoras a task to broadcasting
                        Subscribers[message.Channel].Select(sub => sub.Invoke(message))
                    );
                }
                else
                {
                    // if there are not subcriber's then do not anything
                    return Task.CompletedTask;
                }
            }
            catch (OperationCanceledException)
            {
                // if the cancellation is requested then complete the task of block engine
                Messsger.Complete();
                return Task.CompletedTask;
            }
        }
    }
}
