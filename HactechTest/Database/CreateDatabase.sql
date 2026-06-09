/*
================================================================================
  Phần mềm     : HACTECH TEST (WinForms .NET 9)
  CSDL         : SQL Server
  Tên database : HactechTestDb

  Lưu ý        : File này DROP database cũ rồi tạo lại từ đầu.
                 Chỉ chạy trên môi trường demo / dev.
                 KHÔNG chạy trên production còn dữ liệu thật.

                 Sau khi chạy file này, chạy tiếp các file dữ liệu tĩnh:
                   1. InsertTaiKhoanSeed.sql
                   2. InsertProvinceWardSeed.sql

                 Phần mềm không tự tạo database/schema. Nếu dùng server SQL
                 có sẵn của công ty, hãy chạy các script này trên server đó
                 rồi cấu hình connection string ở màn hình đăng nhập.

  Cấu trúc chính:
    A. Tài khoản phần mềm    : taikhoan_phanmemtest
    B. Lịch sử chạy          : phien_chay, chi_tiet_phien_chay
    C. Seed runner API Shop  : taikhoan_seed, taikhoan_signupthanhcong,
                               tk_timkiem_seed, tk_theodoi_seed, tk_chan_seed,
                               Provinces_seed, Wards_seed,
                               diachi_tk_seed, danhmuc_seed,
                               thuonghieu_seed, sanpham_seed,
                               tk_thich_sanpham_seed, tinnhan_seed,
                               thongbao_seed
    D. Test case động        : test_case_dong
    E. View báo cáo          : v_tong_quan
================================================================================
*/

USE master;
GO

IF DB_ID(N'HactechTestDb') IS NOT NULL
BEGIN
    ALTER DATABASE HactechTestDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE HactechTestDb;
END
GO

CREATE DATABASE HactechTestDb;
GO

USE HactechTestDb;
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO


-- ============================================================================
-- A. TÀI KHOẢN ĐĂNG NHẬP PHẦN MỀM TEST
-- ============================================================================

CREATE TABLE dbo.taikhoan_phanmemtest
(
    id                   INT             IDENTITY(1,1) NOT NULL,
    ten_dang_nhap        NVARCHAR(100)   NOT NULL,
    mat_khau_hash        NVARCHAR(128)   NOT NULL,
    mat_khau_salt        NVARCHAR(64)    NOT NULL,
    ho_ten               NVARCHAR(200)   NULL,
    email                NVARCHAR(200)   NULL,
    so_dien_thoai        NVARCHAR(30)    NULL,
    vai_tro              NVARCHAR(30)    NOT NULL
        CONSTRAINT DF_taikhoan_phanmemtest_vai_tro DEFAULT N'nhan_vien',
    trang_thai           NVARCHAR(30)    NOT NULL
        CONSTRAINT DF_taikhoan_phanmemtest_trang_thai DEFAULT N'hoat_dong',
    tao_luc              DATETIME2(0)    NOT NULL
        CONSTRAINT DF_taikhoan_phanmemtest_tao_luc DEFAULT SYSDATETIME(),
    cap_nhat_luc         DATETIME2(0)    NULL,
    dang_nhap_cuoi_luc   DATETIME2(0)    NULL,

    CONSTRAINT PK_taikhoan_phanmemtest PRIMARY KEY (id),
    CONSTRAINT CK_taikhoan_phanmemtest_vai_tro CHECK
        (vai_tro IN (N'admin', N'nhan_vien')),
    CONSTRAINT CK_taikhoan_phanmemtest_trang_thai CHECK
        (trang_thai IN (N'hoat_dong', N'khoa'))
);
CREATE UNIQUE INDEX UX_taikhoan_phanmemtest_ten
    ON dbo.taikhoan_phanmemtest(ten_dang_nhap);

INSERT INTO dbo.taikhoan_phanmemtest
    (ten_dang_nhap, mat_khau_hash, mat_khau_salt, ho_ten, vai_tro, trang_thai)
