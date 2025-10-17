
# UK Train Dashboard

Console app for live UK train departures using the National Rail Darwin API. Supports AI-powered natural language queries (Groq or OpenRouter, both free) and traditional step-by-step mode.

## Setup

1. Install .NET 9.0 SDK or later
2. Get a Darwin API token: https://www.nationalrail.co.uk/developers/
3. (Optional) Get a free Groq API key: https://console.groq.com/keys
4. Create a `.env` file in the project root:
   ```
   DARWIN_API_TOKEN=your_actual_token_here
   AI_PROVIDER=groq
   GROQ_API_KEY=gsk_your_groq_key_here
   ```
5. Restore dependencies:
   ```
   dotnet restore
   ```
6. Run the app:
   ```
   dotnet run
   ```

## Usage

You can search for trains in two ways:

- Type a natural language query (e.g. `Trains from Paddington to Bristol around 3pm`)
- Use traditional mode (step-by-step station selection)

Darwin API only supports times up to 2 hours in the past or future from now.

## Troubleshooting

- If you see `DARWIN_API_TOKEN not found`, check your `.env` file
- If AI mode is not available, check your Groq/OpenRouter key and provider
- For build errors, run:
  ```
  dotnet clean
  dotnet restore
  dotnet build
  ```

## License

For personal use with the National Rail Darwin API.
