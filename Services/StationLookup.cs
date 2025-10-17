using System;
using System.Collections.Generic;
using System.Linq;

namespace TrainDashboard.Services;

public class Station
{
    public string Code { get; set; }
    public string Name { get; set; }
    public int Id { get; set; }  // Unique identifier for each station

    public Station(string code, string name, int id)
    {
        Code = code;
        Name = name;
        Id = id;
    }
}

public static class StationLookup
{
    // Dictionary of common UK railway stations with their CRS codes
    private static readonly Dictionary<string, string> Stations = new()
    {
        // London stations
        { "London Paddington", "PAD" },
        { "Paddington", "PAD" },
        { "London King's Cross", "KGX" },
        { "King's Cross", "KGX" },
        { "Kings Cross", "KGX" },
        { "London Euston", "EUS" },
        { "Euston", "EUS" },
        { "London Liverpool Street", "LST" },
        { "Liverpool Street", "LST" },
        { "London Victoria", "VIC" },
        { "Victoria", "VIC" },
        { "London Waterloo", "WAT" },
        { "Waterloo", "WAT" },
        { "London St Pancras International", "STP" },
        { "St Pancras", "STP" },
        { "St Pancras International", "STP" },
        { "London Bridge", "LBG" },
        { "London Charing Cross", "CHX" },
        { "Charing Cross", "CHX" },
        { "London Marylebone", "MYB" },
        { "Marylebone", "MYB" },
        { "London Fenchurch Street", "FST" },
        { "Fenchurch Street", "FST" },
        { "London Cannon Street", "CST" },
        { "Cannon Street", "CST" },
        { "London Blackfriars", "BFR" },
        { "Blackfriars", "BFR" },
        { "Moorgate", "MOG" },
        { "Old Street", "OLD" },
        { "City Thameslink", "CTK" },
        
        // Major UK cities
        { "Birmingham New Street", "BHM" },
        { "Birmingham Snow Hill", "BSW" },
        { "Birmingham Moor Street", "BMO" },
        { "Birmingham International", "BHI" },
        { "Manchester Piccadilly", "MAN" },
        { "Manchester Victoria", "MCV" },
        { "Manchester Oxford Road", "MCO" },
        { "Manchester Airport", "MIA" },
        { "Leeds", "LDS" },
        { "Liverpool Lime Street", "LIV" },
        { "Liverpool Central", "LVC" },
        { "Edinburgh", "EDB" },
        { "Edinburgh Haymarket", "EHY" },
        { "Edinburgh Gateway", "EGY" },
        { "Glasgow Central", "GLC" },
        { "Glasgow Queen Street", "GLQ" },
        { "Cardiff Central", "CDF" },
        { "Cardiff Queen Street", "CDQ" },
        { "Bristol Temple Meads", "BRI" },
        { "Bristol Parkway", "BPW" },
        { "Newcastle", "NCL" },
        { "Sheffield", "SHF" },
        { "Nottingham", "NOT" },
        { "Southampton Central", "SOU" },
        { "Reading", "RDG" },
        { "Brighton", "BTN" },
        { "Oxford", "OXF" },
        { "Cambridge", "CBG" },
        { "Cambridge North", "CMB" },
        { "York", "YRK" },
        { "Bath Spa", "BTH" },
        { "Exeter St Davids", "EXD" },
        { "Exeter Central", "EXC" },
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
        { "Portsmouth & Southsea", "PMS" },
        { "Bournemouth", "BMH" },
        { "Swindon", "SWI" },
        { "Dundee", "DEE" },
        { "Cheltenham Spa", "CNM" },
        { "Stratford", "SRA" },
        { "Clapham Junction", "CLJ" },
        { "Watford Junction", "WFJ" },
        
        // Airport and transport hub stations
        { "Gatwick Airport", "GTW" },
        { "Heathrow Airport", "HXX" },
        { "Heathrow Terminal 5", "HWV" },
        { "Heathrow Terminals 2 & 3", "HXC" },
        { "Heathrow Terminal 4", "HAF" },
        { "Stansted Airport", "SSD" },
        { "Luton Airport Parkway", "LTN" },
        // Birmingham International is already defined above
        { "Southampton Airport Parkway", "SOA" },
        { "Ashford International", "AFK" },
        { "Ebbsfleet International", "EBD" },
        
        // Southeast England
        { "Milton Keynes Central", "MKC" },
        { "Guildford", "GLD" },
        { "Winchester", "WIN" },
        { "Salisbury", "SAL" },
        { "Basingstoke", "BSK" },
        { "Woking", "WOK" },
        { "Stevenage", "SVG" },
        { "Slough", "SLO" },
        { "Windsor & Eton Central", "WNC" },
        { "Eastbourne", "EBN" },
        { "Hastings", "HGS" },
        { "Canterbury West", "CBW" },
        { "Dover Priory", "DVP" },
        { "Sevenoaks", "SEV" },
        { "Tonbridge", "TON" },
        { "Worthing", "WRH" },
        { "Chichester", "CCH" },
        
        // Northwest England
        { "Chester", "CTR" },
        { "Preston", "PRE" },
        { "Lancaster", "LAN" },
        { "Carlisle", "CAR" },
        { "Warrington Bank Quay", "WBQ" },
        { "Warrington Central", "WAC" },
        { "Bolton", "BON" },
        { "Wigan North Western", "WGN" },
        { "Blackburn", "BBN" },
        
        // Northeast England
        { "Durham", "DHM" },
        { "Sunderland", "SUN" },
        { "Middlesbrough", "MBR" },
        { "Darlington", "DAR" },
        { "Hull", "HUL" },
        { "Doncaster", "DON" },
        { "Wakefield Westgate", "WKF" },
        { "Bradford Forster Square", "BDF" },
        { "Bradford Interchange", "BDI" },
        { "Harrogate", "HGT" },
        { "Scarborough", "SCA" },
        { "Huddersfield", "HUD" },
        { "Halifax", "HFX" },
        
        // Midlands
        { "Wolverhampton", "WVH" },
        { "Stoke-on-Trent", "SOT" },
        { "Stafford", "STA" },
        { "Crewe", "CRE" },
        { "Northampton", "NMP" },
        { "Rugby", "RUG" },
        { "Lichfield Trent Valley", "LTV" },
        { "Lincoln", "LCN" },
        { "Newark North Gate", "NNG" },
        { "Grantham", "GRA" },
        { "Kettering", "KET" },
        { "Wellingborough", "WEL" },
        { "Nuneaton", "NUN" },
        { "Tamworth", "TAM" },
        { "Chesterfield", "CHD" },
        { "Burton-on-Trent", "BUT" },
        
        // Scotland
        { "Blackpool North", "BPN" },
        { "Stirling", "STG" },
        { "Perth", "PTH" },
        { "Kirkcaldy", "KDY" },
        { "Motherwell", "MTH" },
        { "Paisley Gilmour Street", "PYG" },
        { "Ayr", "AYR" },
        { "Fort William", "FTW" },
        { "Oban", "OBN" },
        { "Aviemore", "AVM" },
        { "Pitlochry", "PIT" },
        { "Kyle of Lochalsh", "KYL" },
        { "Mallaig", "MLG" },
        
        // Wales
        { "Swansea", "SWA" },
        { "Newport", "NWP" },
        { "Bridgend", "BGN" },
        { "Llandudno", "LLD" },
        { "Llandudno Junction", "LLJ" },
        { "Carmarthen", "CMN" },
        { "Aberystwyth", "AYW" },
        
        // West England and Southwest
        { "Gloucester", "GCR" },
        { "Worcester Foregate Street", "WOF" },
        { "Worcester Shrub Hill", "WOS" },
        { "Hereford", "HFD" },
        { "Shrewsbury", "SHR" },
        { "Wrexham General", "WRX" },
        { "Bangor", "BNG" },
        { "Holyhead", "HHD" },
        { "Truro", "TRU" },
        { "Penzance", "PNZ" },
        { "Newquay", "NQY" },
        { "St Ives", "SIV" },
        { "Falmouth Docks", "FAL" },
        { "Par", "PAR" },
        { "Taunton", "TAU" },
        { "Westbury", "WSB" },
        { "Weymouth", "WEY" },
        { "Torquay", "TQY" },
        { "Paignton", "PGN" },
        { "Newton Abbot", "NTA" },
        { "Totnes", "TOT" }
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
        // First sort by name, then assign IDs in alphabetical order
        return Stations
            .Select(s => new Station(s.Value, s.Key, 0)) // Temporary ID of 0
            .OrderBy(s => s.Name)
            .Select((station, index) => 
            {
                station.Id = index + 1; // Assign sequential IDs after sorting
                return station;
            })
            .ToList();
    }
}
