---
title: "Introducing Darch, a tool for building immutable, reproducable, and stateless bootable Linux images. Think Docker, but for bare-metal."
date: 2018-02-01T11:36:35-05:00
comment_issue_id: 2
---

I have [previously blogged](2017-03-20-an_immutable_reproducible_and_inheritable_linux_operating_system.md) about my desire to have a "docker-like" environment for build images that I can boot bare-metal.

With that said, I created [Darch](https://godarch.com/).

# What it is

Darch is an application (written in golang) that makes building and booting rootfs images simple. It will generate ```rootfs.squash``` files and update ```grub.cfg``` for booting them. Even though Darch supports Arch only (Gentoo/VoidLinux soon), you can build and boot the images from any operating system that has grub installed.

During boot, a set of hooks are ran (see [here](https://github.com/godarch/darch/tree/develop/scripts/hooks)) in initcpio that will prepare things like ```/etc/fstab``` and ```/etc/hostname```.

See [here](https://github.com/pauldotknopf/darch-recipes) for my recipes. It is setup with Travis-CI to auto-deploy to Docker Hub, where my bootable images are deployed to. Darch supports pulling from Docker Hub to boot images locally.