# EventHorizon

## Overview
**EventHorizon** is a full-stack web application built for seamless event management. It enables users to browse, book, and manage events, while providing administrators with tools to oversee and control event listings.

## **Live Overview**

I used MonsterASP to host my backend, and Netlify to host the front-end
you can see a live overview : **https://romioeventhorizon.netlify.app/**

Admin Credentials
- username : admin123
- password : Admin123*

**Swagger Documentation**
you can check the swagger documentation for the API here : **https://eventhorizon.runasp.net/swagger/index.html**

or you can download the EventHorizon-FrontEnd Folder
open the index.html then start a liveserver.




## Considerations
As my expertise lies primarily in backend development, the frontend was implemented using simple technologies and using AI tools (vibe coding).


## AI Tools Utilized
To enhance productivity and code quality, the following AI tools were used throughout development:
- Cursor  
- GitHub Copilot  
- ChatGPT  
- V0.dev  
- Claude Sonnet 3.5  

---



### Local Setup Instructions

#### Prerequisites
- [.NET SDK 8.0 or later](https://dotnet.microsoft.com/en-us/download)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- IDE (e.g., Visual Studio 2022 or Visual Studio Code with C# extension)

#### Steps
1. **Clone the Repository**
   ```bash
   git clone https://github.com/your-username/eventhorizon.git
   cd EventHorizon
   ```

2. **Configure the Connection String**
   - Navigate to `appsettings.json`
   - Replace the `Default` value with your SQL Server connection string:
     ```json
     "ConnectionStrings": {
       "Default": "Server=.;Database=EventHorizonDb;Trusted_Connection=True;"
     }
     ```

3. **Run the Project** (you don't need to apply the migrations as i'm applying it when you start the app)
   ```bash
   dotnet run
   ```

5. **Test the API**
   - Use Swagger at `https://localhost:7193/swagger`
   - Or test endpoints using Postman or any API client
   - Or just run the project from your code editor and test them with swagger docs.




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


---

## Frontend

### Technology Stack
- **HTML / CSS / JavaScript (Vanilla JS)**  
  The frontend is built without external frameworks for simplicity.

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
