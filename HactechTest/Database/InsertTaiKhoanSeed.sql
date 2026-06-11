/*
================================================================================
  Insert tài khoản seed chưa đăng ký

  Chạy sau CreateDatabase.sql.
  Mục đích:
    - Chuẩn bị 300 tài khoản chưa đăng ký trong dbo.taikhoan_seed.
    - App sẽ dùng các tài khoản này để gọi API signup và tạo dữ liệu thật
      trên server khi bấm "Kiểm tra seed".

  Lưu ý:
    - Script không insert lại số điện thoại đã được signup thành công trong
      dbo.taikhoan_signupthanhcong.
    - Nếu muốn làm sạch seed rồi insert lại từ đầu, chạy ResetSeedData.sql
      trước, sau đó chạy lại file này và InsertProvinceWardSeed.sql.
================================================================================
*/

USE HactechTestDb;
GO

SET NOCOUNT ON;

DECLARE @i INT = 1;
DECLARE @ma VARCHAR(6);
DECLARE @sdt NVARCHAR(20);
DECLARE @matKhau NVARCHAR(255);
DECLARE @uuid NVARCHAR(255);

WHILE @i <= 300
BEGIN
    SET @ma = RIGHT(REPLICATE('0', 6) + CONVERT(VARCHAR(6), @i), 6);
    SET @sdt = N'0909' + @ma;
    SET @matKhau = N'Test' + @ma;
    SET @uuid = N'thiet-bi-test-' + @ma;

    IF EXISTS (SELECT 1 FROM dbo.taikhoan_signupthanhcong WHERE sdt = @sdt)
    BEGIN
        SET @i += 1;
        CONTINUE;
    END

    IF EXISTS (SELECT 1 FROM dbo.taikhoan_seed WHERE sdt = @sdt)
    BEGIN
        UPDATE dbo.taikhoan_seed
        SET
            mat_khau_hien_tai = @matKhau,
            uuid = @uuid,
            trang_thai = N'san_sang',
            ghi_chu = N'Insert từ HactechTest/Database/InsertTaiKhoanSeed.sql',
            cap_nhat_luc = SYSDATETIME()
        WHERE sdt = @sdt;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.taikhoan_seed
            (sdt, mat_khau_hien_tai, uuid, trang_thai, ghi_chu)
        VALUES
            (@sdt, @matKhau, @uuid, N'san_sang',
             N'Insert từ HactechTest/Database/InsertTaiKhoanSeed.sql');
    END

    SET @i += 1;
END

GO

SELECT
    (SELECT COUNT(*) FROM dbo.taikhoan_seed) AS tai_khoan_chua_dang_ky,
    (SELECT COUNT(*) FROM dbo.taikhoan_signupthanhcong) AS tai_khoan_signup_thanh_cong;
GO
