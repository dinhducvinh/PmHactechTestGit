USE ApiShopTestDb;
GO

DECLARE @i INT = 1;
DECLARE @sdt NVARCHAR(20);
DECLARE @matKhau NVARCHAR(255);
DECLARE @uuid NVARCHAR(255);

WHILE @i <= 100
BEGIN
    SET @sdt = N'0909' + RIGHT(N'000000' + CONVERT(NVARCHAR(10), @i), 6);
    SET @matKhau = N'Test' + RIGHT(N'000000' + CONVERT(NVARCHAR(10), @i), 6);
    SET @uuid = N'thiet-bi-test-' + RIGHT(N'000000' + CONVERT(NVARCHAR(10), @i), 6);

    IF NOT EXISTS (SELECT 1 FROM dbo.taikhoan_seed WHERE sdt = @sdt)
    BEGIN
        INSERT INTO dbo.taikhoan_seed
        (
            sdt,
            mat_khau_hien_tai,
            uuid,
            trang_thai_dang_ky,
            trang_thai,
            ghi_chu
        )
        VALUES
        (
            @sdt,
            @matKhau,
            @uuid,
            N'chua_dang_ky',
            N'san_sang',
            N'Tài khoản seed tạo thủ công bằng SQL để test API shop'
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
