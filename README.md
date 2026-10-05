# Motshwane Consortium Management System

The **Motshwane Consortium Management System** is a web-based application developed to support the management of Motshwane Consortium's **mobile freezer and mobile toilet rental services**.

The system aims to simplify the booking process for customers while providing administrators and staff with interfaces for managing bookings, services, payments and operational activities.

This project is being developed as part of our Work Integrated Learning (WIL) implementation project.



## Project Objectives

The system is designed to:

- Allow customers to browse available mobile freezers and toilets.
- Allow customers to create and manage service bookings.
- Support the submission of proof of payment.
- Allow administrators to manage bookings, customers, services and payments.
- Allow administrators to assign staff to confirmed bookings.
- Allow staff members to view assigned jobs and update job progress.
- Provide a responsive and user-friendly interface across desktop and mobile devices.



## System Interfaces

### Customer Interface

The customer portal currently includes:

- Registration
- Login
- Customer Dashboard
- Browse Freezers & Toilets
- Booking Form
- Upload Proof of Payment
- Booking Confirmation
- My Bookings / Booking History

### Admin Interface

The administration portal includes:

- Admin Dashboard
- Manage Bookings
- Manage Freezers & Toilets
- Manage Customers
- Manage Payments
- Assign Staff
- Reports

### Staff Interface

The staff portal includes:

- Staff Dashboard
- Assigned Jobs
- Job Details
- Update Job Status
- Staff Profile



## Technology Stack

The application is being developed using:

- **ASP.NET Core MVC**
- **C#**
- **.NET 10**
- **Razor Views**
- **HTML5**
- **CSS3**
- **JavaScript**
- **Entity Framework Core**
- **SQL Server / Azure SQL**
- **RESTful APIs**
- **Microsoft Azure**
- **Git & GitHub**

Some technologies will be integrated progressively as development continues.



## Project Architecture

The application follows the **ASP.NET Core MVC architecture**.

- **Models** represent application data.
- **Views** provide the Customer, Admin and Staff user interfaces.
- **Controllers** manage requests and communication between the interface and application logic.
- **Services** provide reusable application functionality and data access.
- **REST APIs** will support communication between the front end and backend services.
- **Database integration** will provide persistent storage for users, bookings, services, payments and staff assignments.

The current implementation uses dummy data where required to allow the interfaces and application flows to be demonstrated while backend and database integration continues.



## Team Responsibilities

| Team Member | Responsibilities |
|-------------|------------------|
| **Lebone** | Front End, Security, Documentation |
| **Thato** | Database Design, Entity Framework Core & REST API |
| **Keren** | Backend Logic, Services, Hosting & Deployment |




## Security

The application is being developed with security considerations including:

- HTTPS
- Input validation
- Anti-forgery protection
- Authentication
- Role-based authorization
- Secure password handling
- Secure payment proof handling
- Protection of sensitive configuration information

Additional security controls will be implemented and tested as backend integration progresses.



## Development Workflow

GitHub is used for version control and team collaboration.


## Running the Application

1. Clone or download the repository.
2. Open the project in **Visual Studio 2022**.
3. Ensure the required **.NET 10 SDK** is installed.
4. Restore any required NuGet packages.
5. Build the solution using **Build > Build Solution**.
6. Run the application using the green **Start** button.
7. Use the navigation menu to access the Customer, Admin and Staff interfaces.



## Current Development Status

The project is currently under active development.

The front-end foundation and the three primary system interfaces have been implemented. Database, API, backend, security and Azure deployment components will continue to be integrated and tested as development progresses.