VALUES
    (
        N'admin',
        CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', CONVERT(NVARCHAR(4000), N'hactech_admin_default_saltadmin')), 2),
        N'hactech_admin_default_salt',
        N'Quản trị viên',
        N'admin',
        N'hoat_dong'
    );
GO


-- ============================================================================
-- B. LỊCH SỬ CHẠY TEST
-- ============================================================================

CREATE TABLE dbo.phien_chay
(
    ID                       INT               IDENTITY(1,1) NOT NULL,
    thoi_diem_chay           DATETIME2(0)      NOT NULL
        CONSTRAINT DF_phien_chay_thoi_diem_chay DEFAULT SYSDATETIME(),
    nguoi_chay               NVARCHAR(200)     NULL,
    ten_may                  NVARCHAR(200)     NULL,
    ten_he_dieu_hanh         NVARCHAR(200)     NULL,
    ten_bo_suu_tap           NVARCHAR(200)     NULL,
    ten_mo_dun               NVARCHAR(200)     NULL,
    che_do_chay              NVARCHAR(20)      NOT NULL
        CONSTRAINT CK_phien_chay_che_do_chay CHECK
        (che_do_chay IN (N'all', N'selected')),
    che_do_loi               NVARCHAR(30)      NOT NULL
        CONSTRAINT CK_phien_chay_che_do_loi CHECK
        (che_do_loi IN (N'stop_on_fail', N'continue_on_fail')),
    base_url                 NVARCHAR(500)     NULL,
    tong_so_test             INT               NOT NULL
        CONSTRAINT DF_phien_chay_tong_so_test DEFAULT 0,
    so_dat                   INT               NOT NULL
        CONSTRAINT DF_phien_chay_so_dat DEFAULT 0,
    so_khong_dat             INT               NOT NULL
        CONSTRAINT DF_phien_chay_so_khong_dat DEFAULT 0,
    ty_le_dat                DECIMAL(5,2)      NOT NULL
        CONSTRAINT DF_phien_chay_ty_le_dat DEFAULT 0,
    thoi_gian_trung_binh_ms  INT               NOT NULL
        CONSTRAINT DF_phien_chay_thoi_gian_trung_binh_ms DEFAULT 0,

    CONSTRAINT PK_phien_chay PRIMARY KEY (ID)
);
CREATE INDEX IX_phien_chay_thoi_diem_chay ON dbo.phien_chay(thoi_diem_chay DESC);
CREATE INDEX IX_phien_chay_nguoi_chay ON dbo.phien_chay(nguoi_chay);
CREATE INDEX IX_phien_chay_ten_may ON dbo.phien_chay(ten_may);
GO

CREATE TABLE dbo.chi_tiet_phien_chay
(
    ID                      BIGINT          IDENTITY(1,1) NOT NULL,
    ID_phien_chay           INT             NOT NULL,
    so_thu_tu               INT             NOT NULL,
    ten_test_case           NVARCHAR(300)   NULL,
    http_method             NVARCHAR(10)    NULL,
    url                     NVARCHAR(1000)  NULL,
    ma_trang_thai_mong_doi  NVARCHAR(100)   NULL,
    ma_trang_thai_thuc_te   NVARCHAR(100)   NULL,
    ket_qua                 NVARCHAR(10)    NOT NULL
        CONSTRAINT CK_chi_tiet_phien_chay_ket_qua CHECK
        (ket_qua IN (N'PASS', N'FAIL', N'SKIP', N'ERROR')),
    thoi_gian_ms            INT             NOT NULL
        CONSTRAINT DF_chi_tiet_phien_chay_thoi_gian_ms DEFAULT 0,
    ly_do                   NVARCHAR(MAX)   NULL,
    request_headers         NVARCHAR(MAX)   NULL,
    request_body            NVARCHAR(MAX)   NULL,
    response_headers        NVARCHAR(MAX)   NULL,
    response_body           NVARCHAR(MAX)   NULL,
    thoi_diem_bat_dau       DATETIME2(0)    NULL,
    thoi_diem_ket_thuc      DATETIME2(0)    NULL,

    CONSTRAINT PK_chi_tiet_phien_chay PRIMARY KEY (ID),
    CONSTRAINT FK_chi_tiet_phien_chay_phien_chay FOREIGN KEY (ID_phien_chay)
        REFERENCES dbo.phien_chay(ID) ON DELETE CASCADE
);
CREATE INDEX IX_chi_tiet_phien_chay_ma_phien
    ON dbo.chi_tiet_phien_chay(ID_phien_chay, so_thu_tu);
