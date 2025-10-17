# Train Dashboard

A real-time UK railway departure board console application powered by the National Rail Darwin API (OpenLDBWS).

## Features

- AI-Powered Natural Language Queries - Ask in plain English like "Trains from Paddington to Bristol around 3pm"
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
- Traditional step-by-step mode still available


## Quick Start

### Prerequisites

- .NET 9.0 SDK or later
- Darwin API Token from National Rail (required)
- AI API Key (optional, 100% FREE options available - Groq or OpenRouter)

### Getting API Tokens

#### Darwin API Token (Required)

1. Visit the National Rail OpenLDBWS registration page: https://www.nationalrail.co.uk/developers/
2. Click on "Register" or "Request Access Token"
3. Fill out the registration form with your details
4. Accept the terms and conditions
5. You will receive your API token via email (usually within minutes)
6. Keep your token secure and never share it publicly

#### AI API Key (Optional - 100% FREE!)

**Option 1: Groq (RECOMMENDED)**
1. Visit: https://console.groq.com/keys
2. Sign up with Google/GitHub (FREE, no credit card needed)
3. Click "Create API Key"
4. Copy the key (starts with `gsk_`)
5. Super fast inference with Llama 3.1 models!

**Option 2: OpenRouter (Also FREE)**
1. Visit OpenAI Platform: https://openrouter.ai/keys
2. Sign up (free)
3. Get API key (starts with `sk-or-v1-`)
4. Access to multiple free models (Llama, Mistral, Gemma)

See `FREE_AI_SETUP.md` for detailed instructions!

### Setup

1. Clone the repository:
   ```bash
   git clone https://github.com/Haru-Tachibana/UK-Train-Timetable.git
   cd UK-Train-Timetable
   ```

2. Configure your API tokens:
   - Create a `.env` file in the project root directory
   - Add your required Darwin API token:
     ```
     DARWIN_API_TOKEN=your_actual_token_here
     ```
   - Optionally add your FREE AI key (Groq recommended):
     ```
     AI_PROVIDER=groq
     GROQ_API_KEY=gsk_your_groq_key_here
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

The application supports two modes:

#### Natural Language Mode (AI-Powered)

If you have configured an OpenAI API key, you can use natural language queries:

1. Start the application
2. Choose option 1 or type your query directly
3. Examples of natural language queries:
   - "I need to get from Paddington to Bristol around 3pm"
   - "Next train to Manchester"
   - "Trains from Kings Cross to Edinburgh leaving after 2pm"
   - "Show me London Euston departures"
   - "What trains arrive at Birmingham around 5pm"

The AI will understand your query and extract:
- Departure and destination stations
- Preferred times
- Whether you want departures or arrivals

#### Traditional Mode (Step-by-Step)

1. Start the application
2. Choose option 2 for traditional mode
3. Select a departure station:
   - Enter a station name (e.g., "Paddington", "Kings Cross")
   - Enter a 3-letter CRS code (e.g., "PAD", "KGX")
   - Type "list" to browse all available stations
4. Optional destination filtering:
   - Choose whether to filter by a specific destination (y/n)
   - If yes, select the destination station using the same methods
5. View the live departure board showing:
   - Destination
   - Scheduled departure time
   - Expected departure time
   - Platform number
   - Service status
   - Train operator

6. Continue using the application:
   - Press Enter to make another search
   - Type "exit" to quit


## Technical Details

### Technologies Used

- .NET 9.0 runtime
- FREE AI providers: Groq (Llama 3.1) or OpenRouter (multiple free models)
- OpenAI SDK (compatible with Groq and OpenRouter)
- System.ServiceModel for SOAP client communication
- DotNetEnv for environment variable management
- Async/await pattern for responsive operations

### Dependencies

- DotNetEnv (v3.1.1)
- OpenAI (v2.0.0)
- System.ServiceModel.Http (v8.0.0)
- System.ServiceModel.Primitives (v8.1.2)
- System.ServiceModel.NetTcp (v8.x)

### Project Structure

```
TrainDashboard/
├── Program.cs                       # Main application and UI with AI integration
├── TrainDashboard.csproj            # Project configuration
├── .env                             # API tokens configuration (not in git)
├── .env.example                     # Template for environment variables
├── DarwinService/
│   ├── Reference.cs                 # SOAP client auto-generated code
│   └── dotnet-svcutil.params.json
└── Services/
    ├── StationLookup.cs             # Station database and search
    ├── StationBoard.cs              # Data models
    ├── TrainService.cs              # Darwin API integration layer
    ├── JourneyQuery.cs              # AI query result models
    └── NaturalLanguageQueryService.cs  # OpenAI integration for NLP
```

### Key Components

- Program.cs: Console UI with dual-mode interaction (AI & traditional)
- NaturalLanguageQueryService: AI-powered natural language query parser using OpenAI
- JourneyQuery: Data model for parsed journey information
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

### AI Natural Language Mode Not Available

- The app will notify you if AI mode is not enabled
- Ensure you have added `OPENAI_API_KEY=your_key` to your `.env` file
- Verify your OpenAI API key is valid
- Check you have available credits in your OpenAI account
- Traditional mode will always work even without an OpenAI key

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

### AI Query Not Understood

- Try rephrasing your query with clearer station names
- Include both departure and destination for better results
- Use specific time references (e.g., "3pm" instead of "afternoon")
- Fall back to traditional mode for complex queries

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