using System.Text.Json;
using System.Text.Json.Serialization;
using HactechTest.ApiShopTesting.Core;
using Microsoft.Data.SqlClient;

namespace HactechTest.Services.Configuration
{
    public sealed class CauHinhUngDung
    {
        private const string TenThuMucCauHinh = "HactechTest";
        private const string TenFileCauHinh = "cauhinh-ungdung.json";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private static CauHinhUngDung? _instance;
        private static readonly object _lock = new();

        public string ConnectionString { get; set; } = "";
        public string BaseUrl { get; set; } = "";
        public int TimeoutGiay { get; set; } = 60;
        public DateTimeOffset CapNhatLuc { get; set; }

        public static CauHinhUngDung Instance
        {
            get
            {
                if (_instance != null) return _instance;
                lock (_lock)
                {
                    _instance ??= Doc();
                    return _instance;
                }
            }
        }

        public static string ThuMucCauHinh =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), TenThuMucCauHinh);

        public static string DuongDanCauHinh =>
            Path.Combine(ThuMucCauHinh, TenFileCauHinh);

        public static CauHinhUngDung Doc()
        {
            var cauHinh = DocTuFile(DuongDanCauHinh) ?? new CauHinhUngDung();
            if (string.IsNullOrWhiteSpace(cauHinh.ConnectionString))
            {
                cauHinh.ConnectionString = DocConnectionStringCu();
            }

            if (string.IsNullOrWhiteSpace(cauHinh.BaseUrl))
            {
                cauHinh.BaseUrl = DocBaseUrlCu();
            }

            return cauHinh;
        }

        public static void TaiLai()
        {
            lock (_lock)
            {
                _instance = Doc();
            }
        }

        public static void LuuChuoiKetNoiDatabase(string connectionString)
        {
            var cauHinh = Instance;
            cauHinh.ConnectionString = connectionString.Trim();
            cauHinh.Luu();
        }

        public static void LuuBaseUrlApi(string baseUrl)
        {
            var cauHinh = Instance;
            cauHinh.BaseUrl = baseUrl.Trim().TrimEnd('/');
            cauHinh.Luu();
        }

        public void Luu()
        {
            CapNhatLuc = DateTimeOffset.Now;
            Directory.CreateDirectory(ThuMucCauHinh);
            File.WriteAllText(DuongDanCauHinh, JsonSerializer.Serialize(this, JsonOptions));

            lock (_lock)
            {
                _instance = this;
            }
        }

        public CauHinhChay TaoCauHinhChay()
        {
            var cauHinh = new CauHinhChay
            {
                BaseUrl = BaseUrl,
                TimeoutGiay = TimeoutGiay
            };

            if (string.IsNullOrWhiteSpace(ConnectionString))
            {
                return cauHinh;
            }

            return cauHinh with
            {
                ConnectionStrings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["HactechTestDb"] = ConnectionString
                }
            };
        }

        public Task<bool> KiemTraSchemaCoBanAsync(CancellationToken ct = default)
        {
            return KiemTraSchemaCoBanAsync(ConnectionString, ct);
        }

        public Task<SqlConnection> OpenConnectionAsync(CancellationToken ct = default)
        {
            return OpenConnectionAsync(ConnectionString, ct);
        }

        public static async Task<bool> KiemTraSchemaCoBanAsync(string connectionString, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return false;
            }

            try
            {
                await using var conn = await OpenConnectionAsync(connectionString, ct);
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = """
                    SELECT
                        CASE WHEN
                            OBJECT_ID(N'dbo.taikhoan_phanmemtest', N'U') IS NOT NULL AND
                            OBJECT_ID(N'dbo.phien_chay', N'U') IS NOT NULL AND
                            OBJECT_ID(N'dbo.chi_tiet_phien_chay', N'U') IS NOT NULL AND
                            OBJECT_ID(N'dbo.taikhoan_seed', N'U') IS NOT NULL AND
                            OBJECT_ID(N'dbo.taikhoan_signupthanhcong', N'U') IS NOT NULL AND
                            OBJECT_ID(N'dbo.test_case_dong', N'U') IS NOT NULL AND
                            OBJECT_ID(N'dbo.tk_thich_sanpham_seed', N'U') IS NOT NULL AND
                            OBJECT_ID(N'dbo.v_tong_quan', N'V') IS NOT NULL AND
                            COL_LENGTH(N'dbo.test_case_dong', N'path_params_json') IS NOT NULL AND
                            COL_LENGTH(N'dbo.taikhoan_seed', N'tk_id_server') IS NULL AND
                            COL_LENGTH(N'dbo.taikhoan_signupthanhcong', N'tk_id_server') IS NOT NULL AND
                            COL_LENGTH(N'dbo.taikhoan_signupthanhcong', N'sdt') IS NOT NULL AND
                            COL_LENGTH(N'dbo.tk_timkiem_seed', N'tk_id_server') IS NOT NULL AND
                            COL_LENGTH(N'dbo.tk_timkiem_seed', N'tk_seed_id') IS NULL AND
                            COL_LENGTH(N'dbo.tk_theodoi_seed', N'follower_tk_id_server') IS NOT NULL AND
                            COL_LENGTH(N'dbo.tk_theodoi_seed', N'followee_tk_id_server') IS NOT NULL AND
                            COL_LENGTH(N'dbo.tk_theodoi_seed', N'follower_tk_seed_id') IS NULL AND
                            COL_LENGTH(N'dbo.tk_theodoi_seed', N'followee_tk_seed_id') IS NULL AND
                            COL_LENGTH(N'dbo.tk_chan_seed', N'blocker_tk_id_server') IS NOT NULL AND
                            COL_LENGTH(N'dbo.tk_chan_seed', N'blocked_tk_id_server') IS NOT NULL AND
                            COL_LENGTH(N'dbo.tk_chan_seed', N'blocker_tk_seed_id') IS NULL AND
                            COL_LENGTH(N'dbo.tk_chan_seed', N'blocked_tk_seed_id') IS NULL AND
                            COL_LENGTH(N'dbo.diachi_tk_seed', N'tk_id_server') IS NOT NULL AND
                            COL_LENGTH(N'dbo.diachi_tk_seed', N'tk_seed_id') IS NULL AND
                            COL_LENGTH(N'dbo.sanpham_seed', N'tk_id_server') IS NOT NULL AND
                            COL_LENGTH(N'dbo.sanpham_seed', N'tk_seed_id') IS NULL AND
                            EXISTS (SELECT 1 FROM sys.key_constraints WHERE parent_object_id = OBJECT_ID(N'dbo.sanpham_seed') AND name = N'PK_sanpham_seed') AND
                            COL_LENGTH(N'dbo.tk_thich_sanpham_seed', N'tk_id_server') IS NOT NULL AND
                            COL_LENGTH(N'dbo.tk_thich_sanpham_seed', N'sp_id_server') IS NOT NULL AND
                            COL_LENGTH(N'dbo.tk_thich_sanpham_seed', N'tk_seed_id') IS NULL AND
                            COL_LENGTH(N'dbo.tk_thich_sanpham_seed', N'sp_seed_id') IS NULL AND
                            COL_LENGTH(N'dbo.tk_thich_sanpham_seed', N'trang_thai') IS NULL AND
                            COL_LENGTH(N'dbo.tinnhan_seed', N'sender_tk_id_server') IS NOT NULL AND
                            COL_LENGTH(N'dbo.tinnhan_seed', N'receiver_tk_id_server') IS NOT NULL AND
                            COL_LENGTH(N'dbo.tinnhan_seed', N'sender_tk_seed_id') IS NULL AND
                            COL_LENGTH(N'dbo.tinnhan_seed', N'receiver_tk_seed_id') IS NULL AND
                            COL_LENGTH(N'dbo.thongbao_seed', N'tk_id_server') IS NOT NULL AND
                            COL_LENGTH(N'dbo.thongbao_seed', N'tk_seed_id') IS NULL
                        THEN 1 ELSE 0 END;
                    """;
                return Convert.ToInt32(await cmd.ExecuteScalarAsync(ct)) == 1;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<SqlConnection> OpenConnectionAsync(string connectionString, CancellationToken ct = default)
        {
            var conn = new SqlConnection(connectionString);
            await conn.OpenAsync(ct);
            return conn;
        }

        private static CauHinhUngDung? DocTuFile(string duongDan)
        {
            if (!File.Exists(duongDan))
            {
                return null;
            }

            try
            {
                var json = File.ReadAllText(duongDan);
                return JsonSerializer.Deserialize<CauHinhUngDung>(json, JsonOptions);
            }
            catch
            {
                return null;
            }
        }

        private static string DocConnectionStringCu()
        {
            var duongDan = Path.Combine(ThuMucCauHinh, "cauhinh-database.json");
            return DocGiaTriChuoiTuJson(duongDan, "ConnectionString");
        }

        private static string DocBaseUrlCu()
        {
            var duongDan = Path.Combine(ThuMucCauHinh, "cauhinh-api.json");
            return DocGiaTriChuoiTuJson(duongDan, "BaseUrl");
        }

        private static string DocGiaTriChuoiTuJson(string duongDan, string tenThuocTinh)
        {
            if (!File.Exists(duongDan))
            {
                return "";
            }

            try
            {
                using var document = JsonDocument.Parse(File.ReadAllText(duongDan));
                if (document.RootElement.TryGetProperty(tenThuocTinh, out var element) &&
                    element.ValueKind == JsonValueKind.String)
                {
                    return element.GetString() ?? "";
                }
            }
            catch
            {
                return "";
            }

            return "";
        }
    }
}
