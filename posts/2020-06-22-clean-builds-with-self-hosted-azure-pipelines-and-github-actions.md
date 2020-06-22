---
title: "Clean builds with self-hosted Azure Pipelines and GitHub Actions"
date: 2020-06-22
comment_issue_id: 15
---

***NOTE:** I will reference Azure Pipelines going forward, but this applies to GitHub Actions as well, unless specifically stated otherwise.*

# The problem

Running builds on Microsoft-hosted Azure Pipelines is very reproducible because every build get's a pristine environment in a disposable VM.

When creating a self-hosted VM, there is no guidance on having this behavior preserved. The docs instead guide you through installing the agent on an existing/mutable operating system.

# The solution

What we need to do is something like this:

1. Create a disposable environment.
2. Start the agent in the environment.
3. Process exactly one job.
4. Stop the agent.
5. Dispose of the environment.
6. Loop back to step 1

# Disposable environments.

There are multiple methods to create disposable environments.

- Docker
  - Pros:
    - Simple to create image.
    - Quick environment create/destroy.
  - Cons:
    - Docker-in-Docker is tough.
      - You can mount the Docker socket into your container, but you introduce shared state between builds.
      - Mounting paths are problematic when using Docker-in-Docker.
- Packer/Vagrant
  - Pros:
    - Better isolation of builds.
    - Using Docker in your builds is idiomatic.
  - Cons:
    - A new tool to learn (at least in my case), Packer.
    - Microsoft has shared their scripts for provisioning their Microsoft-hosted agents, but work is needed to support anything that isn't run in Azure (qemu, VMWare, etc).

Docker was suitable for my case, so that is what I'll focus on.

# The implementation

When running the self-hosted agent locally, there is a hidden gem available, the ```--once``` flag.

```
./run.sh --once
```

This flag will configure the agent to listen for exactly one job, process it, and shutdown immediately.

So now, let's get our agent configured to run in a Docker container. Luckily, the Microsoft docs have this covered [here](https://docs.microsoft.com/en-us/azure/devops/pipelines/agents/docker?view=azure-devops#linux). BUT, there are some slight modifications we must make to allow us to utilize the ```--once``` flag.

```diff
diff --git a/Dockerfile b/Dockerfile
index 3d0c684..f1195da 100644
--- a/Dockerfile
+++ b/Dockerfile
@@ -22,4 +22,4 @@ WORKDIR /azp
 COPY ./start.sh .
 RUN chmod +x start.sh
 
-CMD ["./start.sh"]
\ No newline at end of file
+ENTRYPOINT ["./start.sh"]
\ No newline at end of file
diff --git a/start.sh b/start.sh
index ac679c5..96f4ac5 100644
--- a/start.sh
+++ b/start.sh
@@ -92,4 +92,4 @@ print_header "4. Running Azure Pipelines agent..."
 
 # `exec` the node runtime so it's aware of TERM and INT signals
 # AgentService.js understands how to handle agent self-update and restart
-exec ./externals/node/bin/node ./bin/AgentService.js interactive
\ No newline at end of file
+exec ./externals/node/bin/node ./bin/AgentService.js interactive $*
\ No newline at end of file

```

When you have successfully files mentioned above setup locally and patched, let's build our image.

```bash
docker build . --tag dockeragent
```

Now we can startup the image to process exactly one build.

```bash
echo "your-api-token" > /etc/api-token
docker run --name dockeragent \
  --rm \
  -v /etc/api-token:/api-token \
  -e AZP_URL="https://dev.azure.com/your-company" \
  -e AZP_POOL="Pipelines" \
  -e AZP_AGENT_NAME="dockeragent" \
  -e AZP_TOKEN_FILE="/api-token" \
  dockeragent:latest --once
```

Ok, great, now how do I configure this to run in a loop?

Enter, ```systemd```.

**```/etc/default/dockeragent.service```**

```ini
AZP_URL=https://dev.azure.com/your-company
AZP_POOL=Pipelines
AZP_TOKEN_FILE=/etc/api-token

```

**```/lib/systemd/system/dockeragent.service```**

```ini
[Unit]
Description=Microsoft Azure docker agent
Requires=docker.service
After=docker.service

[Service]
EnvironmentFile=-/etc/default/%n
ExecStart=/usr/bin/docker run --name %n -e AZP_URL=${AZP_URL} -e AZP_POOL=${AZP_POOL} -e AZP_AGENT_NAME=%n -e AZP_TOKEN_FILE=${AZP_TOKEN_FILE} dockeragent:latest --once
ExecStop=/usr/bin/docker stop %n
ExecStopPost=-/usr/bin/docker rm -f %n
Restart=always

[Install]
WantedBy=default.target
```

Once these files are in place, enable the ```systemd``` unit.

```bash
sudo systemctl daemon-reload
sudo systemctl start dockeragent
```

Now your builds will start on boot and run over and over in a pristine/clean environment!

This process is similar to GitHub Actions because their runner is a fork of the Azure runner. They support running in a Docker agent with the ```--once``` flag.
