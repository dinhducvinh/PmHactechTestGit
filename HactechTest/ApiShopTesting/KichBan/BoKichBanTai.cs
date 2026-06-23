using System.Diagnostics;
using HactechTest.ApiShopTesting.Core;
using static HactechTest.ApiShopTesting.Core.HelperTC;
using HactechTest.ApiShopTesting.Seed;

namespace HactechTest.ApiShopTesting.KichBan;

public static partial class BoKichBanApi
{
    // Số lượng tài khoản sẽ được dùng để test tải (100 tài khoản khác nhau để mô phỏng 100 người dùng thật)
    private const int SoTaiKhoanKiemThuTai = 100;
    
    // Số lượng luồng (thread) chạy song song cùng 1 lúc (bắn 100 request đồng thời)
    private const int SoLuongChaySongSong = 100;

    // Hàm gốc để gọi 3 kịch bản tải vào danh sách chạy của phần mềm
    private static void ThemKichBanTai(List<KichBanApi> ds)
    {
        ThemKichBanTaiDangNhap(ds);
        ThemKichBanTaiAuthMe(ds);
        ThemKichBanTaiUserInfo(ds);
    }

    // Kịch bản 1: Load test API Đăng nhập
    private static void ThemKichBanTaiDangNhap(List<KichBanApi> ds)
    {
        ThemTai(ds,
            "LOAD-LOGIN-100-01",
            "Đăng nhập đồng thời 100 tài khoản seed",
            "Gửi 100 request POST /auth/login cùng lúc, mỗi request dùng một tài khoản seed đã đăng ký khác nhau.",
            loiToiDaPhanTram: 10, // Chấp nhận tối đa 10% request bị lỗi (nhận mã khác 1000)
            tbToiDaMs: 3000,      // Thời gian phản hồi trung bình không được vượt quá 3000 mili-giây (3 giây)
            p95ToiDaMs: 8000,     // 95% lượng request phải hoàn thành dưới mức 8000 mili-giây (8 giây)
            (ctx, ct) => ChayTaiAsync( // Hàm điều phối thực thi load test
                ctx,
                "LOAD-LOGIN-100-01",
                "Đăng nhập đồng thời 100 tài khoản seed",
                "POST /auth/login x100",
                loiToiDaPhanTram: 10,
                tbToiDaMs: 3000,
                p95ToiDaMs: 8000,
                ct,
                // Định nghĩa cách tạo dữ liệu gửi đi (body) cho từng yêu cầu (request) đăng nhập
                (taiKhoan, _) => Task.FromResult(new YeuCauApi(
                    HttpMethod.Post,
                    "/auth/login",
                    TaoBodyDangNhap(taiKhoan)))));
    }

    // Kịch bản 2: Load test API Lấy thông tin tài khoản hiện tại (/auth/me)
    private static void ThemKichBanTaiAuthMe(List<KichBanApi> ds)
    {
        ThemTai(ds,
            "LOAD-AUTH-ME-100-01",
            "100 tài khoản gửi GET /auth/me đồng thời",
            "Chuẩn bị token cho 100 tài khoản seed rồi gửi GET /auth/me cùng lúc bằng token riêng của từng tài khoản.",
            loiToiDaPhanTram: 10,
            tbToiDaMs: 2000,
            p95ToiDaMs: 6000,
            async (ctx, ct) =>
            {
                // Bước chuẩn bị: Lấy ra 100 tài khoản đã đăng ký từ cơ sở dữ liệu
                var taiKhoan = LayDanhSachTaiKhoanDaDangKyBatBuoc(ctx, SoTaiKhoanKiemThuTai, $"Cần {SoTaiKhoanKiemThuTai} tài khoản seed đã đăng ký để chạy test tải. Hãy bấm Kiểm tra seed để chuẩn bị đủ dữ liệu mới.");
                
                // Bước chuẩn bị: Lấy Token tự động cho 100 tài khoản này (giống như việc đăng nhập ngầm)
                var tokenTheoTaiKhoan = await LayTokenChoDanhSachTaiKhoanAsync(ctx, taiKhoan, ct);
                
                return await ChayTaiAsync(
                    ctx,
                    "LOAD-AUTH-ME-100-01",
                    "100 tài khoản gửi GET /auth/me đồng thời",
                    "GET /auth/me x100",
                    loiToiDaPhanTram: 10,
                    tbToiDaMs: 2000,
                    p95ToiDaMs: 6000,
                    ct,
                    // Định nghĩa yêu cầu gửi lên server: gọi method GET, kèm theo Token lấy từ mảng phía trên
                    (tk, _) => Task.FromResult(new YeuCauApi(
                        HttpMethod.Get,
                        "/auth/me",
                        token: tokenTheoTaiKhoan[tk.SoThuTu])),
                    taiKhoan);
            });
    }

