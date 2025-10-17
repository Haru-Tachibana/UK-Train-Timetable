# Train Dashboard 🚂

A real-time UK railway departure board console application powered by the National Rail Darwin API (OpenLDBWS).

## Features


## Quick Start

### Prerequisites

1. **.NET 9.0 SDK**: Make sure you have .NET 9.0 or later installed
2. **Darwin API Token**: Get a free API token from [National Rail OpenLDBWS](https://www.nationalrail.co.uk/developers/)

### Setup

1. **Clone the repository**:
   ```bash
   git clone https://github.com/Haru-Tachibana/UK-Train-Timetable.git
   cd UK-Train-Timetable
   ```

2. **Configure your API token**:
   - Copy `.env.example` to `.env`:
     ```bash
     cp .env.example .env
     ```
   - Open `.env` and replace `your_api_token_here` with your actual Darwin API token:
     ```
     DARWIN_API_TOKEN=your_actual_token_here
     ```
   - **Important**: Never commit your `.env` file to version control (it's already in `.gitignore`)

3. **Restore dependencies**:
   ```bash
   dotnet restore
   ```

### Running the Application

```bash
dotnet run
```

Or directly execute the compiled binary:

```bash
./bin/Debug/net9.0/TrainDashboard
```

### How to Use

1. **Select Departure Station**:
   - Enter a station name (e.g., "Paddington", "Kings Cross")
   - Enter a 3-letter CRS code (e.g., "PAD", "KGX")
   - Type "list" to browse all available stations

2. **Optional Destination Filter**:
   - Choose whether to filter by a specific destination
   - If yes, select the destination station the same way

3. **View Live Departures**:
   - See real-time departure information including:
     - Destination
     - Scheduled departure time
     - Expected departure time
     - Platform number
     - Service status (On time/Delayed/Cancelled)
     - Train operator

4. **Repeat or Exit**:
   - Press Enter to search again
   - Type "exit" to quit the application

## Station Examples

The app includes over 100 major UK railway stations:

### London Stations
- Paddington (PAD)
- King's Cross (KGX)
- Euston (EUS)
- Victoria (VIC)
- Waterloo (WAT)
- Liverpool Street (LST)
- And more...

### Major Cities
- Birmingham New Street (BHM)
- Manchester Piccadilly (MAN)
- Edinburgh (EDB)
- Glasgow Central (GLC)
- Leeds (LDS)
- Liverpool Lime Street (LIV)
- And many more...

## API Configuration

The application uses the National Rail Darwin API with the following credentials:

- **Service**: OpenLDBWS (Live Departure Boards Web Service)
- **Token**: ed7cf477-14b5-4066-8cdc-df9b8aa69813
- **Username**: haru
- **Email**: paloma030415@gmail.com

The token is configured in `Program.cs`. If you need to change it, update the `ApiToken` constant.

## Technical Details

### Technologies Used
- **.NET 9.0**: Latest .NET runtime
- **System.ServiceModel**: SOAP client for Darwin API communication
- **Async/Await**: Asynchronous programming for responsive UI

### Project Structure

```
TrainDashboard/
├── Program.cs                    # Main application entry point
├── DarwinService/
│   └── DarwinClient.cs          # SOAP client and data models
└── Services/
    ├── StationLookup.cs         # Station database and search
    └── TrainService.cs          # API integration layer
```

### Key Classes

- **DarwinClient**: SOAP service client for Darwin API
- **StationLookup**: Station name to CRS code mapping and search
- **TrainService**: High-level API wrapper with business logic
- **Program**: Console UI and user interaction

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

### "Unable to fetch departure information"

- Check your internet connection
- Verify the API token is still valid
- Ensure the station CRS code is correct

### "No stations found"

- Try using a partial station name
- Type "list" to see all available stations
- Use the 3-letter CRS code directly

### Build Errors

```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

## API Documentation

For more information about the Darwin API:
- [National Rail Enquiries OpenLDBWS](https://www.nationalrail.co.uk/developers/)
- [Darwin Web Service Guide](https://lite.realtime.nationalrail.co.uk/OpenLDBWS/)

## License

This project is for personal use with the National Rail Darwin API.

---

**Made with ❤️ for UK railway passengers**
