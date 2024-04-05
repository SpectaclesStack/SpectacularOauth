namespace SpectacularStackAuth.Domain.Model
{
        public class DeviceVerification
        {
            public string? device_code { get; set; }
            public int expires_in { get; set; }
            public int interval { get; set; }
            public string? user_code { get; set; }
            public string? verification_uri { get; set; }
        }
}