GO


-- ============================================================================
-- C. SEED RUNNER API SHOP
-- ============================================================================

CREATE TABLE dbo.taikhoan_seed
(
    tk_seed_id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_taikhoan_seed PRIMARY KEY,
    sdt NVARCHAR(20) NOT NULL,
    mat_khau_hien_tai NVARCHAR(255) NOT NULL,
    uuid NVARCHAR(255) NOT NULL,
    trang_thai NVARCHAR(30) NOT NULL
        CONSTRAINT DF_taikhoan_seed_trang_thai DEFAULT N'san_sang',
    ghi_chu NVARCHAR(500) NULL,
    tao_luc DATETIME2(0) NOT NULL
        CONSTRAINT DF_taikhoan_seed_tao_luc DEFAULT SYSDATETIME(),
    cap_nhat_luc DATETIME2(0) NULL
);
CREATE UNIQUE INDEX UX_taikhoan_seed_sdt ON dbo.taikhoan_seed(sdt);
GO

CREATE TABLE dbo.taikhoan_signupthanhcong
(
    tk_id_server NVARCHAR(64) NOT NULL CONSTRAINT PK_taikhoan_signupthanhcong PRIMARY KEY,
    sdt NVARCHAR(20) NOT NULL,
    mat_khau_hien_tai NVARCHAR(255) NOT NULL,
    dang_ky_luc DATETIME2(0) NOT NULL
        CONSTRAINT DF_taikhoan_signupthanhcong_dang_ky_luc DEFAULT SYSDATETIME(),
    doi_mk_luc DATETIME2(0) NULL,
    ghi_chu NVARCHAR(500) NULL
);
CREATE UNIQUE INDEX UX_taikhoan_signupthanhcong_sdt ON dbo.taikhoan_signupthanhcong(sdt);
GO

CREATE TABLE dbo.tk_timkiem_seed
(
    tk_timkiem_seed_id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_tk_timkiem_seed PRIMARY KEY,
    tk_id_server NVARCHAR(64) NOT NULL,
    saved_search_id_server NVARCHAR(64) NULL,
    keyword NVARCHAR(255) NOT NULL,
    trang_thai NVARCHAR(30) NOT NULL
        CONSTRAINT DF_tk_timkiem_seed_trang_thai DEFAULT N'dang_luu',
    tao_boi_test BIT NOT NULL
        CONSTRAINT DF_tk_timkiem_seed_tao_boi_test DEFAULT 1,
    tao_luc DATETIME2(0) NULL,
    xoa_luc DATETIME2(0) NULL,
    ghi_chu NVARCHAR(500) NULL,
    CONSTRAINT FK_tk_timkiem_seed_taikhoan_signupthanhcong FOREIGN KEY (tk_id_server)
        REFERENCES dbo.taikhoan_signupthanhcong(tk_id_server)
);
CREATE INDEX IX_tk_timkiem_seed_tk_trang_thai
    ON dbo.tk_timkiem_seed(tk_id_server, trang_thai);
GO

