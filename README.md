# Bilderim Web API

A .NET Core Web API for the Bilderim betting and bulletin platform.

## Description

Bilderim is a web application that allows users to create and participate in betting bulletins. Users can create accounts, manage their profiles, create betting bulletins, and place bets on existing bulletins. The platform includes features like user authentication, bulletin management, and automated daily processing of bet results.

## Technologies Used

- .NET Core 3.1
- Entity Framework Core
- SQL Server
- Hangfire (for background jobs)
- BCrypt.NET (for password hashing)

## Features

- User Authentication and Authorization
  - Registration and Login
  - Token-based authentication
  - Password hashing with BCrypt
- User Profile Management
  - Profile updates
  - Photo upload
  - Balance management
- Bulletin System
  - Create and manage betting bulletins
  - Set betting rates and expiration dates
  - Bulletin confirmation workflow
- Betting System
  - Place bets on bulletins
  - Automatic daily processing of results
  - User balance updates based on bet results
- Background Jobs
  - Daily processing of expired bulletins
  - Automated result calculations

## Getting Started

### Prerequisites

- .NET Core SDK 3.1
- SQL Server
- Visual Studio 2019 or later (recommended)

### Installation

1. Clone the repository
2. Update the connection string in `appsettings.json` to point to your SQL Server instance
3. Run Entity Framework migrations:
   ```bash
   dotnet ef database update
   ```
4. Build and run the application:
   ```bash
   dotnet build
   dotnet run
   ```

### API Endpoints

- `/login` - User authentication
- `/register` - New user registration
- `/homepage` - Get active bulletins
- `/submitBet` - Place a bet
- `/profiledataupdate` - Update user profile
- Additional endpoints for bulletin management and user operations

## Configuration

The application can be configured through `appsettings.json`:
- Database connection string
- Logging settings
- Hangfire job scheduling
- HTTPS configuration

## Development

The project follows standard .NET Core Web API practices with:
- Controller-based routing
- Entity Framework Core for data access
- Model-View-Controller (MVC) architecture
- Background job processing with Hangfire

## Security

- Passwords are hashed using BCrypt
- Token-based authentication for API endpoints
- Secure user session management 