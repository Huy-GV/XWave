# README.md

## About
- Online store web API
- Features:
    - View and purchase products as customer users
    - View transaction history and manage payment accounts as customer users
    - Manage products as staff and manager users
    - Manage staff activity as manager users
- Technologies used: C#, xUnit, .NET Testcontainer, Hangfire, ASP.NET Core, Docker, MS SQL Server

## Run Locally
### Pre-requisites
- Required installations:
    - [.NET 8.0 runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
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
- Set up passwords for seeded user account:
    ```bash
    cd /directory/containing/XWave.Web.csproj/
    dotnet user-secrets set "SeedData:Password" "YourTestPassword"
    ```
- Set up configurations for JWT authentication:
    ``` bash
    cd /directory/containing/XWave.Web.csproj/

    dotnet user-secrets set "Jwt:Issuer" "YourIssuerName"
    dotnet user-secrets set "Jwt:Key" "AlphanumericKeyAtLeast32CharactersInLength"
    dotnet user-secrets set "Jwt:Audience" "YourAudienceName"
    ```
- Set up database connection
    ``` bash
    cd /directory/containing/XWave.Web.csproj/

    # Optionally set custom database location
    # If this directory does not exist, it will automatically be created
    dotnet user-secrets set "ConnectionStrings:DefaultDbLocation" "\\Path\\To\\Database\\Directory\\DatabaseName.mdf"

    # Set up MS SQL server connection string
    # Example using a local server: "Server=(localdb)\\mssqllocaldb;Database=XWave;Trusted_Connection=True;MultipleActiveResultSets=false;"
    dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Your;Database;Connection;String;"
    ```
- Start the web server in `Release` mode:
    ```bash
    cd /directory/containing/XWave.Web.csproj/
    dotnet run --configuration Release
    ```

### Run Integration Tests
- Pre-requisites:
    - Ensure the secret `SeedData:Password` is set as it is used as the password for the test databases
    - Ensure the Docker engine is running and targeting Linux
- Run the tests from the terminal:
    ```bash
    cd /directory/containing/XWave.IntegrationTest.csproj/
    dotnet test
    ```

## Run Inside Docker Container:
- Start the Docker engine and ensure it is targeting *Linux*
- Generate a certificate and store it in `~/.aspnet/https` on the host machine
- Create an environment file named `docker.env` and specify the following fields:
    - `SeedData__Password`: equivalent to `SeedData:Password`
    - `Jwt__Key`: equivalent to `Jwt:Key`
    - `Jwt__Issuer`: equivalent to `Jwt:Issuer`
    - `Jwt__Audience`: equivalent to `Jwt:Audience`
    - `ConnectionStrings__DefaultConnection`: equivalent to `ConnectionStrings:DefaultConnection`
    - `SqlServer__Password`: password of MS SQL Server database
    - `ASPNETCORE_Kestrel__Certificates__Default__Password`: password of HTTPS certificate
    - `ASPNETCORE_Kestrel__Certificates__Default__Path`: path to certificate file
    - Example:
        ```env
        # docker.env
        SeedData__Password=TestPassword123@@
        Jwt__Key=A1B2C3D4E5F6G7H8I9J0K1L2M3N4O5P6
        Jwt__Issuer=YourIssuerName
        Jwt__Audience=YourAudienceName
        ConnectionStrings__DefaultConnection=Server=sqlserver;Database=XWave;User ID=SA;Password=YourDbPassword;MultipleActiveResultSets=false;

        # must be the same as password in connection string
        SqlServer__Password=YourDbPassword

        # ensure certificate password and name is correct
        ASPNETCORE_Kestrel__Certificates__Default__Password=xwave-cert
        ASPNETCORE_Kestrel__Certificates__Default__Path=/https/xwave-cert.pfx
        ```
- Run the below command in admin mode:
    ```bash
    cd /directory/containing/docker-compose.yaml/
    docker compose --env-file docker.env up --build
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
