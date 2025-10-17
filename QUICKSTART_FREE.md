# 🚀 Quick Start - 100% FREE AI Setup

## TL;DR - Get Running in 3 Minutes

### 1. Get FREE Groq API Key (30 seconds)
```
Visit: https://console.groq.com/keys
Sign up → Create API Key → Copy it
```

### 2. Configure .env (30 seconds)
```bash
cd /Users/yuyangw/TrainDashboard/UK-Train-Timetable

# Edit .env file and add:
AI_PROVIDER=groq
GROQ_API_KEY=gsk_your_actual_key_here
DARWIN_API_TOKEN=your_darwin_token_here
```

### 3. Run! (5 seconds)
```bash
dotnet run
```

You should see:
```
Using Groq AI (llama-3.1-70b-versatile) - FREE!
AI Natural Language Query Mode: ENABLED
```

## Try These Queries

```
I need to get from Paddington to Bristol around 3pm
```

```
Next train to Manchester
```

```
Show me trains from Kings Cross to Edinburgh after 5pm
```

## Why Groq?

✅ **100% FREE** - No credit card, no trial period, just free
✅ **Super Fast** - Responses in ~0.5 seconds
✅ **Powerful** - Llama 3.1 70B model
✅ **Generous Limits** - 30 requests/minute

## Alternative: OpenRouter (Also FREE)

If Groq doesn't work for you:

1. Get key from: https://openrouter.ai/keys
2. Update `.env`:
```bash
AI_PROVIDER=openrouter
OPENROUTER_API_KEY=sk-or-v1-your_key_here
```

## That's It!

No payment info, no trials, just **100% FREE** AI-powered train queries! 🎉

For more details, see `FREE_AI_SETUP.md`
