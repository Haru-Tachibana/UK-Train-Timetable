using System;
using System.Collections.Generic;
using System.Linq;

namespace TrainDashboard.Services;

/// <summary>
/// Station board model (our internal model for StationBoard3/BaseStationBoard)
/// </summary>
public class StationBoard
{
    public DateTime generatedAt { get; set; }
    public string? locationName { get; set; }
    public string? crs { get; set; } 
    public string? filterLocationName { get; set; }
    public string? filterCrs { get; set; }
    public bool platformAvailable { get; set; }
    public ServiceItem[]? trainServices { get; set; }
}

/// <summary>
/// Service item model (our internal model for ServiceItem2/ServiceItem3)
/// </summary>
public class ServiceItem
{
    public string? sta { get; set; }
    public string? eta { get; set; } 
    public string? std { get; set; }
    public string? etd { get; set; }
    public string? platform { get; set; }
    public string? @operator { get; set; } 
    public string? operatorCode { get; set; }
    public bool isCancelled { get; set; }
    public string? serviceType { get; set; }
    public int length { get; set; }
    public bool detachFront { get; set; }
    public bool isReverseFormation { get; set; }
    public string? cancelReason { get; set; }
    public string? delayReason { get; set; }
    public string? serviceID { get; set; }
    public string? adhocAlerts { get; set; }
    public ServiceLocation[]? destination { get; set; }
    public ServiceLocation[]? origin { get; set; }
    public ServiceLocation[]? currentOrigins { get; set; }
    public ServiceLocation[]? currentDestinations { get; set; }
}

/// <summary>
/// Service location model (our internal model for ServiceLocation)
/// </summary>
public class ServiceLocation
{
    public string? locationName { get; set; }
    public string? crs { get; set; }
    public string? via { get; set; }
    public string? futureChangeTo { get; set; }
    public bool assocIsCancelled { get; set; }
}