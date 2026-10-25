# 🚗 Mercedes-Benz Car Store

A fully-featured desktop application built with **WPF (C#)** and **SQL Server** for managing a Mercedes-Benz car dealership. The system supports two roles — Customer and Admin — each with their own dashboard and features.

---

## 📋 Features

### 👤 Customer
- Browse available cars with search, fuel filter, and sort options
- Add cars to cart and manage cart items
- Checkout and purchase with receipt generation
- View order history and download receipts
- Submit queries to admin and view replies

### 🛠️ Admin
- View and manage all customer orders
- Manage car inventory (add, update status, remove cars)
- Update car status (Available / Needs Repair / Needs Restock) with notes
- View sales reports, inventory stats, and customer activity

---

## 🛠️ Tech Stack

| Layer | Technology |
|---|---|
| Language | C# (.NET) |
| UI Framework | WPF (Windows Presentation Foundation) |
| Database | SQL Server |
| IDE | Visual Studio 2022 |
| Reports | Custom Receipt Generator (PDF) |

---

## 🗂️ Project Structure
Car/
├── Models/ # Data models (Car, User, Order, Query)
├── Views/ # WPF Windows and Pages
│ ├── MainWindow.xaml
│ ├── CustomerDashboard.xaml
│ ├── AdminDashboard.xaml
│ ├── CheckoutWindow.xaml
│ └── AddCarWindow.xaml
├── Database/ # Repository classes for DB access
│ ├── CarRepository.cs
│ ├── OrderRepository.cs
│ ├── CarInventoryRepository.cs
│ └── ...
└── Utilities/ # ReceiptGenerator and helpers

---

## ⚙️ Setup Instructions

### Prerequisites
- Visual Studio 2022
- .NET 6 or later
- SQL Server / SQL Server Express
- SQL Server Management Studio (SSMS)

### Steps

1. **Clone the repository**
   ```bash
   git clone https://github.com/YOUR_USERNAME/mercedes-benz-car-store.git
   ```

2. **Set up the database**
   - Open SSMS and run the provided SQL script (`/Database/schema.sql`) to create tables
   - Update the connection string in `Database/DBConnection.cs` (or your config file) to match your SQL Server instance

3. **Open in Visual Studio**
   - Open `Car.sln`
   - Restore NuGet packages if prompted
   - Build the solution (`Ctrl+Shift+B`)

4. **Run the application**
   - Press `F5` or click **Start**
   - Log in as Admin or Customer using seeded credentials

---

## 🖼️ Screenshots

> Add screenshots of your application here

| Customer Dashboard | Admin Dashboard |
|---|---|
| _(screenshot)_ | _(screenshot)_ |

---

## 👩‍💻 Developer

**Aqsa**
BS Software Engineering — 3rd Year
