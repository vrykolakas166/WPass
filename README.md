# WPF Application: WPass

## Overview

This application is designed to securely store user credentials and automatically fill them on websites. The application ensures that all sensitive information is encrypted to maintain user privacy and security.

## Features

- **Auto-fill Functionality:** Automatically fill credentials on websites using the stored information.
- **User-friendly Interface:** Simple and intuitive user interface for managing credentials.
- **Encryption:** Industry-standard encryption to secure sensitive data.

## Installation

1. **Clone the repository:**

    ```bash
    git clone https://github.com/vrykolakas166/WPass.git
    ```

2. **Open the solution in Visual Studio:**

   Navigate to the cloned directory and open the solution file (`.sln`) in Visual Studio.

3. **Build the project:**

   - Build the project to restore dependencies and compile the application.
   - Set WPass.Core as startup project
   - Open Package Manager Console and run
    ```bash
    Update-Database
    ```

5. **Run the application:**

   Once the build is successful, database is created, you're free to go.

## Usage

1. **Adding Credentials:**
   - Click "New" button to add an entry.
   - (Or you can import your data from Edge or Chrome (.csv) by click File -> Import)
   - Enter the website URL, username, and password.
   - Click "Save" to securely store the credentials.

2. **Autofilling Credentials:**
   - Open the desired website in your browser.
   - Using hotkeys to autofill your credentials.

3. **Managing Stored Credentials:**
   - View, edit, or delete stored credentials.

## Security

- **Encryption:** All credentials are encrypted using AES (Advanced Encryption Standard) before being stored locally.

## Requirements

- **.NET Core 8**
- **Visual Studio 2022 or later**
