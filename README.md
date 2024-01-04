# README.md

## About

- Online store web server
- Features:
    - View and Purchase Products
    - View transaction history and manage payment accounts
    - Manage products as staff users
    - Manage staff activity as admin users

## Build and Run
### Pre-requisites
- Required installations:
    - [.NET 6.0 runtime](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
    - [Docker Community](https://www.docker.com/get-started/)
    - [Microsoft SQL Server](https://www.microsoft.com/en-au/sql-server/sql-server-downloads)
- Install required Nuget packages:
    ``` bash
    cd /directory/containing/XWave.sln/
    dotnet restore
    ```

### Set Up Development Environment
- Initialize user secret storage:
    ```bash
    dotnet user-secrets init
    ```
- Set up configurations for JWT authentication:
    ``` bash
    cd /directory/containing/XWave.Web.csproj/

    dotnet user-secrets set "Jwt:Issuer" "YourIssuerName"
    dotnet user-secrets set "Jwt:Key" "VeryLongAlphanumericKey"
    dotnet user-secrets set "Jwt:Audience" "YourAudienceName"
    ```
- Set up database connection
    ``` bash
    cd /directory/containing/XWave.Web.csproj/

    # Optionally set custom database location
    # If this directory does not exist, it will automatically be created
    dotnet user-secrets set "ConnectionStrings:DefaultDbLocation" "\\Path\\To\\Database\\Directory\\DatabaseName.mdf"

    # Set up MS SQL server connection string
    # Example using a local server: "Server=(localdb)\\mssqllocaldb;Database=XWave;Trusted_Connection=True;MultipleActiveResultSets=false;
    dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Your;Database;Connection;String;"
    ```

### Run locally:
- Start the web server in `Release` mode:
    ```bash
    cd /directory/containing/XWave.Web.csproj/
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
