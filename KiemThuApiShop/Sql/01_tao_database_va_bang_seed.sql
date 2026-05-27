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
        tk_id_server NVARCHAR(64) NULL,
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

IF OBJECT_ID(N'dbo.tk_timkiem_seed', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.tk_timkiem_seed
    (
        tk_timkiem_seed_id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_tk_timkiem_seed PRIMARY KEY,
        tk_seed_id INT NOT NULL,
        tk_id_server NVARCHAR(64) NULL,
        saved_search_id_server NVARCHAR(64) NULL,
        keyword NVARCHAR(255) NOT NULL,
        trang_thai NVARCHAR(30) NOT NULL CONSTRAINT DF_tk_timkiem_seed_trang_thai DEFAULT N'dang_luu',
        tao_boi_test BIT NOT NULL CONSTRAINT DF_tk_timkiem_seed_tao_boi_test DEFAULT 1,
        tao_luc DATETIME2(0) NULL,
        xoa_luc DATETIME2(0) NULL,
        ghi_chu NVARCHAR(500) NULL,
        CONSTRAINT FK_tk_timkiem_seed_taikhoan_seed FOREIGN KEY (tk_seed_id) REFERENCES dbo.taikhoan_seed(tk_seed_id)
    );

    CREATE INDEX IX_tk_timkiem_seed_tk_trang_thai ON dbo.tk_timkiem_seed(tk_seed_id, trang_thai);
END
GO

IF OBJECT_ID(N'dbo.tk_theodoi_seed', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.tk_theodoi_seed
    (
        td_seed_id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_tk_theodoi_seed PRIMARY KEY,
        follower_tk_seed_id INT NOT NULL,
        follower_tk_id_server NVARCHAR(64) NULL,
        followee_tk_seed_id INT NOT NULL,
        followee_tk_id_server NVARCHAR(64) NULL,
        theo_doi_luc DATETIME2(0) NOT NULL CONSTRAINT DF_tk_theodoi_seed_theo_doi_luc DEFAULT SYSDATETIME(),
        trang_thai NVARCHAR(30) NOT NULL CONSTRAINT DF_tk_theodoi_seed_trang_thai DEFAULT N'dang_theo_doi',
        ghi_chu NVARCHAR(500) NULL,
        CONSTRAINT FK_tk_theodoi_seed_follower FOREIGN KEY (follower_tk_seed_id) REFERENCES dbo.taikhoan_seed(tk_seed_id),
        CONSTRAINT FK_tk_theodoi_seed_followee FOREIGN KEY (followee_tk_seed_id) REFERENCES dbo.taikhoan_seed(tk_seed_id)
    );

    CREATE UNIQUE INDEX UX_tk_theodoi_seed_cap ON dbo.tk_theodoi_seed(follower_tk_seed_id, followee_tk_seed_id);
END
GO

IF OBJECT_ID(N'dbo.tk_chan_seed', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.tk_chan_seed
    (
        chan_seed_id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_tk_chan_seed PRIMARY KEY,
        blocker_tk_seed_id INT NOT NULL,
        blocker_tk_id_server NVARCHAR(64) NULL,
        blocked_tk_seed_id INT NOT NULL,
        blocked_tk_id_server NVARCHAR(64) NULL,
        chan_luc DATETIME2(0) NOT NULL CONSTRAINT DF_tk_chan_seed_chan_luc DEFAULT SYSDATETIME(),
        trang_thai NVARCHAR(30) NOT NULL CONSTRAINT DF_tk_chan_seed_trang_thai DEFAULT N'dang_chan',
        ghi_chu NVARCHAR(500) NULL,
        CONSTRAINT FK_tk_chan_seed_blocker FOREIGN KEY (blocker_tk_seed_id) REFERENCES dbo.taikhoan_seed(tk_seed_id),
        CONSTRAINT FK_tk_chan_seed_blocked FOREIGN KEY (blocked_tk_seed_id) REFERENCES dbo.taikhoan_seed(tk_seed_id)
    );

    CREATE UNIQUE INDEX UX_tk_chan_seed_cap ON dbo.tk_chan_seed(blocker_tk_seed_id, blocked_tk_seed_id);
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
