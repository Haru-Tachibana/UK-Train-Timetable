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

        /// <summary>
        /// Fetches detailed information about a specific train service
        /// </summary>
        public async Task<ServiceDetails1?> GetServiceDetailsAsync(string serviceId)
        {
            try
            {
                // Create SOAP client binding for HTTPS
                var binding = new BasicHttpsBinding(BasicHttpsSecurityMode.Transport)
                {
                    MaxReceivedMessageSize = 2000000, // allow large XML responses
                    SendTimeout = TimeSpan.FromSeconds(30),
                    ReceiveTimeout = TimeSpan.FromSeconds(30)
                };

                var endpoint = new EndpointAddress(_endpointUrl);

                // Create the WCF client proxy
                using var client = new LDBServiceSoapClient(binding, endpoint);

                // Configure the access token
                var token = new AccessToken { TokenValue = _apiToken };

                // Call the API
                var response = await client.GetServiceDetailsAsync(token, serviceId);

                return response?.GetServiceDetailsResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Error: {ex.Message}");
                return null; // Return null instead of throwing to provide better user experience
            }
        }

        public async Task<StationBoard3?> GetDepartureBoardAsync(
            string stationCode,
            string? destinationCode = null,
            int numberOfRows = 10,
            int timeOffset = 0,
            int timeWindow = 120)
        {
            try
            {
                // Create SOAP client binding for HTTPS
                var binding = new BasicHttpsBinding(BasicHttpsSecurityMode.Transport)
                {
                    MaxReceivedMessageSize = 2000000, // allow large XML responses
                    SendTimeout = TimeSpan.FromSeconds(30),
                    ReceiveTimeout = TimeSpan.FromSeconds(30)
                };

                var endpoint = new EndpointAddress(_endpointUrl);

                // Create the WCF client proxy
                using var client = new LDBServiceSoapClient(binding, endpoint);

                // Configure the access token
                var token = new AccessToken { TokenValue = _apiToken };

                // Set up filter type if destination is provided
                FilterType filterType = FilterType.to;

                // Make sure station codes are uppercase
                if (stationCode != null)
                {
                    stationCode = stationCode.ToUpper();
                }
                if (destinationCode != null)
                {
                    destinationCode = destinationCode.ToUpper();
                }

                // Call the API with custom time window
                var response = await client.GetDepartureBoardAsync(
                    token,
                    (ushort)numberOfRows,
                    stationCode,
                    destinationCode,
                    !string.IsNullOrEmpty(destinationCode) ? filterType : FilterType.from,
                    timeOffset,   // Minutes from now to start showing trains
                    timeWindow    // Minutes window to show trains
                );

                return response?.GetStationBoardResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Error: {ex.Message}");
                return null; // Return null instead of throwing to provide better user experience
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
