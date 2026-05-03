# Dog Grooming Queue System 🐶

A full-stack web application for managing dog grooming appointments with user authentication, real-time scheduling, and automatic discount calculation for loyal customers.

## 📋 Project Overview

This system allows customers to:
- Register and login securely with JWT authentication
- Book dog grooming appointments by size (small/medium/large)
- View, edit, and delete their appointments
- Receive automatic 10% discount after 3 bookings
- Filter appointments by date and customer name
- View detailed appointment information in a popup

## 🏗️ Tech Stack

### Backend
- **.NET 10** with ASP.NET Core
- **SQL Server** database
- **Entity Framework Core** for ORM
- **JWT** for authentication
- **SQL Procedures** for business logic
- **SQL Views** for data retrieval

### Frontend
- **React 19** with Vite
- **React Router** for navigation
- **Responsive CSS** with modern styling

## 📦 Features

### Authentication & Authorization
- User registration with username, password, and full name
- JWT-based login system
- Secure token storage in localStorage
- Logout with token cleanup

### Appointment Management
- **Create**: Book appointments with dog name, size, and date/time
- **Read**: View all user appointments with filtering options
- **Update**: Edit appointment details (except for same-day appointments)
- **Delete**: Remove appointments (restrictions apply)

### Pricing & Discounts
```
Dog Size    Duration    Price
Small       30 min      ₪100
Medium      45 min      ₪150
Large       60 min      ₪200

Loyalty Discount: 10% off after 3+ appointments
```

### Security Features
- Users can only see/edit their own appointments
- Cannot delete same-day appointments
- Cannot edit other users' appointments
- Password stored securely (basic validation)

### Filtering
- Filter by appointment date
- Filter by customer name
- Clear filters button

## 🚀 Getting Started

### Prerequisites
- .NET 10 SDK
- Node.js 18+
- SQL Server (local or cloud)
- Git

### Installation

#### Backend Setup
```bash
cd backend
dotnet restore
dotnet ef database update
dotnet run
```

Backend runs on: `http://localhost:5285`

#### Frontend Setup
```bash
cd frontend
npm install
npm run dev
```

Frontend runs on: `http://localhost:5173` (or next available port)

## 📝 API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login and get JWT token

### Appointments
- `GET /api/appointments` - Get user's appointments
- `GET /api/appointments/filter?date=X&customerName=Y` - Filter appointments
- `POST /api/appointments` - Create new appointment
- `PUT /api/appointments/{id}` - Update appointment
- `DELETE /api/appointments/{id}` - Delete appointment

## 🗄️ Database Schema

### Tables
- **Users**: id, username, password, fullname
- **Appointments**: id, username, dogname, dogsize, date, createdAt, price, durationminutes

### Microsoft SQL Server (assignment: procedure + VIEW in DB)

- Run the API with a **SQL Server** connection string (not `Data Source=`). On Windows, use Visual Studio / Rider launch profile **`sqlserver`** (LocalDB), or copy `backend/appsettings.SqlServer.example.json` into `appsettings.Development.json`. On Mac/Linux, use Docker SQL Server — see `backend/Sql/README.md`.
- Startup runs **`Migrate()`** then creates the VIEW + procedure via `SqlServerRoutineInstaller`.

### Stored procedures (SQL Server)
- **`dbo.sp_GetUserLoyaltyPreview @Username`** — returns `AppointmentCount` and `NextBookingDiscountPercent` (0 or 10). Created at startup by `SqlServerRoutineInstaller`; called from `GetLoyaltyBookingPreview` when the DB provider is SqlServer.
- **SQLite:** does not support `CREATE PROCEDURE`; the same preview logic runs in C# (LINQ).

### Views
- **`vw_AppointmentsWithUsers`** — appointments `INNER JOIN` users (`FullName`). Created at startup on SQLite (`SqliteSchemaFixer`) and SqlServer (`SqlServerRoutineInstaller`). Read via **`GET /api/appointments/upcoming-with-user-info`** (`AppointmentWithUserView`).
- See `backend/Sql/README.md` for file references.

## 🎨 UI/UX

- **Home Page**: Hero panel with feature highlights and auth cards
- **Login/Register**: Clean card-based interface
- **Appointment List**: Grid cards with quick actions
- **Appointment Details**: Modal popup with full information
- **Create Form**: Consistent styling with other components
- **Greeting**: Personalized user greeting with logout button

## 🔒 Security Considerations

- JWT tokens expire after 2 hours
- Passwords should be hashed (use bcrypt in production)
- CORS enabled for localhost development only
- Sensitive data stored in localStorage (secure in production with httpOnly cookies)

## 📊 Database Diagram

```
Users
├── id (Primary Key)
├── username (Unique)
├── password
└── fullname

Appointments
├── id (Primary Key)
├── username (Foreign Key to Users)
├── dogname
├── dogsize
├── date
├── createdAt
├── price
└── durationminutes
```

## 🧪 Testing

### Manual Testing Checklist
- [ ] Register new user
- [ ] Login with credentials
- [ ] Create appointment (all sizes)
- [ ] View appointment details in popup
- [ ] Edit own appointment
- [ ] Cannot edit other user's appointment
- [ ] Filter by date
- [ ] Filter by customer name
- [ ] Delete appointment (not same-day)
- [ ] Cannot delete same-day appointment
- [ ] Verify 10% discount after 3 appointments
- [ ] Logout and redirect to home
- [ ] Access protected pages without token returns to home

## 📂 Project Structure

```
dog-queue-project/
├── backend/
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   ├── AppointmentsController.cs
│   │   └── Sql/ (VIEW + SQL Server procedure scripts)
│   ├── Models/
│   │   ├── User.cs
│   │   ├── Appointment.cs
│   │   └── LoginRequest.cs
│   ├── Data/
│   │   ├── AppDbContext.cs
│   │   └── Migrations/
│   ├── Program.cs
│   └── appsettings.json
├── frontend/
│   ├── src/
│   │   ├── App.jsx
│   │   ├── App.css
│   │   ├── main.jsx
│   │   ├── index.css
│   │   └── components/
│   │       ├── Login/
│   │       ├── Register/
│   │       ├── MyAppointments/
│   │       ├── CreateAppointment/
│   │       └── EditAppointment/
│   ├── package.json
│   └── vite.config.js
└── README.md
```

## 🔄 Workflow

1. **User Registration**: New user creates account
2. **User Login**: JWT token generated and stored
3. **Create Appointment**: Form submits to backend, discount calculated
4. **List Appointments**: Fetches filtered appointments from backend
5. **View Details**: Click card to view full appointment info
6. **Edit Appointment**: Update details (not same-day)
7. **Delete Appointment**: Remove from schedule
8. **Logout**: Clear token and data, return to home

## 🚨 Error Handling

- Invalid credentials: "Invalid credentials" message
- Duplicate username: "Username already exists"
- Failed appointment creation: "Failed ❌"
- Authorization errors: Return 403 Forbidden
- Not found errors: Return 404 Not Found

## 📈 Future Enhancements

- Email notifications for appointments
- Payment processing integration
- Admin dashboard for all appointments
- SMS reminders
- Appointment history and analytics
- Multiple groomer support
- Service add-ons (bath, nail trim, etc.)

## 🤝 Contributing

This is a personal project for learning purposes. Feel free to fork and customize!

## 📄 License

MIT License - Free to use and modify

## 👤 Author

Created by Michaela - Database Project Management System

## 🔗 Repository

https://github.com/michalkiv25/crm-dog-grooming-queue

---

**Last Updated**: May 1, 2026
