namespace HactechTest.Services.Auth
{
    public sealed record TaiKhoanPhanMem(
        int Id,
        string TenDangNhap,
        string? HoTen,
        string? Email,
        string? SoDienThoai,
        string VaiTro,
        string TrangThai,
        DateTimeOffset TaoLuc,
        DateTimeOffset? CapNhatLuc,
        DateTimeOffset? DangNhapCuoiLuc)
    {
        public bool LaAdmin => string.Equals(VaiTro, "admin", StringComparison.OrdinalIgnoreCase);

        public string TenHienThi =>
            string.IsNullOrWhiteSpace(HoTen) ? TenDangNhap : HoTen;
    }

    public sealed record TaoTaiKhoanPhanMemRequest(
        string TenDangNhap,
        string MatKhau,
        string? HoTen,
        string? Email,
        string? SoDienThoai,
        string VaiTro,
        string TrangThai);

    public sealed record CapNhatTaiKhoanPhanMemRequest(
        int Id,
        string TenDangNhap,
        string? MatKhauMoi,
        string? HoTen,
        string? Email,
        string? SoDienThoai,
        string VaiTro,
        string TrangThai);
}