    // Kịch bản 3: Load test API Lấy hồ sơ người dùng (/users/get_user_info)
    private static void ThemKichBanTaiUserInfo(List<KichBanApi> ds)
    {
        ThemTai(ds,
            "LOAD-USER-INFO-100-01",
            "100 tài khoản gửi POST /users/get_user_info đồng thời",
            "Chuẩn bị token cho 100 tài khoản seed rồi gửi POST /users/get_user_info cùng lúc, mỗi tài khoản đọc hộ sơ của chính mình.",
            loiToiDaPhanTram: 10,
            tbToiDaMs: 2500,
            p95ToiDaMs: 7000,
            async (ctx, ct) =>
            {
                var taiKhoan = LayDanhSachTaiKhoanDaDangKyBatBuoc(ctx, SoTaiKhoanKiemThuTai, $"Cần {SoTaiKhoanKiemThuTai} tài khoản seed đã đăng ký để chạy test tải. Hãy bấm Kiểm tra seed để chuẩn bị đủ dữ liệu mới.");
                var tokenTheoTaiKhoan = await LayTokenChoDanhSachTaiKhoanAsync(ctx, taiKhoan, ct);
                
                return await ChayTaiAsync(
                    ctx,
                    "LOAD-USER-INFO-100-01",
                    "100 tài khoản gửi POST /users/get_user_info đồng thời",
                    "POST /users/get_user_info x100",
                    loiToiDaPhanTram: 10,
                    tbToiDaMs: 2500,
                    p95ToiDaMs: 7000,
                    ct,
                    (tk, _) => Task.FromResult(new YeuCauApi(
                        HttpMethod.Post,
                        "/users/get_user_info",
                        new Dictionary<string, object?>(), // Body rỗng
                        tokenTheoTaiKhoan[tk.SoThuTu])),
                    taiKhoan);
            });
    }

    // Hàm tiện ích: Thêm 1 đối tượng Test Case (loại kiểm thử tải) vào danh sách kịch bản chung của hệ thống
    private static void ThemTai(
        List<KichBanApi> ds,
        string ma,
        string ten,
        string moTa,
        double loiToiDaPhanTram,
        int tbToiDaMs,
        int p95ToiDaMs,
        Func<NguCanhKiemThu, CancellationToken, Task<KetQuaChay>> chayRieng)
    {
        ds.Add(new KichBanApi
        {
            Ma = ma,
            Nhom = "Tải", // Nhóm danh mục kịch bản là "Tải"
            TenHienThi = ten,
            MoTa = $"{moTa} PASS nếu lỗi <= {loiToiDaPhanTram:0.##}%, TB <= {tbToiDaMs} ms và P95 <= {p95ToiDaMs} ms.",
            TaoYeuCauAsync = _ => Req(HttpMethod.Get, "/"), // Hàm giả, không dùng tới vì kịch bản tải có luồng chạy riêng
            ChayRiengAsync = chayRieng, // Nơi chứa logic thực thi load test
            MaChapNhan = Ok
        });
    }

