# KiemThuApiShop

Console runner C# để chạy sẵn các kịch bản kiểm thử API shop thương mại điện tử theo `API_TESTCASE_DOCUMENTATION.md`.

Phạm vi hiện có:

- `Auth`: signup, login, me, OTP/reset password, change password, change info, logout.
- `User`: get/set user info.
- `Product`: category, brand, product detail/list/comment/like/add/update/delete/user listings.

## Chạy lần đầu

1. Mở SQL Server/SSMS và chạy script:

```sql
KiemThuApiShop/Sql/01_tao_database_va_bang_seed.sql
```

2. Kiểm tra connection string trong `appsettings.json`:

```json
"connectionStrings": {
  "ApiShopTestDb": "Server=.;Database=ApiShopTestDb;Trusted_Connection=True;TrustServerCertificate=True"
}
```

3. Chạy chương trình:

```powershell
cd KiemThuApiShop
dotnet run -- --base-url http://localhost:8000 --all
```

Nếu SQL Server của bạn không phải `Server=.` thì sửa connection string hoặc truyền trực tiếp:

```powershell
dotnet run -- --connection-string "Server=.\SQLEXPRESS;Database=ApiShopTestDb;Trusted_Connection=True;TrustServerCertificate=True" --base-url http://localhost:8000 --all
```

## Chế độ chạy

```powershell
dotnet run -- --list
dotnet run -- --group Auth
dotnet run -- --group User
dotnet run -- --group Product
dotnet run -- --case AUTH-LOGIN-01
dotnet run -- --case AUTH-LOGIN-01,PRODUCT-CATEGORY-01
dotnet run -- --base-url http://localhost:8000 --all --khong-chuan-bi-du-lieu
```

Nếu không truyền `--all`, `--group`, `--case` hoặc `--list`, chương trình sẽ mở menu chọn trực tiếp trên console.

## Cấu hình

- `baseUrl`: base URL API thật khi nhóm backend bàn giao.
- `connectionStrings.ApiShopTestDb`: database SQL Server lưu seed test và kết quả test case.
- `timeoutGiay`: thời gian chờ mỗi request.
- `tuDongChuanBiDuLieu`: tự signup tài khoản seed, đồng bộ category/brand, tạo address/product/comment/like mồi tốt nhất có thể.
- `soTaiKhoanSeed`: số tài khoản seed tối thiểu trong bảng `taikhoan_seed`.
- `soTaiKhoanDangKyTruoc`: số tài khoản mồi chương trình cố gắng signup trước.

## Dữ liệu seed

Runner lưu seed trực tiếp vào SQL Server, không dùng file JSON nữa.
Mỗi lần chạy, chương trình quét toàn bộ các bảng seed theo thứ tự phụ thuộc. Nếu dữ liệu mồi đã đủ, chương trình bỏ qua bước chuẩn bị; nếu còn thiếu bảng/loại dữ liệu nào, chương trình chỉ gọi API để bổ sung phần thiếu đó.

Các bảng chính:

- `taikhoan_seed`
- `danhmuc_seed`
- `thuonghieu_seed`
- `diachitk_seed`
- `sanpham_seed`
- `binhluan_sp_seed`
- `tk_thich_sanpham_seed`
- `tk_theodoi_seed`
- `tk_chan_seed`
- `ketqua_testcase`

Kết quả sau mỗi lần chạy chỉ được ghi vào bảng `ketqua_testcase`; chương trình không tạo file `.md` hoặc `.json`.

Khi API thật sẵn sàng, bước chuẩn bị dữ liệu sẽ cố gắng:

- Sinh đủ tài khoản seed trong `taikhoan_seed`.
- Signup trước số lượng tài khoản cấu hình trong `soTaiKhoanDangKyTruoc`.
- Gọi `/api/get_categories` và `/api/get_list_brands` để đồng bộ seed tham chiếu vào SQL Server.
- Tạo địa chỉ bằng `/addresses/create` để phục vụ `/api/add_product`.
- Tạo sản phẩm/comment/like mồi phục vụ các case Product.
- Tạo 10 quan hệ follow vào `tk_theodoi_seed` bằng `/set_user_follow`.
- Tạo 10 quan hệ block vào `tk_chan_seed` bằng `/set_user_block`, chọn cặp khác cặp follow để không làm hỏng tiền đề follow.

Những case cần điều kiện chưa thể tạo chỉ bằng API public, ví dụ rule khóa tài khoản, user bị seller chặn, product đã phát sinh đơn hàng, sẽ được đánh dấu `BoQua` với lý do tiếng Việt thay vì báo sai server.

## SQL Server Scripts

- `01_tao_database_va_bang_seed.sql`: tạo database `ApiShopTestDb` và các bảng seed.
- `02_nap_100_tai_khoan_seed.sql`: nạp 100 tài khoản seed thủ công nếu không muốn để runner tự sinh.
- `03_reset_du_lieu_test.sql`: reset dữ liệu mồi về trạng thái ban đầu.

## Báo lỗi

Khi case fail, thông điệp được phân loại:

- `ThatBai`: server trả sai `code` nghiệp vụ hoặc dữ liệu response sai expected.
- `BoQua`: thiếu dữ liệu mồi/điều kiện nghiệp vụ để chạy case tự động.
- `LoiChuanBi`: lỗi trong kịch bản test hoặc seed không hợp lệ.
- `LoiMoiTruong`: sai base URL, server chưa chạy, timeout hoặc lỗi network.
