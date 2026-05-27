USE ApiShopTestDb;
GO

DELETE FROM dbo.tk_chan_seed;
DELETE FROM dbo.tk_theodoi_seed;
DELETE FROM dbo.tk_timkiem_seed;
DELETE FROM dbo.ketqua_testcase;

UPDATE dbo.taikhoan_seed
SET tk_id_server = NULL,
    trang_thai_dang_ky = N'chua_dang_ky',
    trang_thai = N'san_sang',
    dang_ky_luc = NULL,
    doi_mk_luc = NULL,
    cap_nhat_luc = SYSDATETIME();

DBCC CHECKIDENT ('dbo.ketqua_testcase', RESEED, 0);
DBCC CHECKIDENT ('dbo.tk_timkiem_seed', RESEED, 0);
IF COLUMNPROPERTY(OBJECT_ID(N'dbo.tk_theodoi_seed'), N'td_seed_id', 'IsIdentity') = 1
    DBCC CHECKIDENT ('dbo.tk_theodoi_seed', RESEED, 0);
IF COLUMNPROPERTY(OBJECT_ID(N'dbo.tk_chan_seed'), N'chan_seed_id', 'IsIdentity') = 1
    DBCC CHECKIDENT ('dbo.tk_chan_seed', RESEED, 0);
GO
