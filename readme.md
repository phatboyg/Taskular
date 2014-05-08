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

		