    // Hàm cốt lõi thực thi việc giả lập Tải (Load) và tính toán kết quả
    private static async Task<KetQuaChay> ChayTaiAsync(
        NguCanhKiemThu ctx,
        string ma,
        string ten,
        string endpoint,
        double loiToiDaPhanTram,
        int tbToiDaMs,
        int p95ToiDaMs,
        CancellationToken cancellationToken,
        Func<TaiKhoanSignupThanhCongSeed, int, Task<YeuCauApi>> taoYeuCau,
        IReadOnlyList<TaiKhoanSignupThanhCongSeed>? danhSachTaiKhoan = null)
    {
        // 1. Lấy danh sách 100 tài khoản nếu chưa được truyền vào
        var taiKhoan = danhSachTaiKhoan ?? LayDanhSachTaiKhoanDaDangKyBatBuoc(ctx, SoTaiKhoanKiemThuTai, $"Cần {SoTaiKhoanKiemThuTai} tài khoản seed đã đăng ký để chạy test tải. Hãy bấm Kiểm tra seed để chuẩn bị đủ dữ liệu mới.");
        
        // 2. Bắt đầu bấm giờ tổng
        var dongHoTong = Stopwatch.StartNew();
        
        // 3. Gửi đồng loạt 100 request song song và đợi tất cả hoàn thành
        var ketQua = await GuiSongSongAsync(ctx, taiKhoan, SoLuongChaySongSong, taoYeuCau, cancellationToken);
        
        // 4. Dừng bấm giờ tổng
        dongHoTong.Stop();

        // 5. Tính toán các thống kê (metrics)
        var tong = ketQua.Count;                                       // Tổng số request đã gửi (mong đợi là 100)
        var dat = ketQua.Count(x => x.DatMa);                          // Số request thành công (code = 1000)
        var loi = tong - dat;                                          // Số request gặp lỗi
        var tiLeLoi = tong == 0 ? 100 : loi * 100d / tong;             // Tính % lỗi
        var tbMs = tong == 0 ? 0 : ketQua.Average(x => x.ThoiGian.TotalMilliseconds); // Tính thời gian phản hồi trung bình
        var p95Ms = TinhP95Ms(ketQua);                                 // Tính mốc thời gian P95
        
        // 6. Đánh giá xem có vượt qua tiêu chuẩn Pass/Fail theo cấu hình không
        var datNguong = tiLeLoi <= loiToiDaPhanTram && tbMs <= tbToiDaMs && p95Ms <= p95ToiDaMs;
        var tatCaLoiMoiTruong = dat == 0 && ketQua.All(x => x.LoiMoiTruong);
        
        // 7. Tạo chuỗi tóm tắt thông báo để hiển thị ra UI
        var thongDiep = TaoThongDiepTai(ketQua, loiToiDaPhanTram, tbToiDaMs, p95ToiDaMs, dongHoTong.Elapsed);

        return new KetQuaChay
        {
            Ma = ma,
            Nhom = "Tải",
            TenHienThi = ten,
            // Trạng thái chung: Đạt/Thất bại/Lỗi môi trường
            TrangThai = tatCaLoiMoiTruong
                ? TrangThaiKetQua.LoiMoiTruong
                : datNguong ? TrangThaiKetQua.Dat : TrangThaiKetQua.ThatBai, 
            ThongDiep = thongDiep,
            MaMongDoi = $"100 request, code 1000, lỗi <= {loiToiDaPhanTram:0.##}%, TB <= {tbToiDaMs} ms, P95 <= {p95ToiDaMs} ms",
            MaThucTe = $"Đạt {dat}/{tong}; lỗi {tiLeLoi:0.##}%; TB {tbMs:0} ms; P95 {p95Ms:0} ms",
            ThoiGian = dongHoTong.Elapsed,
            Endpoint = $"{endpoint}, song song {SoLuongChaySongSong}",
            ResponseRutGon = thongDiep
        };
    }

