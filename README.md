# Task Management System

## Setup
```
dotnet new webapi --name TaskManagementSystem --output .
```

```
dotnet tool install --global dotnet-ef
```


## Issues
1. Task class collides with .net Task. Recommended to rename in order to avoid confusion.

## Comments
1. Used `JsonConverter` in order to return/send Enum string instead of int value.
2. Used Task<IActionResult> as return type for GET endpoint in order to have better control of HTTP response type.
3. Used `ProducesResponseType` in order to get better response description in Swagger UI.
4. Used dotnet new gitignore
5. Added `MaxLength` attributes for Task props in order to avoid VARCHAR(MAX) in generated tabele.
6. 