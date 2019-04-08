---
title: "Avoiding ConfigureAwait with Castle and Dependency Injection."
date: 2019-04-07
comment_issue_id: 11
---

# The problem

In a desktop application I'm building for work, we have a custom ```SynchronizationContext``` which dispatches all continuations to the UI thread. This makes things easier to reason with during development, since we are always on the same thread.

We developed a large chunk of our application with this approach. It was fine for a while, but as the number of continuations in our service layer increased, we began to notice some drawbacks using with this approach.

Inside of our service layer, we were using ```async/await``` for things that have no concern/knowledge of a GUI. HOwever, every continuation was still happening on the UI thread. As our service layer grew, animations that were once super smooth started to quickly become very choppy. Non-UI related code was being run on the UI for no good reason.

# The solution

We needed a way to trigger each method in our service layer to popup off the current gui-```SynchronizationContext``` and onto the thread pool. We immediately came up with a few solutions.

1. Litter our service layer with ```.ConfigureAwait(false)```.
2. Update our UI layer to clear/restore the ```SynchronizationContext``` whenever we call our service methods.

I didn't like either of these approaches because they required changing many lines of code and were (human) error prone. So I sought out another approach that will prevent us from changing any code, while also ensuring that *every* service method is not continuated on the UI thread. Consider the following:

```c#
public interface IService
{
    Task RunMethod();
}

public class Service : IService
{
    public async Task RunMethod()
    {
        await Task.Yield();
        Thread.Sleep(TimeSpan.FromSeconds(5));
    }
}

public class MyViewModel
{
    private readonly IService _service;

    public MyViewModel(IService service)
    {
        _service = service;
    }
    
    public async Task OnButtonClicked()
    {
        await _service.RunMethod();
    }
}
```

If this code were to be called, as is, when ```OnButtonClicked > RunMethod``` is called, ```Thread.Sleep``` would eventually be ran on the UI thread. Not good!

I need a way to wrap an instance of ```IService``` in another implementation of ```IService``` that simply clears the ```SynchronizationContext``` before called the inner ```IService```. Consider the following:

```c#
public class NoSyncServiceWrapper : IService
{
    private IService _inner;

    public NoSyncServiceWrapper(IService inner)
    {
        _inner = inner;
    }
    
    public Task RunMethod()
    {
        var oldContext = SynchronizationContext.Current;
        SynchronizationContext.SetSynchronizationContext(null);
        try
        {
            return _inner.RunMethod()
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(oldContext);
        }
    }
}
```

Now, if I pass ```NoSyncServiceWrapper``` to the ```MyViewModel```, the concern of ```SynchronizationContext``` will be in neither my service layer or my UI layer. No code needs to be changed!

```c#
var service = new Service();
var viewModel = new MyViewModel(new NoSyncServiceWrapper(service));
await viewModel.OnButtonClicked(); // Never blocking the UI thread!
```

The only problem now is that I have to manage a separate implementation of every service/method pair with this boiler plate code. Yeah.. I'd rather not..

# Using Castle to dynamically generate the wrappers.

[```Castle.Core```](https://www.nuget.org/packages/castle.core/) supports wrapping an instance of an interface with a dynamic implementation of the interface to perform some pre-post logic on the given instance. This is ideal for things like logging/profiling, and in our case, clearing/restoring the ```SynchronizationContext```. It is actually pretty simple!

```c#
public static class ProxyWrapper
{
    private static readonly ProxyGenerator _proxyGenerator = new ProxyGenerator();
    private static readonly SyncContextInterceptor _syncContextInterceptor = new SyncContextInterceptor();
    
    public static object WrapService(Type serviceType, object instance)
    {
        return _proxyGenerator.CreateInterfaceProxyWithTargetInterface(serviceType,
            instance,
            new ProxyGenerationOptions(),
            _syncContextInterceptor);
    }
    
    private class SyncContextInterceptor : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            var syncContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(null);
            try
            {
                invocation.Proceed();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(syncContext);
            }
        }
    }
}
```

Now, all we need to do is this:

```c#
var service = new Service();
var wrappingService = ProxyWrapper.WrapService(typeof(IService), service);
var viewModel = new MyViewModel(wrappingService);
await viewModel.OnButtonClicked(); // Never blocking the UI thread!
```

# Using Dependency Injection

Ideally, you'd want your proxies configured/wrapped in your container. Most containers support intercepting/replacing services before they are given to constructors. I am using Microsoft's ```Microsoft.Extensions.DependencyInjection```, which unfortunately doesn't support it (see [this](https://github.com/aspnet/Extensions/issues/1294) issue). So instead, I have to get creative when registering my services.

```c#
var services = new ServiceCollection();
services.AddTransient<MyViewModel>();
services.AddTransient(typeof(IService), provider =>
{
    var instance = ActivatorUtilities.CreateInstance(provider, typeof(Service));
    return ProxyWrapper.WrapService(typeof(IService), instance;
});

var provider = services.BuildServiceProvider();

var viewModel = provider.GetService<MyViewModel>();
await viewModel.OnButtonClicked(); // Never blocking the UI thread!
```

# Conclusion

Using ```Castle.Core``` to dynamically wrap my service layer to clear the ```SynchronizationContext``` allowed me to leave my existing code-base largely unmodified. Using dependency injection, I was able wrap the services in a way which is transparent to my entire service layer.
