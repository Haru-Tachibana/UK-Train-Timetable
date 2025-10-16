using System;
using System.Linq;
using System.Threading.Tasks;
using TrainDashboard.Services;
using TrainDashboard.DarwinService;

namespace TrainDashboard;

class Program
{
    private const string ApiToken = "ed7cf477-14b5-4066-8cdc-df9b8aa69813";
    
    static async Task Main(string[] args)
    {
        // Configure console window (works better in Windows, but we can try for macOS)
        Console.Title = "UK Train Dashboard";
        Console.Clear();
        DisplayHeader();
        
        var trainService = new TrainService(ApiToken);
        
        while (true)
        {
            try
            {
                // Get departure station
                Console.WriteLine("\n" + new string('=', 80));
                var departureStation = await SelectStationAsync("DEPARTURE");
                if (departureStation == null) break;
                
                // Ask if user wants to filter by destination
                Console.WriteLine("\nWould you like to filter by destination? (y/n): ");
                var filterChoice = Console.ReadLine()?.Trim().ToLower();
                
                string? destinationStation = null;
                if (filterChoice == "y" || filterChoice == "yes")
                {
                    destinationStation = await SelectStationAsync("DESTINATION");
                    if (destinationStation == null) continue;
                }
                
                // Fetch and display departure board
                Console.WriteLine("\nFetching live departure information...\n");
                await DisplayDepartureBoardAsync(trainService, departureStation, destinationStation);
                
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
    
    static void DisplayHeader()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(@"
╔════════════════════════════════════════════════════════════════════════════╗
║                          TRAIN DASHBOARD                                   ║
║                  Real-time UK Rail Departure Information                   ║
╚════════════════════════════════════════════════════════════════════════════╝
        ");
        Console.ResetColor();
    }
    
    static async Task<string?> SelectStationAsync(string stationType)
    {
        Console.WriteLine($"\n{stationType} Station Selection:");
        Console.WriteLine("─────────────────────────────────");
        Console.Write("Enter station name or CRS code (or type 'list' to see all stations): ");
        
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
                Console.WriteLine($"{i + 1,2}. {pageStations[i].Name,-40} ({pageStations[i].Code})");
            }
            
            Console.WriteLine(new string('─', 80));
            Console.WriteLine("Enter number to select, 'n' for next page, 'p' for previous page, or 'b' to go back:");
            
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
            else if (int.TryParse(input, out int selection) && selection > 0 && selection <= pageStations.Count)
            {
                var selected = pageStations[selection - 1];
                Console.WriteLine($"Selected: {selected.Name} ({selected.Code})");
                return selected.Code;
            }
        }
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
            return;
        }
        
        Console.WriteLine("\n" + new string('═', 80));
        Console.WriteLine($"{"Destination",-25} {"Sch",-6} {"Exp",-6} {"Plat",-5} {"Status",-12} {"Operator",-20}");
        Console.WriteLine(new string('═', 80));
        
        foreach (var service in board.trainServices)
        {
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
            if (operatorName.Length > 19)
                operatorName = operatorName.Substring(0, 16) + "...";
            
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
            
            Console.WriteLine($"{destination,-25} {scheduled,-6} {expected,-6} {platform,-5} {status,-12} {operatorName,-20}");
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
        }
        
        Console.WriteLine(new string('═', 80));
    }
}
