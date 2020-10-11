# oli-workshop - Workshop helpers 
A very helpful collection of tools and code libraries for dotnet core.

## Introduction 🚀

Set of bookstores and libraries of great help that I have used personally and mainly to develop problems that require robust solutions.

### Threading Helpers.

This set of libraries helps with tasks, asynchronous processes and high complexity of parallelism, allowing you flexibility and robustness of process in few instructions.

By means of the API available in these helpers, an architecture that supports concurrency of multiple actions, events and tasks can be consolidated for parallel  execution and without deadlock or excessive blocks.

## Installation 🔧

> I working in this...

## Code Samples ⚙️

> These are some examples of the use of the ** Threading ** library which is quite simple to >  use.

> For example :
```csharp
// set the max concurrent threads
var manager = new ThreadManager(4);

manager.EnqueueAction(delegate {
	Thread.Sleep(680);
	Console.WriteLine("action 2");
});
manager.EnqueueAction(delegate { 
	Thread.Sleep(980);
 	Console.WriteLine("action 3");
});
manager.EnqueueAction(delegate { Console.WriteLine("action 1"); });

// block this thread to test and that allow the queue to finish
manager.WaitInBussy();
```


```csharp
// set the max concurrent threads
/// this test is similar to <see cref="ThreadManagerTest"/> but this is async version
// set the max concurrent threads
var manager = new ThreadManager(4, 20);

manager.EnqueueAction(delegate { Thread.Sleep(680); Console.WriteLine("action 2"); });
manager.EnqueueAction(delegate { Thread.Sleep(980); Console.WriteLine("action 3"); });
manager.EnqueueAction(delegate { Thread.Sleep(980); Console.WriteLine("action 3"); });
manager.EnqueueAction(delegate { Thread.Sleep(980); Console.WriteLine("action 3"); });
manager.EnqueueAction(delegate { Thread.Sleep(980); Console.WriteLine("action 3"); });
manager.EnqueueAction(delegate { Thread.Sleep(980); Console.WriteLine("action 3"); });
manager.EnqueueAction(delegate { Thread.Sleep(980); Console.WriteLine("action 3"); });
manager.EnqueueAction(delegate { Console.WriteLine("action 1"); });

// stopping the queue process
manager.StopQueue();

// block this thread to test and that allow the queue to finish
manager.WaitInBussy();

Console.WriteLine("excute here when all action is finished");

// as result any action was executed because the method StopQueue was called.
```
These are some examples of the use of the ** Threading.Reactive ** library which is quite simple to >  use and also contains important helper to help with broadcasting flow.

> For example :

```csharp
public Task Sample()
{
    // initalize with max concurrent the 8 messages
    var broadcasting = new BroadcastConcurrent(8, 1000);

    // use this method to subscribe a method with reactive flow
    broadcasting.Subscribe(TestChannel, async m => {

        // emulate a job
        await Task.Delay(100);

        // print a message to inform the task is successful
        Console.WriteLine("print the message: {0}", Encoding.UTF8.GetString(m.GetContentInBytes()));
    });

    // post 8 messages
    broadcasting.PostString(TestChannel, "hello in real time");
    broadcasting.PostString(TestChannel, "hello in real time 2");
    broadcasting.PostString(TestChannel, "hello in real time 3");
    broadcasting.PostString(TestChannel, "hello in real time 4");
    broadcasting.PostString(TestChannel, "hello in real time 5");
    broadcasting.PostString(TestChannel, "hello in real time 6");
    broadcasting.PostString(TestChannel, "hello in real time 7");
    broadcasting.PostString(TestChannel, "hello in real time 8");

    // wait for 110 miliseconds
    return Task.Delay(110);
}
```

> The cancellation method is avaliable to close the broadcasting flow.

```csharp
public Task TestCancellation()
{
    // basic index iteration
    int i = 1;

    // initalize with max concurrent the 8 messages
    var broadcasting = new BroadcastConcurrent(1, 1000);

    broadcasting.Subscribe(TestChannel, async m => {

		// emulate a job
		await Task.Delay(i*250);

        // print a message to inform the task is successful
        Console.WriteLine("print the message: {0}",m.GetString());
        
        // increment time awating
        i++;
    });

    // post 8 messages
    broadcasting.PostString(TestChannel, "hello in real time 1");
    broadcasting.PostString(TestChannel, "hello in real time 2");
    broadcasting.PostString(TestChannel, "hello in real time 3");
    broadcasting.PostString(TestChannel, "hello in real time 4");
    broadcasting.PostString(TestChannel, "not should be show");

    // create simple timer async to ckeck the close feature
   	Task.Delay(1000).ContinueWith(prev => {

		// close the broadcasting
    	broadcasting.Close(); 
  	});

    // wait for 110 miliseconds
    return broadcasting.Process;
}
```