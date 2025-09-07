#  LandingApp

LandingApp is a multifunctional landing page built with C# (ASP.NET Core) for the backend and JavaScript for the frontend.

The application is designed to efficiently collect leads, integrate seamlessly with Google Sheets,chatting with user via AI and collect leads and display tariff information to users.
---

##  Features
- Lead collection - Users can submit their information through forms or via the integrated chat  
- Tariff display - Multiple tariff plans are displayed. Users can choose a plan, and pressing the "Connect" button will automatically scroll them to the lead submission section.
- Google Sheets Export – Leads collected via forms or chat are exported directly to Google Sheets for easy tracking and analysis
- AI Chat Integration – The landing page includes a smart chat system that interacts with visitors, collects leads, and provides an engaging experience
- Multilanguage support (RU/KZ)  - Supports Russian and Kazakh languages, ensuring accessibility for a wider audience.
- Responsive Design – Fully responsive layout using Bootstrap, optimized for both desktop and mobile devices
- SEO Optimized – The landing page has structured HTML, meta tags, and optimized content to improve search engine visibility.
---

## 🛠 Technologies
| Backend        | Frontend               | Database        | Integrations          |
| -------------- | --------------------- | ---------------| -------------------- |
| C# ASP.NET Core| JavaScript, Bootstrap | MSSQL / EF Core| Google Sheets API, OpenRouter API, SeqLog |

---

## 📂 Project Structure
```text
LandingApp/
│
├─ Controllers/       # Controllers for chat, forms, logs, and pages
├─ Data/              # Database context
├─ Dto/               # Data Transfer Objects
├─ Helpers/           # Helper methods, session extensions
├─ Interfaces/        # Service and repository interfaces
├─ Logging/           # Logging actions
├─ Mapping/           # AutoMapper configuration
├─ Migrations/        # EF Core migrations
├─ Models/            # Data models (leads, tariffs, chat)
├─ Repository/        # Data repositories
├─ Scripts/           # Additional scripts 
├─ Services/          # Business logic and integrations
├─ Views/             # Razor Pages frontend
├─ wwwroot/           # Static files (JS, CSS, images)
├─ appsettings.json   # App configuration
├─ LandingApp.sln     # Visual Studio solution file
````


## Page Review
# RU
<img width="1899" height="892" alt="image" src="https://github.com/user-attachments/assets/41464d7b-e121-402e-b6f7-3827bab5e12b" />
Chat
<img width="1899" height="788" alt="image" src="https://github.com/user-attachments/assets/733c1950-8a8d-4157-bf61-6e0c1021d800" />
Tariff Section
<img width="1898" height="884" alt="image" src="https://github.com/user-attachments/assets/78db5f37-0a9c-4a4b-a28f-3b1906d0658d" />
Other Sections
<img width="916" height="821" alt="image" src="https://github.com/user-attachments/assets/0c1dca1f-3a3b-4b2a-bf56-005a9251bb33" />
<img width="630" height="602" alt="image" src="https://github.com/user-attachments/assets/49c3d7bc-3517-42d1-bccc-3fff3f1dfbb8" />
Aggreement page
<img width="1914" height="878" alt="image" src="https://github.com/user-attachments/assets/1d783e3c-0864-45ea-b671-784faf24dbce" />
Privacy page
<img width="1897" height="886" alt="image" src="https://github.com/user-attachments/assets/114b2bb7-9e5c-47f1-844a-882d6a8cee39" />

## Mobile version (RU)
<img width="379" height="781" alt="image" src="https://github.com/user-attachments/assets/34759105-4b08-49fc-9e58-e191129ad7f1" />
<img width="374" height="790" alt="image" src="https://github.com/user-attachments/assets/7e7aec97-79dd-4025-bac2-9a6b0baf274f" />
<img width="373" height="783" alt="image" src="https://github.com/user-attachments/assets/6eb56f11-c976-4b05-901c-0f6a6924af73" />
<img width="376" height="786" alt="image" src="https://github.com/user-attachments/assets/5ea93bdb-73c7-42bb-8874-0930f76f6d42" />
<img width="374" height="792" alt="image" src="https://github.com/user-attachments/assets/cee3e73d-41cc-4899-a01f-7a22d2702c90" />
<img width="374" height="782" alt="image" src="https://github.com/user-attachments/assets/cfdfe54c-69e6-4b88-bc44-d907b52f940d" />


# KZ
<img width="1898" height="887" alt="image" src="https://github.com/user-attachments/assets/cfab9f54-b5d5-4756-832e-d1200157c77d" />
Chat
<img width="1896" height="793" alt="image" src="https://github.com/user-attachments/assets/88413e8e-eaff-468c-8931-2833afe30a3e" />
Tariff section
<img width="1902" height="887" alt="image" src="https://github.com/user-attachments/assets/ff8cd882-7db7-421b-85ee-b01c999fda6f" />
Other sections
<img width="720" height="823" alt="image" src="https://github.com/user-attachments/assets/a6a1988b-4900-47c1-8cfe-a8ea29168486" />
<img width="583" height="825" alt="image" src="https://github.com/user-attachments/assets/8b4b498b-591a-4344-a0f3-1ee850fe0d2d" />


Aggreement page
<img width="1908" height="689" alt="image" src="https://github.com/user-attachments/assets/17186731-e92b-4d4e-be28-66a405365271" />
Privacy page
<img width="1898" height="891" alt="image" src="https://github.com/user-attachments/assets/5760ff72-b594-450b-991e-37cc05d4f7fe" />


## Mobile Version (KZ)
<img width="383" height="783" alt="image" src="https://github.com/user-attachments/assets/608b9a0a-82af-4eb3-ad82-63a38f324891" />
<img width="381" height="786" alt="image" src="https://github.com/user-attachments/assets/6148bcf9-3935-43a2-bfa1-fadafb774b3f" />
<img width="376" height="790" alt="image" src="https://github.com/user-attachments/assets/c4f14608-a83c-4616-b798-77a89f4764a1" />
<img width="374" height="784" alt="image" src="https://github.com/user-attachments/assets/e2f4f8ff-d8af-442b-accd-6845278a280c" />
<img width="369" height="787" alt="image" src="https://github.com/user-attachments/assets/f755ea1a-7bc7-4182-836b-1a4005e3e475" />
<img width="372" height="790" alt="image" src="https://github.com/user-attachments/assets/4425d238-d5a3-46bd-a688-54a144990a6b" />


# If you want to see more detailed version,follow this instructions
## ⚡ Setup & Run

# Clone the repository
```bash
git clone https://github.com/aggresivveqq/-LandingApp
```

# Restore dependencies and build the project
```bash
dotnet restore
dotnet build
```

# Configure sensitive files

google-credentials.json (do not commit secrets)

appsettings.json (do not commit secrets)

# Configure database connection string

# Apply database migrations
```bash

dotnet ef database update

```
# Run the application
```bash
dotnet run
```
Open in your browser

http://localhost:PORT

## 🔒 Security

### Do not commit secrets (API keys, credentials) to GitHub.

### Exclude bin/, obj/, publish/, google-credentials.json ,appsettings.json and etc. via .gitignore.

### Use environment variables or GitHub Secrets for sensitive data.

## 📫 Contact

#### Developer: Chingiskhan Akhmetov
#### GitHub: https://github.com/aggresivveqq
#### Telegram: https://t.me/ezzz1y
## ⚠️ Project Status

This project is **no longer maintained**.  
No further updates or commits will be made.  
Feel free to use the code as-is for personal or educational purposes.


