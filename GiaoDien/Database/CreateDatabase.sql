/*
================================================================================
  Phần mềm     : API TESTER (WinForms .NET 9 + ASP.NET Core 9 Web API)
  CSDL         : SQL Server
  Tên database : ApiTesterDb

  Lưu ý        : File này DROP database cũ rồi tạo lại từ đầu.
                 Chỉ chạy trên môi trường demo / dev.
                 KHÔNG chạy trên production còn dữ liệu thật.

  Quy ước đặt tên:
    - Bảng/cột/constraint/index : tiếng Việt KHÔNG dấu, snake_case
    - Khoá chính của mỗi bảng   : `ID`
    - Khoá ngoại                : `ID_<ten_bang_cha>`
    - Tiền tố ràng buộc         : PK_/FK_/UQ_/CK_/DF_/IX_

  Cấu trúc:
    A. Cây cấu trúc test     : bo_suu_tap, mo_dun, test_case,
                               tham_so_test_case, header_test_case
    B. Biến                  : bien_bo_suu_tap, moi_truong, bien_moi_truong
    C. Lịch sử chạy          : phien_chay, chi_tiet_phien_chay
    D. Local state account   : tai_khoan_test, ho_so_tai_khoan_test
    E. View Tổng quan        : v_tong_quan
    F. Seed dữ liệu          : 3 môi trường mặc định
================================================================================
*/

-- ============================================================================
-- 0. DROP & CREATE DATABASE
-- ============================================================================
USE master;
GO

IF DB_ID(N'ApiTesterDb') IS NOT NULL
BEGIN
    ALTER DATABASE ApiTesterDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE ApiTesterDb;
END
GO

CREATE DATABASE ApiTesterDb;
GO

USE ApiTesterDb;
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO


-- ============================================================================
-- A. CÂY CẤU TRÚC TEST
-- ============================================================================

-- A.1 bo_suu_tap (Project / Collection)
CREATE TABLE dbo.bo_suu_tap
(
    ID                  INT             IDENTITY(1,1) NOT NULL,
    ten                 NVARCHAR(200)   NOT NULL,
    mo_ta               NVARCHAR(1000)  NULL,
    base_url            NVARCHAR(500)   NULL,
    thoi_diem_tao       DATETIME2(0)    NOT NULL
        CONSTRAINT DF_bo_suu_tap_thoi_diem_tao DEFAULT SYSDATETIME(),
    thoi_diem_cap_nhat  DATETIME2(0)    NOT NULL
        CONSTRAINT DF_bo_suu_tap_thoi_diem_cap_nhat DEFAULT SYSDATETIME(),

    CONSTRAINT PK_bo_suu_tap PRIMARY KEY (ID),
    CONSTRAINT UQ_bo_suu_tap_ten UNIQUE (ten)
);
GO


-- A.2 mo_dun (Module)
CREATE TABLE dbo.mo_dun
(
    ID                  INT            IDENTITY(1,1) NOT NULL,
    ID_bo_suu_tap       INT            NOT NULL,
    ten                 NVARCHAR(200)  NOT NULL,
    mo_ta               NVARCHAR(1000) NULL,
    thu_tu              INT            NOT NULL
        CONSTRAINT DF_mo_dun_thu_tu DEFAULT 0,
    thoi_diem_tao       DATETIME2(0)   NOT NULL
        CONSTRAINT DF_mo_dun_thoi_diem_tao DEFAULT SYSDATETIME(),
    thoi_diem_cap_nhat  DATETIME2(0)   NOT NULL
        CONSTRAINT DF_mo_dun_thoi_diem_cap_nhat DEFAULT SYSDATETIME(),

    CONSTRAINT PK_mo_dun PRIMARY KEY (ID),
    CONSTRAINT FK_mo_dun_bo_suu_tap FOREIGN KEY (ID_bo_suu_tap)
        REFERENCES dbo.bo_suu_tap(ID) ON DELETE CASCADE,
    CONSTRAINT UQ_mo_dun_bo_suu_tap_ten UNIQUE (ID_bo_suu_tap, ten)
);
GO


