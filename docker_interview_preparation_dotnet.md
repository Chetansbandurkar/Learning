# Docker Interview Preparation for a .NET Backend Developer

Beginner-friendly notes with answers, explanations, practical commands, and examples based on the `AuthenticationServer` project.

> **Project context:** The API targets .NET 9. The image is `authentication-server:v2`. The container listens on port `8080`, exposed on Windows as port `8081`. SQL Server currently runs on the Windows host, outside the API container.

## Table of Contents

1. Docker fundamentals
2. Dockerfile instructions
3. Images, containers, and lifecycle
4. Networking and ports
5. ASP.NET Core configuration in Docker
6. Docker Compose, volumes, and databases
7. Debugging and troubleshooting
8. Image optimization and production practices
9. Common Docker commands
10. Scenario-based interview questions
11. Practice interview questions
12. Quick revision sheet

---

# 1. Docker fundamentals

## Q1. What is Docker, and why do we use it?

**Answer:** Docker is a platform for packaging and running applications in containers. A container includes the application and the files, libraries, and runtime components it needs.

**Explanation:** An application may run on one developer's machine but fail on another because of different runtimes, libraries, or configuration. Docker helps provide a consistent runtime environment across development, testing, and deployment.

**Example:** We packaged the ASP.NET Core `AuthenticationServer` API into a Docker image and ran it as a container.

**Interview-ready answer:**
> Docker packages an application and its dependencies into an image that can run as a container. It helps make application behavior more consistent across environments and simplifies deployment.

## Q2. What is the difference between a Docker image and a container?

**Answer:**
- **Image:** The packaged template used to create containers.
- **Container:** A running or stopped instance created from an image.

**Explanation:** One image can be used to create multiple containers. The image is treated as immutable during normal use; a container has its own writable layer for changes made while it runs.

**Example:**
```powershell
docker build -t authentication-server:v2 .
docker run -d --name authentication-server -p 8081:8080 authentication-server:v2
```
The first command builds an image. The second creates and starts a container from it.

## Q3. What is a Dockerfile?

**Answer:** A Dockerfile is a text file containing instructions that Docker uses to build an image.

Common instructions include:
- `FROM` — choose a base image.
- `WORKDIR` — set the working directory.
- `COPY` — copy files into the image.
- `RUN` — execute a command during image build.
- `EXPOSE` — document the port the application listens on.
- `ENTRYPOINT` — define the main process to start in a container.
- `CMD` — define a default command or arguments.

## Q4. What is the difference between a container and a virtual machine?

**Answer:** A virtual machine runs a guest operating system on virtualized hardware. Containers isolate application processes while sharing the host kernel through the container runtime.

**Explanation:** Containers are generally lighter and start faster because they do not each need a full guest operating system. Containers still provide isolation, but they are not identical to virtual machines and should not automatically be considered a security boundary equivalent to a separate VM.

## Q5. What are the benefits of Docker?

**Answer:**
- Consistent application environments.
- Repeatable builds and deployments.
- Easier scaling by running more container instances.
- Isolation between application processes and their filesystems.
- Convenient packaging of application dependencies.
- Better fit for automated build and deployment pipelines.

**Caveat:** Docker does not automatically make an application secure, highly available, or scalable. Those outcomes depend on configuration and the surrounding infrastructure.

## Q6. What is a base image?

**Answer:** A base image is the starting image specified by `FROM` in a Dockerfile.

**Example:**
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0
```
This uses Microsoft's .NET 9 SDK image, which includes tools needed to restore, build, and publish .NET applications.

---

# 2. Dockerfile instructions

## Q7. Explain the Dockerfile used for AuthenticationServer.

The learning Dockerfile is essentially:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0

WORKDIR /app

COPY . .

RUN dotnet restore

RUN dotnet publish -c Release -o /app/publish

WORKDIR /app/publish

EXPOSE 8080

ENTRYPOINT ["dotnet", "AuthenticationServer.dll"]
```

### Line-by-line explanation

**`FROM mcr.microsoft.com/dotnet/sdk:9.0`**

Starts from an image containing the .NET 9 SDK. This is convenient for learning because the SDK can restore and publish the application.