CREATE TABLE dbo.tk_theodoi_seed
(
    td_seed_id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_tk_theodoi_seed PRIMARY KEY,
    follower_tk_id_server NVARCHAR(64) NOT NULL,
    followee_tk_id_server NVARCHAR(64) NOT NULL,
    theo_doi_luc DATETIME2(0) NOT NULL
        CONSTRAINT DF_tk_theodoi_seed_theo_doi_luc DEFAULT SYSDATETIME(),
    trang_thai NVARCHAR(30) NOT NULL
        CONSTRAINT DF_tk_theodoi_seed_trang_thai DEFAULT N'dang_theo_doi',
    ghi_chu NVARCHAR(500) NULL,
    CONSTRAINT FK_tk_theodoi_seed_follower FOREIGN KEY (follower_tk_id_server)
        REFERENCES dbo.taikhoan_signupthanhcong(tk_id_server),
    CONSTRAINT FK_tk_theodoi_seed_followee FOREIGN KEY (followee_tk_id_server)
        REFERENCES dbo.taikhoan_signupthanhcong(tk_id_server)
);
CREATE UNIQUE INDEX UX_tk_theodoi_seed_cap
    ON dbo.tk_theodoi_seed(follower_tk_id_server, followee_tk_id_server);
GO

CREATE TABLE dbo.tk_chan_seed
(
    chan_seed_id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_tk_chan_seed PRIMARY KEY,
    blocker_tk_id_server NVARCHAR(64) NOT NULL,
    blocked_tk_id_server NVARCHAR(64) NOT NULL,
    chan_luc DATETIME2(0) NOT NULL
        CONSTRAINT DF_tk_chan_seed_chan_luc DEFAULT SYSDATETIME(),
    trang_thai NVARCHAR(30) NOT NULL
        CONSTRAINT DF_tk_chan_seed_trang_thai DEFAULT N'dang_chan',
    ghi_chu NVARCHAR(500) NULL,
    CONSTRAINT FK_tk_chan_seed_blocker FOREIGN KEY (blocker_tk_id_server)
        REFERENCES dbo.taikhoan_signupthanhcong(tk_id_server),
    CONSTRAINT FK_tk_chan_seed_blocked FOREIGN KEY (blocked_tk_id_server)
        REFERENCES dbo.taikhoan_signupthanhcong(tk_id_server)
);
CREATE UNIQUE INDEX UX_tk_chan_seed_cap
    ON dbo.tk_chan_seed(blocker_tk_id_server, blocked_tk_id_server);
GO

CREATE TABLE dbo.Provinces_seed
(
    id INT NOT NULL,
    name NVARCHAR(255) NOT NULL,

    CONSTRAINT PK_Provinces_seed PRIMARY KEY (id)
);
GO

CREATE TABLE dbo.Wards_seed
(
    id INT NOT NULL,
    name NVARCHAR(255) NOT NULL,
    provinces_id INT NOT NULL,

    CONSTRAINT PK_Wards_seed PRIMARY KEY (id),
    CONSTRAINT FK_Wards_seed_Provinces_seed FOREIGN KEY (provinces_id)
        REFERENCES dbo.Provinces_seed(id)
);
CREATE INDEX IX_Wards_seed_provinces_id ON dbo.Wards_seed(provinces_id);
GO

CREATE TABLE dbo.diachi_tk_seed
(
    diachi_seed_id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_diachi_tk_seed PRIMARY KEY,
    tk_id_server NVARCHAR(64) NOT NULL,
    diachi_id_server NVARCHAR(64) NULL,
    ward_id INT NOT NULL,
    province_id INT NOT NULL,
    address NVARCHAR(300) NOT NULL,
    full_address NVARCHAR(500) NOT NULL,
    address_detail NVARCHAR(300) NOT NULL,
    lat DECIMAL(12,8) NOT NULL,
    lng DECIMAL(12,8) NOT NULL,
    receiver_name NVARCHAR(200) NOT NULL,
    phone NVARCHAR(30) NOT NULL,
    is_default BIT NOT NULL CONSTRAINT DF_diachi_tk_seed_is_default DEFAULT 1,
    muc_dich_seed NVARCHAR(50) NOT NULL CONSTRAINT DF_diachi_tk_seed_muc_dich DEFAULT N'ca_hai',
    trang_thai NVARCHAR(30) NOT NULL CONSTRAINT DF_diachi_tk_seed_trang_thai DEFAULT N'chua_tao',
    tao_luc DATETIME2(0) NULL,
    xac_minh_luc DATETIME2(0) NULL,
    ghi_chu NVARCHAR(500) NULL,

    CONSTRAINT FK_diachi_tk_seed_ward FOREIGN KEY (ward_id)
        REFERENCES dbo.Wards_seed(id),
    CONSTRAINT FK_diachi_tk_seed_province FOREIGN KEY (province_id)
        REFERENCES dbo.Provinces_seed(id),
    CONSTRAINT FK_diachi_tk_seed_taikhoan_signupthanhcong FOREIGN KEY (tk_id_server)
        REFERENCES dbo.taikhoan_signupthanhcong(tk_id_server)
);
CREATE INDEX IX_diachi_tk_seed_tk_trang_thai
    ON dbo.diachi_tk_seed(tk_id_server, trang_thai);
