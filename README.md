# EventHorizon

## Overview
**EventHorizon** is a full-stack web application built for seamless event management. It enables users to browse, book, and manage events, while providing administrators with tools to oversee and control event listings.

## Considerations
As my expertise lies primarily in backend development, the frontend was implemented using simple technologies to focus on core functionality and maintainability.

## AI Tools Utilized
To enhance productivity and code quality, the following AI tools were used throughout development:
- Cursor  
- GitHub Copilot  
- ChatGPT  
- V0.dev  
- Claude Sonnet 3.5  

---

## Backend

### Technology Stack
- **Framework:** ASP.NET Core  
- **Database:** SQL Server  
- **Authentication:** JWT (JSON Web Tokens)

### Architecture
The application follows an **N-Tier Architecture** to ensure a clean separation of concerns, scalability, and maintainability. Given the project's size, this architecture was selected instead of Clean Architecture for its simplicity.

### Design Patterns and Principles
- **Repository Pattern** – Encapsulates data access logic  
- **Dependency Injection** – Promotes modularity and testability  
- **SOLID Principles** – Ensures maintainable and extensible code  

### Setup Instructions
**Prerequisites:**
- .NET SDK 6.0 or later  
- SQL Server instance  

### API Endpoints

#### Authentication
- `POST /api/account/register` — Register a new user  
- `POST /api/account/login` — Authenticate and receive a JWT token  
- `POST /api/account/logout` — Log out the user (authentication required)  

#### Events
- `GET /api/event` — Retrieve a list of events with pagination and search  
- `POST /api/event` — Create a new event (admin only)  
- `PUT /api/event/{id}` — Update an existing event (admin only)  
- `DELETE /api/event/{id}` — Delete an event (admin only)  

#### Categories
- `GET /api/category` — Retrieve a list of categories  
- `POST /api/category` — Create a new category  
- `PUT /api/category/{id}` — Update a category  
- `DELETE /api/category/{id}` — Delete a category  

#### Bookings
- `GET /api/booking` — Retrieve user bookings (authentication required)  
- `POST /api/booking` — Book an event (authentication required)  
- `DELETE /api/booking/{id}` — Cancel a booking (authentication required)  

---

## Frontend

### Technology Stack
- **HTML / CSS / JavaScript (Vanilla JS)**  
  The frontend is built without external frameworks for simplicity and full control over the user interface.

### Setup Instructions
**Prerequisites:**
- Modern web browser (e.g., Chrome, Firefox, Edge)

### Features

#### User Authentication
- Register and log in using the navbar  
- JWT tokens are stored in `localStorage` for session persistence

#### Event Browsing
- View all events with search and pagination functionality  
- Event cards display event image, date, venue, and price

#### Booking Management
- Book events directly from the events page  
- View and manage bookings on the "My Bookings" page

#### Admin Panel
- Accessible to authenticated admin users  
- Create, update, and delete events through a dedicated interface

### File Structure
root/
├── index.html → Main landing page
├── events.html → Event listings
├── my-bookings.html → User booking history
├── admin.html → Admin dashboard
├── css/
│ └── style.css → Stylesheets
├── js/
│ ├── auth.js → Authentication logic
│ ├── main.js → Homepage interactions
│ ├── events.js → Event page functionality
│ └── my-bookings.js → Booking page logic
└── resources/ → Static assets (images, etc.)


---

## Usage Instructions

### 1. Register / Login
- Use the navbar to access the registration or login forms  
- Enter your credentials to gain access to user-specific features

### 2. Browse Events
- Navigate to the **Events** page  
- Use the search bar to find specific events  
- Click "Book Now" to reserve a spot

### 3. Manage Bookings
- Access **My Bookings** to view or cancel your reservations

### 4. Admin Features
- Log in with admin credentials  
- Access the **Admin Panel** to manage event listings (create, update, delete)