**`WORKDIR /app`**

Sets `/app` as the current working directory inside the image. This is not the Windows project directory.

**`COPY . .`**

Copies files from the Docker build context into the current working directory in the image. The build context is the directory supplied at the end of `docker build`, here `.`.

**`RUN dotnet restore`**

Restores the NuGet packages required by the project.

**`RUN dotnet publish -c Release -o /app/publish`**

Publishes the application in Release configuration to `/app/publish`.

**`WORKDIR /app/publish`**

Sets the directory from which the application will be started. This mattered because the published DLL was inside `/app/publish`; running the DLL from `/app` caused a “DLL does not exist” error.

**`EXPOSE 8080`**

Documents that the application expects to listen on port `8080` inside the container. It does not publish the port on the host by itself.

**`ENTRYPOINT ["dotnet", "AuthenticationServer.dll"]`**

Defines the main process started when the container runs.

## Q8. What is the difference between `RUN`, `CMD`, and `ENTRYPOINT`?

**`RUN`:** Executes during image build and creates a new image layer for the resulting filesystem changes.

```dockerfile
RUN dotnet restore
```

**`CMD`:** Supplies a default command or default arguments for container startup. It can be overridden when running the container.

**`ENTRYPOINT`:** Defines the main executable or process for the container.

```dockerfile
ENTRYPOINT ["dotnet", "AuthenticationServer.dll"]
```

**Interview tip:** `RUN` is for build-time work; `CMD` and `ENTRYPOINT` affect container startup.

## Q9. What is the difference between `COPY` and `ADD`?

**Answer:** `COPY` copies files and directories from the build context into the image. `ADD` supports additional behaviors, such as extracting local tar archives and fetching remote URLs in supported cases.

**Best practice:** Prefer `COPY` unless an `ADD` feature is specifically required. A separate download step can make remote-file handling clearer and easier to verify.

## Q10. What does `EXPOSE` do? Does it publish the port?

**Answer:** `EXPOSE` documents the port the application listens on inside the container. It does **not** publish that port on the host.

To publish it, use `-p`:

```powershell
docker run -d --name my-api -p 8081:8080 my-api:v1
```

The mapping is `HOST_PORT:CONTAINER_PORT`.

## Q11. What is the Docker build context?

**Answer:** The build context is the set of files Docker can access for instructions such as `COPY`. In this command:

```powershell
docker build -t authentication-server:v2 .
```

The final `.` means the current directory is the build context.

**Why it matters:** A large context slows builds and may accidentally include secrets or unnecessary files. Use a `.dockerignore` file to exclude items such as `bin/`, `obj/`, `.git/`, and local secret/configuration files that should not be copied.

Example `.dockerignore`:
```text
bin/
obj/
.git/
.vs/
*.user
```

Review exclusions against your build needs; do not exclude files required to build the app.

## Q12. What is a multi-stage Docker build?

**Answer:** A multi-stage build uses multiple `FROM` stages. One stage builds and publishes the application; a later, smaller stage contains only the published output and the runtime needed to execute it.

**Why use it?** It can reduce final image size and avoid shipping build tools in the runtime image.

Example for a typical .NET 9 web API:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["AuthenticationServer.csproj", "./"]
RUN dotnet restore "AuthenticationServer.csproj"

