---
title: "An immutable, reproducible and inheritable Linux operating system."
date: 2017-09-20T14:50:41-04:00
draft: true
---

***Be warned**! This is just "thought-expirement" post. Nothing has been tried or tested.*

# My problem

I have multiple computers. Each system has a different OS (or version) and configuration. One has Ubuntu 14, the others have Ubuntu 16. Some have had their packages updated, some have been neglected. Some are using NVIDIA graphics, others onboard Intel graphics.

The context switching from machine-to-machine for simple tasks is mind numbing.

Is Qt Creator installed? If so, what version of Qt? Why does my compilation fail at home, but not at work? Old gcc? The list goes on.

# What I want

## Inheritance

I would like to treat my operating system like inheritance in OOP. Here is my pseudo code to illustrate.

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

Theoretically, Linux would mount ```gaming.squashfs``` at ```/``` on boot. The ```squashfs``` file system is read-only. To make ```/``` writable, we would have to overlay ```tmpfs``` on ```/```, which would make all changes in ```/``` happen in RAM, wiping itself clean on fresh boot.

This would have also have a positive side effect of *forcing* you to capture all your operating system changes via your build scripts.

# Proposal

## Operating system

I'm a fan of Arch Linux. It gives you a lot of control and bleeding edge packages (via AUR). I haven't jumped aboard though because of the level of maintenance that has to be done, especially if your ```pacman -Syu``` breaks your install. Bleeding edge can bleed a little.

## Building and inheritance

So, how do I build a an Arch rootfs while also allowing me to inherit "layers" from each other?

Docker.

```dockerfile
# my-base - Packages that I want present on all systems.
FROM base/archlinux
# Update everything
RUN pacman -Syu
# Enable the AUR
RUN pacman -S yaourt
# Use KDE
RUN pacman -S plasma-meta
# Install Google Chrome
RUN pacman -S google-chrome
```

```dockerfile
# my-base-nvidia - My base image with NVidia layered on top.
FROM my-base
# Install NVidia drivers
RUN pacman -S nvidia-dkms 
```

```dockerfile
# my-development - My development machine that has a NVidia graphics card.
FROM my-base-nvidia
# Install dev tools
RUN pacman -S base-devel
```

```dockerfile
# my-gaming - My gaming machine, which also has a NVidia graphics card.
FROM my-base-nvidia
RUN pacman -S steam
```

> What? How do you expect to boot these docker images on bare-metal?

Well, yeah. You can't. However, Arch has support for preparing rootfs file systems on any Linux machine, provided you have the ```arch-chroot``` bash script is installed. This can be done in Docker.

So, I imagine creating a base ```arch-rootfs``` Docker image that contains a clean rootfs and ```arch-chroot``` available for customization.

```dockerfile
# arch-rootfs
FROM base/archlinux
# Install arch-chroot
RUN arch-install-scripts
# Create our rootfs directory
RUN mkdir /rootfs
# Populate our rootfs with a base Arch install
RUN cd /rootfs &&
    curl -O https://arch.com/root.tar.gz &&
    tar xzf root.tar.gz
```

Now we will have to add some additional commands to our layers.

```dockerfile
FROM arch-rootfs
# Update everything
RUN arch-chroot /rootfs pacman -Syu
# Enable the AUR
RUN arch-chroot /rootfs pacman -S yaourt
# Use KDE
RUN arch-chroot /rootfs pacman -S plasma-meta
# Install Google Chrome
RUN arch-chroot /rootfs pacman -S google-chrome
```

It isn't exactly clean, but it will work.

> How will you extract the /rootfs for booting?

```
docker save my-gaming > my-gaming.tar
```

Within this tarball is a ```/rootfs``` directory containing our image. We will want to eventually convert this to a ```squashfs``` file.

To use boot these ```squashfs``` images, we would have to install Arch the normal way. Then customize the initcpio with some [runtime hooks](https://wiki.archlinux.org/index.php/mkinitcpio#Runtime_hooks) that will mount ```squashfs``` images, overlay ```tempfs``` on ```/```, ```chroot``` into the new root, and call ```init```.

## Image storage

The docker files that build my layers will be checked into a GitHub repository. Travis-CI will monitor checkins, rebuild and deploy my images/layers to Docker Hub. I will use Docker Hub to make my builds available easily to all my machines.

I can also imagine many people using this approach, allowing people to quickly boot into stranger's operating systems to play around and expirement. For example:

```bash
# Checkout and build some guy named Steve's gaming build.
build steve/gaming
# And reboot into it!
boot steve/gaming
reboot now
```

Now you are playing around in someone else's configuration! Fun!

Let's get back to our build.

```bash
boot paul/gaming
reboot now
```

# In summary

* Reproducible builds of Arch Linux, using Docker.
* Inheritance of Arch layers, using Docker.
* Conversion of Docker images to ```squashfs``` files for read-only booting.
* Mounting ```tempfs``` on ```/``` for writes that persist only during the current boot.
* Storing images on Docker Hub for using in other machines, or by complete strangers!

What do you think?