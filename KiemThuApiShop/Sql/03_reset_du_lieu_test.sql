USE ApiShopTestDb;
GO

DELETE FROM dbo.ketqua_testcase;
DELETE FROM dbo.donhang_sanpham_seed;
DELETE FROM dbo.tk_chan_seed;
DELETE FROM dbo.tk_theodoi_seed;
DELETE FROM dbo.tk_thich_sanpham_seed;
DELETE FROM dbo.binhluan_sp_seed;
DELETE FROM dbo.sanpham_seed;
DELETE FROM dbo.diachitk_seed;
DELETE FROM dbo.thuonghieu_seed;
DELETE FROM dbo.danhmuc_seed;

UPDATE dbo.taikhoan_seed
SET
    tk_id = NULL,
    trang_thai_dang_ky = N'chua_dang_ky',
    trang_thai = N'san_sang',
    dang_ky_luc = NULL,
    doi_mk_luc = NULL,
    cap_nhat_luc = SYSDATETIME(),
    ghi_chu = N'Reset dữ liệu test; tài khoản sẵn sàng đăng ký lại'
WHERE tao_luc IS NOT NULL;
GO

SELECT COUNT(*) AS so_tai_khoan_con_lai FROM dbo.taikhoan_seed;
GO
