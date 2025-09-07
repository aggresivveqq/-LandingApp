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

## ⚠️ Project Status

This project is **no longer maintained**.  
No further updates or commits will be made.  
Feel free to use the code as-is for personal or educational purposes.
### Btw this is my first overall project :D

