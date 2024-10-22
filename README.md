
# DropStockAPI

DropStockAPI is a RESTful API built with .NET 8 for managing products, orders, and categories in an inventory management system. It includes authentication, authorization, and image management capabilities using Cloudinary.

## Table of Contents

- [Features](#features)
- [Technologies Used](#technologies-used)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Installation](#installation)
- [Configuration](#configuration)
- [Running the API](#running-the-api)
- [API Endpoints](#api-endpoints)
- [Testing](#testing)
- [Contributing](#contributing)
- [License](#license)

## Features

- CRUD operations for products, orders, and categories.
- User authentication and authorization using JWT.
- Role management for Admin, Manager, and User.
- Integration with Cloudinary for image upload and management.
- Postgres database integration.

## Technologies Used

- **.NET 8**: Core framework used to build the API.
- **Entity Framework Core**: For database interaction.
- **PostgreSQL**: Database for storing application data.
- **Cloudinary**: Image hosting and management.
- **Identity**: For user management and authentication.
- **Swagger**: API documentation and testing.

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) installed on your machine.
- [PostgreSQL](https://www.postgresql.org/download/) installed and running.
- An IDE like [Visual Studio](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/).
- [Cloudinary Account](https://cloudinary.com/) for image management.

### Installation

1. **Clone the repository**:

   ```bash
   git clone https://github.com/yourusername/DropStockAPI.git
   cd DropStockAPI
   ```

2. **Set up the database**:

   Make sure your PostgreSQL server is running and update the connection string in `appsettings.json` if needed.

3. **Install dependencies**:

   ```bash
   dotnet restore
   ```

4. **Apply migrations**:

   Run the following command to apply migrations and set up the database:

   ```bash
   dotnet ef database update
   ```

## Configuration

Update the `appsettings.json` file with your specific settings:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=dotnetdbapi;Username=postgres;Password=123456"
  },
  "JwtSettings": {
    "ValidIssuer": "DropStockAPI",
    "ValidAudience": "DropStockWebApp",
    "SecurityKey": "your_secret_key",
    "ExpiryInMinutes": 200
  },
  "Cloudinary": {
    "CloudName": "your_cloud_name",
    "ApiKey": "your_api_key",
    "ApiSecret": "your_api_secret"
  }
}
```

- **ConnectionStrings**: Update with your PostgreSQL connection details.
- **JwtSettings**: Set the issuer, audience, and security key for JWT authentication.
- **Cloudinary**: Add your Cloudinary credentials for image management.

## Running the API

1. **Run the application**:

   Start the API using the command below:

   ```bash
   dotnet run
   ```

2. **Access the API**:

   The API will be running at `https://localhost:7196`. You can access the Swagger UI for testing endpoints at `https://localhost:7196/swagger`.

## API Endpoints

### Authentication

- **Register User**: `POST /api/authenticate/register-user`
- **Register Manager**: `POST /api/authenticate/register-manager`
- **Register Admin**: `POST /api/authenticate/register-admin`
- **Login**: `POST /api/authenticate/login`
- **Logout**: `POST /api/authenticate/logout`

### Products

- **Get Products**: `GET /api/product`
- **Get Product by ID**: `GET /api/product/{id}`
- **Create Product**: `POST /api/product`
- **Update Product**: `PUT /api/product/{id}`
- **Delete Product**: `DELETE /api/product/{id}`

### Orders

- **Get Orders**: `GET /api/order`
- **Get Order by ID**: `GET /api/order/{id}`
- **Create Order**: `POST /api/order`
- **Update Order**: `PUT /api/order/{id}`
- **Delete Order**: `DELETE /api/order/{id}`

### Categories

- **Get Categories**: `GET /api/category`
- **Create Category**: `POST /api/category`
- **Update Category**: `PUT /api/category/{id}`
- **Delete Category**: `DELETE /api/category/{id}`

## Testing

- You can use tools like [Postman](https://www.postman.com/) or Swagger UI to test the API endpoints.
- Ensure you use the correct JWT token when accessing protected endpoints.

## Contributing

Contributions are welcome! Please fork this repository and submit a pull request for any feature additions or bug fixes.

## License

This project is licensed under the MIT License.
