# 🛠️ Maintenance Request System

A comprehensive maintenance request management system built with **ASP.NET Core Web API**, **Entity Framework Core**, and **SQL Server**.

The system manages the complete lifecycle of maintenance requests, from creation and assignment to resolution and tracking, with role-based access control for Employees, Technicians, and Administrators.

---

## 🚀 Technologies & Tools

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet\&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-10.0-512BD4?logo=dotnet\&logoColor=white)
![EF Core](https://img.shields.io/badge/EF%20Core-10.0-512BD4?logo=dotnet\&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL%20Server-Database-CC2927?logo=microsoftsqlserver\&logoColor=white)
![JWT](https://img.shields.io/badge/JWT-Authentication-000000?logo=jsonwebtokens\&logoColor=white)
![Swagger](https://img.shields.io/badge/Swagger-API%20Documentation-85EA2D?logo=swagger\&logoColor=black)

---

## 📌 Overview

**Maintenance Request System** is a role-based web API designed to simplify and organize maintenance operations within an organization.

The system provides different permissions and features based on the user's role.

### 👨‍💼 Employee

Employees can:

* Create new maintenance requests.
* Edit their requests.
* Track request status.
* View request history.
* Add comments.
* Follow the progress of their requests.

### 🔧 Technician

Technicians can:

* View assigned maintenance tasks.
* View request details.
* Update request status.
* Add technical notes.
* Add comments.
* Track assigned tasks.

### 👨‍💻 Admin

Administrators can:

* Manage users.
* Manage maintenance categories.
* Assign technicians to requests.
* Manage request statuses.
* Monitor the overall maintenance process.
* View analytical dashboards and statistics.
* Track system activities and request history.

---

## ✨ Key Features

### 🔐 Authentication & Authorization

* ASP.NET Core Identity.
* JWT-based authentication.
* Role-based authorization.
* Secure login and registration.
* Protected API endpoints.

### 🎫 Request Management

* Create and manage maintenance requests.
* Automatically generate unique request numbers.
* Example:

```text
REQ-20260926-XXXX
```

* Track the complete request lifecycle.
* Assign requests to technicians.
* Update request status.
* View request details and history.

### 📜 Audit Trail & History

The system keeps track of important operations and changes made to maintenance requests.

This allows administrators and users to follow the complete history of a request.

### 💬 Comments System

Each maintenance request can have an interactive comment section where authorized users can:

* Add comments.
* View previous comments.
* Communicate about the request.
* Add technical notes when necessary.

### 🛡️ Security & Reliability

The system includes:

* JWT Authentication.
* Role-Based Authorization.
* Rate Limiting.
* Global Exception Handling.
* Validation of incoming requests.
* Protected API endpoints.

---

## 🏗️ Architecture

The project follows a clean and organized layered structure:

```text
MaintenanceRequestSystem/
│
├── Controllers/
│   ├── AuthController.cs
│   ├── RequestsController.cs
│   ├── CategoriesController.cs
│   ├── UsersController.cs
│   └── DashboardController.cs
│
├── DTOs/
│   ├── Auth/
│   ├── Requests/
│   ├── Categories/
│   ├── Users/
│   └── Dashboard/
│
├── Data/
│   ├── ApplicationDbContext.cs
│   └── SeedData.cs
│
├── Models/
│   ├── User.cs
│   ├── MaintenanceRequest.cs
│   ├── Category.cs
│   ├── Comment.cs
│   └── RequestHistory.cs
│
├── Services/
│   ├── AuthService.cs
│   ├── RequestService.cs
│   ├── UserService.cs
│   ├── CategoryService.cs
│   └── DashboardService.cs
│
├── Middleware/
│   ├── GlobalExceptionMiddleware.cs
│   └── ...
│
├── Extensions/
│   ├── ServiceExtensions.cs
│   └── ...
│
├── Helpers/
│   └── ...
│
├── Migrations/
│
├── appsettings.json
├── Program.cs
└── MaintenanceRequestSystem.csproj
```

---

## 👥 Default Seed Accounts

The application includes default accounts for testing.

| Role           | Email                   | Password    |
| -------------- | ----------------------- | ----------- |
| 👨‍💻 Admin    | `admin@maintenance.com` | `Admin@123` |
| 🔧 Technician  | `tech@maintenance.com`  | `Tech@123`  |
| 👨‍💼 Employee | `emp@maintenance.com`   | `Emp@123`   |

> ⚠️ These credentials are intended for development and testing purposes only. Change them before using the system in a production environment.

---

# 📡 API Endpoints

## 🔐 Authentication

| Method | Endpoint             | Description                  |
| ------ | -------------------- | ---------------------------- |
| `POST` | `/api/auth/register` | Register a new user          |
| `POST` | `/api/auth/login`    | Login and generate JWT       |
| `GET`  | `/api/auth/me`       | Get current user information |

---

## 📝 Maintenance Requests

| Method   | Endpoint                      | Description              |
| -------- | ----------------------------- | ------------------------ |
| `GET`    | `/api/requests`               | Get maintenance requests |
| `GET`    | `/api/requests/{id}`          | Get request by ID        |
| `POST`   | `/api/requests`               | Create a new request     |
| `PUT`    | `/api/requests/{id}`          | Update a request         |
| `DELETE` | `/api/requests/{id}`          | Delete a request         |
| `PUT`    | `/api/requests/{id}/status`   | Update request status    |
| `PUT`    | `/api/requests/{id}/assign`   | Assign technician        |
| `GET`    | `/api/requests/{id}/history`  | Get request history      |
| `GET`    | `/api/requests/{id}/comments` | Get request comments     |
| `POST`   | `/api/requests/{id}/comments` | Add a comment            |

---

## 📂 Categories

| Method   | Endpoint               | Description        |
| -------- | ---------------------- | ------------------ |
| `GET`    | `/api/categories`      | Get all categories |
| `GET`    | `/api/categories/{id}` | Get category by ID |
| `POST`   | `/api/categories`      | Create a category  |
| `PUT`    | `/api/categories/{id}` | Update a category  |
| `DELETE` | `/api/categories/{id}` | Delete a category  |

---

## 👥 Users

| Method   | Endpoint               | Description      |
| -------- | ---------------------- | ---------------- |
| `GET`    | `/api/users`           | Get all users    |
| `GET`    | `/api/users/{id}`      | Get user by ID   |
| `PUT`    | `/api/users/{id}`      | Update user      |
| `DELETE` | `/api/users/{id}`      | Delete user      |
| `PUT`    | `/api/users/{id}/role` | Update user role |

---

## 📊 Dashboard

| Method | Endpoint                     | Description                |
| ------ | ---------------------------- | -------------------------- |
| `GET`  | `/api/dashboard/overview`    | Get overall statistics     |
| `GET`  | `/api/dashboard/requests`    | Get request statistics     |
| `GET`  | `/api/dashboard/categories`  | Get category statistics    |
| `GET`  | `/api/dashboard/technicians` | Get technician performance |

---

# ⚙️ Setup & Installation

## 1️⃣ Clone the Repository

```bash
git clone https://github.com/saifsamehelsawy/Maintenance-Request-System.git
```

Navigate to the project directory:

```bash
cd Maintenance-Request-System
```

---

## 2️⃣ Configure the Database

Open:

```text
appsettings.json
```

Configure your SQL Server connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=MaintenanceRequestDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

Replace `YOUR_SERVER` with your SQL Server instance name.

---

## 3️⃣ Apply Database Migrations

Make sure Entity Framework Core tools are installed:

```bash
dotnet tool install --global dotnet-ef
```

Then apply the existing migrations:

```bash
dotnet ef database update
```

---

## 4️⃣ Run the Application

Run the project using:

```bash
dotnet run
```

The API will be available through the URL shown in the terminal.

---

# 📖 Swagger API Documentation

After running the application, open Swagger UI:

```text
/swagger
```

Swagger provides an interactive interface for:

* Viewing API endpoints.
* Testing requests.
* Sending authentication tokens.
* Inspecting request and response models.
* Testing role-protected endpoints.

---

# 🔄 Request Lifecycle

A maintenance request follows a structured lifecycle:

```text
Employee
   │
   ▼
Create Request
   │
   ▼
Pending
   │
   ▼
Assigned to Technician
   │
   ▼
In Progress
   │
   ▼
Technician Updates Request
   │
   ▼
Resolved
   │
   ▼
Closed
```

The request history records important changes throughout this lifecycle.

---

# 🔒 Security

The application implements several security mechanisms:

* JWT Authentication
* ASP.NET Core Identity
* Role-Based Authorization
* Password Hashing
* Rate Limiting
* Request Validation
* Global Exception Handling
* Protected API Endpoints

Sensitive configuration values should not be committed to source control.

---

# 🧪 Testing

The API can be tested using:

* Swagger UI
* Postman
* REST Client
* Any HTTP client

For protected endpoints:

1. Login using one of the test accounts.
2. Copy the generated JWT token.
3. Click **Authorize** in Swagger.
4. Enter:

```text
Bearer YOUR_TOKEN
```

5. Test the protected endpoints according to the user's role.

---

# 📈 Future Improvements

Potential future improvements include:

* Real-time notifications using SignalR.
* Email notifications.
* Mobile application.
* Advanced analytics.
* Export reports to Excel/PDF.
* File and image attachments.
* Advanced search and filtering.
* SLA and priority management.
* More detailed administrative dashboards.

---

# 👨‍💻 Author

**Saif Sameh Fathy El-Sawy**

Computer Science & AI Student
Data Science & Data Analysis Enthusiast

GitHub: **[@saifsamehelsawy](https://github.com/saifsamehelsawy)**

---

## ⭐ Support

If you find this project useful, consider giving the repository a ⭐ on GitHub.

---

## 📄 License

This project is developed for educational and portfolio purposes.
