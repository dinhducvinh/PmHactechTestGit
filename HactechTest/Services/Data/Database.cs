using Microsoft.Data.SqlClient;

namespace HactechTest.Services.Data
{
    public sealed class Database
    {
        private readonly string _connectionString;

        public Database(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<bool> KiemTraSchemaCoBanAsync(CancellationToken ct = default)
        {
            try
            {
                await using var conn = await OpenConnectionAsync(ct);
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

        public async Task<SqlConnection> OpenConnectionAsync(CancellationToken ct = default)
        {
            var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);
            return conn;
        }
    }
}
