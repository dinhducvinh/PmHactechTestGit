namespace ApiTest.Services
{
    /// <summary>
    /// Container đơn giản giữ singletons (config, database, runner) để các UserControl
    /// truy cập mà không phải tự khởi tạo lại. Khởi tạo 1 lần ở Program.cs.
    /// </summary>
    public sealed class AppHost
    {
        public AppConfig Config { get; }
        public Database Database { get; }
        public SeedAccountStore SeedStore { get; }
        public PhienChayStore PhienChayStore { get; }
        public Runner.IOtpProvider OtpProvider { get; }
        public Runner.ApiTestRunner Runner { get; }

        public string DbStartupMessage { get; }
        public bool DbAvailable { get; }
        public string? DbError { get; }
        public int SeededUnregistered { get; }

        private static AppHost? _instance;

        public static AppHost Instance =>
            _instance ?? throw new InvalidOperationException("AppHost chưa được khởi tạo. Gọi InitializeAsync trước.");

        public static bool IsInitialized => _instance != null;

        public static async Task<AppHost> InitializeAsync(CancellationToken ct = default)
        {
            var config = AppConfig.Instance;
            var database = new Database(config.Database);
            var seed = new SeedAccountStore(database, config.TestData);
            var phien = new PhienChayStore(database);
            var otp = new Runner.OtpProvider(config.TestData);
            var runner = new Runner.ApiTestRunner(config);

            string startupMsg;
            bool available;
            string? err = null;
            int seededCount = 0;

            try
            {
                if (config.Database.AutoInitialize)
                {
                    startupMsg = await database.EnsureDatabaseAsync(ct);
                }
                else
                {
                    startupMsg = await database.TestConnectionAsync(ct)
                        ? "Đã kết nối CSDL ApiTesterDb."
                        : "Không kết nối được CSDL (AutoInitialize=false).";
                }
                available = await database.TestConnectionAsync(ct);

                if (available && config.Database.SeedUnregisteredCount > 0)
                {
                    seededCount = await seed.EnsureUnregisteredSeedAsync(
                        config.Database.SeedUnregisteredCount, ct);
                }
            }
            catch (Exception ex)
            {
                startupMsg = "Lỗi khởi tạo CSDL.";
                available = false;
                err = ex.Message;
            }

            _instance = new AppHost(config, database, seed, phien, otp, runner,
                startupMsg, available, err, seededCount);
            return _instance;
        }

        private AppHost(
            AppConfig config, Database database, SeedAccountStore seed, PhienChayStore phien,
            Runner.IOtpProvider otp, Runner.ApiTestRunner runner,
            string startupMsg, bool available, string? err, int seededCount)
        {
            Config = config;
            Database = database;
            SeedStore = seed;
            PhienChayStore = phien;
            OtpProvider = otp;
            Runner = runner;
            DbStartupMessage = startupMsg;
            DbAvailable = available;
            DbError = err;
            SeededUnregistered = seededCount;
        }

        public TestContext CreateTestContext(string baseUrl)
        {
            return new TestContext
            {
                Config = Config,
                Database = Database,
                SeedStore = SeedStore,
                OtpProvider = OtpProvider,
                Runner = Runner,
                BaseUrl = baseUrl
            };
        }
    }
}
