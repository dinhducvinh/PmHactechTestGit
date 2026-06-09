/*
================================================================================
  Insert dữ liệu seed tỉnh/thành phố và phường/xã

  Chạy trên database: HactechTestDb
  Có thể chạy nhiều lần, dữ liệu sẽ được cập nhật nếu đã tồn tại.
================================================================================
*/

USE HactechTestDb;
GO

MERGE dbo.Provinces_seed AS target
USING (VALUES
    (1, N'Hà Nội'),
    (2, N'Bắc Ninh'),
    (3, N'Ninh Bình'),
    (4, N'Hải Phòng'),
    (5, N'Quảng Ninh')
) AS source (id, name)
ON target.id = source.id
WHEN MATCHED THEN
    UPDATE SET name = source.name
WHEN NOT MATCHED THEN
    INSERT (id, name)
    VALUES (source.id, source.name);
GO

MERGE dbo.Wards_seed AS target
USING (VALUES
    (1,  N'Phường Hoàn Kiếm',  1),
    (2,  N'Phường Cửa Nam',    1),
    (3,  N'Phường Ba Đình',    1),
    (4,  N'Phường Giảng Võ',   1),
    (5,  N'Phường Đống Đa',    1),
    (6,  N'Phường Cầu Giấy',   1),
    (7,  N'Phường Suối Hoa',   2),
    (8,  N'Phường Võ Cường',   2),
    (9,  N'Phường Vũ Ninh',    2),
    (10, N'Phường Kinh Bắc',   2),
    (11, N'Phường Đại Phúc',   2),
    (12, N'Phường Đông Thành', 3),
    (13, N'Phường Tân Thành',  3),
    (14, N'Phường Ninh Khánh', 3),
    (15, N'Phường Ninh Sơn',   3),
    (16, N'Phường Nam Bình',   3),
    (17, N'Phường Hồng Bàng',  4),
    (18, N'Phường Lê Chân',    4),
    (19, N'Phường Ngô Quyền',  4),
    (20, N'Phường Lạch Tray',  4),
    (21, N'Phường Máy Tơ',     4),
    (22, N'Phường Đằng Giang', 4),
    (23, N'Phường Hồng Gai',   5),
    (24, N'Phường Bãi Cháy',   5),
    (25, N'Phường Hồng Hải',   5),
    (26, N'Phường Cao Xanh',   5),
    (27, N'Phường Hà Khẩu',    5),
    (28, N'Phường Cẩm Thành',  5)
) AS source (id, name, provinces_id)
ON target.id = source.id
WHEN MATCHED THEN
    UPDATE SET
        name = source.name,
        provinces_id = source.provinces_id
WHEN NOT MATCHED THEN
    INSERT (id, name, provinces_id)
    VALUES (source.id, source.name, source.provinces_id);
GO

SELECT COUNT(*) AS tong_tinh_thanh FROM dbo.Provinces_seed;
SELECT COUNT(*) AS tong_phuong_xa FROM dbo.Wards_seed;
GO
