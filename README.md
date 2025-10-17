# Train Dashboard

A real-time UK railway departure board console application powered by the National Rail Darwin API (OpenLDBWS).

## Features

- Real-time departure board display for UK railway stations
- Station search by name or CRS code
- Optional destination filtering
- Live service status updates (on time, delayed, cancelled)
- Platform information and expected departure times
- Color-coded status display for easy scanning
- Comprehensive station database with fuzzy search
- Service details including operator and service type
- Delay and cancellation reason display
- Support for both departures and arrivals


## Quick Start

### Prerequisites

- .NET 9.0 SDK or later
- Darwin API Token from National Rail

### Getting an API Token

1. Visit the National Rail OpenLDBWS registration page: https://www.nationalrail.co.uk/developers/
2. Click on "Register" or "Request Access Token"
3. Fill out the registration form with your details
4. Accept the terms and conditions
5. You will receive your API token via email (usually within minutes)
6. Keep your token secure and never share it publicly

### Setup

1. Clone the repository:
   ```bash
   git clone https://github.com/Haru-Tachibana/UK-Train-Timetable.git
   cd UK-Train-Timetable
   ```

2. Configure your API token:
   - Create a `.env` file in the project root directory
   - Add your API token:
     ```
     DARWIN_API_TOKEN=your_actual_token_here
     ```
   - Important: Never commit your `.env` file to version control

3. Restore dependencies:
   ```bash
   dotnet restore
   ```

### Running the Application

Build and run with:
```bash
dotnet run
```

Or build first, then execute:
```bash
dotnet build
./bin/Debug/net9.0/TrainDashboard
```

For production deployment:
```bash
dotnet publish -c Release
./bin/Release/net9.0/publish/TrainDashboard
```

### How to Use

1. Start the application using one of the methods above

2. Select a departure station:
   - Enter a station name (e.g., "Paddington", "Kings Cross")
   - Enter a 3-letter CRS code (e.g., "PAD", "KGX")
   - Type "list" to browse all available stations

3. Optional destination filtering:
   - Choose whether to filter by a specific destination (y/n)
   - If yes, select the destination station using the same methods

4. View the live departure board showing:
   - Destination
   - Scheduled departure time
   - Expected departure time
   - Platform number
   - Service status
   - Train operator

5. Continue using the application:
   - Press Enter to make another search
   - Type "exit" to quit


## Technical Details

### Technologies Used

- .NET 9.0 runtime
- System.ServiceModel for SOAP client communication
- DotNetEnv for environment variable management
- Async/await pattern for responsive operations

### Dependencies

- DotNetEnv (v3.1.1)
- System.ServiceModel.Http (v8.0.0)
- System.ServiceModel.Primitives (v8.1.2)
- System.ServiceModel.NetTcp (v8.x)

### Project Structure

```
TrainDashboard/
├── Program.cs                # Main application and UI
├── TrainDashboard.csproj     # Project configuration
├── .env                      # API token configuration (not in git)
├── DarwinService/
│   ├── Reference.cs          # SOAP client auto-generated code
│   └── dotnet-svcutil.params.json
└── Services/
    ├── StationLookup.cs      # Station database and search
    ├── StationBoard.cs       # Data models
    └── TrainService.cs       # API integration layer
```

### Key Components

- Program.cs: Console UI and user interaction flow
- StationLookup: Station name to CRS code mapping with fuzzy search
- TrainService: High-level wrapper for Darwin API operations
- StationBoard: Internal data models for departure information
- DarwinService: Auto-generated SOAP client for Darwin API

## Display Information

The departure board shows:

| Column | Description |
|--------|-------------|
| Destination | Final destination(s) of the train |
| Sch | Scheduled departure time |
| Exp | Expected departure time |
| Plat | Platform number (TBC if not yet confirmed) |
| Status | Service status (On time/Delayed/CANCELLED) |
| Operator | Train operating company |

### Status Color Coding

- **White**: On time services
- **Yellow**: Delayed services
- **Red**: Cancelled services

## Troubleshooting

### "DARWIN_API_TOKEN not found in environment variables"

- Ensure you have created a `.env` file in the project root
- Verify the file contains: `DARWIN_API_TOKEN=your_token_here`
- Check that you have replaced the placeholder with your actual token

### "Unable to fetch departure information"

- Verify your internet connection is active
- Confirm your API token is valid and not expired
- Ensure the station CRS code exists
- Check if the Darwin API service is operational

### "No stations found"

- Try using a partial station name instead of the full name
- Type "list" to see all available stations
- Use the 3-letter CRS code directly if known
- Check for typos in the station name

### Build or Runtime Errors

Clean and rebuild the project:
```bash
dotnet clean
dotnet restore
dotnet build
```

Verify .NET SDK version:
```bash
dotnet --version
```

## API Documentation

For more information about the Darwin API:
- National Rail Enquiries OpenLDBWS: https://www.nationalrail.co.uk/developers/
- Darwin Web Service Guide: https://lite.realtime.nationalrail.co.uk/OpenLDBWS/

## License

This project is for personal use with the National Rail Darwin API.