---
title: "Announcing apt-tool: an alternative to debootstrap and multistrap for deterministic and predictable root filesystems."
date: 2019-11-26
comment_issue_id: 13
---

[GitHub Project: AptTool](https://github.com/pauldotknopf/apt-tool)

# Overview

The ```apt-tool``` command is a tool to generate root filesystems from configuration files.

It differs from ```debootstrap``` and ```multistrap``` in many ways, but the key difference is that ```apt-tool``` behaves more like what you'd see in tools like ```npm``` and it's ```packages.json```/```packages-lock.json``` pair.

The idea is that once you generate ```apt-tools```'s ```image-lock.json``` file from it's ```image.json``` file, all consecutive builds of the root filesystem will be completely predictable.

# Why?

Think about Linux devices/appliances. Each update to your appliance is typically and entire root filesystem and configuration. At this point, your operating system isn't just a vehicle that *runs* your application, your operating system *is* your application.

With your applications (Python/C#/etc), you typically would have a build server that produces the same outputs from the same inputs. In other words, you need to be able to recompile older versions years later and have some confidence that the output is the same.

Since your operating system is now considered your *application*, you'd need the same predictability in your outputs. If you have a git repository that produces your root file system with ```debootstrap``` or ```multistrap```,  each build from the same commit (even days later) could have updated packages that aren't captured. I realize that you typically want package updates, but it's better that each update is captured via a git commit (updating the ```image-lock.json``` file) for identifying issues that may arise down the road via ```git-bisect``.`

Also, I found ```debootstrap``` and ```multistrap``` to be problematic. Their outputs are unpredictable, ```debootstrap``` doesn't support multi apt repositories, ```multistrap``` doesn't configure packages properly (errors during install), and they don't leverage ```dpkg``` as much as they should.

In the end, it just made sense for me to write my own tool. It also uses ```dpkg``` and ```apt``` as much as possible to ensure a bug-free experience.

# Overview

It uses .NET, so install using the following command.

```
sudo dotnet tool install AptTool --tool-path /usr/local/bin/
```

Check out a full example [here](https://github.com/pauldotknopf/apt-tool-example). NOTE: This example will also build a bootable .img file with the generate rootfs.

The idea is simple though.

**```image.json```**:

```json
{
    "packages": {
        "locales": "latest",
        "grub-efi-amd64": "latest",
        "linux-image-amd64": "latest",
        "network-manager": "latest"
    },
    "repositories": [
        {
            "uri": "http://deb.debian.org/debian",
            "distribution": "buster",
            "components": [
                "main",
                "contrib",
                "non-free"
            ]
        }
    ]
}
```

Then, you need to generate the ```image-lock.json```:

```
apt-tool install
```

**```image-lock.json```**:

```json
{
  "installedPackages": {
    "gcc-8-base": {
      "version": "8.3.0-6",
      "architecture": "amd64"
    },
    "libc6": {
      "version": "2.28-10",
      "architecture": "amd64"
    },
    "libgcc1": {
      "version": "1:8.3.0-6",
      "architecture": "amd64"
    },
    ...
  }
}
```

Now, we can generate a root file system.

```
sudo apt-tool generate-rootfs
```

A stage2 script is placed in ```/stage2/stage2.sh``` that needs to be run afterwards via chroot. This can be ran automatically by using the ```--run-stage2``` flag.