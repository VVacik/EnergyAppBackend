# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy files and dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy code and publish
COPY . ./
RUN dotnet publish -c Release -o out

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy App
COPY --from=build /app/out .

# External port
EXPOSE 8080

# Command for starting
ENTRYPOINT ["dotnet", "CodiblyTask.Server.dll"]