    // Hàm điều phối xử lý gửi request song song
    private static async Task<IReadOnlyList<KetQuaLanTai>> GuiSongSongAsync(
        NguCanhKiemThu ctx,
        IReadOnlyList<TaiKhoanSignupThanhCongSeed> taiKhoan,
        int soLuongSongSong,
        Func<TaiKhoanSignupThanhCongSeed, int, Task<YeuCauApi>> taoYeuCau,
        CancellationToken cancellationToken)
    {
        // Sử dụng SemaphoreSlim để giới hạn nghiêm ngặt chỉ cho phép tối đa 'soLuongSongSong' (vd: 100) luồng chạy cùng một lúc
        using var gioiHan = new SemaphoreSlim(soLuongSongSong);

        var congViec = taiKhoan.Select(async (tk, index) =>
        {
            // Luồng sẽ chờ ở đây nếu số lượng luồng đang gửi API đã đạt mức tối đa. Nếu còn trống, nó sẽ đi tiếp
            await gioiHan.WaitAsync(cancellationToken);
            try
            {
                // Thực hiện việc gửi 1 request độc lập
                return await GuiMotYeuCauTaiAsync(ctx, tk, index, taoYeuCau, cancellationToken);
            }
            finally
            {
                // Sau khi có response, nhả slot ra để luồng khác (nếu có) được phép chạy
                gioiHan.Release();
            }
        });

        // Đợi tất cả request hoàn thành mới gom kết quả lại
        return await Task.WhenAll(congViec);
    }

