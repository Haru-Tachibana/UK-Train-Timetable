using System;
using System.Collections.Generic;
using System.Linq;

namespace TrainDashboard.Services;

public class Station
{
    public string Code { get; set; }
    public string Name { get; set; }

    public Station(string code, string name)
    {
        Code = code;
        Name = name;
    }
}

public static class StationLookup
{
    // Dictionary of common UK railway stations with their CRS codes
    private static readonly Dictionary<string, string> Stations = new()
    {
        // London stations
        { "Paddington", "PAD" },
        { "King's Cross", "KGX" },
        { "Kings Cross", "KGX" },
        { "Euston", "EUS" },
        { "Liverpool Street", "LST" },
        { "Victoria", "VIC" },
        { "Waterloo", "WAT" },
        { "St Pancras International", "STP" },
        { "London Bridge", "LBG" },
        { "Charing Cross", "CHX" },
        { "Marylebone", "MYB" },
        { "Fenchurch Street", "FST" },
        
        // Major UK cities
        { "Birmingham New Street", "BHM" },
        { "Manchester Piccadilly", "MAN" },
        { "Manchester Airport", "MIA" },
        { "Leeds", "LDS" },
        { "Liverpool Lime Street", "LIV" },
        { "Edinburgh", "EDB" },
        { "Glasgow Central", "GLC" },
        { "Cardiff Central", "CDF" },
        { "Bristol Temple Meads", "BRI" },
        { "Newcastle", "NCL" },
        { "Sheffield", "SHF" },
        { "Nottingham", "NOT" },
        { "Southampton Central", "SOU" },
        { "Reading", "RDG" },
        { "Brighton", "BTN" },
        { "Oxford", "OXF" },
        { "Cambridge", "CBG" },
        { "York", "YRK" },
        { "Bath Spa", "BTH" },
        { "Exeter St Davids", "EXD" },
        { "Plymouth", "PLY" },
        { "Aberdeen", "ABD" },
        { "Inverness", "INV" },
        { "Peterborough", "PBO" },
        { "Coventry", "COV" },
        { "Leicester", "LEI" },
        { "Derby", "DBY" },
        { "Norwich", "NRW" },
        { "Ipswich", "IPS" },
        { "Portsmouth Harbour", "PMH" },
        { "Bournemouth", "BMH" },
        { "Swindon", "SWI" },
        { "Dundee", "DEE" },
        { "Cheltenham Spa", "CNM" },
        { "Stratford", "SRA" },
        { "Clapham Junction", "CLJ" },
        
        // Additional common stations
        { "Gatwick Airport", "GTW" },
        { "Heathrow Airport", "HXX" },
        { "Stansted Airport", "SSD" },
        { "Luton Airport Parkway", "LTN" },
        { "Milton Keynes Central", "MKC" },
        { "Guildford", "GLD" },
        { "Winchester", "WIN" },
        { "Salisbury", "SAL" },
        { "Chester", "CTR" },
        { "Preston", "PRE" },
        { "Lancaster", "LAN" },
        { "Carlisle", "CAR" },
        { "Durham", "DHM" },
        { "Sunderland", "SUN" },
        { "Middlesbrough", "MBR" },
        { "Hull", "HUL" },
        { "Doncaster", "DON" },
        { "Wakefield Westgate", "WKF" },
        { "Bradford Forster Square", "BDF" },
        { "Harrogate", "HGT" },
        { "Scarborough", "SCA" },
        { "Wolverhampton", "WVH" },
        { "Stoke-on-Trent", "SOT" },
        { "Crewe", "CRE" },
        { "Blackpool North", "BPN" },
        { "Stirling", "STG" },
        { "Perth", "PTH" },
        { "Swansea", "SWA" },
        { "Newport", "NWP" },
        { "Gloucester", "GCR" },
        { "Worcester Foregate Street", "WOS" },
        { "Hereford", "HFD" },
        { "Shrewsbury", "SHR" },
        { "Wrexham General", "WRX" },
        { "Bangor", "BNG" },
        { "Holyhead", "HHD" }
    };

    public static string? GetStationCode(string stationName)
    {
        // Try exact match first
        if (Stations.TryGetValue(stationName, out var code))
        {
            return code;
        }

        // Try case-insensitive match
        var match = Stations.FirstOrDefault(s => 
            string.Equals(s.Key, stationName, StringComparison.OrdinalIgnoreCase));
        
        return match.Key != null ? match.Value : null;
    }

    public static List<string> SearchStations(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return Stations.Keys.OrderBy(s => s).ToList();
        }

        var lowerSearch = searchTerm.ToLowerInvariant();
        return Stations.Keys
            .Where(s => s.ToLowerInvariant().Contains(lowerSearch))
            .OrderBy(s => s)
            .ToList();
    }

    public static string? GetStationName(string crsCode)
    {
        var match = Stations.FirstOrDefault(s => 
            string.Equals(s.Value, crsCode, StringComparison.OrdinalIgnoreCase));
        
        return match.Key;
    }

    public static bool IsValidCrsCode(string crsCode)
    {
        return Stations.Values.Any(c => 
            string.Equals(c, crsCode, StringComparison.OrdinalIgnoreCase));
    }

    public static List<Station> GetAllStations()
    {
        return Stations
            .Select(s => new Station(s.Value, s.Key))
            .OrderBy(s => s.Name)
            .ToList();
    }
}
