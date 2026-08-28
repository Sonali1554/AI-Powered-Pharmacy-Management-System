# 💊 AI-Powered Pharmacy Management System

### A Smart Web-Based Solution for Medicine, Inventory & Pharmacy Operations

Deployes link
https://ai-powered-pharmacy-management-system.onrender.com/

## 📌 Overview

The **AI-Powered Pharmacy Management System** is a web-based application developed using **ASP.NET Core MVC and C#** to simplify and organize pharmacy operations.

The system provides a centralized platform for managing medicines, monitoring inventory and stock levels, handling user authentication, and viewing important pharmacy-related information through a structured dashboard.

The application follows the **Model-View-Controller (MVC)** architecture, making the system modular, maintainable, and scalable.

---

## 🔄 System Workflow

The overall workflow of the Pharmacy Management System is:

```text
                    ┌──────────────────┐
                    │      User        │
                    └────────┬─────────┘
                             │
                             ▼
                    ┌──────────────────┐
                    │   Authentication │
                    │   & User Login   │
                    └────────┬─────────┘
                             │
                             ▼
                    ┌──────────────────┐
                    │    Dashboard     │
                    └────────┬─────────┘
                             │
              ┌──────────────┼──────────────┐
              │              │              │
              ▼              ▼              ▼
       ┌────────────┐ ┌────────────┐ ┌─────────────┐
       │  Medicine  │ │ Inventory  │ │  Analytics  │
       │ Management │ │ Management │ │  Dashboard  │
       └──────┬─────┘ └──────┬─────┘ └─────────────┘
              │              │
              └──────┬───────┘
                     ▼
             ┌─────────────────┐
             │   Application   │
             │     Logic       │
             └────────┬────────┘
                      │
                      ▼
             ┌─────────────────┐
             │     Database    │
             │    SQL Server   │
             └─────────────────┘
```

### Workflow Explanation

1. **User Authentication**

   * Users access the system through the authentication module.
   * Identity and authorization are used to manage secure access.

2. **Dashboard**

   * After login, the user can access the main dashboard.
   * Important pharmacy and inventory information can be viewed from a centralized location.

3. **Medicine Management**

   * Users can add, view, update, and manage medicine records.
   * Medicine-related information is stored in the database.

4. **Inventory Management**

   * Stock quantities are maintained and monitored.
   * The system helps identify medicines with low stock levels.

5. **Analytics**

   * Pharmacy and inventory information can be presented through the dashboard.
   * This helps users understand the current state of the pharmacy.

6. **Database**

   * Application data is stored and managed using the database layer.
   * Entity Framework Core is used for database interaction.

---

## ✨ Key Features

### 💊 Medicine Management

* Add new medicines
* Update medicine information
* View medicine details
* Search and manage medicine records

### 📦 Inventory & Stock Management

* Track medicine stock
* Monitor available quantities
* Manage inventory records
* Identify low-stock medicines

### 📊 Dashboard & Analytics

* Centralized dashboard
* Display important pharmacy information
* Monitor inventory-related statistics
* Provide an overview of system data

### 🔐 Authentication & User Management

* User authentication
* Secure login
* Identity-based access management
* Authorization support

### 🗄️ Database Management

* Centralized data storage
* Entity Framework Core integration
* Structured database operations

---

## 🛠️ Technology Stack

### Backend

* **C#**
* **ASP.NET Core MVC**
* **Entity Framework Core**

### Frontend

* **HTML5**
* **CSS3**
* **JavaScript**
* **Bootstrap**

### Database

* **SQL Server**

### Tools

* **Visual Studio / Visual Studio Code**
* **Git**
* **GitHub**

---

## 🏗️ Project Architecture

The project follows the **MVC architecture**:

```text
User
  │
  ▼
Views ───────► Controllers
                 │
                 ▼
               Models
                 │
                 ▼
              Database
```

### Main Components

* **Models** – Define application data and entities.
* **Views** – Provide the user interface.
* **Controllers** – Handle user requests and application logic.
* **Data** – Contains database-related configuration and context.
* **Areas/Identity** – Handles authentication and identity-related functionality.
* **wwwroot** – Contains static files such as CSS, JavaScript, and images.

---

## 📂 Project Structure

```text
PharmacyManagementSystem/
│
├── Areas/
│   └── Identity/
│       └── Pages/
│
├── Controllers/
│
├── Data/
│
├── Models/
│
├── Properties/
│
├── Views/
│
├── wwwroot/
│   ├── css/
│   ├── js/
│   └── images/
│
├── PharmacyManagmentSystem.csproj
├── PharmacyManagmentSystem.slnx
└── README.md
```