    // Hàm thực hiện việc gửi và đo lường kết quả của 1 request duy nhất
    private static async Task<KetQuaLanTai> GuiMotYeuCauTaiAsync(
        NguCanhKiemThu ctx,
        TaiKhoanSignupThanhCongSeed taiKhoan,
        int index,
        Func<TaiKhoanSignupThanhCongSeed, int, Task<YeuCauApi>> taoYeuCau,
        CancellationToken cancellationToken)
    {
        var dongHo = Stopwatch.StartNew(); // Bắt đầu bấm giờ cho riêng request này
        try
        {
            // Cấu trúc nội dung request sẽ gửi
            var yeuCau = await taoYeuCau(taiKhoan, index);
            cancellationToken.ThrowIfCancellationRequested();
            
            // Tiến hành gọi API tới Server
            var response = await ctx.Api.GuiAsync(yeuCau, cancellationToken);
            
            dongHo.Stop(); // Đã nhận được response => Dừng bấm giờ ngay

            // Kiểm tra xem server có trả về mã mong đợi (ví dụ: code=1000) không
            var datMa = Ok.Contains(response.MaSoSanh);
            var loi = datMa
                ? null
                : $"tk_id_server {taiKhoan.TaiKhoanIdServer}: mã thực tế {response.MaSoSanh}, HTTP {(int)response.HttpStatusCode}, message {response.Message ?? "(không có)"}";
            
            // Trả về kết quả đo lường của request này
            return new KetQuaLanTai(datMa, response.MaSoSanh, dongHo.Elapsed, loi, LoiMoiTruong: false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Nếu người dùng chủ động bấm Hủy test
            throw;
        }
        catch (TaskCanceledException ex)
        {
            // Lỗi quá thời gian phản hồi (Timeout)
            dongHo.Stop();
            return new KetQuaLanTai(false, "TIMEOUT", dongHo.Elapsed, $"tk_id_server {taiKhoan.TaiKhoanIdServer}: quá timeout. {ex.Message}", LoiMoiTruong: true);
        }
        catch (HttpRequestException ex)
        {
            // Lỗi mạng, kết nối, DNS, server chết...
            dongHo.Stop();
            return new KetQuaLanTai(false, "ENV_ERROR", dongHo.Elapsed, $"tk_id_server {taiKhoan.TaiKhoanIdServer}: không gửi được API. {ex.Message}", LoiMoiTruong: true);
        }
        catch (Exception ex)
        {
            // Các lỗi phát sinh khác
            dongHo.Stop();
            return new KetQuaLanTai(false, "ERROR", dongHo.Elapsed, $"tk_id_server {taiKhoan.TaiKhoanIdServer}: {RutGon(ex.Message, 180)}", LoiMoiTruong: false);
        }
    }

    // Hàm tính P95: 95th Percentile (Phân vị thứ 95)
    // Giải thích: 
    // Sắp xếp thời gian chạy của tất cả các request từ nhanh nhất đến chậm nhất.
    // Lấy thời gian phản hồi của request nằm ở vị trí thứ 95%. 
    // Điều đó có nghĩa là 95% lượng request chạy NHANH HƠN HOẶC BẰNG mức thời gian này. Đây là chỉ số phản ánh tốc độ chuẩn xác hơn mức Trung Bình.
    private static double TinhP95Ms(IReadOnlyList<KetQuaLanTai> ketQua)
    {
        if (ketQua.Count == 0)
        {
            return 0;
        }

        // Lấy danh sách thời gian và sắp xếp tăng dần
        var thoiGian = ketQua
            .Select(x => x.ThoiGian.TotalMilliseconds)
            .OrderBy(x => x) 
            .ToList();
            
        // Tìm index của phần tử ở mức 95%
        var viTri = Math.Clamp((int)Math.Ceiling(thoiGian.Count * 0.95) - 1, 0, thoiGian.Count - 1);
        return thoiGian[viTri];
    }

    // Hàm tạo chuỗi nội dung báo cáo thống kê, sẽ được ghi vào DB và hiện trên UI
    private static string TaoThongDiepTai(
        IReadOnlyList<KetQuaLanTai> ketQua,
        double loiToiDaPhanTram,
        int tbToiDaMs,
        int p95ToiDaMs,
        TimeSpan thoiGianTong)
    {
        var tong = ketQua.Count;
        var dat = ketQua.Count(x => x.DatMa);
        var loi = tong - dat;
        var tiLeLoi = tong == 0 ? 100 : loi * 100d / tong;
        var tbMs = tong == 0 ? 0 : ketQua.Average(x => x.ThoiGian.TotalMilliseconds);
        var p95Ms = TinhP95Ms(ketQua);
        
        // Nhóm và thống kê xem có bao nhiêu request trả về mã code nào (vd: 1000: 95, 1004: 5, timeout: 2)
        var maThucTe = string.Join(", ", ketQua
            .GroupBy(x => x.MaThucTe)
            .OrderByDescending(x => x.Count())
            .ThenBy(x => x.Key)
            .Take(5)
            .Select(x => $"{x.Key}: {x.Count()}"));
            
        // Trích xuất 3 thông báo lỗi đại diện đầu tiên để người dùng dễ nhìn thấy tại sao lỗi
        var mauLoi = string.Join(" | ", ketQua
            .Where(x => !x.DatMa && !string.IsNullOrWhiteSpace(x.Loi))
            .Select(x => x.Loi!)
            .Distinct()
            .Take(3));

        var thongDiep =
            $"Chạy {tong} request, song song {SoLuongChaySongSong}. Đạt {dat}/{tong}, lỗi {loi} ({tiLeLoi:0.##}%). " +
            $"TB {tbMs:0} ms, P95 {p95Ms:0} ms, tổng {(int)Math.Round(thoiGianTong.TotalMilliseconds)} ms. " +
            $"Ngưỡng: lỗi <= {loiToiDaPhanTram:0.##}%, TB <= {tbToiDaMs} ms, P95 <= {p95ToiDaMs} ms. Mã thực tế: {maThucTe}.";

        return string.IsNullOrWhiteSpace(mauLoi)
            ? thongDiep
            : $"{thongDiep} Mẫu lỗi: {mauLoi}";
    }

    // Một class chứa dữ liệu thu thập được từ 1 request đơn lẻ (thành công/thất bại, tốn bao lâu, lỗi chữ là gì)
    private sealed record KetQuaLanTai(
        bool DatMa,
        string MaThucTe,
        TimeSpan ThoiGian,
        string? Loi,
        bool LoiMoiTruong);
}
