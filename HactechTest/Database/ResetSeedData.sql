/*
================================================================================
  Reset dữ liệu seed API Shop

  Mục đích:
    - Xóa dữ liệu mồi phụ thuộc: ví, saved search, follow, block, địa chỉ,
      danh mục, thương hiệu, sản phẩm, giỏ hàng, đơn hàng, like/report sản phẩm, tin nhắn, thông báo,
      tỉnh/thành phố, phường/xã.
    - Xóa tài khoản seed để chạy lại các file insert dữ liệu tĩnh từ đầu.
    - Reset identity của toàn bộ bảng seed.
    - Không xóa lịch sử chạy test: phien_chay, chi_tiet_phien_chay.

  Sau khi reset:
    - Chạy lại CreateDatabase.sql nếu muốn tạo lại tài khoản seed mặc định.
    - Hoặc bấm "Kiểm tra seed" trong phần mềm để đồng bộ lại dữ liệu mồi từ API.

  Chạy trên database: HactechTestDb
================================================================================
*/

USE HactechTestDb;
GO

DELETE FROM dbo.thongbao_seed;
DELETE FROM dbo.tinnhan_seed;
DELETE FROM dbo.reward_proof_seed;
DELETE FROM dbo.report_seed;
DELETE FROM dbo.tk_thich_sanpham_seed;
DELETE FROM dbo.donhang_seed;
DELETE FROM dbo.giohang_seed;
DELETE FROM dbo.sanpham_seed;
DELETE FROM dbo.diachi_tk_seed;
DELETE FROM dbo.tk_chan_seed;
DELETE FROM dbo.tk_theodoi_seed;
DELETE FROM dbo.tk_timkiem_seed;
DELETE FROM dbo.thuonghieu_seed;
DELETE FROM dbo.danhmuc_seed;
DELETE FROM dbo.Wards_seed;
DELETE FROM dbo.Provinces_seed;
DELETE FROM dbo.wallet_seed;
DELETE FROM dbo.taikhoan_signupthanhcong;
DELETE FROM dbo.taikhoan_seed;
GO

DBCC CHECKIDENT ('dbo.taikhoan_seed', RESEED, 0);
DBCC CHECKIDENT ('dbo.tk_timkiem_seed', RESEED, 0);
DBCC CHECKIDENT ('dbo.tk_theodoi_seed', RESEED, 0);
DBCC CHECKIDENT ('dbo.tk_chan_seed', RESEED, 0);
DBCC CHECKIDENT ('dbo.diachi_tk_seed', RESEED, 0);
DBCC CHECKIDENT ('dbo.tk_thich_sanpham_seed', RESEED, 0);
DBCC CHECKIDENT ('dbo.tinnhan_seed', RESEED, 0);
DBCC CHECKIDENT ('dbo.thongbao_seed', RESEED, 0);
GO

SELECT
    (SELECT COUNT(*) FROM dbo.taikhoan_seed) AS tai_khoan_chua_dang_ky,
    (SELECT COUNT(*) FROM dbo.taikhoan_signupthanhcong) AS tai_khoan_signup_thanh_cong,
    (SELECT COUNT(*) FROM dbo.wallet_seed) AS wallet_seed,
    (SELECT COUNT(*) FROM dbo.reward_proof_seed) AS reward_proof_seed,
    (SELECT COUNT(*) FROM dbo.giohang_seed) AS giohang_seed,
    (SELECT COUNT(*) FROM dbo.donhang_seed) AS donhang_seed,
    (SELECT COUNT(*) FROM dbo.report_seed) AS report_seed;
GO