-- A.3 test_case
CREATE TABLE dbo.test_case
(
    ID                      INT            IDENTITY(1,1) NOT NULL,
    ID_mo_dun               INT            NOT NULL,
    ten                     NVARCHAR(300)  NOT NULL,
    mo_ta                   NVARCHAR(1000) NULL,

    -- URL + Method
    http_method             NVARCHAR(10)   NOT NULL
        CONSTRAINT CK_test_case_http_method CHECK
        (http_method IN (N'GET', N'POST', N'PUT', N'DELETE',
                         N'PATCH', N'OPTIONS', N'HEAD')),
    base_url                NVARCHAR(500)  NULL,
    duong_dan               NVARCHAR(500)  NULL,
    body_raw                NVARCHAR(MAX)  NULL,

    -- Kết quả mong đợi
    ma_trang_thai_mong_doi  INT            NULL,
    che_do_so_khop          NVARCHAR(20)   NOT NULL
        CONSTRAINT DF_test_case_che_do_so_khop DEFAULT N'subset'
        CONSTRAINT CK_test_case_che_do_so_khop CHECK
        (che_do_so_khop IN (N'subset', N'exact')),
    body_mong_doi           NVARCHAR(MAX)  NULL,

    -- Script
    script_truoc            NVARCHAR(MAX)  NULL,
    script_sau              NVARCHAR(MAX)  NULL,

    thu_tu                  INT            NOT NULL
        CONSTRAINT DF_test_case_thu_tu DEFAULT 0,
    thoi_diem_tao           DATETIME2(0)   NOT NULL
        CONSTRAINT DF_test_case_thoi_diem_tao DEFAULT SYSDATETIME(),
    thoi_diem_cap_nhat      DATETIME2(0)   NOT NULL
        CONSTRAINT DF_test_case_thoi_diem_cap_nhat DEFAULT SYSDATETIME(),

    CONSTRAINT PK_test_case PRIMARY KEY (ID),
    CONSTRAINT FK_test_case_mo_dun FOREIGN KEY (ID_mo_dun)
        REFERENCES dbo.mo_dun(ID) ON DELETE CASCADE
);
CREATE INDEX IX_test_case_mo_dun ON dbo.test_case(ID_mo_dun);
GO


-- A.4 tham_so_test_case (query params)
CREATE TABLE dbo.tham_so_test_case
(
    ID               INT            IDENTITY(1,1) NOT NULL,
    ID_test_case     INT            NOT NULL,
    ten_tham_so      NVARCHAR(200)  NOT NULL,
    gia_tri_tham_so  NVARCHAR(2000) NULL,
    dang_bat         BIT            NOT NULL
        CONSTRAINT DF_tham_so_test_case_dang_bat DEFAULT 1,
    thu_tu           INT            NOT NULL
        CONSTRAINT DF_tham_so_test_case_thu_tu DEFAULT 0,

    CONSTRAINT PK_tham_so_test_case PRIMARY KEY (ID),
    CONSTRAINT FK_tham_so_test_case_test_case FOREIGN KEY (ID_test_case)
        REFERENCES dbo.test_case(ID) ON DELETE CASCADE
);
CREATE INDEX IX_tham_so_test_case_ma_test_case ON dbo.tham_so_test_case(ID_test_case);
GO


-- A.5 header_test_case
CREATE TABLE dbo.header_test_case
(
    ID              INT            IDENTITY(1,1) NOT NULL,
    ID_test_case    INT            NOT NULL,
    ten_header      NVARCHAR(200)  NOT NULL,
    gia_tri_header  NVARCHAR(2000) NULL,
    dang_bat        BIT            NOT NULL
        CONSTRAINT DF_header_test_case_dang_bat DEFAULT 1,
    thu_tu          INT            NOT NULL
        CONSTRAINT DF_header_test_case_thu_tu DEFAULT 0,

    CONSTRAINT PK_header_test_case PRIMARY KEY (ID),
    CONSTRAINT FK_header_test_case_test_case FOREIGN KEY (ID_test_case)
        REFERENCES dbo.test_case(ID) ON DELETE CASCADE
);
CREATE INDEX IX_header_test_case_ma_test_case ON dbo.header_test_case(ID_test_case);
GO


