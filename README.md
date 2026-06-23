# Alumni Management System (AlumniNet)

A premium, feature-rich web application designed to connect university alumni and current students, facilitate professional mentorship, manage events and ticketing, run donation campaigns, and provide administrators with powerful dashboards and reporting (screenshots attached at end) .

---

## 🚀 Core Features

### 1. Multi-Portal SaaS Dashboard
* **Modern Interface**: A clean, responsive dashboard using a modern Slate Navy and Indigo styling. Includes a collapsible sticky sidebar for easy navigation.
* **Role-Based Access Control**: Tailored dashboards for **Admins**, **Alumni**, and **Students**.

### 2. User Profiles & Directory
* **Interactive Profiles**: Detailed profile views for both Students (semester, CGPA, department) and Alumni (current company, job title, industry, LinkedIn, bio).
* **Alumni Directory**: Students can browse and search the alumni network with filters for department, industry, graduation year, or name.

### 3. Professional Mentorship Program
* **Request & Match**: Students can send mentorship requests to alumni with a custom message.
* **Security & Privacy**: Alumni contact details (email and phone) are hidden by default and only unlocked for students once the mentorship request is accepted.

### 4. Events & Ticketing System
* **Event Creation**: Administrators can schedule, edit, or cancel events with date constraints, capacity limits, and ticket pricing.
* **Self-Service Tickets**: Users can browse upcoming events, register, and receive unique ticket codes (free or mock-paid).
* **Attendance Tracking**: Admins can view attendee rosters and mark attendance directly from the portal.

### 5. Fundraising & Donations
* **Donation Campaigns**: Admins can run targeted fundraising campaigns with progress bars tracking target vs. raised amounts.
* **Mock Checkout**: Alumni can make donations with preset or custom amounts, with the option to remain anonymous.
* **Database Level Automation**: Campaign totals are updated instantly via automated database triggers.

### 6. Admin Panel & Reports
* **User Management**: View and filter registered users.
* **Invite Codes**: Generate and manage administrative invite codes to bootstrap new admins.
* **Data Exports & Reports**: Review mentorship logs, event registrations, and donation reports.

---

## 🛠️ Technology Stack

