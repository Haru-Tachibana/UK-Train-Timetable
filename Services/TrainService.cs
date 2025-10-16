using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using TrainDashboard.DarwinService;

namespace TrainDashboard.Services
{
    public class TrainService
    {
        private readonly string _apiToken;
        private readonly string _endpointUrl = "https://lite.realtime.nationalrail.co.uk/OpenLDBWS/ldb12.asmx";

        public TrainService(string apiToken)
        {            
            _apiToken = apiToken ?? throw new ArgumentNullException(nameof(apiToken));
        }

        public async Task<StationBoard3?> GetDepartureBoardAsync(
            string stationCode,
            string? destinationCode = null,
            int numberOfRows = 10)
        {
            try
            {
                // Create SOAP client binding for HTTPS
                var binding = new BasicHttpsBinding(BasicHttpsSecurityMode.Transport)
                {
                    MaxReceivedMessageSize = 2000000 // allow large XML responses
                };

                var endpoint = new EndpointAddress(_endpointUrl);

                // Create the WCF client proxy
                using var client = new LDBServiceSoapClient(binding, endpoint);

                // Configure the access token
                var token = new AccessToken { TokenValue = _apiToken };

                // Set up filter type if destination is provided
                FilterType filterType = FilterType.to;

                // Call the API
                var response = await client.GetDepartureBoardAsync(
                    token,
                    (ushort)numberOfRows,
                    stationCode,
                    destinationCode,
                    !string.IsNullOrEmpty(destinationCode) ? filterType : FilterType.from,
                    0, // timeOffset
                    120 // timeWindow
                );

                return response?.GetStationBoardResult;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching departure board: {ex.Message}", ex);
            }
        }

    public static string GetServiceStatus<T>(T service) where T : ServiceItem
    {
        if (service.isCancelled)
            return "CANCELLED";

        var scheduled = service.std;
        var expected = service.etd;

        if (string.Equals(expected, "On time", StringComparison.OrdinalIgnoreCase))
            return "On time";

        if (string.Equals(expected, "Cancelled", StringComparison.OrdinalIgnoreCase))
            return "CANCELLED";

        if (string.Equals(expected, "Delayed", StringComparison.OrdinalIgnoreCase))
            return "Delayed";

        if (!string.IsNullOrEmpty(expected) && expected != scheduled)
            return "Delayed";

        return "On time";
    }        public static string GetDestinationString(ServiceItem3 service)
        {
            if (service.destination == null || service.destination.Length == 0)
                return "Unknown";

            var destinations = service.destination
                .Select(d => d.locationName ?? "Unknown")
                .ToList();

            return string.Join(" & ", destinations);
        }

        public static string GetOriginString(ServiceItem3 service)
        {
            if (service.origin == null || service.origin.Length == 0)
                return "Unknown";

            var origins = service.origin
                .Select(o => o.locationName ?? "Unknown")
                .ToList();

            return string.Join(" & ", origins);
        }

        /// <summary>
        /// Determines the train’s current running status (On time, Delayed, Cancelled, etc.)
        /// </summary>
        public static string GetServiceStatus(ServiceItem service)
        {
            if (service.isCancelled)
                return "CANCELLED";

            var scheduled = service.std;
            var expected = service.etd;

            if (string.Equals(expected, "On time", StringComparison.OrdinalIgnoreCase))
                return "On time";

            if (string.Equals(expected, "Cancelled", StringComparison.OrdinalIgnoreCase))
                return "CANCELLED";

            if (string.Equals(expected, "Delayed", StringComparison.OrdinalIgnoreCase))
                return "Delayed";

            if (!string.IsNullOrEmpty(expected) && expected != scheduled)
                return "Delayed";

            return "On time";
        }

        /// <summary>
        /// Formats the train’s destination(s) for display.
        /// </summary>
        public static string GetDestinationString(ServiceItem service)
        {
            if (service.destination == null || service.destination.Length == 0)
                return "Unknown";

            var destinations = service.destination
                .Select(d => d.locationName ?? "Unknown")
                .ToList();

            return string.Join(" & ", destinations);
        }

        /// <summary>
        /// Formats the train’s origin(s) for display.
        /// </summary>
        public static string GetOriginString(ServiceItem service)
        {
            if (service.origin == null || service.origin.Length == 0)
                return "Unknown";

            var origins = service.origin
                .Select(o => o.locationName ?? "Unknown")
                .ToList();

            return string.Join(" & ", origins);
        }
    }
}