CREATE INDEX IX_diachi_tk_seed_diachi_server
    ON dbo.diachi_tk_seed(diachi_id_server);
GO

CREATE TABLE dbo.danhmuc_seed
(
    dm_id_server NVARCHAR(64) NOT NULL CONSTRAINT PK_danhmuc_seed PRIMARY KEY,
    ten_danh_muc NVARCHAR(255) NOT NULL,
    dm_cha_id_server NVARCHAR(64) NULL,
    co_danh_muc_con BIT NULL,
    co_thuong_hieu BIT NULL,
    co_kich_co BIT NULL,
    yeu_cau_can_nang BIT NULL,
    trang_thai NVARCHAR(30) NOT NULL CONSTRAINT DF_danhmuc_seed_trang_thai DEFAULT N'san_sang',
    dong_bo_luc DATETIME2(0) NULL
);
CREATE INDEX IX_danhmuc_seed_trang_thai ON dbo.danhmuc_seed(trang_thai);
GO

CREATE TABLE dbo.thuonghieu_seed
(
    thuonghieu_id_server NVARCHAR(64) NOT NULL CONSTRAINT PK_thuonghieu_seed PRIMARY KEY,
    ten_thuong_hieu NVARCHAR(255) NOT NULL,
    dm_id_server NVARCHAR(64) NULL,
    trang_thai NVARCHAR(30) NOT NULL CONSTRAINT DF_thuonghieu_seed_trang_thai DEFAULT N'san_sang',
    dong_bo_luc DATETIME2(0) NULL
);
CREATE INDEX IX_thuonghieu_seed_dm_trang_thai
    ON dbo.thuonghieu_seed(dm_id_server, trang_thai);
GO

CREATE TABLE dbo.sanpham_seed
(
    sp_id_server NVARCHAR(64) NOT NULL CONSTRAINT PK_sanpham_seed PRIMARY KEY,
    tk_id_server NVARCHAR(64) NOT NULL,
    dm_id_server NVARCHAR(64) NULL,
    thuonghieu_id_server NVARCHAR(64) NULL,
    ship_from_id_server NVARCHAR(64) NULL,
    ten_sp NVARCHAR(300) NOT NULL,
    gia DECIMAL(18,2) NOT NULL,
    trang_thai NVARCHAR(30) NOT NULL CONSTRAINT DF_sanpham_seed_trang_thai DEFAULT N'san_sang',
    tao_boi_test BIT NOT NULL CONSTRAINT DF_sanpham_seed_tao_boi_test DEFAULT 1,
    tao_luc DATETIME2(0) NULL,
    xac_minh_luc DATETIME2(0) NULL,
    ghi_chu NVARCHAR(500) NULL,
    CONSTRAINT FK_sanpham_seed_taikhoan_signupthanhcong FOREIGN KEY (tk_id_server)
        REFERENCES dbo.taikhoan_signupthanhcong(tk_id_server),
    CONSTRAINT FK_sanpham_seed_danhmuc FOREIGN KEY (dm_id_server)
        REFERENCES dbo.danhmuc_seed(dm_id_server),
    CONSTRAINT FK_sanpham_seed_thuonghieu FOREIGN KEY (thuonghieu_id_server)
        REFERENCES dbo.thuonghieu_seed(thuonghieu_id_server)
);
CREATE INDEX IX_sanpham_seed_tk_trang_thai
    ON dbo.sanpham_seed(tk_id_server, trang_thai);
