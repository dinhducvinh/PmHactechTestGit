using System.IO;
using System.Text.Json;

namespace ApiTest.Services
{
    public sealed class AppConfig
    {
        public DatabaseConfig Database { get; init; } = new();
        public ApiServerConfig ApiServer { get; init; } = new();
        public TestDataConfig TestData { get; init; } = new();

        private static AppConfig? _instance;
        private static readonly object _lock = new();

        public static AppConfig Instance
        {
            get
            {
                if (_instance != null) return _instance;
                lock (_lock)
                {
                    _instance ??= Load();
                    return _instance;
                }
            }
        }

        private static AppConfig Load()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            if (!File.Exists(path))
            {
                return new AppConfig();
            }

            try
            {
                var json = File.ReadAllText(path);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                };
                return JsonSerializer.Deserialize<AppConfig>(json, options) ?? new AppConfig();
            }
            catch
            {
                return new AppConfig();
            }
        }

        public void SaveBaseUrl(string newBaseUrl)
        {
            ApiServer.BaseUrl = newBaseUrl;
            var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(path, JsonSerializer.Serialize(this, options));
            }
            catch
            {
                // best-effort persistence
            }
        }
    }

    public sealed class DatabaseConfig
    {
        public string ConnectionString { get; set; } =
            "Server=(localdb)\\MSSQLLocalDB;Database=ApiTesterDb;Integrated Security=true;TrustServerCertificate=true;Connection Timeout=10";
        public string MasterConnectionString { get; set; } =
            "Server=(localdb)\\MSSQLLocalDB;Database=master;Integrated Security=true;TrustServerCertificate=true;Connection Timeout=10";
        public bool AutoInitialize { get; set; } = true;
        public int SeedUnregisteredCount { get; set; } = 100;
    }

    public sealed class ApiServerConfig
    {
        public string BaseUrl { get; set; } = "http://localhost:5080";
        public int TimeoutSeconds { get; set; } = 15;
    }

    public sealed class TestDataConfig
    {
        public string PhoneNumberPrefix { get; set; } = "099";
        public string DefaultPassword { get; set; } = "abc12345";
        public string DefaultUuid { get; set; } = "00000000-0000-0000-0000-000000000001";
        public string OtpStrategy { get; set; } = "static";
        public string StaticOtp { get; set; } = "123456";
        public string OtpResponseField { get; set; } = "data.otp";
    }
}
