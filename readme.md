# Taskular

## Introduction

Taskular leverages the Task Parallel Library (TPL) to provide the ability to compose task chains. Taskular extends the built-in language features, including _async_ and _await_, and provides additional capabilities such as task execution, compensation, and fault handling.

## Features

### Retry

When a task composition generates a fault, _Taskular_ can catch the fault and automatically retry the task composition using a specified retry policy. 

### Repeat

A task composition can be scheduled to repeat at a designated interval until it is cancelled.

## Usage

The task composer is the core of Taskular. For example, to execute a single method the syntax is shown below.

	var task = ComposerFactory.Compose(composer =>
		{
			composer.Execute(() => Console.WriteLine("Hello, World."));
		});

### Task Execution

In the above example, a single task is created which begins executing immediately. The returned _Task_ can then be used like any other Task, for example, it can be awaited, etc. The power comes into play in how multiple tasks can easily be composed into a sequential execution chain.

    var task = ComposerFactory.Compose(composer =>
        {
            composer.Execute(() => Console.WriteLine("Hello, World."));
            composer.Execute(() => Console.WriteLine("Hello, again."));
        });

Each _Execute_ method delegate will be executed sequentially. If the first _Execute_ method faults, the second method will not be executed as the chain faulted. 

It's also possible to perform task execution synchronously, without relying on the Task scheduler. To execute a task synchronously, the ExecuteOptions are used.

    var task = ComposerFactory.Compose(composer =>
        {
            composer.Execute(() => Console.WriteLine("Hello, World."), ExecuteOptions.Synchronously);
            composer.Execute(() => Console.WriteLine("Hello, again."), ExecuteOptions.Synchronously);
        });

In this example, the both methods are executed immediately, without using a Task. This makes it possible to build highly composed task chains that execute quickly, without the overhead of the TPL. In most cases, this is useful to build a chain of tasks on an existing TPL task, to keep execution asynchronous but fast. As long as the tasks are completed, they will continue to execute synchronously. However, if a Task is scheduled in the middle that requires asynchronous execution, it can fit easily into the Task chain.

Asynchronous methods can also be executed, using the ExecuteAsync method. For example.

    var task = ComposerFactory.Compose(new Payload(), composer =>
    {
        composer.ExecuteAsync(async (payload, token) =>
        {
            var result = await SomeAsyncMethod(payload.Name);

            payload.Result = result;
        });
    });

The result of the asynchronous method is awaited, and then used to set the property on the payload. The cancellation token for the task is also available, allowing asynchronous methods to use the token to cancel operations, such as an _HttpClient_ request to a server.


### Compensation

In order to handle faults, Taskular has the ability to specify a compensation method.

    var task = ComposerFactory.Compose(composer =>
        {
            composer.Execute(() => throw new InvalidOperationException());
            composer.Compensate(compensation => compensation.Handled());
            composer.Execute(() => Console.WriteLine("Hello, World."));
        });

The first _Execute_ method throws an exception, but the following _Compensate_ method marks the exception as handled, allowing the next _Execute_ method to execute.


### Repeating Tasks

Taskular provides the ability to repeat a task at a regular interval. The _Repeat_ method is used as shown below.

	var stop = new CancellationTokenSource();

    Task task = ComposerFactory.Compose(x => x.Repeat(1000, composer =>
    {
        composer.Execute(() => Console.WriteLine("Hello, World."));
    }, stop.Token));

The example above will recompose the _Execute_ task every 1000 milliseconds. To cancel the repeating task, the _CancellationTokenSource_ is used.

It's important to note that in this case, if the task faults, the repeating task will fault and the task itself will be faulted. If it is important that the task be repeated even if it fails, the task should compensate the fault and mark it as handled as shown below.

    Task task = ComposerFactory.Compose(x => x.Repeat(1000, composer =>
    {
        composer.Execute(() => Console.WriteLine("Hello, World."));
        composer.Execute(() => throw new InvalidOperationException("OMG!"));
        composer.Compensate(x => x.Handled());
    }));