---

## 🚀 How to Run the Project

### Prerequisites

Install the following:

* .NET SDK
* SQL Server
* Visual Studio or Visual Studio Code
* Git

### 1. Clone the Repository

```bash
git clone https://github.com/Sonali1554/AI-Powered-Pharmacy-Management-System.git
```

### 2. Navigate to the Project

```bash
cd AI-Powered-Pharmacy-Management-System
```

### 3. Restore Dependencies

```bash
dotnet restore
```

### 4. Configure Database

Update the database connection string in:

```text
appsettings.json
```

according to your local SQL Server configuration.

### 5. Apply Database Migration

If Entity Framework migrations are configured:

```bash
dotnet ef database update
```

### 6. Run the Application

```bash
dotnet run
```

Open the URL displayed in the terminal in your browser.

---

## 📊 Main Modules

| Module                  | Purpose                                      |
| ----------------------- | -------------------------------------------- |
| 🔐 Authentication       | Secure user login and access                 |
| 🏠 Dashboard            | Centralized overview of pharmacy information |
| 💊 Medicine Management  | Manage medicine records                      |
| 📦 Inventory Management | Track medicine stock and quantities          |
| 📊 Analytics            | View useful pharmacy and inventory insights  |
| 🗄️ Database            | Store and manage application data            |

---

## 🔮 Future Enhancements

* AI-based medicine recommendations
* Automated medicine expiry notifications
* Low-stock alerts
* Sales and revenue forecasting
* Advanced analytics
* Prescription management
* Supplier management
* Invoice generation
* Role-based access control
* Cloud deployment

---

## 🎯 Learning Outcomes

This project provided hands-on experience with:

* ASP.NET Core MVC
* C# development
* CRUD operations
* Entity Framework Core
* SQL Server integration
* Authentication and authorization
* MVC architecture
* Inventory management
* Dashboard development
* Git and GitHub

---

## 📸 Screenshots
Home Screen:

<img width="1600" height="794" alt="image" src="https://github.com/user-attachments/assets/fc35a141-ecff-4687-9f9b-c0ce77416970" />

<img width="1600" height="900" alt="image" src="https://github.com/user-attachments/assets/eabbf816-dc2a-4e34-8e17-48c7b4bd6cef" />

Pharmacy Invoice:

<img width="1600" height="1000" alt="image" src="https://github.com/user-attachments/assets/45670716-0dec-49d5-8cbb-7f0c29c9d086" />

Medicine:
<img width="1600" height="804" alt="image" src="https://github.com/user-attachments/assets/93a14cb5-2c09-4ca5-b851-cca4f8b55e9c" />

Addition of Medicine:

<img width="1600" height="808" alt="image" src="https://github.com/user-attachments/assets/9f7cd059-95fd-4f53-953e-39e03d994495" />
<img width="1600" height="808" alt="image" src="https://github.com/user-attachments/assets/394e79bf-e5c3-4828-a676-bd9269285d55" />
<img width="1600" height="808" alt="image" src="https://github.com/user-attachments/assets/40fb2b84-6f8b-406f-84bf-0a8e5d4cae47" />

Inventory & Stock Management:

<img width="1017" height="522" alt="image" src="https://github.com/user-attachments/assets/c861d811-590e-407d-97d7-7a47195c20e3" />
<img width="1017" height="478" alt="image" src="https://github.com/user-attachments/assets/1f91035c-bc6d-4483-87fb-b52a98641378" />

Overall Output:

<img width="1054" height="1250" alt="image" src="https://github.com/user-attachments/assets/6b12e8db-d26e-4113-949e-17b5c1d0a3bf" />
<img width="1600" height="922" alt="image" src="https://github.com/user-attachments/assets/50821c56-7f16-43c7-ae42-5ce47a25370e" />
<img width="1600" height="484" alt="image" src="https://github.com/user-attachments/assets/065fe9fb-1822-4a29-8b6c-c02c753816c7" />
<img width="1600" height="490" alt="image" src="https://github.com/user-attachments/assets/1cdd24b6-e86d-42f5-b206-b385b4209a23" />
<img width="1600" height="751" alt="image" src="https://github.com/user-attachments/assets/524c4b65-3115-46e3-87bb-ae62244df0ff" />


---

⭐ If you find this project useful, consider giving the repository a star.
