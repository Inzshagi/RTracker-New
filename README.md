
# RTracker - Task Management System

RTracker is a task management software system designed to help users create, manage, and track their tasks efficiently. 

The system features secure user authentication, task creation, and management functionality, all backed by an SQLite database for data storage.

## Project Overview

RTracker allows users to:
- **Create accounts**: Users can register with an email, username, and password.
- **Log in**: Users can securely log into their accounts.
- **Manage tasks**: Users can add, edit, delete, and view their tasks with the option to filter tasks based on title.
- **Due date notifications**: The system alerts users when a task's due date is approaching.
- **Multi-user support**: Each user’s tasks are associated with their unique user ID.

## Features
- **Password Hashing**: Passwords are hashed using BCrypt for secure storage.
- **SQLite Database**: User and task data are stored in an SQLite database.
- **Search Functionality**: Users can filter tasks by title.
- **Due Date Alert**: Users are notified when tasks are approaching their due date.

## Setup Instructions

### Prerequisites
Before setting up RTracker, ensure that the following are installed on your system:
- Visual Studio (with .NET Framework)
- SQLite (SQLite database and .NET provider)
- BCrypt.Net (for password hashing)

### Steps to Set Up
1. Get the repository link: `https://github.com/Inzshagi/RTracker-New.git`
2. Open Visual Studio and create an empty folder where the project will be cloned.
3. Use the **Git** menu in Visual Studio to clone the repository:
   - Click on **Git** -> **Clone Repository**.
   - Paste the repository link and select the empty folder you created as the destination.
4. Once cloned, install the required dependencies via the **NuGet Package Manager Console**:
   - `BCrypt.Net-Next`
   - `System.Data.SQLite`
5. Build and run the application by pressing **F5**.

## Usage

### Registration
1. Launch the application and navigate to the registration page.
2. Enter a valid email, username, and password.
3. Click **Register** to create a new account.

### Login
1. After registering, click **Login**.
2. Enter your credentials (username and password).
3. Once logged in, you will be directed to the task management form.

### Task Management
- **Add Task**: Enter a task title, description, status, and due date, then click **Add**.
- **Edit Task**: Select a task and make edits, then click **Save**.
- **Delete Task**: Select a task and click **Delete**.
- **Search Tasks**: Use the search bar to filter tasks by title.

### Logout
- Click **Logout** to exit the application and return to the registration form.

## Contact
For any questions or feedback, feel free to reach out:
- Email: zyronbioalzate31@gmail.com

You can also feel free to report any issues or suggest enhancements through the repository's **Issues** section.
