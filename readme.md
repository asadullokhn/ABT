﻿

# ABT Package - Build your application much faster
[![preview version](https://img.shields.io/nuget/vpre/ABT)](https://www.nuget.org/packages/ABT/absoluteLatest)

## Installation 
You can get ABT [Nuget](https://www.nuget.org/packages/ABT/) package by typing:
```powershell
dotnet add package ABT --version 5.0.1
```


## About
This package will help you quickly build the foundation of your application. It is enough to take an object and use the generic method - Create<>, which accepts at least two types: The first -> type (entity) to save in the database. The second is DTO

### Sample:
```csharp
var connectionString = "connection string";
var builder = new ArchitectureBuilder(ArchitectureBuilder.EfContext.Npgsql, connectionString);
builder.Create<Book, BookForCreationDto>();
builder.Create<User, UserForCreationDto, UserViewModel>();
// and so on...
```

<br>

If you have any suggestions, comments or questions, please feel free to contact me on:
<br>
GitHub: @asadullokhn
<br>
LinkedIn: @asadullokhn
<br>
Telegram: @asadullokhn
<br>
E-Mail: asadullokhnurullaev@gmail.com


Hope you will like this ❤️