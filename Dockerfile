# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Kopiowanie plików projektu i przywrócenie zależności
COPY *.csproj ./
RUN dotnet restore

# Kopiowanie całego kodu i publikacja
COPY . ./
RUN dotnet publish -c Release -o out

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Kopiowanie gotowej aplikacji
COPY --from=build /app/out .

# Ustawienie portu, który będzie dostępny na zewnątrz
EXPOSE 8080

# Komenda uruchamiająca aplikację
ENTRYPOINT ["dotnet", "CodiblyTask.Server.dll"]
