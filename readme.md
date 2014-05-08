# Taskular

## Introduction

Taskular leverages the Task Parallel Library (TPL) to provide the ability to compose task chains. Taskular extends the built-in language features, including _async_ and _await_, and provides additional capabilities such as task execution, compensation, and fault handling.

## Features

### Retry

When a task composition generates a fault, _Taskular_ can catch the fault and automatically retry the task composition using a specified retry policy. 

### Repeat

A task composition can be scheduled to repeat at a designated interval until it is cancelled.

## Usage

The task composer is the core of Taskular. 

	var task = ComposerFactory.Compose(composer =>
		{
			composer.Execute(() => Console.WriteLine("Hello, World."));
		});

		

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


