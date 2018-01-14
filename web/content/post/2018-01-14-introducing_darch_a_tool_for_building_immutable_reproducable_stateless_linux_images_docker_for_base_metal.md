---
title: "Introducing Darch, a tool for building immutable, reproducable, and stateless bootable Linux images. Think docker, but for bare-metal."
date: 2018-01-14T11:36:35-05:00
---

[GitHub Project](https://github.com/pauldotknopf/darch)

I have [previously blogged]({{< ref "2017-03-20-an_immutable_reproducible_and_inheritable_linux_operating_system.md" >}}) about my desire to have a "docker-like" environment for build images that I can boot bare-metal.

With that said, I created [Darch](https://github.com/pauldotknopf/darch).

# What it is

Darch is an application (written in golang) that makes building and booting rootfs images simple. It will generate ```rootfs.squash``` files and update ```grub.cfg``` for booting them. Even though Darch supports Arch only (Gentoo/VoidLinux soon), you can build and boot the images from any operating system that has grub installed.

During boot, a set of hooks are ran (see [here](https://github.com/pauldotknopf/darch/tree/develop/scripts/hooks)) in initcpio that will prepare things like ```/etc/fstab``` and ```/etc/hostname```.

See [here](https://github.com/pauldotknopf/darch-images) for my image repository. It is setup with Travis-CI to auto-deploy to DockerHub, where my bootable images are deployed to. Darch supports pulling from DockerHub to boot images locally.

# Steps to get started

First, [install Darch](https://github.com/pauldotknopf/darch#install), then...

```
# Build the images. This generates local Docker images.
darch build $(darch build-dep)
# Extract Docker image to local file system.
darch stage upload plasma
# This effectively calls grub-mkconfig.
darch stage sync-boot-loader
```

Then, reboot into grub, and you will see a new menu entry for your Darch image.

# Hooks

Images need to have certain machine-specific configurations. Out of the box, there are hooks for ```/etc/fstab``` and ```/etc/hostname```. When images are uploaded to the stage, the hooks are ran against it (```darch stage run-hooks``` to re-run them).

## fstab

```bash
darch hooks help fstab
```

1. Create a ```/var/darch/hooks/fstab.config``` file.
2. The file is a list of ```image-name=value``` entries, where ```image-name``` is a globbing pattern matching "image-name:tag", and ```value``` is the name of the file that will be placed into the booted image. Add a catch-all at the end of the file (```*=default_fstab```) to use the same ```/etc/fstab``` for all images.
3. Re-run the hooks after making changes with ```darch stage run-hooks```.

## hostname

```bash
darch hooks help hostname
```

1. Create a ```/var/darch/hooks/hostname.config``` file.
2. The file is a list of ```image-name=value```, similar to the fstab hook above, but where ```value``` represents the value that will be put into ```/etc/hostname```.
3. Re-run the hooks after making changes with ```darch stage run-hooks```.

# Documentation

At the moment, you should refer to the ```darch``` command for documentation. A wiki will eventually be created.

# Going forward

I am really happy with this solution. With future support for different distributions, users can easily tinker with Gentoo, Void Linux, and even Ubuntu.

I really like the approach for using Docker. It's inheritance made it really easy to isolate and speed up the build process. However, I don't like that the images are mixed together with traditional Docker images, and a daemon is required to be running to perform the build.

In the future, I will be using the internals of the Docker daemon directly inside of Darch. 

* Layer store
* Images (tagging/pushing/pulling)

The layers/images/mounts will be stored in ```/var/lib/darch```, it's own directory. A running daemon will not be required, since Darch doesn't actually run processes/containers. The built layers/images will be compatible with any Docker registry, including DockerHub.