* **Backend**: ASP.NET Core Web API (C# targeting **.NET 10.0**)
* **Frontend**: Responsive Single-Page UI (HTML5, Vanilla CSS, and JavaScript using Fetch API)
* **Database**: SQL Server / SQL Express (Entity Framework Core 10.0, stored procedures, custom views, and database triggers)
* **Security**: JWT Bearer Authentication, password hashing via BCrypt

---

## 📋 Prerequisites

To run this application locally, ensure you have:
1. **.NET 10.0 SDK** or higher installed.
2. **MS SQL Server** (LocalDB, Express, or Developer Edition) running locally.
3. A local static web server tool (e.g. VS Code **Live Server** extension, **Python**, or **Node.js/npx**).

---

## ⚙️ Step-by-Step Installation & Setup

### Step 1: Database Setup
The system uses native SQL Server configurations (triggers, stored procedures, and views).

1. Open your SQL Server management client (e.g., SSMS, Azure Data Studio) and connect to your instance.
2. Run the SQL scripts located in the `database/` folder in the **following order**:
   1. `database/01_create_database.sql` — *Drops and creates the `AlumniMS` database*
   2. `database/02_schema.sql` — *Creates all 11 tables and constraints*
   3. `database/03_indexes.sql` — *Creates non-clustered performance indexes*
   4. `database/04_views.sql` — *Creates dashboard summary views*
   5. `database/05_stored_procedures.sql` — *Creates core database procedures*
   6. `database/06_triggers.sql` — *Creates triggers (e.g., auto-updating campaign fundraising figures)*
   7. `database/07_seed_data.sql` — *Populates departments and seeds initial admins*
   8. `database/08_sample_data.sql` — *Seeds sample alumni, students, events, and donation campaigns*

3. Open `appsettings.json` in the backend project root and verify the connection string:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=AlumniMS;Trusted_Connection=True;TrustServerCertificate=True;"
   }
   ```
   *(Update the server value if your local instance name differs from `localhost\SQLEXPRESS`).*

---

### Step 2: Run the Backend API
1. Open your terminal in the root of the project directory.
2. Build the project:
   ```powershell
   dotnet build
   ```
3. Run the API:
   ```powershell
   dotnet run --launch-profile http
   ```
   The backend API will start and listen on **`http://localhost:5180`**.
   * You can test backend connectivity and view interactive endpoints via Swagger at: [**`http://localhost:5180/swagger/index.html`**](http://localhost:5180/swagger/index.html).

---

### Step 3: Run the Frontend App
Because of CORS configuration, the static frontend files must be served from a web server on port **`5500`**.

1. Open your terminal and navigate to the project directory:
   ```powershell
   cd AlumniManagementSystem
   ```
2. Serve the directory using Python or Node:
   * **Python**:
     ```powershell
     python -m http.server 5500
     ```
   * **Node.js (npx)**:
     ```powershell
     npx http-server -p 5500
     ```
   * **VS Code**: Simply open the project folder in VS Code, right-click `frontend/index.html`, and select **Open with Live Server**.

3. In your web browser, navigate to the portal landing page:
   👉 [**`http://localhost:5500/frontend/index.html`**](http://localhost:5500/frontend/index.html)

---

## 🔑 Test Accounts
All seeded accounts share the same password for ease of testing:

* **Password**: `Password@123`

| Role | Username / Email | Description |
| :--- | :--- | :--- |
| **Admin** | `admin1@university.edu` | Full control over users, events, campaigns, reports |
| **Admin** | `admin2@university.edu` | Secondary admin account |
| **Alumni** | `zuhar@alumni.edu` | Profile: BS Computer Science, TechCorp (Software Engineer) |
| **Alumni** | `ramsha@alumni.edu` | Profile: BS Software Engineering, SoftSolutions (QA Engineer) |
| **Student** | `std1@student.edu` | Profile: BS Computer Science, Semester 5, CGPA 3.80 |
| **Student** | `std2@student.edu` | Profile: BS Software Engineering, Semester 3, CGPA 3.50 |

---

## 📂 Project Structure

```
AlumniManagementSystem/
│
├── Controllers/         # REST API Controllers (Auth, Events, Profile, etc.)
├── Data/                # DbContext class and EF configurations
├── Entities/            # C# Domain Models (User, Event, Donation, etc.)
├── Services/            # Business logic layers (AuthService, EventService, etc.)
├── database/            # SQL setup scripts (01_create_database.sql through 08_sample_data.sql)
│
└── frontend/            # Static assets and views
    ├── css/             # Stylesheets (main.css, sidebar.css)
    ├── js/              # Client-side routing, API fetches, auth guards
    └── pages/           # Portal-specific dashboard views (student, alumni, admin)
```
<img width="1908" height="874" alt="image" src="https://github.com/user-attachments/assets/47cd6eb5-6c39-4d68-a846-26c887efbda2" />

<img width="1341" height="444" alt="image" src="https://github.com/user-attachments/assets/de970876-fea3-4eea-95b6-2b5a9423f3a9" />

<img width="1435" height="864" alt="image" src="https://github.com/user-attachments/assets/4556545d-1e61-4a92-807f-d40a57bbfa03" />

<img width="1345" height="704" alt="image" src="https://github.com/user-attachments/assets/1080288c-f754-4ff6-88f3-95b1d93b179b" />

<img width="1658" height="857" alt="image" src="https://github.com/user-attachments/assets/9a2a9624-72d4-47bf-9830-2a2bb31eacaf" />

ADMIN:

<img width="1727" height="876" alt="image" src="https://github.com/user-attachments/assets/001e5aea-7498-403d-b174-c3ae21d35457" />

<img width="1764" height="861" alt="image" src="https://github.com/user-attachments/assets/e78f362d-7463-42a8-bd97-b734e3e0c42a" />

<img width="1791" height="864" alt="image" src="https://github.com/user-attachments/assets/485db559-e2b3-44fe-bcb8-16a68c4ca4e5" />

<img width="1880" height="866" alt="image" src="https://github.com/user-attachments/assets/37ba9187-85f8-4cda-803c-c8eb25881d60" />





