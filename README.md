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

3. **Config the project:**

   - In development mode: Create launchSettings.json in Properties in WPass project. Here is a sample:
    ```bash
    {
      "profiles": {
        "WPass": {
          "commandName": "Project",
          "environmentVariables": {
            "MY_ENCRYPTION_KEY": "123456",
            "MY_DATABASE_PASSWORD": "123456789"
          }
        }
      }
    }
    ```
   - If Production mode: Create en_key.dat and db_pwd.dat files using DpapiHelper.cs in WPass.Core project. Here's usage:
    ```cs
    DpapiHelper.SavePassword("123456", "en_key.dat");
    DpapiHelper.SavePassword("123456789", "db_pwd.dat");
    // There are several ways to create these files,
    // you can directly add above code to the start of App.cs and run debug for once,
    // then go to bin/debug folder and get those files.
    ```
   - REMEMBER to copy 2 created dat files to WPass project, then set their properties:
    ```Properties
    Build Action: Content
    Copy to Output Directory: Copy if newer (or Copy always)
    ```

5. **Run the application in Visual Studio:**

   Once the configuration process is finished, you're free to go.

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
