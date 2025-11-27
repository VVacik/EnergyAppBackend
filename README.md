# EnergyAppBackend

Backend for calculating daily energy mix and optimal energy usage window.  
Built with **.NET 8** and deployed using **Docker + Render**.

---

##  **Technologies**

- **.NET 8 (ASP.NET Core Web API)**
- **C#**
- **HttpClient**
- **Docker**
- **Render.com (hosting)**
- **xUnit (unit tests)**

---

##  **Local Setup**

1. Navigate to the **backend folder**:
   cd CodiblyTask.Server
   dotnet restore
   dotnet run
Default URL: http://localhost:5108/energy

## Docker Setup
  Build the Docker image:
    docker build -t energy-app-backend .
  Run the container:
    docker run -p 5108:8080 energy-app-backend


## Project Structure
  CodiblyTask.Server/
   ├── Controllers/
   ├── Clients/
   ├── Services/
   ├── Models/
   ├── Program.cs
   ├── Dockerfile

   
## API Endpoints

GET /energy/mix – Returns daily energy mix

GET /energy/optimal?hours=2 – Returns optimal energy usage window


## Author

Procejt created for recruitment process


 