GO

CREATE TABLE dbo.tk_thich_sanpham_seed
(
    thich_seed_id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_tk_thich_sanpham_seed PRIMARY KEY,
    tk_id_server NVARCHAR(64) NOT NULL,
    sp_id_server NVARCHAR(64) NOT NULL,
    thich_luc DATETIME2(0) NOT NULL CONSTRAINT DF_tk_thich_sanpham_seed_thich_luc DEFAULT SYSDATETIME(),
    trang_thai NVARCHAR(30) NOT NULL CONSTRAINT DF_tk_thich_sanpham_seed_trang_thai DEFAULT N'san_sang',
    ghi_chu NVARCHAR(500) NULL,
    CONSTRAINT FK_tk_thich_sanpham_seed_taikhoan_signupthanhcong FOREIGN KEY (tk_id_server)
        REFERENCES dbo.taikhoan_signupthanhcong(tk_id_server),
    CONSTRAINT FK_tk_thich_sanpham_seed_sanpham FOREIGN KEY (sp_id_server)
        REFERENCES dbo.sanpham_seed(sp_id_server)
);
CREATE UNIQUE INDEX UX_tk_thich_sanpham_seed_cap
    ON dbo.tk_thich_sanpham_seed(tk_id_server, sp_id_server);
GO

CREATE TABLE dbo.tinnhan_seed
(
    tn_seed_id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_tinnhan_seed PRIMARY KEY,
    conversation_id_server NVARCHAR(64) NULL,
    message_id_server NVARCHAR(64) NULL,
    sender_tk_id_server NVARCHAR(64) NOT NULL,
    receiver_tk_id_server NVARCHAR(64) NOT NULL,
    product_id_server NVARCHAR(64) NULL,
    type_message NVARCHAR(30) NOT NULL CONSTRAINT DF_tinnhan_seed_type_message DEFAULT N'text',
    noi_dung NVARCHAR(MAX) NULL,
    trang_thai NVARCHAR(30) NOT NULL CONSTRAINT DF_tinnhan_seed_trang_thai DEFAULT N'da_gui',
    tao_boi_test BIT NOT NULL CONSTRAINT DF_tinnhan_seed_tao_boi_test DEFAULT 1,
    gui_luc DATETIME2(0) NULL,
    ghi_chu NVARCHAR(500) NULL,
    CONSTRAINT FK_tinnhan_seed_sender FOREIGN KEY (sender_tk_id_server)
        REFERENCES dbo.taikhoan_signupthanhcong(tk_id_server),
    CONSTRAINT FK_tinnhan_seed_receiver FOREIGN KEY (receiver_tk_id_server)
        REFERENCES dbo.taikhoan_signupthanhcong(tk_id_server),
    CONSTRAINT FK_tinnhan_seed_sanpham FOREIGN KEY (product_id_server)
        REFERENCES dbo.sanpham_seed(sp_id_server)
);
CREATE INDEX IX_tinnhan_seed_conversation
    ON dbo.tinnhan_seed(conversation_id_server, trang_thai);
CREATE INDEX IX_tinnhan_seed_sender
    ON dbo.tinnhan_seed(sender_tk_id_server, trang_thai);
CREATE INDEX IX_tinnhan_seed_receiver
    ON dbo.tinnhan_seed(receiver_tk_id_server, trang_thai);
CREATE INDEX IX_tinnhan_seed_product
    ON dbo.tinnhan_seed(product_id_server)
    WHERE product_id_server IS NOT NULL;
GO

