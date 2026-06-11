using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using HactechTest.ApiShopTesting.Core;

namespace HactechTest.Services.Reports
{
    public sealed class BaoCaoPhienChayService
    {
        private static readonly JsonSerializerOptions JsonOptionsBaoCao = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public KetQuaLuuBaoCao LuuBaoCao(
            string duongDanNguoiDungChon,
            ThongTinBaoCaoPhienChay thongTin,
            IReadOnlyList<KetQuaChay> ketQua)
        {
            var folder = Path.GetDirectoryName(duongDanNguoiDungChon) ?? Environment.CurrentDirectory;
            var tenFile = Path.GetFileNameWithoutExtension(duongDanNguoiDungChon);
            var duongDanHtml = Path.Combine(folder, tenFile + ".html");
            var duongDanJson = Path.Combine(folder, tenFile + ".json");
            var duLieuBaoCao = TaoDuLieuBaoCao(thongTin, ketQua);

            File.WriteAllText(duongDanJson, JsonSerializer.Serialize(duLieuBaoCao, JsonOptionsBaoCao), Encoding.UTF8);
            File.WriteAllText(duongDanHtml, TaoHtmlBaoCao(duLieuBaoCao), Encoding.UTF8);

            return new KetQuaLuuBaoCao(duongDanHtml, duongDanJson);
        }

        public DuLieuBaoCaoPhienChay TaoDuLieuBaoCao(
            ThongTinBaoCaoPhienChay thongTin,
            IReadOnlyList<KetQuaChay> ketQua)
        {
            var tong = ketQua.Count;
            var dat = ketQua.Count(x => x.TrangThai == TrangThaiKetQua.Dat);

            return new DuLieuBaoCaoPhienChay
            {
                ThoiDiemChay = thongTin.ThoiDiemChay?.ToString("yyyy-MM-dd HH:mm:ss"),
                ThoiDiemKetThuc = thongTin.ThoiDiemKetThuc?.ToString("yyyy-MM-dd HH:mm:ss"),
                NguoiThucHien = thongTin.NguoiThucHien,
                MayChay = thongTin.MayChay,
                HeDieuHanh = thongTin.HeDieuHanh,
                BaseUrl = thongTin.BaseUrl,
                CheDoChay = thongTin.CheDoChay,
                CheDoLoi = thongTin.CheDoLoi,
                Tong = tong,
                Dat = dat,
                KhongDat = tong - dat,
                TyLeDat = tong == 0 ? 0 : Math.Round(100m * dat / tong, 2),
                KetQua = ketQua.Select((x, i) => new DongBaoCaoTestCase
                {
                    Stt = i + 1,
                    Ma = x.Ma,
                    Nhom = x.Nhom,
                    TenHienThi = x.TenHienThi,
                    TrangThai = x.TrangThai.ToString(),
                    MaMongDoi = x.MaMongDoi,
                    MaThucTe = x.MaThucTe,
                    HttpStatus = x.HttpStatus,
                    ThoiGianMs = (int)Math.Round(x.ThoiGian.TotalMilliseconds),
                    Endpoint = x.Endpoint,
                    RequestBodyJson = x.RequestBodyJson,
                    ThongDiep = x.ThongDiep,
                    ResponseRutGon = x.ResponseRutGon,
                    TrangThaiGoc = x.TrangThai
                }).ToList()
            };
        }

