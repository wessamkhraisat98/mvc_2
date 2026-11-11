# WebApplication1

ASP.NET Core MVC application for university cafeteria supply requests.

Requires .NET 10 and SQL Server. The default connection uses the local SQL Server
and creates the `WebApplication1Cafeteria` database on first run. Set
`ConnectionStrings__DefaultConnection` if your server or connection differs.

To create the first SuperAdmin, run these commands in PowerShell from the project
folder, replacing the sample values with your own:

```powershell
$env:SuperAdmin__Email = 'your-email@example.com'
$env:SuperAdmin__Password = 'Your-own-strong-password!'
dotnet run
```

Sign in with that account to manage users and assign roles. The configured
email must be unused when the first SuperAdmin is created. Public registration
always assigns the User role. After the first successful startup, stop the app
and clear the two bootstrap variables; the account stays in the database.

```powershell
Remove-Item Env:SuperAdmin__Email, Env:SuperAdmin__Password
dotnet run
```