-- ============================================================================
-- B. BIẾN
-- ============================================================================

-- B.1 bien_bo_suu_tap (Collection variables)
CREATE TABLE dbo.bien_bo_suu_tap
(
    ID                  INT            IDENTITY(1,1) NOT NULL,
    ID_bo_suu_tap       INT            NOT NULL,
    ten_bien            NVARCHAR(200)  NOT NULL,
    gia_tri_bien        NVARCHAR(2000) NULL,
    thoi_diem_cap_nhat  DATETIME2(0)   NOT NULL
        CONSTRAINT DF_bien_bo_suu_tap_thoi_diem_cap_nhat DEFAULT SYSDATETIME(),

    CONSTRAINT PK_bien_bo_suu_tap PRIMARY KEY (ID),
    CONSTRAINT FK_bien_bo_suu_tap_bo_suu_tap FOREIGN KEY (ID_bo_suu_tap)
        REFERENCES dbo.bo_suu_tap(ID) ON DELETE CASCADE,
    CONSTRAINT UQ_bien_bo_suu_tap_ten UNIQUE (ID_bo_suu_tap, ten_bien)
);
GO


-- B.2 moi_truong (Environments)
CREATE TABLE dbo.moi_truong
(
    ID              INT            IDENTITY(1,1) NOT NULL,
    ten             NVARCHAR(100)  NOT NULL,
    mo_ta           NVARCHAR(500)  NULL,
    thoi_diem_tao   DATETIME2(0)   NOT NULL
        CONSTRAINT DF_moi_truong_thoi_diem_tao DEFAULT SYSDATETIME(),

    CONSTRAINT PK_moi_truong PRIMARY KEY (ID),
    CONSTRAINT UQ_moi_truong_ten UNIQUE (ten)
);
GO


-- B.3 bien_moi_truong (Environment variables)
CREATE TABLE dbo.bien_moi_truong
(
    ID                  INT            IDENTITY(1,1) NOT NULL,
    ID_moi_truong       INT            NOT NULL,
    ten_bien            NVARCHAR(200)  NOT NULL,
    gia_tri_bien        NVARCHAR(2000) NULL,
    thoi_diem_cap_nhat  DATETIME2(0)   NOT NULL
        CONSTRAINT DF_bien_moi_truong_thoi_diem_cap_nhat DEFAULT SYSDATETIME(),

    CONSTRAINT PK_bien_moi_truong PRIMARY KEY (ID),
    CONSTRAINT FK_bien_moi_truong_moi_truong FOREIGN KEY (ID_moi_truong)
        REFERENCES dbo.moi_truong(ID) ON DELETE CASCADE,
    CONSTRAINT UQ_bien_moi_truong_ten UNIQUE (ID_moi_truong, ten_bien)
);
GO


-- ============================================================================
-- C. LỊCH SỬ CHẠY TEST
-- ============================================================================

-- C.1 phien_chay (Run sessions)
CREATE TABLE dbo.phien_chay
(
    ID                       INT               IDENTITY(1,1) NOT NULL,

    -- Ai/khi nào/ở đâu
    thoi_diem_chay           DATETIME2(0)      NOT NULL
        CONSTRAINT DF_phien_chay_thoi_diem_chay DEFAULT SYSDATETIME(),
    nguoi_chay               NVARCHAR(200)     NULL,
    ten_may                  NVARCHAR(200)     NULL,
    ten_he_dieu_hanh         NVARCHAR(200)     NULL,

    -- Phạm vi chạy (snapshot tên để vẫn hiển thị được khi BST/module bị xoá)
    ID_bo_suu_tap            INT               NULL,
    ten_bo_suu_tap           NVARCHAR(200)     NULL,
    ID_mo_dun                INT               NULL,
    ten_mo_dun               NVARCHAR(200)     NULL,

    -- Chế độ chạy
    che_do_chay              NVARCHAR(20)      NOT NULL                -- 'all' | 'selected'
        CONSTRAINT CK_phien_chay_che_do_chay CHECK
        (che_do_chay IN (N'all', N'selected')),
    che_do_loi               NVARCHAR(30)      NOT NULL                -- 'stop_on_fail' | 'continue_on_fail'
        CONSTRAINT CK_phien_chay_che_do_loi CHECK
        (che_do_loi IN (N'stop_on_fail', N'continue_on_fail')),

    base_url                 NVARCHAR(500)     NULL,

    -- Số liệu tổng kết
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

    CONSTRAINT PK_phien_chay PRIMARY KEY (ID),
    CONSTRAINT FK_phien_chay_bo_suu_tap FOREIGN KEY (ID_bo_suu_tap)
        REFERENCES dbo.bo_suu_tap(ID),
    CONSTRAINT FK_phien_chay_mo_dun FOREIGN KEY (ID_mo_dun)
        REFERENCES dbo.mo_dun(ID)
);
CREATE INDEX IX_phien_chay_thoi_diem_chay ON dbo.phien_chay(thoi_diem_chay DESC);
CREATE INDEX IX_phien_chay_nguoi_chay     ON dbo.phien_chay(nguoi_chay);
CREATE INDEX IX_phien_chay_ten_may        ON dbo.phien_chay(ten_may);
GO


