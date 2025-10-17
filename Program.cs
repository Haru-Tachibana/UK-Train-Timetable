using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TrainDashboard.Services;
using TrainDashboard.DarwinService;
using DotNetEnv;

namespace TrainDashboard;

class Program
{
    static async Task Main(string[] args)
    {
        // Load environment variables from .env file
        // Try multiple locations to ensure it's found
        var envPaths = new[]
        {
            ".env",  // Current directory
            Path.Combine(Directory.GetCurrentDirectory(), ".env"),  // Explicit current directory
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".env"),  // App directory
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", ".env")  // Project root from bin/Debug/net9.0
        };

        bool envLoaded = false;
        foreach (var envPath in envPaths)
        {
            if (File.Exists(envPath))
            {
                Env.Load(envPath);
                envLoaded = true;
                break;
            }
        }

        if (!envLoaded)
        {
            // Try the default load as fallback
            try
            {
                Env.Load();
            }
            catch
            {
                // Will check for token below
            }
        }
        
        // Get API token from environment variable
        var apiToken = Environment.GetEnvironmentVariable("DARWIN_API_TOKEN");
        
        if (string.IsNullOrEmpty(apiToken))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Error: DARWIN_API_TOKEN not found in environment variables.");
            Console.WriteLine("Please create a .env file with your API token.");
            Console.WriteLine($"Current directory: {Directory.GetCurrentDirectory()}");
            Console.WriteLine($"Looking for .env in:");
            foreach (var path in envPaths)
            {
                var fullPath = Path.GetFullPath(path);
                var exists = File.Exists(fullPath);
                Console.WriteLine($"  {fullPath} - {(exists ? "EXISTS" : "NOT FOUND")}");
            }
            Console.WriteLine("\nSee .env.example for the required format.");
            Console.ResetColor();
            return;
        }
        
        // Configure console window (works better in Windows, but we can try for macOS)
        Console.Title = "UK Train Dashboard";
        Console.Clear();
        DisplayHeader();
        
        var trainService = new TrainService(apiToken);
        
        // Initialize AI service (optional) - supports FREE Groq or OpenRouter
        var aiApiKey = Environment.GetEnvironmentVariable("GROQ_API_KEY")
                    ?? Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");
        var aiProvider = Environment.GetEnvironmentVariable("AI_PROVIDER"); // groq or openrouter
        var aiService = new NaturalLanguageQueryService(aiApiKey, aiProvider);
        
        if (aiService.IsEnabled)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("AI Natural Language Query Mode: ENABLED");
            Console.ResetColor();
        }
        
        while (true)
        {
            try
            {
                // Ask user for input mode
                Console.WriteLine("\n" + new string('=', 80));
                
                if (aiService.IsEnabled)
                {
                    Console.WriteLine("How would you like to search?");
                    Console.WriteLine("1. Natural language (e.g., 'Trains from Paddington to Bristol around 3pm')");
                    Console.WriteLine("2. Traditional mode (step-by-step selection)");
                    Console.Write("\nEnter choice (1/2) or type your query directly: ");
                    
                    var modeInput = Console.ReadLine()?.Trim();
                    
                    if (string.IsNullOrEmpty(modeInput))
                    {
                        continue;
                    }
                    
                    if (modeInput.Equals("exit", StringComparison.OrdinalIgnoreCase) || 
                        modeInput.Equals("quit", StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }
                    
                    // If user chose option 2 or explicitly wants traditional mode
                    if (modeInput == "2")
                    {
                        await TraditionalQueryModeAsync(trainService);
                        continue;
                    }
                    
                    // If user chose option 1, ask for natural query
                    string naturalQuery;
                    if (modeInput == "1")
                    {
                        Console.Write("\nWhat journey are you planning? ");
                        naturalQuery = Console.ReadLine()?.Trim() ?? "";
                    }
                    else
                    {
                        // User typed something else - treat it as a natural language query
                        naturalQuery = modeInput;
                    }
                    
                    if (!string.IsNullOrEmpty(naturalQuery))
                    {
                        await ProcessNaturalLanguageQueryAsync(trainService, aiService, naturalQuery);
                    }
                }
                else
                {
                    // AI not available, use traditional mode
                    await TraditionalQueryModeAsync(trainService);
                }
                
                // Ask if user wants to continue
                Console.WriteLine("\n" + new string('=', 80));
                Console.WriteLine("Press Enter to search again, or type 'exit' to quit: ");
                var continueChoice = Console.ReadLine()?.Trim().ToLower();
                if (continueChoice == "exit" || continueChoice == "quit" || continueChoice == "q")
                {
                    break;
                }
                
                Console.Clear();
                DisplayHeader();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
            }
        }
        
        Console.WriteLine("\nThank you for using Train Dashboard!");
    }
    
    static async Task ProcessNaturalLanguageQueryAsync(TrainService trainService, NaturalLanguageQueryService aiService, string query)
    {
        Console.WriteLine("\nProcessing your query with AI...");
        
        var parsedQuery = await aiService.ParseQueryAsync(query);
        
        if (parsedQuery == null || !parsedQuery.IsValid)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("I couldn't understand that query. Let's try the traditional mode.");
            Console.ResetColor();
            await TraditionalQueryModeAsync(trainService);
            return;
        }
        
        // Show what was understood
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("\nI understood:");
        if (!string.IsNullOrEmpty(parsedQuery.DepartureStation))
            Console.WriteLine($"  From: {parsedQuery.DepartureStation}");
        if (!string.IsNullOrEmpty(parsedQuery.DestinationStation))
            Console.WriteLine($"  To: {parsedQuery.DestinationStation}");
        if (parsedQuery.PreferredDepartureTime.HasValue)
            Console.WriteLine($"  Around: {parsedQuery.PreferredDepartureTime.Value:hh\\:mm}");
        Console.ResetColor();
        
        // Resolve station codes
        string? departureCode = null;
        string? destinationCode = null;
        
        // Resolve departure station
        if (!string.IsNullOrEmpty(parsedQuery.DepartureStation))
        {
            departureCode = await ResolveStationAsync(parsedQuery.DepartureStation, "DEPARTURE");
        }
        
        if (departureCode == null)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Could not determine departure station. Please try again.");
            Console.ResetColor();
            return;
        }
        
        // Resolve destination station if provided
        if (!string.IsNullOrEmpty(parsedQuery.DestinationStation))
        {
            destinationCode = await ResolveStationAsync(parsedQuery.DestinationStation, "DESTINATION");
        }
        
        // Fetch and display
        Console.WriteLine("\nFetching live departure information...\n");
        await DisplayDepartureBoardAsync(trainService, departureCode, destinationCode);
    }
    
    static async Task<string?> ResolveStationAsync(string stationInput, string stationType)
    {
        // Check if it's already a valid CRS code
        if (stationInput.Length == 3 && StationLookup.IsValidCrsCode(stationInput))
        {
            return stationInput.ToUpper();
        }
        
        // Try direct lookup
        var code = StationLookup.GetStationCode(stationInput);
        if (code != null)
        {
            return code;
        }
        
        // Search for matches
        var matches = StationLookup.SearchStations(stationInput);
        
        if (matches.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"No stations found matching '{stationInput}'.");
            Console.ResetColor();
            return null;
        }
        
        if (matches.Count == 1)
        {
            code = StationLookup.GetStationCode(matches[0]);
            Console.WriteLine($"Using: {matches[0]} ({code})");
            return code;
        }
        
        // Multiple matches - let user choose
        Console.WriteLine($"\nMultiple stations found for '{stationInput}':");
        for (int i = 0; i < Math.Min(matches.Count, 10); i++)
        {
            var matchCode = StationLookup.GetStationCode(matches[i]);
            Console.WriteLine($"{i + 1}. {matches[i]} ({matchCode})");
        }
        
        Console.Write($"\nSelect {stationType} station (1-{Math.Min(matches.Count, 10)}): ");
        var choice = Console.ReadLine()?.Trim();
        
        if (int.TryParse(choice, out int selection) && selection > 0 && selection <= Math.Min(matches.Count, 10))
        {
            code = StationLookup.GetStationCode(matches[selection - 1]);
            Console.WriteLine($"Selected: {matches[selection - 1]} ({code})");
            return code;
        }
        
        return null;
    }
    
    static async Task TraditionalQueryModeAsync(TrainService trainService)
    {
        // Get departure station
        var departureStation = await SelectStationAsync("DEPARTURE");
        if (departureStation == null) return;
        
        // Ask if user wants to filter by destination
        Console.WriteLine("\nWould you like to filter by destination? (y/n): ");
        var filterChoice = Console.ReadLine()?.Trim().ToLower();
        
        string? destinationStation = null;
        if (filterChoice == "y" || filterChoice == "yes")
        {
            destinationStation = await SelectStationAsync("DESTINATION");
            if (destinationStation == null) return;
        }
        
        // Fetch and display departure board
        Console.WriteLine("\nFetching live departure information...\n");
        await DisplayDepartureBoardAsync(trainService, departureStation, destinationStation);
    }
    
    
    static void DisplayHeader()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine();
        Console.WriteLine("    _      ___     ___     ___        __   ___   __   __   ___     ___   ");
        Console.WriteLine("   | |    |___|   |   \\   | __|      /\"/  /   \\  \\ \\ / /  | __|   |   \\  ");
        Console.WriteLine("   |_|    |___|   | |) |  | _|      / /   | - |   \\ V /   | _|    | |) | ");
        Console.WriteLine("  _(_)_   _____   |___/   |___|   _/_/_   |_|_|   _|_|_   |___|   |___/  ");
        Console.WriteLine("_| " + new string('\"', 3) + " |_|     |_|" + new string('\"', 5) + "|_|" + new string('\"', 5) + "|_|" + new string('\"', 5) + "|_|" + new string('\"', 5) + "|_| " + new string('\"', 3) + " |_|" + new string('\"', 5) + "|_|" + new string('\"', 5) + "| ");
        Console.WriteLine(string.Concat(Enumerable.Repeat("`\"-0-0-'", 9)));
        Console.ResetColor();
    }
    
    static async Task<string?> SelectStationAsync(string stationType)
    {
        Console.WriteLine($"\n{stationType} Station Selection:");
        Console.WriteLine("─────────────────────────────────");
        Console.Write("Enter station name or CRS code (or type 'list' to see all stations, 'search' for direct search): ");
        
        var input = Console.ReadLine()?.Trim();
        
        if (string.IsNullOrEmpty(input))
        {
            return null;
        }
        
        if (input.Equals("exit", StringComparison.OrdinalIgnoreCase) || 
            input.Equals("quit", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }
        
        // If user types 'list', show all stations
        if (input.Equals("list", StringComparison.OrdinalIgnoreCase))
        {
            return await SelectFromListAsync(stationType);
        }
        
        // If user wants to search
        if (input.Equals("search", StringComparison.OrdinalIgnoreCase))
        {
            return await SearchStationDirectlyAsync(stationType, StationLookup.GetAllStations());
        }
        
        // Check if it's a 3-letter CRS code
        if (input.Length == 3 && StationLookup.IsValidCrsCode(input))
        {
            var stationName = StationLookup.GetStationName(input);
            Console.WriteLine($"Selected: {stationName} ({input.ToUpper()})");
            return input.ToUpper();
        }
        
        // Try to find station by name
        var code = StationLookup.GetStationCode(input);
        if (code != null)
        {
            Console.WriteLine($"Selected: {input} ({code})");
            return code;
        }
        
        // Search for partial matches
        var matches = StationLookup.SearchStations(input);
        if (matches.Count == 0)
        {
            Console.WriteLine($"No stations found matching '{input}'. Please try again.");
            return await SelectStationAsync(stationType);
        }
        
        if (matches.Count == 1)
        {
            code = StationLookup.GetStationCode(matches[0]);
            Console.WriteLine($"Selected: {matches[0]} ({code})");
            return code;
        }
        
        // Multiple matches - let user choose
        Console.WriteLine($"\nMultiple stations found matching '{input}':");
        for (int i = 0; i < Math.Min(matches.Count, 20); i++)
        {
            var matchCode = StationLookup.GetStationCode(matches[i]);
            Console.WriteLine($"{i + 1}. {matches[i]} ({matchCode})");
        }
        
        if (matches.Count > 20)
        {
            Console.WriteLine($"... and {matches.Count - 20} more. Please be more specific.");
        }
        
        Console.Write("\nEnter number to select, or type a new search: ");
        var choice = Console.ReadLine()?.Trim();
        
        if (int.TryParse(choice, out int selection) && selection > 0 && selection <= Math.Min(matches.Count, 20))
        {
            code = StationLookup.GetStationCode(matches[selection - 1]);
            Console.WriteLine($"Selected: {matches[selection - 1]} ({code})");
            return code;
        }
        
        // User entered a new search
        if (!string.IsNullOrEmpty(choice))
        {
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            return await SelectStationAsync(stationType);
        }
        
        return null;
    }
    
    static async Task<string?> SelectFromListAsync(string stationType)
    {
        var stations = StationLookup.GetAllStations();
        const int pageSize = 20;
        int currentPage = 0;
        int totalPages = (stations.Count + pageSize - 1) / pageSize;
        
        while (true)
        {
            Console.Clear();
            DisplayHeader();
            Console.WriteLine($"\n{stationType} Station Selection - Page {currentPage + 1} of {totalPages}:");
            Console.WriteLine(new string('─', 80));
            
            var pageStations = stations.Skip(currentPage * pageSize).Take(pageSize).ToList();
            for (int i = 0; i < pageStations.Count; i++)
            {
                // Display station with its unique ID rather than a page-specific number
                Console.WriteLine($"{pageStations[i].Id,3}. {pageStations[i].Name,-40} ({pageStations[i].Code})");
            }
            
            Console.WriteLine(new string('─', 80));
            Console.WriteLine("Enter station ID to select, 'n' for next page, 'p' for previous page, 'b' to go back, or 's' to search:");
            
            var input = Console.ReadLine()?.Trim().ToLower();
            
            if (input == "n" && currentPage < totalPages - 1)
            {
                currentPage++;
            }
            else if (input == "p" && currentPage > 0)
            {
                currentPage--;
            }
            else if (input == "b")
            {
                Console.Clear();
                DisplayHeader();
                return await SelectStationAsync(stationType);
            }
            else if (input == "s" || input == "search")
            {
                // Direct search functionality
                return await SearchStationDirectlyAsync(stationType, stations);
            }
            else if (int.TryParse(input, out int selection))
            {
                // Find station by its unique ID across all pages
                var selected = stations.FirstOrDefault(s => s.Id == selection);
                if (selected != null)
                {
                    Console.WriteLine($"Selected: {selected.Name} ({selected.Code})");
                    return selected.Code;
                }
                else
                {
                    Console.WriteLine("Invalid station ID. Press any key to continue...");
                    Console.ReadKey(true);
                }
            }
        }
    }
    
    static async Task<string?> SearchStationDirectlyAsync(string stationType, List<Station> allStations)
    {
        Console.Clear();
        DisplayHeader();
        Console.WriteLine($"\n{stationType} Station Search:");
        Console.WriteLine(new string('─', 80));
        Console.Write("Enter station name or part of name: ");
        
        var searchTerm = Console.ReadLine()?.Trim();
        
        if (string.IsNullOrEmpty(searchTerm))
        {
            return null;
        }
        
        // Search stations matching the search term
        var matchingStations = allStations
            .Where(s => s.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            .OrderBy(s => s.Name)
            .ToList();
            
        if (matchingStations.Count == 0)
        {
            Console.WriteLine($"No stations found matching '{searchTerm}'.");
            Console.WriteLine("Press any key to return to station list...");
            Console.ReadKey(true);
            return null;
        }
        
        Console.WriteLine($"\nFound {matchingStations.Count} stations matching '{searchTerm}':");
        Console.WriteLine(new string('─', 80));
        
        // Display all matching stations with their unique IDs
        foreach (var station in matchingStations)
        {
            Console.WriteLine($"{station.Id,3}. {station.Name,-40} ({station.Code})");
        }
        
        Console.WriteLine(new string('─', 80));
        Console.Write("Enter station ID to select or press Enter to go back: ");
        
        var input = Console.ReadLine()?.Trim();
        
        if (int.TryParse(input, out int selection))
        {
            var selected = allStations.FirstOrDefault(s => s.Id == selection);
            if (selected != null)
            {
                Console.WriteLine($"Selected: {selected.Name} ({selected.Code})");
                return selected.Code;
            }
        }
        
        return null;
    }
    
    static async Task DisplayDepartureBoardAsync(TrainService trainService, string departureCode, string? destinationCode)
    {
        var board = await trainService.GetDepartureBoardAsync(departureCode, destinationCode, 15);
        
        if (board == null)
        {
            Console.WriteLine("Unable to fetch departure information. Please try again.");
            return;
        }
        
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Departures from: {board.locationName} ({board.crs})");
        Console.WriteLine($"Date: {board.generatedAt:dd MMM yyyy}");
        if (!string.IsNullOrEmpty(board.filterLocationName))
        {
            Console.WriteLine($"Calling at: {board.filterLocationName}");
        }
        Console.WriteLine($"Generated at: {board.generatedAt:HH:mm:ss}");
        Console.ResetColor();
        
        if (board.trainServices == null || board.trainServices.Length == 0)
        {
            Console.WriteLine("\nNo services found.");
            Console.WriteLine("This could be due to one of the following reasons:");
            Console.WriteLine("1. There are no scheduled services at this time");
            Console.WriteLine("2. The station code may not match exactly with what the National Rail API expects");
            Console.WriteLine("3. The National Rail API data feed might be temporarily unavailable");
            Console.WriteLine("\nTry a different station or try again later.");
            return;
        }
        
        // Store service IDs for selection
        var serviceIds = new Dictionary<int, string>();
        int index = 1;
        
        Console.WriteLine("\n" + new string('═', 80));
        Console.WriteLine($"{"#",-3} {"Destination",-25} {"Sch",-6} {"Exp",-6} {"Plat",-5} {"Status",-12} {"Operator",-15}");
        Console.WriteLine(new string('═', 80));
        
        foreach (var service in board.trainServices)
        {
            // Store service ID for selection
            if (!string.IsNullOrEmpty(service.serviceID))
            {
                serviceIds[index] = service.serviceID;
            }
            
            // Work directly with the ServiceItem properties
            var destination = "";
            if (service.destination != null && service.destination.Length > 0)
            {
                destination = string.Join(" & ", service.destination.Select(d => d.locationName ?? "Unknown"));
            }
            else
            {
                destination = "Unknown";
            }
            
            var scheduled = service.std ?? "N/A";
            var expected = service.etd ?? "N/A";
            var platform = service.platform ?? "TBC";
            
            // Determine service status
            var status = "On time";
            if (service.isCancelled)
            {
                status = "CANCELLED";
            }
            else if (string.Equals(service.etd, "Delayed", StringComparison.OrdinalIgnoreCase))
            {
                status = "Delayed";
            }
            else if (!string.IsNullOrEmpty(service.etd) && service.etd != service.std && 
                    !string.Equals(service.etd, "On time", StringComparison.OrdinalIgnoreCase))
            {
                status = "Delayed";
            }
            
            var operatorName = service.@operator ?? "Unknown";
            
            // Truncate long names to fit
            if (destination.Length > 24)
                destination = destination.Substring(0, 21) + "...";
            if (operatorName.Length > 14)
                operatorName = operatorName.Substring(0, 11) + "...";
            
            // Color code based on status
            if (status == "CANCELLED")
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            else if (status == "Delayed")
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.White;
            }
            
            Console.WriteLine($"{index,-3} {destination,-25} {scheduled,-6} {expected,-6} {platform,-5} {status,-12} {operatorName,-15}");
            Console.ResetColor();
            
            // Show cancellation/delay reasons if available
            if (!string.IsNullOrEmpty(service.cancelReason))
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"  → Cancellation: {service.cancelReason}");
                Console.ResetColor();
            }
            if (!string.IsNullOrEmpty(service.delayReason))
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"  → Delay: {service.delayReason}");
                Console.ResetColor();
            }
            
            index++;
        }
        
        Console.WriteLine(new string('═', 80));
        
        // Ask user if they want to see more details about a specific train
        Console.WriteLine("\nTo view detailed information about a specific train, enter its number.");
        Console.WriteLine("Or press Enter to return to the main menu.");
        Console.Write("> ");
        
        var selection = Console.ReadLine();
        
        if (!string.IsNullOrWhiteSpace(selection) && int.TryParse(selection, out int selectedIndex) && 
            serviceIds.TryGetValue(selectedIndex, out string? serviceId) && serviceId != null)
        {
            await DisplayTrainDetailsAsync(trainService, serviceId);
        }
    }
    
    static async Task DisplayTrainDetailsAsync(TrainService trainService, string serviceId)
    {
        Console.Clear();
        DisplayHeader();
        
        Console.WriteLine("\nFetching detailed train journey information...");
        
        try
        {
            var serviceDetails = await trainService.GetServiceDetailsAsync(serviceId);
            
            if (serviceDetails == null)
            {
                Console.WriteLine("Unable to fetch train details. Press any key to continue...");
                Console.ReadKey(true);
                return;
            }
            
            // Display journey header
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\n{serviceDetails.@operator} Service");
            Console.WriteLine(new string('═', 80));
            
            string origin = "Unknown Origin";
            string destination = "Unknown Destination";
            
            // Get origin from previous calling points
            if (serviceDetails.previousCallingPoints != null && 
                serviceDetails.previousCallingPoints.Length > 0 && 
                serviceDetails.previousCallingPoints[0].callingPoint != null && 
                serviceDetails.previousCallingPoints[0].callingPoint.Length > 0)
            {
                origin = serviceDetails.previousCallingPoints[0].callingPoint[0].locationName;
            }
            
            // Get destination from subsequent calling points
            if (serviceDetails.subsequentCallingPoints != null && 
                serviceDetails.subsequentCallingPoints.Length > 0 && 
                serviceDetails.subsequentCallingPoints[0].callingPoint != null &&
                serviceDetails.subsequentCallingPoints[0].callingPoint.Length > 0)
            {
                destination = serviceDetails.subsequentCallingPoints[0].callingPoint.Last().locationName;
            }
            
            Console.WriteLine($"Journey: {origin} → {destination}");
            Console.WriteLine($"Date: {DateTime.Now:dd MMM yyyy}");
            Console.WriteLine($"Train ID: {serviceId}");
            Console.ResetColor();
            
            // Display service attributes if available
            Console.WriteLine($"Service Type: {serviceDetails.serviceType}");
            
            // Display platform and other information for current station
            Console.WriteLine($"\nCurrent Station: {serviceDetails.locationName} ({serviceDetails.crs})");
            if (!string.IsNullOrEmpty(serviceDetails.platform))
            {
                Console.WriteLine($"Platform: {serviceDetails.platform}");
            }
            
            string status = "On time";
            if (serviceDetails.isCancelled)
            {
                status = "CANCELLED";
            }
            else if (!string.IsNullOrEmpty(serviceDetails.etd) && serviceDetails.etd != "On time")
            {
                status = serviceDetails.etd;
            }
            else if (!string.IsNullOrEmpty(serviceDetails.eta) && serviceDetails.eta != "On time")
            {
                status = serviceDetails.eta;
            }
            
            Console.WriteLine($"Status: {status}");
            
            if (!string.IsNullOrEmpty(serviceDetails.delayReason))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Delay Reason: {serviceDetails.delayReason}");
                Console.ResetColor();
            }
            
            if (!string.IsNullOrEmpty(serviceDetails.cancelReason))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Cancellation Reason: {serviceDetails.cancelReason}");
                Console.ResetColor();
            }
            
            // Show journey details with previous, current, and subsequent calling points
            Console.WriteLine("\nComplete Journey:");
            Console.WriteLine(new string('─', 80));
            Console.WriteLine($"{"Station",-30} {"Scheduled",-10} {"Expected",-10} {"Status",-10}");
            Console.WriteLine(new string('─', 80));
            
            // Display previous calling points (stations the train has already passed)
            if (serviceDetails.previousCallingPoints != null && serviceDetails.previousCallingPoints.Length > 0)
            {
                foreach (var leg in serviceDetails.previousCallingPoints)
                {
                    if (leg.callingPoint != null)
                    {
                        foreach (var stop in leg.callingPoint)
                        {
                            // Use green color for stations the train has already passed
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            string stopTime = stop.st ?? "N/A";
                            string actualTime = stop.at ?? "N/A";
                            Console.WriteLine($"{stop.locationName,-30} {stopTime,-10} {actualTime,-10} {"Departed",-10}");
                            Console.ResetColor();
                        }
                    }
                }
            }
            
            // Display current station in bright color
            Console.ForegroundColor = ConsoleColor.White;
            string currentScheduled = serviceDetails.std ?? serviceDetails.sta ?? "N/A";
            string currentExpected = serviceDetails.etd ?? serviceDetails.eta ?? "N/A";
            string currentStatus = status;
            Console.WriteLine($"{serviceDetails.locationName,-30} {currentScheduled,-10} {currentExpected,-10} {currentStatus,-10} {"<-- YOU ARE HERE"}");
            Console.ResetColor();
            
            // Display subsequent calling points (stations the train will call at)
            if (serviceDetails.subsequentCallingPoints != null && serviceDetails.subsequentCallingPoints.Length > 0)
            {
                foreach (var leg in serviceDetails.subsequentCallingPoints)
                {
                    if (leg.callingPoint != null)
                    {
                        foreach (var stop in leg.callingPoint)
                        {
                            // Use yellow color for upcoming stations
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            string stopTime = stop.st ?? "N/A";
                            string expectedTime = stop.et ?? "N/A";
                            Console.WriteLine($"{stop.locationName,-30} {stopTime,-10} {expectedTime,-10} {"Expected",-10}");
                            Console.ResetColor();
                        }
                    }
                }
            }
            
            Console.WriteLine(new string('─', 80));
            
            // Display additional service information
            Console.WriteLine($"\nAdditional Information:");
            
            if (serviceDetails.formation != null && serviceDetails.formation.coaches != null)
            {
                Console.WriteLine($"Train Formation: {serviceDetails.formation.coaches.Length} coaches");
            }
            
            if (!string.IsNullOrEmpty(serviceDetails.rsid))
            {
                Console.WriteLine($"RSID: {serviceDetails.rsid}");
            }
            
            Console.WriteLine("\nPress any key to return to the main menu...");
            Console.ReadKey(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching train details: {ex.Message}");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
        }
    }
}
