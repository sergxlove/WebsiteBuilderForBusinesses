# WebsiteBuilderForBusinesses

![Static Badge](https://img.shields.io/badge/language-C%23-red)
![Static Badge](https://img.shields.io/badge/powered_by-.NET_10-blue)
![Static Badge](https://img.shields.io/badge/platforms-Windows,Linux-purple)
![Static Badge](https://img.shields.io/badge/version-1.0-orange)
![Static Badge](https://img.shields.io/badge/developer-sergxlove-green)
![Static Badge](https://img.shields.io/badge/year-2026-green)

## About

WebsiteBuilderForBusinesses is an ASP.NET Core web application that provides an intuitive website builder with a block-based structure, drag-and-drop editor, and real-time preview. The platform allows users to quickly create landing pages and multi-page websites without programming knowledge using pre-built templates and a flexible component system, while supporting the export of pure HTML/CSS, publishing to their own hosting, and integrating with popular services through a simple and extensible interface.

## Architecture 

![image](https://github.com/sergxlove/WebsiteBuilderForBusinesses/blob/master/resources/ArchitectureWebBuilder.png)

## Preview 

![image](https://github.com/sergxlove/WebsiteBuilderForBusinesses/blob/master/resources/mainPage.png)
![image](https://github.com/sergxlove/WebsiteBuilderForBusinesses/blob/master/resources/projectsPage.png)

## Install 

The program requires Docker and .NET 10 runtime to run. 

1. Download the archive from the "Release" tab and extract it
2. Go to the unpacked folder using powershell, with the command cd ...\WebBuilder
3. Run the command in the directory: docker compose up -d
4. Run the command in the directory: Get-Content "backupWebbuilder.sql" | docker exec -i webbuilder-db psql -U postgres -d db
5. Open the WebsiteBuilderForBusinesses.API.exe file
6. Go to http://localhost:5001

To stop the program, close the .exe executable file and run the command: docker compose down

## Info 

### Pages 

- /page/login - login page, available for unauthenticated users

- /index - the main page of the website builder with all the functionality is available to authorized users with any role

- /page/reg - user registration page, available for authorized users with the administrator role

- /page/projects - a project list page available to authorized users with any role

- /page/admin - the administrator page with user management capabilities is available to authorized users with the administrator role

- /swagger - a page with information and the ability to test all endpoints, , available for unauthenticated users 

When errors 401, 403, 404 occur, there are graphical pages that display the error number and message

### Login information

By default, the system has 1 administrator user and 1 user user. Login details:

- Login: admin@mail.ru ; Password: admin123 ; Role: admin
- Login: user@mail.ru ; Password: user123 ; Role: user

### Logs 

Logging is done to the console and to seq. To open the seq page, go to http://localhost:5341 and enter your login details: Login: admin ; Password: M0xynmCS4rYePGni



