# Locamonda

A real estate web application built with **ASP.NET Core MVC** as a **team project for a Software Engineering course at university**.

The application provides a platform for browsing and managing properties, with separate workflows for customers, property owners, and administrators.

## Features

* User registration and authentication
* Role-based authorization
* Property listing and management
* Property categories and locations
* Property approval and availability management
* Property booking
* Customer reviews and ratings
* Separate functionality for:

  * **Customer**
  * **Owner**
  * **Admin**

## Tech Stack

* **C#**
* **ASP.NET Core MVC (.NET 8)**
* **Entity Framework Core**
* **ASP.NET Core Identity**
* **SQL Server / LocalDB**
* **Razor Views**
* **HTML / CSS / JavaScript**
* **Bootstrap**
* **Git & GitHub**

## Architecture

The application follows the **MVC (Model-View-Controller)** pattern.

```text
Locamonda
├── Controllers
├── Models
├── Views
├── Data
├── Areas
├── wwwroot
└── Program.cs
```

Entity Framework Core is used for database access and migrations, while ASP.NET Core Identity handles authentication and role management.

## User Roles

### Customer

* Browse available properties
* View property details
* Make bookings
* Add reviews

### Owner

* Add properties
* Update property information
* Remove properties
* Manage listed properties

### Admin

* Manage application data
* Review property listings
* Approve properties
* Manage platform operations

## Database

The main entities include:

* `Property`
* `Location`
* `Category`
* `Booking`
* `Review`
* `ApplicationUser`

Entity relationships and database operations are managed using **Entity Framework Core**.

Properties available to customers are filtered based on their current status, including whether they are active, approved, and available.

## Getting Started

### Prerequisites

* [.NET 8 SDK](https://dotnet.microsoft.com/)
* SQL Server or SQL Server LocalDB
* Visual Studio / VS Code

### Clone the repository

```bash
git clone https://github.com/AyaRayanx/locamonda-.git
cd locamonda-
```

### Configure the database

Update the connection string in `appsettings.json` according to your SQL Server setup.

Example:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=locamonda;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

### Apply migrations

```bash
dotnet ef database update
```

### Run the application

```bash
dotnet run
```

Open the local URL displayed in the terminal.

## Project Structure

| Layer          | Responsibility                          |
| -------------- | --------------------------------------- |
| Models         | Application entities and relationships  |
| Controllers    | Request handling and application logic  |
| Views          | User interface                          |
| Data           | Database context and configuration      |
| Areas/Identity | Authentication and user management      |
| wwwroot        | CSS, JavaScript, and other static files |

## Team Project

Locamonda was developed as a **team project for a Software Engineering course at university**.

The project involved applying software engineering concepts to a complete web application, including:

* MVC architecture
* Database design
* Authentication and authorization
* Role-based access control
* CRUD operations
* Team collaboration
* Git and GitHub version control

## Future Improvements

* REST API
* Advanced property search and filtering
* Pagination
* Property image management
* Email notifications
* Online payment integration
* Automated testing
* Docker deployment


