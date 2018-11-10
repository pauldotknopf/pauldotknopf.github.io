---
title: "Give Ubuntu/Darch a quick ride in a virtual machine."
date: 2018-11-09
comment_issue_id: 8
---

 I've been using [Darch](https://godarch.com) for a while now as my daily driver. I think it is a wonderful tool that many people would enjoy using if they would only give it a shot. I also understand that it is difficult to get an environment setup appropriately to evaluate Darch, considering it's integration with the bootloader/grub.

The goal of this post is to provide guidance to quickly setup a local VM (using your choice of a hypervisor) with a working Darch environment. I will then walk you through the process of building and booting into Ubuntu.

# Getting a bootable machine

## Option 1: Download pre-built VM images

* [boot.img.bz2](https://github.com/godarch/darch/releases/download/v0.26.4/boot.img.bz2) - A raw disk image.
* [boot.vdi](https://github.com/godarch/darch/releases/download/v0.26.4/boot.vdi) - A disk image for VirtualBox.
* [boot.vmdk](https://github.com/godarch/darch/releases/download/v0.26.4/boot.vmdk) - A disk image for VMWare.

## Option 2: Generate locally, from scratch (Linux-only)

Generating a raw non-EUFI msdos partitioned disk image file (named ```boot.img```) with Darch installed is very simple.

```bash
mkdir darch-vm
cd darch-vm
curl -s https://raw.githubusercontent.com/godarch/darch/develop/scripts/gen-bootable-image.sh | sudo bash /dev/stdin
```

The ```boot.img``` file can be ```dd```'d directly to a disk. Or, you can create a disk image that is suitable for importing into a hypervisor with the following commands.

```bash
# VirtualBox
qemu-img convert -O vdi boot.img boot.vdi
# VMWare
qemu-img convert -O vmdk boot.img boot.vmdk
```
<div class="alert alert-warning">
  Be sure to allocate at least 4GB of memory to your VM.
</div>

# Building your first image

In the initial grub entries, you will see Debian. This is your base OS. You won't boot into it most of the time. It could be any OS, it just needs Darch installed on it. Consider it your recovery OS in case of emergencies. It is where we will build our first Darch image.

Boot into the Debian install and login with the user "darch" and password "darch".

In your home directory is an ```example-recipes``` directory. Within this directory are some example recipes (go figure) to build and boot an Ubuntu image. Take a look at the commands in the ```./build``` script. These are the essential commands to build your Ubuntu image.

```bash
# Pull down our base ubuntu image.
sudo darch images pull godarch/ubuntu:cosmic

# Build our recipes.
sudo darch recipes build $(sudo darch recipes build-dep custom)

# Stage them for booting.
sudo darch stage upload custom --force
```

After running these commands, you can now restart your VM and boot into a fresh Ubuntu image.

## TADA!!

*Now what?*

Well, a few things. First, take note that in your Ubuntu image, anything you ```apt-get install``` is completely wiped on a fresh boot.

*WTF!?!*

Yeah, I know, but this ensures that you have a reproducible, stable and clean operating system at *all* times. You can play with any package/configuration without fear that you'll break anything permanently (just reboot if you do).

*Everything gets discarded though? What about my home directory?*

Darch supports hooks and comes with some out of the box for some common use cases. For example, the VM you are using has a fstab hook pre-configured for you that will auto mount a partition to your home directory. There are other hooks for custom ```/etc/hostname``` values for different images, restoring local ssh server fingerprints, etc.

*What if there is a package/configuration that I want to persist?*

This is where your personalized recipes come into play. For example, take a look at [my recipes](https://github.com/pauldotknopf/darch-recipes). In in my "development" image, I install things like git, KeyBase, Docker, etc.

Try it yourself.

# Customize your recipe

Inside of ```~/example-recipes```, there is a ```custom``` recipe. I placed this here specifically for you to play and experiment with. Modify ```./custom/script``` to perform any operation you'd like. Configure the timezone? Install a package? Enable DHCP? Whatever.

Also, instead of running ```./build``` again (which will work), let's build *only* your Ubuntu derived ```custom``` recipe so that we can avoid having to build Ubuntu again.

```
sudo darch recipes build custom
sudo darch stage upload custom --force
```

Reboot again to see your changes permanently baked into your image.

That is pretty much it!

Apart from hardware issues, I will never have to worry about a broken Linux install again. Clean, stable and reproducible!

Another cool thing worth mentioning is that you can configure your recipes to be auto-built and deployed to Docker Hub via Travis CI. This works great for me because I can commit/push any change I want, and I will eventually be able to pull it (```darch images pull my/image```) and boot it. I have multiple machines (and a laptop) that all pull off of the same Docker Hub feed. This means I have multiple machines running of the *same exact bits*.

Darch supports building images for Debian (stable and testing), Ubuntu, Arch and VoidLinux. Reach out if you'd like to see other distributions supported.

Visit [godarch.com](https://godarch.com/) to learn more!
