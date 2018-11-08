---
listed: false
title: "Give Ubuntu/Darch a quick ride in a virtual machine."
date: 2018-11-07
comment_issue_id: 8
---

 I've been using [Darch](https://godarch.com) for a while now as my daily driver. I think it is a wonderful tool that many people would enjoy using if they would only give it a shot. I also understand that it is difficult to get an environment setup appropriatly to evaluate Darch, considering it's integration with the bootloader/grub.

The goal of this post is to provide the guidance to quickly setup a local VM (using your choise of a hypervisor) to setup a working Darch environment. I will then walk you through the process of building and booting into Ubuntu.

The generation of a raw non-EUFI msdos partitioned disk image file (named ```boot.img```) with Darch installed is very simple.

```bash
mkdir darch-vm
cd darch-vm
curl -s https://raw.githubusercontent.com/godarch/darch/develop/scripts/gen-bootable-image.sh | sudo bash /dev/stdin
```

The ```boot.img``` file can be ```dd```'d directly to a disk. Or, you can create a disk image suitable for importing into a hypervisor with the following commands.

```bash
# VirtualBox
qemu-img convert -O vdi boot.img boot.vdi
# VMWare
qemu-img convert -O vmdk boot.img boot.vmdk
```
