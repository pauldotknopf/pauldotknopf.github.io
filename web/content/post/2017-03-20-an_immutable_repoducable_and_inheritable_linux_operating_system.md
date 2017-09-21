---
title: "An immutable, repoducable and inheritable Linux operating system."
date: 2017-09-20T14:50:41-04:00
draft: true
---

# My problem

I have multiple computers. Each system has a different OS (or version) and configuration. One has Ubuntu 14, the others have Ubuntu 16. Some have had their packages updated, some have been deglected. Some are using NVIDIA graphics, others onboard Intel graphics.

The context switching from machine to machine to do simple tasks is mind numbing.

Is Qt Creator installed? If so, what version of Qt? Why does my compilation fail at home, but not at work? Old gcc? The list goes on.

# What I want

## Inheritance

I would like to treat my operating system like inheritence in OOP. Here is my psuedo code to illustrate.

```c#
public abstract class BaseImage
{
    protected abstract void Install();

    protected void Run(string command)
    {
        // Run "command" in your operating system.
        // This command could be shell scripts, apt-get/pacman commands, etc.
    }
}

public class Arch64 : BaseImage
{
    protected override void Install()
    {
        Run("./install-arch-rootfs.sh");
    }
}
    
public class NVidia : Arch64
{
    protected override void Install()
    {
        Run("./install-nvidia-drivers.sh");
    }
}

public class Development : NVidia
{
    protected override void Install()
    {
        Run("./install-devtools.sh");
    }
}

public class Gaming : NVidia
{
    protected override void Install()
    {
        Run("./install-stream.sh");
    }
}
```
I'd imagine building these images with something like this.

```bash
> build --type Gaming --output gaming.rootfs
```

Notice the ```squashfs``` extension, which brings me to my next point.

## Temporary boots

When I boot images, I'd like all changes made to my operating system to be discarded after boot. This would allow me to test new packages with no fear that it will break my system.

Theoretically, Linux would mount ```gaming.squashfs``` at ```/``` on boot. The ```squashfs``` file system is readonly. To make ```/``` writable, we would have to overlay ```tmpfs``` on ```/```, which would make all changes in ```/``` happen in RAM, wiping itself clean on fresh boot.

This would have also have a positive side effect of *forcing* you to capture all your operating system changes via your build scripts.

# What exists now?

## NixOS

https://nixos.org/

NixOS is marketed as "The Purely Functional Linux Distribution".

The functional nature of the operating system gives you a 100% consistent build. The resulting build is entirely dependent on the input Nix expressions. However, this also requires you to create Nix expressions for literally everything. This level of control is amazing, but I'd much rather prefer traditional package managers and formats (pacman, apt-get, rpm, deb, etc). Not a total deal breaker for me, but this would make NixOS my last choice.

## OSTree

https://ostree.readthedocs.io/en/latest/

OSTree isn't really an operating system or distribution, but it's worth mentioning. It is more a way of *controlling* a distribution. It is used in many ways, but it's primary purpose is support atomic upgrades/rollbacks on embedded systems. On boot, it will read a local OSTree repository at a specific commit, and create symlinks at ```/```. You can reboot into different "branches" and "commits".

I bring this up because it might be a nice solution to allow me to quickly swap between operating systems.