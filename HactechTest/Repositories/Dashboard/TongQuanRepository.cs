using HactechTest.Models.Dashboard;
using Microsoft.Data.SqlClient;

namespace HactechTest.Repositories.Dashboard;

public sealed class TongQuanRepository
{
    private readonly string _connectionString;

    public TongQuanRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<TongQuanThongKe> LayThongKeAsync(BoLocTongQuan boLoc, CancellationToken ct = default)
    {
        await using var conn = await KetNoiSql.MoAsync(_connectionString, ct);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT
                ISNULL(SUM(tong_so_test), 0),
                ISNULL(SUM(so_dat), 0),
                ISNULL(SUM(so_khong_dat), 0)
            FROM dbo.phien_chay
            WHERE thoi_diem_chay >= @tu_ngay
              AND thoi_diem_chay < @den_ngay
              AND (@nguoi_chay IS NULL OR nguoi_chay = @nguoi_chay);
            """;

        cmd.Parameters.Add("@tu_ngay", System.Data.SqlDbType.DateTime2).Value = boLoc.NgayLoc.Date;
        cmd.Parameters.Add("@den_ngay", System.Data.SqlDbType.DateTime2).Value = boLoc.NgayLoc.Date.AddDays(1);
        cmd.Parameters.Add("@nguoi_chay", System.Data.SqlDbType.NVarChar, 200).Value =
            string.IsNullOrWhiteSpace(boLoc.NguoiChay) ? DBNull.Value : boLoc.NguoiChay;

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
        {
            return new TongQuanThongKe(0, 0, 0);
        }

        return new TongQuanThongKe(
            Convert.ToInt32(reader.GetValue(0)),
            Convert.ToInt32(reader.GetValue(1)),
            Convert.ToInt32(reader.GetValue(2)));
    }

    public async Task<IReadOnlyList<string>> LayNguoiChayAsync(CancellationToken ct = default)
    {
        var ketQua = new List<string>();
        await using var conn = await KetNoiSql.MoAsync(_connectionString, ct);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT DISTINCT nguoi_chay
            FROM dbo.phien_chay
            WHERE nguoi_chay IS NOT NULL AND LTRIM(RTRIM(nguoi_chay)) <> N''
            ORDER BY nguoi_chay;
            """;

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            ketQua.Add(reader.GetString(0));
        }

        return ketQua;
    }
}
