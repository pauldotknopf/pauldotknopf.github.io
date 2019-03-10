---
title: "You don’t need Cake anymore; the way to build .NET projects going forward."
date: 2019-03-10
comment_issue_id: 10
---

## The perfect void

Early on in the .NET realm, developers got creative with their build processes. Some used MSBuild proj files (like me, eww), some used Ruby's Rake system, etc, etc. *Feel free to share what you used, before Cake, in the comments. I'm super curious!*

The .NET community hadn't landed on a "recommended" approach. Then, in 2014 (when the first NuGet package was published), [Cake](https://cakebuild.net/) came on the scene and that changed. It filled a perfect void.

* .NET developers didn't need to learn Make/Bash/PowerShell/etc, all they needed was C#!
* Only one simple script is needed (aside from bootstrapping), executed with a simple command.
* Cross platform.
* Any .NET library could be referenced, bringing an immense amount of power to the build process.

Because .NET developers were starved for something that felt like *home*, they (rightfully, IMO) were willing to power through the rough edges the Cake brought to the table.

Cake became the "go to" for build processes. The community grew, plugins were built, and all was happy!

## Here comes .NET Core

But as with all great tools, the technology around them catches up. Enter, .NET Core:

* .NET Core is available on all platforms.
* The ```.csproj``` format is now simplified, making executing simple a ```Program.cs``` file as simple as ```dotnet run```.
* Because it's ```.csproj```, you can reference any .NET Standard library.

Many of the pain points that .NET developers experienced before Cake are now addressed directly in the toolchain, but there is one thing that Cake does well that cannot be replaced by .NET Core alone, *targets*.

Since .NET Core has been released, many projects have attempted to fill this new void.

* [Bullseye](https://github.com/adamralph/bullseye) - A .NET library for describing and running targets and their dependencies. Used in conjunction with [SimpleExec](https://github.com/adamralph/simple-exec), it can be really powerful.
* [NUKE](https://nuke.build/) - Build Automation System for C#/.NET

Using ```Bullseye``` and ```SimpleExec```, your ```Program.cs``` (similar to ```build.cake```) would look as follows:

```csharp
using static Bullseye.Targets;
using static SimpleExec.Command;

namespace Build
{
    static class Program
    {
        static void Main(string[] args)
        {
            Target("clean", () =>
            {
                Run("rm", "-r ./output");
            });
            
            Target("build", () =>
            {
                Run("dotnet", "build");
            });
            
            Target("publish", DependsOn("clean"), () =>
            {
                Run("dotnet", "publish -o ./output");
            });
            
            RunTargetsAndExit(args);
        }
    }
}
```

Now this is rather rudementary, but NUKE takes this multiple steps even further. It is pretty much in feature-parity with Cake in terms of the functionality it provides. I'll leave it to you to [see for yourself](https://nuke.build/), if you wish.

## But who cares? Why not Cake?

Cake has it's problems that any task runner/lib could have, but there is one large class of issues that simply will not happen with the *others* (NUKE, Bullseye, etc) because of one thing, *preprocessing*.

* The ```*.cake``` files aren't valid C# files. That means that ```cake.exe``` must interpret them, dynamically compile them, and then execute them. When you do this, you loose support for the standard toolchain (```dotnet```), and built in IDE support (with debugging!). Wouldn't you want to open your build projects in Visual Studio? Launch a debugger with F5?
* In order to run ```cake.exe```, you need to bootstrap your build, which can fail for various reasons.
* The DSL for Cake also has it's own dependency resolver, which is also common source of failure/fustrations.

These class of issues are entirely moot when you use something like ```NUKE``` since you are just dealing with a standard .NET Core console application, like any other. The preprocessor made sense back in the day when msbuild was a mess, and when developers just wanted a single file with a single command. But those days are over and .NET Core is here.

Take a look at all [the reported issues](https://github.com/cake-build/cake/issues?q=is%3Aissue) of Cake. While you peruse each issue, ask yourself, *"is this issue a result of the preprocessor?"*

I believe the maintainers of Cake recognized this disadvantage when .NET Core initially came on the scene, and started the [Frosting](https://github.com/cake-build/frosting) project. My memory says that this project had been de facto abandoned, however, it has had some recent commits. Either way, they aren't promoting it. Maybe someone can enlighten me here? Regardless, I wish it had picked up more stream than it did.

Another thing is that a common theme within the Cake ecosystem is to wrap every shelled command into a fluent/typed interface. *But why?* Consider the two following examples.

```csharp
NpmInstall(settings => settings.AddPackage("gulp").InstallGlobally());
```
```bash
npm install gulp -g
```

Do you really need to make the ```npm``` command type-safe? Which one looks nicer? What happens if the wrappers don't wrap a certain flag? What if your version of ```npm``` doesn't match the supported version that the wrappers shell out to? Is it really so hard to just invoke ```npm``` directly? Take a look at [the issues](https://github.com/cake-contrib/Cake.Npm/issues?q=is%3Aissue+) associated with this wrapper, and think to yourself, *"how many of these issues are a result of my insisting on keeping things type safe?"*

## Counter arguements

> But Cake has an extensive amount of modules/plugins!

Yes, it does. There is an extensive amount of projects integrated into Cake. However...

*Do you need a plugin?* Consider the ```npm``` plugin/wrapper mentioned above. You could do with out it.

*Are there CLI alternatives for what you are doing?* There likely is. Consider the [```Cake.Slack```](https://github.com/cake-contrib/Cake.Slack) integration and it's CLI alternative, [```slack-cli```](https://github.com/rockymadden/slack-cli). Or, consider using a pure .NET lib to integrate with the Slack API directly (see [here](https://github.com/Inumedia/SlackAPI)). I mean, this is just a .NET project, reference your libs and get on with your business!

And here is a less productive counter arguement. Why does ```Cake.Slack``` *even exist?* Why can't the users of Cake just reference the C# API directly in a Cake target? What exactly does ```Cake.Slack``` provide *as an integration*? IMO, it's a useless merging of two unlrelated concepts. The Cake ecosystem is full of examples like this.

> But my build system is really complex, Cake helps!

I've heard this before, but it simply doesn't make sense.

Have you ever been writing a C# project and thought to yourself "I wish I was using Cake right now, then I could do X". Probably not. But you've likely been in a ```build.cake``` and thought the opposite!

Take a look at [FakeItEasy](https://github.com/FakeItEasy/FakeItEasy/blob/f6a2ed3ad9af70175c9766beff75bcde55824bcb/tools/FakeItEasy.Build/Program.cs) or [Qml.Net](https://github.com/qmlnet/qmlnet/blob/cebef7514f00a6c1273f263afc910edfda522d46/build/scripts/Program.cs) for some more complicated scenarios.

> It works great for me, I'm comfortable with it.

I'd argue that because of the preprocessor it burdens you with, unless it provides strategic value that you couldn't do with out, you shouldn't use it. Think about the others on your team that aren't comfortable with it. You'd be just as comfortable writing a regular C# project. Even more so, considering you'd have complete IDE and debugger support!

## Conclusion

Most of the things that people attribute to Cake are things they do *in* Cake, and not *because* of Cake. When all the smoke and mirrors are gone, Cake is just a target runner with a preprocessor. The "plugins" and "features" most people refer to are mirrages.

What feature of Cake is an *absolute must* for you? I'd really like to hear some examples that don't have an equally viable approach using pure .NET libs or shelling out to a command. I'd also wager that most CLI/lib alternatives would be more supported than what you'd get with a Cake plugin/module.

I know, I know, "to each his own". You are right. I can't argue with that if that is where the arguement stops.

What are your thoughts?
