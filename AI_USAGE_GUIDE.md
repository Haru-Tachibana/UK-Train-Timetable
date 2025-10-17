# AI Natural Language Query Guide

## Overview

The Train Dashboard now supports natural language queries powered by OpenAI's GPT-4o-mini model. Instead of navigating through menus, simply describe your journey in plain English.

## Setup

1. Get an OpenAI API key from https://platform.openai.com/api-keys
2. Add it to your `.env` file:
   ```
   OPENAI_API_KEY=sk-your-actual-key-here
   ```
3. Ensure you have available credits in your OpenAI account
4. Restart the application

## How It Works

The AI service:
1. Takes your natural language input
2. Extracts key information (departure, destination, time preferences)
3. Resolves station names to CRS codes
4. Fetches live departure information from the National Rail API

## Example Queries

### Basic Departures
- "Show me trains from Paddington"
- "Departures from London Euston"
- "Trains leaving Manchester"

### With Destinations
- "Trains from Paddington to Bristol"
- "I need to get from Kings Cross to Edinburgh"
- "How do I get from Birmingham to Liverpool"

### With Time Preferences
- "Trains from Paddington to Bristol around 3pm"
- "Next train to Manchester after 2pm"
- "Departures from Euston leaving after 5pm"
- "Show me trains to Leeds at 9am"

### Natural Variations
- "I want to go from London to Brighton"
- "What trains run from Oxford to Reading"
- "Take me from Cardiff to Bristol"
- "Get me to Edinburgh from London"

### Arrivals
- "What trains arrive at Birmingham around 5pm"
- "Arrivals at Manchester this evening"
- "Trains arriving in London from Birmingham"

## Tips for Best Results

### Station Names
- Use common names: "Paddington", "Kings Cross", "Euston"
- Include "London" if needed: "London Victoria", "London Bridge"
- The AI handles variations: "King's Cross" vs "Kings Cross"
- CRS codes work too: "PAD", "KGX", "MAN"

### Time Formats
The AI understands various time formats:
- "3pm", "15:00", "3:00pm"
- "quarter past 2", "2:15"
- "half past 3", "3:30"
- "around 4pm", "about 5pm"
- "after 2pm", "before 6pm"
- "morning", "afternoon", "evening" (less precise)

### Clear vs Ambiguous Queries

✅ **Clear Queries** (Better Results):
- "Trains from Paddington to Bristol around 3pm"
- "Next train to Manchester"
- "Show me London Euston departures"

⚠️ **Ambiguous Queries** (May Need Clarification):
- "Next train" (no departure station)
- "Trains to London" (which London station?)
- "Trains tomorrow" (not yet supported)

## What the AI Extracts

The AI identifies:
- **Departure Station**: Where you're traveling from
- **Destination Station**: Where you're going (optional)
- **Time Preference**: When you want to travel (optional)
- **Query Type**: Departures vs arrivals

## Fallback Options

If the AI can't understand your query:
1. You'll see: "I couldn't understand that query"
2. The app will automatically switch to traditional mode
3. Follow the step-by-step prompts
4. Or try rephrasing your query more clearly

## Cost Considerations

- Each AI query uses the OpenAI API (costs apply)
- GPT-4o-mini is cost-effective (~$0.00015 per query)
- Traditional mode is always free and available
- No AI key = traditional mode only (no extra cost)

## Privacy & Security

- Your queries are sent to OpenAI for processing
- OpenAI's privacy policy applies: https://openai.com/policies/privacy-policy
- Your API key is stored locally in `.env` file
- Never commit your `.env` file to version control
- National Rail API calls remain direct (not through OpenAI)

## Limitations

Current AI mode:
- Only processes current day departures
- Doesn't support future dates yet
- Can't book tickets (information only)
- Limited to UK train stations
- Requires internet connection for both OpenAI and National Rail APIs

## Troubleshooting

### AI Mode Not Appearing
- Check your `.env` file has `OPENAI_API_KEY=...`
- Verify the key starts with `sk-`
- Restart the application after adding the key

### "Error parsing query with AI"
- Check your internet connection
- Verify you have OpenAI credits available
- Try using traditional mode instead

### Query Not Understood
- Be more specific with station names
- Include "from X to Y" structure
- Use explicit times: "3pm" not "this afternoon"
- Try traditional mode for complex queries

## Example Session

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

Fetching live departure information...

[Departure board displays...]
```

## Best Practices

1. **Start Simple**: Try basic queries first
2. **Be Specific**: Include both departure and destination when known
3. **Use Known Stations**: Stick to major stations you're familiar with
4. **Check Results**: Verify the AI understood correctly
5. **Use Traditional Mode**: For complex or multi-leg journeys
6. **Monitor Costs**: Track your OpenAI API usage if on a budget

## Future Enhancements

Planned features:
- Multi-date journey planning
- Voice input support
- Journey preferences (fastest, cheapest, etc.)
- Platform notifications
- Historical delay patterns

---

For more information, see the main README.md file.
