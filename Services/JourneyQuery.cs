using System;

namespace TrainDashboard.Services;

/// <summary>
/// Represents a parsed natural language journey query
/// </summary>
public class JourneyQuery
{
    /// <summary>
    /// Departure station name or CRS code
    /// </summary>
    public string? DepartureStation { get; set; }

    /// <summary>
    /// Destination station name or CRS code (optional)
    /// </summary>
    public string? DestinationStation { get; set; }

    /// <summary>
    /// Preferred departure time (optional)
    /// </summary>
    public TimeSpan? PreferredDepartureTime { get; set; }

    /// <summary>
    /// Preferred arrival time (optional)
    /// </summary>
    public TimeSpan? PreferredArrivalTime { get; set; }

    /// <summary>
    /// Whether this is for departures (true) or arrivals (false)
    /// </summary>
    public bool IsDeparture { get; set; } = true;

    /// <summary>
    /// Date for the journey (defaults to today)
    /// </summary>
    public DateTime JourneyDate { get; set; } = DateTime.Today;

    /// <summary>
    /// Indicates whether the query was successfully parsed
    /// </summary>
    public bool IsValid => !string.IsNullOrEmpty(DepartureStation);

    /// <summary>
    /// Confidence score from the AI parsing (0-1)
    /// </summary>
    public double Confidence { get; set; } = 1.0;

    /// <summary>
    /// Any additional context or notes from the query
    /// </summary>
    public string? Notes { get; set; }
}
