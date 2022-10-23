FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app
EXPOSE 5000

# Copy everything
COPY ./XWave.Core ./XWave.Core
COPY ./XWave.Web ./XWave.Web

# Restore as distinct layers
RUN dotnet restore ./XWave.Core/XWave.Core.csproj
RUN dotnet restore ./XWave.Web/XWave.Web.csproj
# Build and publish a release
RUN dotnet publish ./XWave.Web/XWave.Web.csproj -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "XWave.Web.dll"]