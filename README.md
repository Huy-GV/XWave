# README.md

## About

- Online store web server
- Features:
    - View and Purchase Products
    - View transaction history and manage payment accounts
    - Manage products as staff users
    - Manage staff activity as admin users

## Pre-requisites

- Required installations:
    - [.NET 6.0 runtime](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
    - [Docker Community](https://www.docker.com/get-started/)
    - [Microsoft SQL Server](https://www.microsoft.com/en-au/sql-server/sql-server-downloads)
    - Microsoft Visual Studio 2022 or VS Code
- Packages:
    - ASP.NET Core
    - Entity Framework Core
    - Unit testing: Moq, FsCheck, FluentAssertions

## Build and Run
### Run locally:
- Install required Nuget packages:
    ```bash
    dotnet restore
    ```
- Start the web server in `Release` mode:
    ```bash
    cd /directory/containing/XWave.Web.csproj
    dotnet run --configuration Release
    ```

### Run via Docker:
- Start the Docker engine and ensure it is targeting Linux
- Generate a certificate and store it in `~/.aspnet/https` on the host machine, modify `docker-compose.yaml` to use the correct certificate name, password and host directory.
- Run the below command in admin mode:
    ```bash
    cd /directory/containing/docker-compose.yaml/
    docker compose up
    ```

### Usage
- Sending request to retrieve available products: open `https://localhost:5000/api/product`
- To view full Swagger API documentation:
    - Start the server in `Debug` mode:
        ```bash
        cd /directory/containing/XWave.Web.csproj
        dotnet run --configuration Debug
        ```
    - Open `https://localhost:5001/index.html`
