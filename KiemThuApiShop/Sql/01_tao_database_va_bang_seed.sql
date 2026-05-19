IF DB_ID(N'ApiShopTestDb') IS NULL
BEGIN
    CREATE DATABASE ApiShopTestDb;
END
GO

USE ApiShopTestDb;
GO

IF OBJECT_ID(N'dbo.taikhoan_seed', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.taikhoan_seed
    (
        tk_seed_id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_taikhoan_seed PRIMARY KEY,
        tk_id NVARCHAR(64) NULL,
        sdt NVARCHAR(20) NOT NULL,
        mat_khau_hien_tai NVARCHAR(255) NOT NULL,
        uuid NVARCHAR(255) NOT NULL,
        trang_thai_dang_ky NVARCHAR(30) NOT NULL CONSTRAINT DF_taikhoan_seed_trang_thai_dang_ky DEFAULT N'chua_dang_ky',
        trang_thai NVARCHAR(30) NOT NULL CONSTRAINT DF_taikhoan_seed_trang_thai DEFAULT N'san_sang',
        dang_ky_luc DATETIME2(0) NULL,
        doi_mk_luc DATETIME2(0) NULL,
        ghi_chu NVARCHAR(500) NULL,
        tao_luc DATETIME2(0) NOT NULL CONSTRAINT DF_taikhoan_seed_tao_luc DEFAULT SYSDATETIME(),
        cap_nhat_luc DATETIME2(0) NULL
    );

    CREATE UNIQUE INDEX UX_taikhoan_seed_sdt ON dbo.taikhoan_seed(sdt);
END
GO

IF OBJECT_ID(N'dbo.danhmuc_seed', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.danhmuc_seed
    (
        dm_id NVARCHAR(64) NOT NULL CONSTRAINT PK_danhmuc_seed PRIMARY KEY,
        ten_danh_muc NVARCHAR(255) NOT NULL,
        dm_cha_id NVARCHAR(64) NULL,
        co_danh_muc_con BIT NOT NULL CONSTRAINT DF_danhmuc_seed_co_con DEFAULT 0,
        co_thuong_hieu BIT NOT NULL CONSTRAINT DF_danhmuc_seed_co_thuong_hieu DEFAULT 0,
        co_kich_co BIT NOT NULL CONSTRAINT DF_danhmuc_seed_co_kich_co DEFAULT 0,
        yeu_cau_can_nang BIT NOT NULL CONSTRAINT DF_danhmuc_seed_can_nang DEFAULT 0,
        sort INT NULL,
        trang_thai NVARCHAR(30) NOT NULL CONSTRAINT DF_danhmuc_seed_trang_thai DEFAULT N'san_sang',
        dong_bo_luc DATETIME2(0) NULL
    );
END
GO

IF OBJECT_ID(N'dbo.thuonghieu_seed', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.thuonghieu_seed
    (
        thuonghieu_id NVARCHAR(64) NOT NULL CONSTRAINT PK_thuonghieu_seed PRIMARY KEY,
        ten_thuong_hieu NVARCHAR(255) NOT NULL,
        dm_id NVARCHAR(64) NULL,
        trang_thai NVARCHAR(30) NOT NULL CONSTRAINT DF_thuonghieu_seed_trang_thai DEFAULT N'san_sang',
        dong_bo_luc DATETIME2(0) NULL
    );
END
GO

IF OBJECT_ID(N'dbo.diachitk_seed', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.diachitk_seed
    (
        diachitk_seed_id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_diachitk_seed PRIMARY KEY,
        tk_seed_id INT NOT NULL,
        tk_id NVARCHAR(64) NULL,
        diachi_id NVARCHAR(64) NOT NULL,
        ten_nguoi_nhan NVARCHAR(255) NOT NULL,
        sdt NVARCHAR(20) NOT NULL,
        diachi_daydu NVARCHAR(500) NOT NULL,
        phuong_xa_id INT NULL,
        vi_do DECIMAL(18,6) NULL,
        kinh_do DECIMAL(18,6) NULL,
        mac_dinh BIT NOT NULL CONSTRAINT DF_diachitk_seed_mac_dinh DEFAULT 1,
        trang_thai NVARCHAR(30) NOT NULL CONSTRAINT DF_diachitk_seed_trang_thai DEFAULT N'san_sang',
        tao_luc DATETIME2(0) NULL,
        CONSTRAINT FK_diachitk_seed_taikhoan_seed FOREIGN KEY (tk_seed_id) REFERENCES dbo.taikhoan_seed(tk_seed_id)
    );

    CREATE UNIQUE INDEX UX_diachitk_seed_diachi_id ON dbo.diachitk_seed(diachi_id);
END
GO

IF OBJECT_ID(N'dbo.sanpham_seed', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.sanpham_seed
    (
        sp_seed_id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_sanpham_seed PRIMARY KEY,
        sp_id NVARCHAR(64) NOT NULL,
        seller_tk_seed_id INT NOT NULL,
        seller_tk_id NVARCHAR(64) NULL,
        dm_id NVARCHAR(64) NOT NULL,
        thuonghieu_id NVARCHAR(64) NULL,
        diachi_id NVARCHAR(64) NULL,
        ten_sp NVARCHAR(255) NOT NULL,
        gia DECIMAL(18,2) NOT NULL,
        trang_thai NVARCHAR(30) NOT NULL CONSTRAINT DF_sanpham_seed_trang_thai DEFAULT N'san_sang',
        loai_seed NVARCHAR(80) NOT NULL CONSTRAINT DF_sanpham_seed_loai DEFAULT N'mac_dinh',
        tao_boi_test BIT NOT NULL CONSTRAINT DF_sanpham_seed_tao_boi_test DEFAULT 1,
        xac_minh_luc DATETIME2(0) NULL,
        ghi_chu NVARCHAR(500) NULL,
        CONSTRAINT FK_sanpham_seed_taikhoan_seed FOREIGN KEY (seller_tk_seed_id) REFERENCES dbo.taikhoan_seed(tk_seed_id)
    );

    CREATE UNIQUE INDEX UX_sanpham_seed_sp_id ON dbo.sanpham_seed(sp_id);
END
GO

IF OBJECT_ID(N'dbo.binhluan_sp_seed', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.binhluan_sp_seed
    (
        bl_seed_id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_binhluan_sp_seed PRIMARY KEY,
        sp_seed_id INT NOT NULL,
        sp_id NVARCHAR(64) NOT NULL,
        tk_seed_id INT NOT NULL,
        tk_id NVARCHAR(64) NULL,
        bl_id NVARCHAR(64) NULL,
        noi_dung NVARCHAR(1000) NOT NULL,
        trang_thai NVARCHAR(30) NOT NULL CONSTRAINT DF_binhluan_sp_seed_trang_thai DEFAULT N'san_sang',
        tao_luc DATETIME2(0) NULL,
        ghi_chu NVARCHAR(500) NULL,
        CONSTRAINT FK_binhluan_sp_seed_sanpham_seed FOREIGN KEY (sp_seed_id) REFERENCES dbo.sanpham_seed(sp_seed_id),
        CONSTRAINT FK_binhluan_sp_seed_taikhoan_seed FOREIGN KEY (tk_seed_id) REFERENCES dbo.taikhoan_seed(tk_seed_id)
    );
END
GO

IF OBJECT_ID(N'dbo.tk_thich_sanpham_seed', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.tk_thich_sanpham_seed
    (
        tk_seed_id INT NOT NULL,
        tk_id NVARCHAR(64) NULL,
        sp_seed_id INT NOT NULL,
        sp_id NVARCHAR(64) NOT NULL,
        trang_thai NVARCHAR(30) NOT NULL CONSTRAINT DF_tk_thich_sanpham_seed_trang_thai DEFAULT N'dang_like',
        tao_luc DATETIME2(0) NULL,
        CONSTRAINT PK_tk_thich_sanpham_seed PRIMARY KEY (tk_seed_id, sp_seed_id),
        CONSTRAINT FK_tk_thich_sanpham_seed_taikhoan_seed FOREIGN KEY (tk_seed_id) REFERENCES dbo.taikhoan_seed(tk_seed_id),
        CONSTRAINT FK_tk_thich_sanpham_seed_sanpham_seed FOREIGN KEY (sp_seed_id) REFERENCES dbo.sanpham_seed(sp_seed_id)
    );
END
GO

IF OBJECT_ID(N'dbo.tk_theodoi_seed', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.tk_theodoi_seed
    (
        tk_seed_id INT NOT NULL,
        tk_id NVARCHAR(64) NULL,
        followee_tk_seed_id INT NOT NULL,
        followee_tk_id NVARCHAR(64) NULL,
        trang_thai NVARCHAR(30) NOT NULL CONSTRAINT DF_tk_theodoi_seed_trang_thai DEFAULT N'dang_theo_doi',
        tao_luc DATETIME2(0) NOT NULL CONSTRAINT DF_tk_theodoi_seed_tao_luc DEFAULT SYSDATETIME(),
        CONSTRAINT PK_tk_theodoi_seed PRIMARY KEY (tk_seed_id, followee_tk_seed_id),
        CONSTRAINT FK_tk_theodoi_seed_tk FOREIGN KEY (tk_seed_id) REFERENCES dbo.taikhoan_seed(tk_seed_id),
        CONSTRAINT FK_tk_theodoi_seed_followee FOREIGN KEY (followee_tk_seed_id) REFERENCES dbo.taikhoan_seed(tk_seed_id)
    );
END
GO

IF OBJECT_ID(N'dbo.tk_chan_seed', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.tk_chan_seed
    (
        chan_tk_seed_id INT NOT NULL,
        chan_tk_id NVARCHAR(64) NULL,
        bi_chan_tk_seed_id INT NOT NULL,
        bi_chan_tk_id NVARCHAR(64) NULL,
        trang_thai NVARCHAR(30) NOT NULL CONSTRAINT DF_tk_chan_seed_trang_thai DEFAULT N'dang_chan',
        tao_luc DATETIME2(0) NOT NULL CONSTRAINT DF_tk_chan_seed_tao_luc DEFAULT SYSDATETIME(),
        CONSTRAINT PK_tk_chan_seed PRIMARY KEY (chan_tk_seed_id, bi_chan_tk_seed_id),
        CONSTRAINT FK_tk_chan_seed_chan FOREIGN KEY (chan_tk_seed_id) REFERENCES dbo.taikhoan_seed(tk_seed_id),
        CONSTRAINT FK_tk_chan_seed_bi_chan FOREIGN KEY (bi_chan_tk_seed_id) REFERENCES dbo.taikhoan_seed(tk_seed_id)
    );
END
GO

IF OBJECT_ID(N'dbo.donhang_sanpham_seed', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.donhang_sanpham_seed
    (
        donhang_sp_seed_id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_donhang_sanpham_seed PRIMARY KEY,
        donhang_id NVARCHAR(64) NOT NULL,
        sp_seed_id INT NOT NULL,
        sp_id NVARCHAR(64) NOT NULL,
        trang_thai NVARCHAR(30) NOT NULL CONSTRAINT DF_donhang_sanpham_seed_trang_thai DEFAULT N'san_sang',
        tao_luc DATETIME2(0) NOT NULL CONSTRAINT DF_donhang_sanpham_seed_tao_luc DEFAULT SYSDATETIME(),
        CONSTRAINT FK_donhang_sanpham_seed_sanpham_seed FOREIGN KEY (sp_seed_id) REFERENCES dbo.sanpham_seed(sp_seed_id)
    );
END
GO

IF OBJECT_ID(N'dbo.ketqua_testcase', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ketqua_testcase
    (
        ketqua_id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ketqua_testcase PRIMARY KEY,
        ma_testcase NVARCHAR(80) NOT NULL,
        nhom NVARCHAR(50) NOT NULL,
        ten_hien_thi NVARCHAR(255) NOT NULL,
        trang_thai NVARCHAR(30) NOT NULL,
        ma_mong_doi NVARCHAR(100) NULL,
        ma_thuc_te NVARCHAR(100) NULL,
        http_status INT NULL,
        endpoint NVARCHAR(255) NULL,
        thong_diep NVARCHAR(MAX) NULL,
        response_rut_gon NVARCHAR(MAX) NULL,
        chay_luc DATETIME2(0) NOT NULL CONSTRAINT DF_ketqua_testcase_chay_luc DEFAULT SYSDATETIME()
    );
END
GO