        public string TaoHtmlBaoCao(DuLieuBaoCaoPhienChay baoCao)
        {
            static string E(string? s) => WebUtility.HtmlEncode(s ?? "");

            var sb = new StringBuilder();
            const string tieuDeBangKetQua = "<table><thead><tr><th>STT</th><th>Mã</th><th>Nhóm</th><th>Tên</th><th>Kết quả</th><th>Mong đợi</th><th>Thực tế</th><th>HTTP</th><th>ms</th><th>Endpoint</th><th>Thông điệp</th></tr></thead><tbody>";

            void GhiDongKetQua(DongBaoCaoTestCase item)
            {
                var cls = item.TrangThaiGoc == TrangThaiKetQua.Dat ? "pass" : "fail";
                var rowCls = item.TrangThaiGoc == TrangThaiKetQua.Dat ? "" : " class=\"fail-row\"";
                sb.AppendLine($"<tr{rowCls}>");
                sb.AppendLine($"<td>{item.Stt}</td><td>{E(item.Ma)}</td><td>{E(item.Nhom)}</td><td>{E(item.TenHienThi)}</td>");
                sb.AppendLine($"<td class=\"{cls}\">{E(DinhDangKetQuaKiemThu.TrangThaiHienThi(item.TrangThaiGoc))}</td><td>{E(item.MaMongDoi)}</td><td>{E(item.MaThucTe)}</td>");
                sb.AppendLine($"<td>{item.HttpStatus?.ToString() ?? ""}</td><td>{item.ThoiGianMs}</td><td>{E(item.Endpoint)}</td><td>{E(item.ThongDiep)}</td>");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("<!doctype html><html lang=\"vi\"><head><meta charset=\"utf-8\"><title>Báo cáo API Shop</title>");
            sb.AppendLine("<style>body{font-family:Segoe UI,Arial,sans-serif;margin:24px;background:#f4f5f7;color:#1f2933}section{background:#fff;border:1px solid #d9dee5;padding:16px;margin-bottom:16px}table{width:100%;border-collapse:collapse}th,td{border:1px solid #d9dee5;padding:8px;text-align:left;vertical-align:top}th{background:#eef1f5}.pass{color:#198754;font-weight:700}.fail{color:#dc3545;font-weight:700}.fail-row{background:#fff1f1}.meta{display:grid;grid-template-columns:180px 1fr;gap:6px 12px}.empty{color:#5b6470;font-style:italic}</style>");
            sb.AppendLine("</head><body><section><h1>Báo cáo kiểm thử API Shop</h1><div class=\"meta\">");
            sb.AppendLine($"<strong>Thời điểm chạy</strong><span>{E(baoCao.ThoiDiemChay)}</span>");
            sb.AppendLine($"<strong>Người thực hiện</strong><span>{E(baoCao.NguoiThucHien)}</span>");
            sb.AppendLine($"<strong>Máy chạy</strong><span>{E(baoCao.MayChay)}</span>");
            sb.AppendLine($"<strong>OS</strong><span>{E(baoCao.HeDieuHanh)}</span>");
            sb.AppendLine($"<strong>Base URL</strong><span>{E(baoCao.BaseUrl)}</span>");
            sb.AppendLine($"<strong>Tổng kết</strong><span>Tổng: {baoCao.Tong}, Đạt: {baoCao.Dat}, Không đạt: {baoCao.KhongDat}, Tỷ lệ: {baoCao.TyLeDat}%</span>");
            sb.AppendLine("</div></section><section><h2>Testcase không PASS</h2>");

            var khongPass = baoCao.KetQua
                .Where(x => x.TrangThaiGoc != TrangThaiKetQua.Dat)
                .ToList();

            if (khongPass.Count == 0)
            {
                sb.AppendLine("<p class=\"empty\">Không có testcase không PASS.</p>");
            }
            else
            {
                sb.AppendLine(tieuDeBangKetQua);
                foreach (var item in khongPass)
                {
                    GhiDongKetQua(item);
                }

                sb.AppendLine("</tbody></table>");
            }

            sb.AppendLine("</section><section><h2>Chi tiết testcase</h2>");
            sb.AppendLine(tieuDeBangKetQua);

            foreach (var item in baoCao.KetQua)
            {
                GhiDongKetQua(item);
            }

            sb.AppendLine("</tbody></table></section></body></html>");
            return sb.ToString();
        }

    }

    public sealed record ThongTinBaoCaoPhienChay(
        DateTimeOffset? ThoiDiemChay,
        DateTimeOffset? ThoiDiemKetThuc,
        string NguoiThucHien,
        string MayChay,
        string HeDieuHanh,
        string BaseUrl,
        string CheDoChay,
        string CheDoLoi);

    public sealed record KetQuaLuuBaoCao(string DuongDanHtml, string DuongDanJson);

    public sealed class DuLieuBaoCaoPhienChay
    {
        [JsonPropertyName("thoiDiemChay")]
        public string? ThoiDiemChay { get; init; }

        [JsonPropertyName("thoiDiemKetThuc")]
        public string? ThoiDiemKetThuc { get; init; }

        [JsonPropertyName("nguoiThucHien")]
        public required string NguoiThucHien { get; init; }

        [JsonPropertyName("mayChay")]
        public required string MayChay { get; init; }

        [JsonPropertyName("heDieuHanh")]
        public required string HeDieuHanh { get; init; }

        [JsonPropertyName("baseUrl")]
        public required string BaseUrl { get; init; }

        [JsonPropertyName("cheDoChay")]
        public required string CheDoChay { get; init; }

        [JsonPropertyName("cheDoLoi")]
        public required string CheDoLoi { get; init; }

        [JsonPropertyName("tong")]
        public int Tong { get; init; }

        [JsonPropertyName("dat")]
        public int Dat { get; init; }

        [JsonPropertyName("khongDat")]
        public int KhongDat { get; init; }

        [JsonPropertyName("tyLeDat")]
        public decimal TyLeDat { get; init; }

        [JsonPropertyName("ketQua")]
        public required List<DongBaoCaoTestCase> KetQua { get; init; }
    }

    public sealed class DongBaoCaoTestCase
    {
        [JsonPropertyName("stt")]
        public int Stt { get; init; }

        public required string Ma { get; init; }
        public required string Nhom { get; init; }
        public required string TenHienThi { get; init; }

        [JsonPropertyName("trangThai")]
        public required string TrangThai { get; init; }

        public string? MaMongDoi { get; init; }
        public string? MaThucTe { get; init; }
        public int? HttpStatus { get; init; }

        [JsonPropertyName("thoiGianMs")]
        public int ThoiGianMs { get; init; }

        public string? Endpoint { get; init; }
        public string? RequestBodyJson { get; init; }
        public required string ThongDiep { get; init; }
        public string? ResponseRutGon { get; init; }

        [JsonIgnore]
        public TrangThaiKetQua TrangThaiGoc { get; init; }
    }
}
