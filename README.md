# Hotel Listing API

### REST Web API built using ASP.Net Core 6.0 . It returns list of hoteşls and countries have that hotels. It also have JWT Tokens, ASP.NET Core Identity for user authentication and ip rate limiting for security. 

<br>

## How To Run
---

Download the project from github or clone the project using Git. Open project with Visual Studio 2022.
<br/>
For first time opening project, go to Package Manager Console select HotelListings.Data as default project then run this command
    
    update-database

After that you can use Visual studio to run project or open cmd cd into HotelListing then run this command
    
    dotnet watch run

>Use swagger UI or Postman to do requests. **Root path is https://localhost:7299/api**

<br>

## How To Use
---
**You can use get functions without registering**

 <br>

### 1. Get Hotel List
     GET [root]/hotel
>You can get page number and how many records per page (max 50 records) 


    GET [root]/Hotel?PageNumber=1&PageSize=10
 >Ex. 10 records for page 1
 
 <br>

### 2. Get Hotel By ID
    Get [root]/hotel/{id}

<br>

### 3. Get Country List
    GET [root]/Country

> You can also get specific page and limit records per page just like **hotel** endpoint  

<br>

### 4. Get Country By ID
    Get [root]/Country/{id}

<br>

**You have to register as user to use Put and Delete endpoints**

 ### 5. Registeration

> Use template below to register as **user** or **admin**. Only **email, password, and roles** parts are **mandatory**

    POST [root]/Account/register
    
>Request body (Json) 

```json
{
  "email": "user@example.com",
  "password": "string",
  "firstName": "string",
  "lastName": "string",
  "phoneNumber": "string",
  "roles": [
    "string"
  ]
}
```


<br>

### 6. Login

    POST [root]/Account/login

```Json
{
  "email": "user@example.com",
  "password": "string"
}
```

> You will get access and refresh token as reponse, use tokens to post, put and delete hotels and countries. You need to logged as user to make put and delete operations however only admins can post new hotel or country.

<br>

### 7. Get Refresh Tokens
    POST [root]/Account/refreshtoken

Request body (Json)

```json
{
  "token": "string",
  "refreshToken": "string"
}
```

> Use this function to extend your login time

<br>

### 8. Post Hotel

    POST [root]/hotel

Request body

```json
{
  "name": "string",
  "address": "string",
  "rating": 5,
  "countryId": 0
}
```

<br>

### 9. Post Country

    POST [root]/country

Request body

``` json
{
  "name": "string",
  "shortName": "st"
}
```
<br>

### 10. Update Country

    PUT [root]/country/{id}

Request body

```json
{
  "name": "string",
  "shortName": "st",
  "id": 0,
  "hotels": [
    {
      "name": "string",
      "address": "string",
      "rating": 5,
      "countryId": 0,
      "id": 0,
      "country": "string"
    }
  ]
```

<br>

### 11. Update Hotel

    PUT [root]/hotel

Request body

```json
{
  "name": "string",
  "address": "string",
  "rating": 5,
  "countryId": 0
}
```

<br>

### 12. Delete Country
    DELETE [root]/country/{id}

<br>

### 13. Delete Hotel
    DELETE [root]/hotel/{id}
