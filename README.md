# Motshwane Consortium Management System

The Motshwane Consortium Management System is a web-based and mobile rental management application developed for Motshwane Consortium Mobile Freezer & Toilet, a business providing mobile freezer and mobile toilet rental services for events, construction sites, and other occasions.

The system digitalises the rental process by allowing customers to browse available services, make bookings, upload proof of payment and track booking statuses. Administrators can manage bookings, review payments, assign staff and monitor operations, while staff members can view assigned jobs and update their progress.

This application was developed as part of the INSY7315 Work Integrated Learning (WIL) Project.

Live Application
Deployed Website:
https://motshwane-app-e2dse2c6c8fkf0f5.southafricanorth-01.azurewebsites.net/


The application is hosted on Microsoft Azure App Service, with Azure SQL Database providing persistent data storage.

The system aims to:

Provide an accessible online platform for mobile freezer and toilet rentals.
Allow customers to register, sign in and browse available rental units.
Enable customers to create bookings and track their progress.
Support secure proof-of-payment submissions.
Allow administrators to approve or reject submitted payment proofs.
Enable administrators to assign available staff to confirmed bookings.
Allow staff members to manage assigned jobs and update delivery progress.
Prevent conflicting bookings and duplicate staff assignments.
Improve operational efficiency and customer service.
Provide a responsive interface for desktop and mobile devices.

System Features
1. Customer Portal
The customer interface provides:

Customer registration and authentication.
Customer dashboard.
Browsing and filtering mobile freezers and toilets.
Booking creation and scheduling.
Booking confirmation and reference generation.
Proof-of-payment upload.
Booking history and payment-status tracking.
Access to booking information and updates.
Customers can upload proof of payment using supported PDF, JPG, and PNG files, subject to a 5 MB file-size limit.

2. Administrator Portal
The administration interface includes:

Administrator dashboard.
Booking management.
Freezer and toilet service management.
Customer information.
Payment verification.
Payment approval and rejection.
Staff assignment.
Booking and operational reports.
Administrators can review submitted payment proofs and approve or reject them. Once a payment is approved, the corresponding booking becomes eligible for staff assignment.

3. Staff Portal
The staff interface includes:

Staff dashboard.
Assigned jobs.
Job details.
Delivery and collection information.
Job-status updates.
Staff members can update assigned jobs through the following stages:

Assigned → In Progress → Delivered → Completed

The system enforces the correct sequence of status changes.

Booking and Payment Workflow
The application follows this workflow:

Customer Registration: The customer creates an account or signs in.
Browse Services: The customer selects an available mobile freezer or toilet.
Create Booking: The customer enters the rental date and delivery location.
Booking Confirmation: The system generates a booking reference and records the booking as Pending.
Upload Proof of Payment: The customer submits a payment document using the Upload Proof page.
Payment Review: The administrator reviews the submitted proof.
Approval or Rejection: Approved payments confirm the booking. Rejected proofs may be resubmitted.
Staff Assignment: The administrator assigns an available staff member to the confirmed booking.
Job Management: The assigned staff member updates the job status as work progresses.
Completion: The job is marked Completed once the required work has been finalised.

Technology Stack
Technology

Purpose

ASP.NET Core MVC

Web application framework

C#

Application and backend development

.NET 10

Application runtime

Razor Views

Server-rendered user interfaces

HTML5, CSS3, JavaScript

Front-end development

Bootstrap

Responsive interface components

Entity Framework Core

Database access and object-relational mapping

SQL Server / Azure SQL

Data storage

RESTful APIs

Application endpoints and data operations

Microsoft Azure App Service

Cloud hosting

Git & GitHub

Version control and collaboration

GitHub Actions

Automated build and testing workflows

Visual Studio

Development and debugging

System Architecture
The application follows the Model–View–Controller (MVC) architectural pattern, supported by a service layer and Entity Framework Core.

Models
Models represent the application’s data structures, including users, customers, bookings, rental units, payments, and staff assignments.

Views
Razor Views provide the customer, administrator, and staff interfaces.

Controllers
Controllers handle HTTP requests, validate user interactions, and coordinate application operations.

The project includes:

HomeController
CustomerController
AdminController
StaffController
BookingsApiController
CustomersApiController
UnitsApiController
Services
The service layer contains reusable business functionality for booking management, service catalogues, payments, and file storage.

Database
Entity Framework Core manages database operations, while SQL Server and Azure SQL provide persistent storage.

API Integration
RESTful API controllers provide endpoints for managing booking, customer, and rental-unit data.

Security Features
The application incorporates the following security controls:

HTTPS for encrypted communication.
Cookie-based authentication.
Role-based authorisation for customers, administrators, and staff.
Password hashing using ASP.NET Core password-hashing functionality.
Server-side input validation.
Anti-forgery token validation.
Restricted access to role-specific pages.
File-type and file-size validation for payment proofs.
Customer ownership checks for payment submissions.
Protection of sensitive database connection strings through configuration and user secrets.
Business Rules and Validation
The system enforces important operational rules:

Bookings cannot be created for past dates.
Rental units cannot be double-booked for overlapping rental periods.
Customers can submit payment proofs only for their own bookings.
Only supported payment-proof file types are accepted.
Payments must be approved before staff assignment.
A booking cannot be assigned more than once.
Staff members cannot receive conflicting assignments on the same date.
Staff job statuses must progress in the correct order.
Users cannot access functionality outside their authorised roles.

Project Structure
MotshwaneConsortiumGroup/
│
├── .github/
├── Controllers/
├── Data/
├── Docs/
├── DTOs/
├── Extensions/
├── Migrations/
├── Models/
├── Services/
├── Views/
│   ├── Admin/
│   ├── Customer/
│   ├── Home/
│   ├── Shared/
│   └── Staff/
├── wwwroot/
├── MotshwaneConsortiumGroup.Tests/
├── appsettings.json
├── Program.cs
├── MotshwaneConsortiumGroup.csproj
└── MotshwaneConsortiumGroup.slnx
Running the Application Locally

Prerequisites
Ensure the following are installed:

Visual Studio with the ASP.NET and web development workload.
.NET 10 SDK.
Git.
SQL Server LocalDB or another configured SQL Server instance.
Installation and Setup
Clone the repository:
git clone https://github.com/lebmots/MotshwaneConsortiumGroup.git
Open MotshwaneConsortiumGroup.slnx in Visual Studio.
Set MotshwaneConsortiumGroup as the startup project.
Restore the required NuGet packages.
Configure the database connection string for your local environment.
Build the solution using Build → Build Solution.
Select IIS Express or an appropriate project launch profile.
Start the application using F5 or Ctrl + F5.
Open the localhost address displayed by Visual Studio.
The application initialisation process applies database migrations and seeds demonstration accounts where configured.

Testing
The solution includes a dedicated test project:

MotshwaneConsortiumGroup.Tests

Tests can be executed using Visual Studio Test Explorer:

Open Test → Test Explorer.
Build the solution.
Select Run All Tests.
Review the results and resolve any failures.
The system should also be tested through its complete customer, administrator, and staff workflows before release.

Deployment
The application is deployed to Microsoft Azure App Service and uses Azure SQL Database.

The deployment environment provides:

Online access through a hosted website.
Cloud-based database storage.
HTTPS connectivity.
Environment-specific configuration.
GitHub-based source control.
GitHub Actions build and testing automation.
Changes should be committed to GitHub, tested, and deployed through the project’s configured deployment process.

Team Responsibilities
Team Member

Responsibilities

Lebone Bokamoso Princess Motshwane

Front-end development, customer interfaces, security, UI improvements, system integration, documentation, and presentation

Thato Temwa Nyirenda

Database design, Entity Framework Core, data models, migrations, REST API development, and integration

Keren Benjamin Malanda

Backend logic, application services, business rules, testing, Azure hosting, and deployment

The project was completed collaboratively, with all team members contributing to integration, testing, troubleshooting, and final project delivery.

Current Project Status
The Motshwane Consortium Management System has progressed from an initial prototype to a deployed ASP.NET Core MVC web application.

The project currently includes:

Customer, administrator, and staff interfaces.
Customer registration and authentication.
Rental-unit browsing and booking functionality.
Booking history and status tracking.
Payment-proof submission functionality.
Administrator payment-review and staff-assignment workflows implemented in the project.
Staff job management.
SQL Server and Azure SQL integration.
Azure App Service deployment.
Automated build and testing workflows.
Final verification is ongoing, particularly for the visibility and operation of payment-upload actions, administrator payment approval and rejection, staff assignment, and mobile responsiveness on the deployed website.

Future Enhancements
Potential improvements include:

Integrated online payments.
Automated email and SMS notifications.
Real-time delivery tracking.
Enhanced reporting and analytics.
Improved mobile navigation and responsive booking tables.
Additional rental service categories.
Expanded customer self-service features.

Conclusion
The Motshwane Consortium Management System demonstrates the practical application of software engineering, database management, web development, security, cloud deployment, and collaborative development.

By bringing customer bookings, payment verification, staff assignments, and operational management into a centralised platform, the project aims to improve the efficiency and reliability of mobile freezer and toilet rental services.

INSY7315 — Work Integrated Learning
Motshwane Consortium Mobile Freezer & Toilet Booking System
Developed by LST Collective