-- C.2 chi_tiet_phien_chay (chi tiết từng test case trong 1 phiên)
CREATE TABLE dbo.chi_tiet_phien_chay
(
    ID                      BIGINT          IDENTITY(1,1) NOT NULL,
    ID_phien_chay           INT             NOT NULL,

    so_thu_tu               INT             NOT NULL,
    ID_test_case            INT             NULL,
    ten_test_case           NVARCHAR(300)   NULL,

    http_method             NVARCHAR(10)    NULL,
    url                     NVARCHAR(1000)  NULL,

    ma_trang_thai_mong_doi  INT             NULL,
    ma_trang_thai_thuc_te   INT             NULL,

    ket_qua                 NVARCHAR(10)    NOT NULL                   -- PASS / FAIL / SKIP / ERROR
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
        REFERENCES dbo.phien_chay(ID) ON DELETE CASCADE,
    CONSTRAINT FK_chi_tiet_phien_chay_test_case FOREIGN KEY (ID_test_case)
        REFERENCES dbo.test_case(ID)
);
CREATE INDEX IX_chi_tiet_phien_chay_ma_phien
    ON dbo.chi_tiet_phien_chay(ID_phien_chay, so_thu_tu);
CREATE INDEX IX_chi_tiet_phien_chay_ma_test_case
    ON dbo.chi_tiet_phien_chay(ID_test_case);
GO


-- ============================================================================
-- D. LOCAL STATE CHO NHÓM API TÀI KHOẢN
--    (theo file LOCAL_STATE_TAI_KHOAN.md - dùng cho test các flow auth)
-- ============================================================================

-- D.1 tai_khoan_test
CREATE TABLE dbo.tai_khoan_test
(
    ID                    INT             IDENTITY(1,1) NOT NULL,
    ID_nguoi_dung_server  NVARCHAR(100)   NULL,
    so_thu_tu_sinh        INT             NOT NULL,
    loai_tai_khoan        NVARCHAR(20)    NOT NULL
        CONSTRAINT CK_tai_khoan_test_loai_tai_khoan CHECK
        (loai_tai_khoan IN (N'sdt', N'email')),
    tai_khoan             NVARCHAR(255)   NOT NULL,
    mat_khau              NVARCHAR(100)   NOT NULL,
    trang_thai            NVARCHAR(30)    NOT NULL
        CONSTRAINT CK_tai_khoan_test_trang_thai CHECK
        (trang_thai IN (N'chua_dang_ky', N'cho_xac_nhan_otp', N'da_dang_ky')),
    thoi_diem_tao_ban_ghi DATETIME2(0)    NOT NULL
        CONSTRAINT DF_tai_khoan_test_thoi_diem_tao_ban_ghi DEFAULT SYSDATETIME(),
    thoi_diem_cap_nhat    DATETIME2(0)    NOT NULL
        CONSTRAINT DF_tai_khoan_test_thoi_diem_cap_nhat DEFAULT SYSDATETIME(),

    CONSTRAINT PK_tai_khoan_test PRIMARY KEY (ID),
    CONSTRAINT UQ_tai_khoan_test_so_thu_tu_sinh UNIQUE (so_thu_tu_sinh),
    CONSTRAINT UQ_tai_khoan_test_tai_khoan UNIQUE (tai_khoan)
);
CREATE INDEX IX_tai_khoan_test_trang_thai
    ON dbo.tai_khoan_test(trang_thai);
