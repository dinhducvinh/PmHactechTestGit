USE ApiShopTestDb;
GO

/*
    Script này chỉ tạo danh sách 100 số điện thoại seed ở trạng thái chưa đăng ký.
    Không gọi API và không đánh dấu tài khoản đã đăng ký tại bước SQL.

    Cột mat_khau_hien_tai ở thời điểm này là mật khẩu dự kiến sẽ dùng để gọi
    POST /auth/signup. Sau khi signup thành công, chương trình C# sẽ cập nhật lại:
      - tk_id_server
      - trang_thai_dang_ky = N'da_dang_ky'
      - mat_khau_hien_tai = mật khẩu thực tế vừa dùng để signup
      - dang_ky_luc

    Chạy lại script này sẽ không ghi đè các tài khoản đã tồn tại, tránh làm lệch
    dữ liệu seed đã đồng bộ với server.
*/

DECLARE @i INT = 1;
DECLARE @sdt NVARCHAR(20);
DECLARE @matKhauDuKien NVARCHAR(255);
DECLARE @uuid NVARCHAR(255);

WHILE @i <= 100
BEGIN
    SET @sdt = N'0909' + RIGHT(N'000000' + CONVERT(NVARCHAR(10), @i), 6);
    SET @matKhauDuKien = N'Test' + RIGHT(N'000000' + CONVERT(NVARCHAR(10), @i), 6);
    SET @uuid = N'thiet-bi-test-' + RIGHT(N'000000' + CONVERT(NVARCHAR(10), @i), 6);

    IF NOT EXISTS (SELECT 1 FROM dbo.taikhoan_seed WHERE sdt = @sdt)
    BEGIN
        INSERT INTO dbo.taikhoan_seed
        (
            tk_id_server,
            sdt,
            mat_khau_hien_tai,
            uuid,
            trang_thai_dang_ky,
            trang_thai,
            dang_ky_luc,
            doi_mk_luc,
            ghi_chu
        )
        VALUES
        (
            NULL,
            @sdt,
            @matKhauDuKien,
            @uuid,
            N'chua_dang_ky',
            N'san_sang',
            NULL,
            NULL,
            N'Số điện thoại seed tạo thủ công bằng SQL; chưa đăng ký trên server'
        );
    END

    SET @i += 1;
END
GO

SELECT
    COUNT(*) AS tong_tai_khoan_seed,
    SUM(CASE WHEN trang_thai_dang_ky = N'chua_dang_ky' THEN 1 ELSE 0 END) AS chua_dang_ky,
    SUM(CASE WHEN trang_thai_dang_ky = N'da_dang_ky' THEN 1 ELSE 0 END) AS da_dang_ky
FROM dbo.taikhoan_seed;
GO
