---
layout: post
title:  "Performance: C++/CLI vs COM"
date:   2013-03-07
categories: development
tags: [sample post, readability]
disqus-identifier: post-1053
redirect_from: "/blog/performance-ccli-vs-com"
---
In my current project, we are going to require a great deal of native code due to interation with drivers and hardware. The application is going to be based in .NET, at least the UI portion, but the heart has to live in native C++.

I began investigating different ways to interop with .NET and unmanaged code. There seems to be three approaches.

1. PInvoke
2. COM/COM+/Interop
3. C++/CLI

I did not look into the PInvoke method because I know that it will not be suitable for my needs. Things get complicated really quick with PInvoke. It is ideal for small/quick access to Win32, but not beyond that. With that said, I produced some metrics about the performance between COM+ and C++/CLI.

I have one class (unmanaged) that I use in every metric.

{% highlight c++ %}
#pragma once
__declspec(dllexport) class NativeClass
{
public:
	__declspec(dllexport) NativeClass(void);
	__declspec(dllexport) ~NativeClass(void);
	__declspec(dllexport) int GetWindowsVersion(int numberOfExecutions);
};

{% endhighlight %}

There are duplications of this class but they all do the same thing. The method we are testing is a simple GetVersionEx from the WinAPI.

{% highlight c++ %}
int NativeClass::GetWindowsVersion(int numberOfExecutions)
{
	OSVERSIONINFO osvi;

	for(int x = 0; x < numberOfExecutions; x++)
	{
		ZeroMemory(&osvi, sizeof(OSVERSIONINFO));
		osvi.dwOSVersionInfoSize = sizeof(OSVERSIONINFO);
		GetVersionEx(&osvi);
	}

	return osvi.dwBuildNumber;
}
{% endhighlight %}

The parameter "numberOfExecutions" tells the unmanaged class how many times it should invoke GetVersionEx. This native class is written/duplicated in multiple projects to similate the same process. I have 4 projects.

1. PerformanceTest - This is a console application that runs the performance in C#/.NET
2. CLITest - This is a C++/CLI project that contains two classes.
The first class is a wrapper around an unmanaged class that is compiled in the same C++/CLI project.
The second class is a wrapper around an unmanaged class that is compiled in a seperate native dll (not C++/CLI).
3. COMTest - This is a project that has a COM wrapper around an unmanaged class.

I tested two scenarios.

1. Calling an unmanaged class from .NET multiple times.
2. Calling an unmanaged class from .NET a single time while doing a large task (numberOfExecutions = 10000000).

Here are the results from the console application for the 3 situations.

1. .NET to CLI calling an unmanaged class compiled in C++/CLI.
2. .NET to CLI calling an unmanaged class compiled in native C++.
3. .NET to COM calling an unmanaged class compiled in native C++.

Here is the output of the test.

    CLI..........................
         GetWindowsVersion:
             Calls: 100000
             Executions: 1
             Result: 00:00:00.0190000
         GetWindowsVersion:
             Calls: 1
             Executions: 100000000
             Result: 00:00:15.7790000
    CLI to native................
         GetWindowsVersion:
             Calls: 100000
             Executions: 1
             Result: 00:00:00.0180000
         GetWindowsVersion:
             Calls: 1
             Executions: 100000000
             Result: 00:00:04.8020000
    COM..........................
         GetWindowsVersion:
             Calls: 100000
             Executions: 1
             Result: 00:00:02.6120000
         GetWindowsVersion:
             Calls: 1
             Executions: 100000000
             Result: 00:00:04.7310000

Conclusion
----------

Native code compiled and running under C++/CLI is a great deal slower than running pure native code. COM would be the obviouse choice due to it running pure unmanaged code, but C++/CLI can give the same performance if used as a wrapper around a pure native assembly. In addition, COM/ATL is very difficult to learn, however, Visual Studio's class wizards make it easier. Also, it seems that, making a great deal of calls to native code is MUCH more efficient through C++/CLI as opposed to COM, but this may be an unlikely scenario. Given these results and given the fact that this native code I am writing will be written primarily for .NET (as opposed to COM/activex/etc), I would recommend using the C++/CLI wrapper approach. Same performance, more flexible interop.

The project used for running these tests can be found on github [here](https://github.com/theonlylawislove/CLICOMPerformanceTest).
