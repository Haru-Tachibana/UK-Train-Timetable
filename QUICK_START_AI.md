# Quick Start: Testing AI Natural Language Queries

## Setup (2 minutes)

1. **Get an OpenAI API Key**
   ```
   Visit: https://platform.openai.com/api-keys
   Click: "Create new secret key"
   Copy: sk-proj-...
   ```

2. **Configure Your Environment**
   ```bash
   cd /Users/yuyangw/TrainDashboard/UK-Train-Timetable
   
   # Edit or create .env file
   echo "DARWIN_API_TOKEN=your_darwin_token" > .env
   echo "OPENAI_API_KEY=sk-your-openai-key" >> .env
   ```

3. **Build and Run**
   ```bash
   dotnet build
   dotnet run
   ```

## Test Queries

Copy and paste these queries to test:

### Basic Tests
```
I need to get from Paddington to Bristol around 3pm
```

```
Show me trains from Manchester to London
```

```
Next train to Birmingham
```

### Time-Based Tests
```
Trains from Euston to Manchester after 5pm
```

```
Edinburgh departures around 9am
```

### Arrival Tests
```
What trains arrive at Liverpool around 2pm
```

## Expected Behavior

### Successful Query
```
How would you like to search?
1. Natural language (e.g., 'Trains from Paddington to Bristol around 3pm')
2. Traditional mode (step-by-step selection)

Enter choice (1/2) or type your query directly: I need to get from Paddington to Bristol around 3pm

Processing your query with AI...

I understood:
  From: Paddington
  To: Bristol
  Around: 15:00

Multiple stations found for 'Bristol':
1. Bristol Temple Meads (BRI)
2. Bristol Parkway (BPW)

Select DESTINATION station (1-2): 1
Selected: Bristol Temple Meads (BRI)

Fetching live departure information...

[Departure board displays]
```

### Without OpenAI Key
```
How would you like to search?
1. Natural language (e.g., 'Trains from Paddington to Bristol around 3pm')
2. Traditional mode (step-by-step selection)

[Only shows if AI is enabled]
```

## Troubleshooting

### AI Mode Not Available
- Check `.env` file exists
- Verify `OPENAI_API_KEY=sk-...` is present
- Restart the application

### "Error parsing query with AI"
- Check internet connection
- Verify OpenAI API key is valid
- Check you have credits in your OpenAI account

### Station Not Found
- Try more specific names: "Bristol Temple Meads" not just "Bristol"
- Use CRS codes: "PAD", "BRI", "MAN"
- Select from multiple matches when prompted

## Success Criteria

✅ Application starts and shows "AI Natural Language Query Mode: ENABLED"
✅ Can type natural language queries directly
✅ AI correctly extracts departure and destination
✅ Station resolution works with disambiguation
✅ Departure board displays correctly
✅ Can switch between AI and traditional modes
✅ Traditional mode still works without AI

## Sample Session Log

```
$ dotnet run

    _      ___     ___     ___        __   ___   __   __   ___     ___   
   | |    |___|   |   \   | __|      /"/  /   \  \ \ / /  | __|   |   \  
   |_|    |___|   | |) |  | _|      / /   | - |   \ V /   | _|    | |) | 
  _(_)_   _____   |___/   |___|   _/_/_   |_|_|   _|_|_   |___|   |___/  

AI Natural Language Query Mode: ENABLED

================================================================================
How would you like to search?
1. Natural language (e.g., 'Trains from Paddington to Bristol around 3pm')
2. Traditional mode (step-by-step selection)

Enter choice (1/2) or type your query directly: Trains from Paddington to Manchester after 2pm

Processing your query with AI...

I understood:
  From: Paddington
  To: Manchester
  Around: 14:00

Using: Paddington (PAD)
Using: Manchester Piccadilly (MAN)

Fetching live departure information...

Departures from: London Paddington (PAD)
Date: 17 Oct 2025
Calling at: Manchester Piccadilly
Generated at: 14:23:15

════════════════════════════════════════════════════════════════════════════════
#   Destination               Sch    Exp    Plat  Status       Operator       
════════════════════════════════════════════════════════════════════════════════
1   Manchester Piccadilly     14:30  14:30  7     On time      Avanti West... 
2   Manchester Piccadilly     15:00  15:00  8     On time      Avanti West... 
3   Manchester Piccadilly     15:30  15:32  7     Delayed      Avanti West... 

[... more results ...]

================================================================================
Press Enter to search again, or type 'exit' to quit: 
```

## Next Steps

- Try different phrasings for the same query
- Test with stations near you
- Try both AI and traditional modes
- Check the AI_USAGE_GUIDE.md for more examples
- Report any issues or unexpected behavior

## Cost Note

Each AI query costs approximately $0.00015 (GPT-4o-mini pricing). Testing 100 queries = ~$0.015 (less than 2 cents).
