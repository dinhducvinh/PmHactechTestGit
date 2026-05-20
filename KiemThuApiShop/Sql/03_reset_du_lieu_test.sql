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
DELETE FROM taikhoan_seed;


-- Reset IDENTITY về 0 (giá trị tiếp theo sẽ là 1)
DBCC CHECKIDENT ('dbo.ketqua_testcase',       RESEED, 0);
DBCC CHECKIDENT ('dbo.donhang_sanpham_seed',  RESEED, 0);
DBCC CHECKIDENT ('dbo.tk_chan_seed',          RESEED, 0);
DBCC CHECKIDENT ('dbo.tk_theodoi_seed',       RESEED, 0);
DBCC CHECKIDENT ('dbo.tk_thich_sanpham_seed', RESEED, 0);
DBCC CHECKIDENT ('dbo.binhluan_sp_seed',      RESEED, 0);
DBCC CHECKIDENT ('dbo.sanpham_seed',          RESEED, 0);
DBCC CHECKIDENT ('dbo.diachitk_seed',         RESEED, 0);
DBCC CHECKIDENT ('dbo.thuonghieu_seed',       RESEED, 0);
DBCC CHECKIDENT ('dbo.danhmuc_seed',          RESEED, 0);
DBCC CHECKIDENT ('dbo.taikhoan_seed',          RESEED, 0);

GO