COPY . .
RUN dotnet publish "AuthenticationServer.csproj" \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "AuthenticationServer.dll"]
```

This example assumes the `.csproj` is in the build-context root. If the project has a different layout, adjust the paths. Configure ASP.NET Core to listen on port 8080 at runtime, as we did with `ASPNETCORE_HTTP_PORTS=8080`.

---

# 3. Images, containers, and lifecycle

## Q13. How do you build an image?

```powershell
docker build -t authentication-server:v2 .
```

- `docker build`: build an image.
- `-t`: assign a name and tag.
- `authentication-server:v2`: image name and tag.
- `.`: build context.

## Q14. How do you start a container?

```powershell
docker run -d --name authentication-server -p 8081:8080 authentication-server:v2
```

This creates a container from the image and starts it in detached mode.

## Q15. What is the difference between `docker run` and `docker start`?

**`docker run`** creates a new container from an image and starts it.

**`docker start`** starts an existing stopped container.

For example:
```powershell
docker stop authentication-server
docker start authentication-server
```

## Q16. What is the difference between `docker stop` and `docker kill`?

**`docker stop`** asks the main process to stop gracefully, allowing time for cleanup before Docker forcefully terminates it if necessary.

**`docker kill`** sends a signal to terminate the container's main process immediately by default.

Prefer `docker stop` for normal operations.

## Q17. What happens when you remove a container? Does the image get removed too?

**Answer:** Removing a container does not normally remove the image from which it was created. The image remains available to create another container.

```powershell
docker rm -f authentication-server
docker images
```

The `-f` option force-removes the container. It does not remove `authentication-server:v2`.

## Q18. Can multiple containers be created from one image?

Yes. An image can be used to create multiple containers. Each container has its own runtime state and writable layer. They need distinct names and any published host ports must not conflict.

## Q19. What is the difference between `docker ps` and `docker ps -a`?

- `docker ps` shows running containers.
- `docker ps -a` shows all containers, including stopped containers.

A container that exited after a crash can still appear in `docker ps -a`.

---

# 4. Networking and ports

## Q20. Explain port mapping using our project.

We ran the API with:

```powershell
-p 8081:8080
```

The format is:

```text
-p HOST_PORT:CONTAINER_PORT
```

- Host port `8081`: the port accessed from Windows.
- Container port `8080`: the port on which ASP.NET Core listens inside the container.

So we browse to:

```text
http://localhost:8081/swagger
```

Docker forwards the traffic to port `8080` in the container.

## Q21. Why did we use `host.docker.internal` instead of `localhost` for SQL Server?

**Answer:** `localhost` inside the API container points to the container itself. Our SQL Server runs on Windows, outside that container. On Docker Desktop, `host.docker.internal` is a hostname that lets the container reach the host machine.

Our connection string used:

```text
Server=host.docker.internal,1433;
Database=AuthenticationDb;
```

The comma before `1433` is SQL Server connection-string syntax for specifying the TCP port.

## Q22. What is a Docker network?

**Answer:** A Docker network connects containers and controls how they communicate. On a user-defined bridge network, containers can generally reach each other by container name or network alias.

**Example:** If an API service and a SQL Server service are in the same Compose network, the API can usually connect to `sqlserver,1433` when the database service is named `sqlserver` and listens on port 1433 inside its container.

## Q23. What is the difference between host port and container port?

**Answer:** The host port is exposed on the machine running Docker. The container port is where the application listens inside the container.

In `-p 8081:8080`, `8081` is the host port and `8080` is the container port.

## Q24. What is the bridge network?

**Answer:** A bridge network connects containers on the same Docker host. User-defined bridge networks provide automatic DNS resolution between attached containers and are a common choice for local multi-container applications.

## Q25. How would the API connect to SQL Server if SQL Server were also containerized?

**Answer:** Put both services on the same Docker Compose network and use the database service name as the hostname. For a service named `sqlserver`, the connection string commonly uses:

```text
Server=sqlserver,1433;
Database=AuthenticationDb;
```

The API should use the database container's internal port. Publishing SQL Server's port to Windows is optional and is only needed if a host application such as SSMS must connect through that published port.

---

# 5. ASP.NET Core configuration in Docker

## Q26. How do you pass configuration to an ASP.NET Core container?

**Answer:** You can use environment variables, mounted configuration files, and a secrets-management system. ASP.NET Core configuration maps double underscores (`__`) in environment-variable names to colons (`:`) in configuration keys.

Example:
```powershell
-e ASPNETCORE_ENVIRONMENT=Development
-e ASPNETCORE_HTTP_PORTS=8080
-e "ConnectionStrings__DefaultConnection=..."
```

The variable `ConnectionStrings__DefaultConnection` corresponds to the configuration key `ConnectionStrings:DefaultConnection`.

## Q27. Why did Swagger return 404 in our container?

**Answer:** The application enabled Swagger only inside this condition:

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

The container initially ran in Production mode, so Swagger middleware was not enabled. We set `ASPNETCORE_ENVIRONMENT=Development` for local learning and restarted the container.

**Important:** Do not switch production deployments to Development merely to make Swagger available. Configure API documentation access intentionally.

## Q28. What is the difference between appsettings.json and environment variables?

**Answer:** `appsettings.json` stores application configuration in a file. Environment variables provide configuration at runtime and can override JSON settings according to ASP.NET Core's configuration-provider precedence.

**Example:** The local JSON file may use a Windows trusted connection, while the container receives a SQL username/password connection string through an environment variable.

## Q29. Should you put passwords or JWT signing keys directly in a Dockerfile?

**Answer:** No. Do not bake credentials into Dockerfiles or image layers. Use environment-specific secret injection or a dedicated secrets manager. Environment variables are convenient for learning, but they may be visible to users with Docker access and should not be treated as a fully secure vault.

For local development, keep sensitive values out of source control. Rotate any credential that has been exposed.

## Q30. What does `ASPNETCORE_HTTP_PORTS=8080` do?

**Answer:** It configures ASP.NET Core's HTTP listening port to `8080` in supported modern .NET container setups. This must agree with the port mapping and the port your application actually listens on.

`EXPOSE 8080` alone does not make the app listen on that port.

---

# 6. Docker Compose, volumes, and databases

## Q31. What is Docker Compose?

**Answer:** Docker Compose is a tool for defining and managing multi-container applications through a YAML file. You can specify services, networks, volumes, ports, environment variables, and dependencies in one place.

Typical commands:
```powershell
docker compose up -d
docker compose ps
docker compose logs -f
docker compose down
```

## Q32. What is a Docker volume?

**Answer:** A volume is persistent storage managed by Docker. It can keep data outside a container's writable layer so the data can survive container replacement or removal.

Volumes are often used for database files, uploaded content, and other data that must persist.

## Q33. What is the difference between a volume and a bind mount?

**Volume:** Docker manages the storage location.

**Bind mount:** A specific file or directory on the host is mounted into the container.

Use volumes for many persistent application-data scenarios. Bind mounts are useful during development, for example when you want a container to see source files from your workstation.

## Q34. Does removing a container delete its data?

**Answer:** It depends on where the data is stored.

- Data written only to the container's writable layer is removed with the container.
- Data stored in a volume normally remains after container removal.
- Data stored on the Windows host outside the container is not deleted merely because the API container is removed.

In our current setup, SQL Server runs directly on Windows, so removing the API container does not itself remove the Windows database.

## Q35. Why use Docker Compose rather than a long `docker run` command?

**Answer:** Compose keeps the application configuration in a readable, reusable YAML file. It is easier to manage multiple services, networks, and volumes than to repeatedly type long commands.

**Security note:** Do not commit plaintext production secrets into a Compose file in source control.

## Q36. What does `docker compose down` do?

**Answer:** It stops and removes the containers and default network created by `docker compose up`. Named volumes are generally retained unless you explicitly request their removal with the appropriate options.

Be careful with volume-removal options, especially when databases contain important data.

## Q37. Does Compose `depends_on` guarantee the database is ready?

**Answer:** Not by itself. Basic dependency ordering can start the database container before the API container, but the database may not yet be ready to accept connections. A health check and a readiness-aware dependency configuration can help, and the application should still handle transient connection failures.

---

# 7. Debugging and troubleshooting

## Q38. How do you debug a container that exits immediately?

Use this sequence:

```powershell
docker ps -a
docker logs --tail 100 authentication-server
docker inspect authentication-server
```

**Explanation:**
1. `docker ps -a` shows whether the container exited.
2. `docker logs` displays the application's standard output and error.
3. `docker inspect` shows details such as exit status, environment, and configuration.

Then investigate the entry point, configuration, missing files, port conflicts, or dependency failures.

## Q39. How do you view application logs continuously?

```powershell
docker logs -f authentication-server
```

The `-f` option follows new log entries as they are written.

Press `Ctrl+C` to stop following the logs; this does not normally stop the container.

## Q40. How do you execute a command inside a running container?

```powershell
docker exec -it authentication-server sh
```

This starts a shell inside the running container if the image contains `sh`. Minimal images may not contain a shell, in which case use other debugging methods.

## Q41. Your API starts, but database requests fail. What do you check?

**Answer:**
1. Read the container logs.
2. Verify the effective connection-string configuration without exposing its password.
3. Confirm SQL Server is listening on the expected port.
4. Verify the hostname: use `host.docker.internal` when connecting from Docker Desktop to SQL Server on Windows.
5. Check firewall, TCP/IP, SQL authentication, login permissions, and database name.
6. Test a database-backed endpoint.

A successful application startup does not prove the database connection works; the connection may only be attempted when an endpoint accesses the database.

## Q42. Swagger returns 404, but the API process is running. Why?

**Answer:** The route may not be registered, Swagger may be disabled in the current environment, or the requested path may be wrong. Check the application pipeline, environment, and logs. In our project, Swagger was gated by `app.Environment.IsDevelopment()`.

## Q43. The host port is already in use. What do you do?

**Answer:** Check which process or container uses that port. Either stop the conflicting service or map a different host port.

For example:
```powershell
docker run -d --name authentication-server -p 8082:8080 authentication-server:v2
```

The API remains on port 8080 inside the container, but it is reached on Windows through port 8082.

## Q44. How do you monitor container resource usage?

```powershell
docker stats
```

It displays live resource metrics such as CPU and memory usage for running containers.

---

# 8. Image optimization and production practices

## Q45. How can you reduce Docker image size for a .NET application?

**Answer:**
- Use a multi-stage build.
- Use the SDK image for building and the ASP.NET Core runtime image for running.
- Copy only the published output into the final stage.
- Use `.dockerignore` to keep irrelevant files out of the build context.
- Avoid unnecessary tools and files in the final image.

## Q46. Why should containers run as non-root where possible?

**Answer:** Running as a non-root user reduces the permissions available to a compromised process. Use an appropriate non-root user supported by the chosen base image, and make sure the application has access to the directories it needs.

## Q47. What is a container health check?

**Answer:** A health check periodically tests whether an application or service is healthy. It can help orchestration tools distinguish a running process from a service that is ready to handle requests.

A process can be running while its database connection or other dependency is unavailable. Design health checks to reflect the readiness requirements of the service.

## Q48. What is Docker layer caching?

**Answer:** Docker builds images in layers. When a build instruction and its relevant inputs have not changed, Docker may reuse a cached result, reducing build time.

**Optimization example:** Copy the project file and restore packages before copying the rest of the source. Changes to application code then may not invalidate the NuGet restore layer.

## Q49. Is a Docker container a complete security boundary?

**Answer:** No. Containers provide process and filesystem isolation, but they share the host kernel. Security depends on container configuration, image quality, privileges, runtime settings, host security, and patching.

## Q50. What is the difference between development and production containers?

**Answer:** Development containers prioritize iteration, debugging, and convenient tooling. Production containers should be deliberately configured for security, reliability, observability, and efficient runtime size.

For our learning API, we enabled the Development environment to access Swagger. A production deployment should use production configuration, secure secrets, appropriate HTTPS settings, and a deliberate policy for API documentation.

---

# 9. Common Docker commands

Run these from PowerShell. Replace names and tags when working with other projects.

| Task | Command |
|---|---|
| Verify Docker | `docker version` |
| Build image | `docker build -t authentication-server:v2 .` |
| List images | `docker images` |
| Run container | `docker run -d --name authentication-server -p 8081:8080 authentication-server:v2` |
| Show running containers | `docker ps` |
| Show all containers | `docker ps -a` |
| View logs | `docker logs authentication-server` |
| Follow logs | `docker logs -f authentication-server` |
| Stop container | `docker stop authentication-server` |
| Start existing container | `docker start authentication-server` |
| Remove container | `docker rm authentication-server` |
| Force-remove container | `docker rm -f authentication-server` |
| Inspect container | `docker inspect authentication-server` |
| Run shell inside container | `docker exec -it authentication-server sh` |
| Monitor resources | `docker stats` |
| Build using Compose | `docker compose build` |
| Start Compose services | `docker compose up -d` |
| Follow Compose logs | `docker compose logs -f` |
| Stop/remove Compose services | `docker compose down` |

**Remember:** `docker rm` removes a container, not its image. `docker rmi` removes an image, subject to Docker's usage constraints.

---

# 10. Scenario-based interview questions

## Q51. “It works on my machine” — how does Docker help?

**Answer:** Docker provides a repeatable image with a defined runtime and dependencies. The same image can be promoted through development, testing, and deployment environments, while environment-specific configuration is supplied separately.

## Q52. Your container works locally but fails on another machine. What might be wrong?

Possible causes include:
- The image was not transferred or pulled correctly.
- A required environment variable is missing.
- The target port is already in use.
- The app depends on a host resource that is not available there.
- Database credentials, networking, or firewall rules differ.
- The image architecture or runtime assumptions differ.

Use logs and container inspection to narrow down the problem.

## Q53. Your .NET API cannot connect to SQL Server. How do you investigate?

**Answer:** Check the hostname from the container's point of view, SQL Server's TCP listener, port reachability, credentials, database permissions, and the actual connection string. For SQL Server on the Windows host with Docker Desktop, `host.docker.internal` is often the correct host name.

## Q54. Your image is unexpectedly large. What do you do?

**Answer:** Inspect the image and its layers, add a `.dockerignore`, use a multi-stage build, and copy only the published output into the runtime stage. Verify that the resulting image still contains the files and runtime libraries the app needs.

## Q55. The API container was removed. Is the database lost?

**Answer:** Not in our current setup, because SQL Server is running directly on Windows, outside the API container. In a containerized database setup, persistence depends on how the database files are stored, especially whether a persistent volume is configured.

## Q56. How would you deploy the API and database as a local multi-container app?

**Answer:** Define an API service and a SQL Server service in Compose. Put them on a shared network, provide the database credentials securely, configure the API connection string using the database service name, persist the database files in a volume, and add appropriate health checks and readiness handling.

---

# 11. Practice interview questions

Try answering these without looking at the earlier sections.

1. Explain Docker in 30 seconds.
2. Explain the difference between an image and a container.
3. Walk through your AuthenticationServer Dockerfile.
4. What does `docker build -t authentication-server:v2 .` do?
5. Explain `-p 8081:8080`.
6. Why did we use `host.docker.internal`?
7. Why was Swagger returning 404?
8. How would you debug a container that exits?
9. What is the difference between a volume and a bind mount?
10. How would you containerize SQL Server and connect the API to it?
11. How would you make the Docker image smaller?
12. How should secrets be passed into a container?
13. Why doesn't `EXPOSE 8080` publish a host port?
14. What is the difference between `docker run` and `docker start`?
15. Does `depends_on` guarantee a database is ready?

**Suggested preparation order:** First learn questions 1–12, then practise questions 13–30 using your own project, and finally study Compose, volumes, debugging, and production practices.

---

# 12. Quick revision sheet

| Concept | One-line reminder |
|---|---|
| Dockerfile | Instructions to build an image |
| Image | Packaged template |
| Container | Instance created from an image |
| `FROM` | Choose the base image |
| `WORKDIR` | Set working directory |
| `COPY` | Copy files into the image |
| `RUN` | Execute a build-time command |
| `ENTRYPOINT` | Main process for container startup |
| `CMD` | Default command or arguments |
| `EXPOSE` | Document the container port |
| `-p` | Publish/map a host port to a container port |
| Volume | Persistent Docker-managed storage |
| Bind mount | Mount a host path into a container |
| Compose | Define and manage multiple services |
| `docker logs` | Inspect application output |
| `docker inspect` | Inspect container configuration and state |
| `docker exec` | Execute a command in a running container |
| `host.docker.internal` | Reach the Windows host from Docker Desktop |
| Multi-stage build | Separate build tools from the final runtime image |
| Health check | Test service health/readiness |
| `.dockerignore` | Exclude files from the build context |

## Final interview tip

Do not just memorize the definitions. Be prepared to explain **why each command was needed in your own AuthenticationServer project**. Your port mapping, SQL Server connectivity, Swagger environment issue, and Dockerfile working-directory fix are practical examples you can confidently discuss.