CREATE INDEX IX_tai_khoan_test_loai_trang_thai
    ON dbo.tai_khoan_test(loai_tai_khoan, trang_thai);
CREATE UNIQUE INDEX UX_tai_khoan_test_ID_nguoi_dung_server
    ON dbo.tai_khoan_test(ID_nguoi_dung_server)
    WHERE ID_nguoi_dung_server IS NOT NULL;
GO


-- D.2 ho_so_tai_khoan_test (snapshot hồ sơ - quan hệ 1-1 với tai_khoan_test)
CREATE TABLE dbo.ho_so_tai_khoan_test
(
    ID_tai_khoan               INT             NOT NULL,
    ten_hien_thi               NVARCHAR(255)   NULL,
    trang_thai_hien_tai        NVARCHAR(500)   NULL,
    avatar_hien_tai            NVARCHAR(1000)  NULL,
    anh_bia_hien_tai           NVARCHAR(1000)  NULL,
    anh_bia_web_hien_tai       NVARCHAR(1000)  NULL,
    email_lien_he              NVARCHAR(255)   NULL,
    so_dien_thoai_lien_he      NVARCHAR(20)    NULL,
    ten                        NVARCHAR(100)   NULL,
    ho                         NVARCHAR(100)   NULL,
    so_luong_tin_dang          INT             NULL,
    dang_truc_tuyen            BIT             NULL,
    ID_dia_chi_mac_dinh_server NVARCHAR(100)   NULL,
    dia_chi_mac_dinh           NVARCHAR(500)   NULL,
    thanh_pho_mac_dinh         NVARCHAR(255)   NULL,
    bi_khoa                    BIT             NOT NULL
        CONSTRAINT DF_ho_so_tai_khoan_test_bi_khoa DEFAULT 0,
    thoi_diem_cap_nhat         DATETIME2(0)    NOT NULL
        CONSTRAINT DF_ho_so_tai_khoan_test_cap_nhat DEFAULT SYSDATETIME(),

    CONSTRAINT PK_ho_so_tai_khoan_test PRIMARY KEY (ID_tai_khoan),
    CONSTRAINT FK_ho_so_tai_khoan_test_tai_khoan FOREIGN KEY (ID_tai_khoan)
        REFERENCES dbo.tai_khoan_test(ID) ON DELETE CASCADE
);
GO


-- ============================================================================
-- E. VIEW PHỤC VỤ DASHBOARD "TỔNG QUAN"
-- ============================================================================
CREATE VIEW dbo.v_tong_quan
AS
SELECT
    CAST(thoi_diem_chay AS DATE)                      AS ngay_chay,
    nguoi_chay                                        AS nguoi_chay,
    COUNT(*)                                          AS so_phien,
    SUM(tong_so_test)                                 AS tong_test,
    SUM(so_dat)                                       AS tong_dat,
    SUM(so_khong_dat)                                 AS tong_khong_dat,
    CASE WHEN SUM(tong_so_test) = 0 THEN 0
         ELSE CAST(100.0 * SUM(so_dat)
                   / SUM(tong_so_test) AS DECIMAL(5,2))
    END                                               AS ty_le_dat,
    AVG(thoi_gian_trung_binh_ms)                      AS thoi_gian_trung_binh_ms
FROM dbo.phien_chay
GROUP BY CAST(thoi_diem_chay AS DATE), nguoi_chay;
GO


-- ============================================================================
-- F. SEED DỮ LIỆU MẶC ĐỊNH
-- ============================================================================
INSERT INTO dbo.moi_truong (ten, mo_ta) VALUES
    (N'dev',     N'Môi trường phát triển'),
    (N'staging', N'Môi trường kiểm thử nội bộ'),
    (N'prod',    N'Môi trường vận hành');
GO


PRINT N'Đã tạo xong CSDL ApiTesterDb.';
GO
