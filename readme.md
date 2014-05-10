# Taskular

## Introduction

Taskular leverages the Task Parallel Library (TPL) to provide the ability to compose task chains. Taskular extends the built-in language features, including _async_ and _await_, and provides additional capabilities such as task execution, compensation, and fault handling. Additional features, including task retry, and automatically repeating tasks, are also included.

### Requirements

Taskular requires .NET 4.5 (or later).

### License

Apache 2.0 - see LICENSE

### Getting Taskular

Taskular is distributed using NuGet (http://nuget.org) and should be available in the Visual Studio Package Manager. _Note, that Taskular is strong-named, but the ```AssemblyVersion``` is locked at 1.0.0 to avoid breaking references and builds. Strong names suck, but it's hard to do some things without one._

### Source

The project site is located on GitHub: https://github.com/phatboyg/Taskular.git

A few stupid things to think about if you're going to contribute to the project:

 1. Create all pull requests off the ```develop``` branch, in a separate branch of your own.
 2. Use Visual Studio 2013 if possible.
 3. Spaces, not Tabs. (Tools -> Options -> Text Editor -> All Languages -> Tabs to use "Tab Size" = 4, "Indent Size" = 4, and "Insert Spaces"
 4. Unit tests matter, so make sure you cover your changes with tests, and run them all to ensure nothing is broken.

The project uses ```rake``` for the continuous integration builds, so if you have it, use it to verify your changes. 

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

### Task<T>

In addition to support the base _Task_, Taskular can also support the composition of _Task<T>_ chains.

    Payload payload = new Payload { Name = "World"};

    Task<Payload> task = ComposerFactory.Compose(payload, composer =>
        {
            composer.Execute(p => Console.WriteLine("Hello, {0}.", p.Name));
        });

This allows the Task result to be passed to each task in the chain.

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