CREATE TABLE dbo.thongbao_seed
(
    tb_seed_id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_thongbao_seed PRIMARY KEY,
    notification_id_server NVARCHAR(64) NULL,
    tk_id_server NVARCHAR(64) NOT NULL,
    title NVARCHAR(300) NULL,
    content NVARCHAR(MAX) NULL,
    object_id_server NVARCHAR(64) NULL,
    notification_type NVARCHAR(100) NULL,
    da_doc BIT NULL,
    trang_thai NVARCHAR(30) NOT NULL CONSTRAINT DF_thongbao_seed_trang_thai DEFAULT N'dang_luu',
    lay_luc DATETIME2(0) NULL,
    doc_luc DATETIME2(0) NULL,
    ghi_chu NVARCHAR(500) NULL,
    CONSTRAINT FK_thongbao_seed_taikhoan_signupthanhcong FOREIGN KEY (tk_id_server)
        REFERENCES dbo.taikhoan_signupthanhcong(tk_id_server)
);
CREATE UNIQUE INDEX UX_thongbao_seed_notification
    ON dbo.thongbao_seed(notification_id_server)
    WHERE notification_id_server IS NOT NULL;
CREATE INDEX IX_thongbao_seed_tk_trang_thai
    ON dbo.thongbao_seed(tk_id_server, trang_thai);
GO


-- ============================================================================
-- D. TEST CASE ĐỘNG DO NGƯỜI DÙNG THÊM TỪ GIAO DIỆN
-- ============================================================================

CREATE TABLE dbo.test_case_dong
(
    id                   INT             IDENTITY(1,1) NOT NULL,
    ma                   NVARCHAR(80)    NOT NULL,
    nhom                 NVARCHAR(120)   NOT NULL
        CONSTRAINT DF_test_case_dong_nhom DEFAULT N'Custom',
    ten_hien_thi         NVARCHAR(300)   NOT NULL,
    mo_ta                NVARCHAR(1000)  NULL,
    http_method          NVARCHAR(10)    NOT NULL,
    endpoint             NVARCHAR(500)   NOT NULL,
    auth_mode            NVARCHAR(30)    NOT NULL
        CONSTRAINT DF_test_case_dong_auth_mode DEFAULT N'none',
    path_params_json     NVARCHAR(MAX)   NULL,
    headers_json         NVARCHAR(MAX)   NULL,
    body_json            NVARCHAR(MAX)   NULL,
    expected_codes       NVARCHAR(200)   NULL,
    expected_http_status INT             NULL,
    expected_json_path   NVARCHAR(300)   NULL,
    expected_json_value  NVARCHAR(500)   NULL,
    tao_luc              DATETIME2(0)    NOT NULL
        CONSTRAINT DF_test_case_dong_tao_luc DEFAULT SYSDATETIME(),
    cap_nhat_luc         DATETIME2(0)    NULL,

    CONSTRAINT PK_test_case_dong PRIMARY KEY (id),
    CONSTRAINT CK_test_case_dong_method CHECK
        (http_method IN (N'GET', N'POST', N'PUT', N'PATCH', N'DELETE')),
    CONSTRAINT CK_test_case_dong_auth_mode CHECK
        (auth_mode IN (N'none', N'bearer_seed'))
);
CREATE UNIQUE INDEX UX_test_case_dong_ma ON dbo.test_case_dong(ma);
CREATE INDEX IX_test_case_dong_nhom ON dbo.test_case_dong(nhom);
GO

-- ============================================================================
-- E. VIEW BÁO CÁO
-- ============================================================================

CREATE VIEW dbo.v_tong_quan
AS
SELECT
    CAST(thoi_diem_chay AS DATE) AS ngay_chay,
    nguoi_chay AS nguoi_chay,
    COUNT(*) AS so_phien,
    SUM(tong_so_test) AS tong_test,
    SUM(so_dat) AS tong_dat,
    SUM(so_khong_dat) AS tong_khong_dat,
    CASE WHEN SUM(tong_so_test) = 0 THEN 0
         ELSE CAST(100.0 * SUM(so_dat) / SUM(tong_so_test) AS DECIMAL(5,2))
    END AS ty_le_dat,
    AVG(thoi_gian_trung_binh_ms) AS thoi_gian_trung_binh_ms
FROM dbo.phien_chay
GROUP BY CAST(thoi_diem_chay AS DATE), nguoi_chay;
GO

PRINT N'Đã tạo xong CSDL HactechTestDb.';
GO

