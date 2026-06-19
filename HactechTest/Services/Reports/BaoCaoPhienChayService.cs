using System.Net;
using System.IO.Compression;
using System.Globalization;
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
            var duongDanExcel = Path.Combine(folder, tenFile + ".xlsx");
            var duLieuBaoCao = TaoDuLieuBaoCao(thongTin, ketQua);

            LuuBaoCao(duongDanNguoiDungChon, duLieuBaoCao);

            return new KetQuaLuuBaoCao(duongDanHtml, duongDanJson, duongDanExcel);
        }

        public KetQuaLuuBaoCao LuuBaoCao(string duongDanNguoiDungChon, DuLieuBaoCaoPhienChay duLieuBaoCao)
        {
            var folder = Path.GetDirectoryName(duongDanNguoiDungChon) ?? Environment.CurrentDirectory;
            var tenFile = Path.GetFileNameWithoutExtension(duongDanNguoiDungChon);
            var duongDanHtml = Path.Combine(folder, tenFile + ".html");
            var duongDanJson = Path.Combine(folder, tenFile + ".json");
            var duongDanExcel = Path.Combine(folder, tenFile + ".xlsx");

            File.WriteAllText(duongDanJson, JsonSerializer.Serialize(duLieuBaoCao, JsonOptionsBaoCao), Encoding.UTF8);
            File.WriteAllText(duongDanHtml, TaoHtmlBaoCao(duLieuBaoCao), Encoding.UTF8);
            LuuExcelBaoCao(duongDanExcel, duLieuBaoCao);

            return new KetQuaLuuBaoCao(duongDanHtml, duongDanJson, duongDanExcel);
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

        public DuLieuBaoCaoPhienChay TaoDuLieuBaoCaoTuDongKetQua(
            ThongTinBaoCaoPhienChay thongTin,
            IReadOnlyList<DongBaoCaoTestCase> ketQua)
        {
            var tong = ketQua.Count;
            var dat = ketQua.Count(x => x.TrangThaiGoc == TrangThaiKetQua.Dat);

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
                KetQua = ketQua.ToList()
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

        public void LuuExcelBaoCao(string duongDanExcel, DuLieuBaoCaoPhienChay baoCao)
        {
            if (File.Exists(duongDanExcel))
            {
                File.Delete(duongDanExcel);
            }

            using var archive = ZipFile.Open(duongDanExcel, ZipArchiveMode.Create);
            ThemZipEntry(archive, "[Content_Types].xml", TaoContentTypesXml());
            ThemZipEntry(archive, "_rels/.rels", TaoRootRelsXml());
            ThemZipEntry(archive, "xl/workbook.xml", TaoWorkbookXml());
            ThemZipEntry(archive, "xl/_rels/workbook.xml.rels", TaoWorkbookRelsXml());
            ThemZipEntry(archive, "xl/styles.xml", TaoStylesXml());
            ThemZipEntry(archive, "xl/worksheets/sheet1.xml", TaoWorksheetXml(baoCao));
        }

        private static void ThemZipEntry(ZipArchive archive, string tenEntry, string noiDung)
        {
            var entry = archive.CreateEntry(tenEntry);
            using var stream = entry.Open();
            using var writer = new StreamWriter(stream, new UTF8Encoding(false));
            writer.Write(noiDung.TrimStart());
        }

        private static string TaoContentTypesXml()
        {
            return """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
                  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
                  <Default Extension="xml" ContentType="application/xml"/>
                  <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
                  <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
                  <Override PartName="/xl/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"/>
                </Types>
                """;
        }

        private static string TaoRootRelsXml()
        {
            return """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
                </Relationships>
                """;
        }

        private static string TaoWorkbookXml()
        {
            return """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                  <sheets>
                    <sheet name="Báo cáo" sheetId="1" r:id="rId1"/>
                  </sheets>
                </workbook>
                """;
        }

        private static string TaoWorkbookRelsXml()
        {
            return """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
                  <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/>
                </Relationships>
                """;
        }

        private static string TaoStylesXml()
        {
            return """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
                  <fonts count="2">
                    <font><sz val="11"/><name val="Calibri"/></font>
                    <font><b/><sz val="11"/><name val="Calibri"/></font>
                  </fonts>
                  <fills count="2">
                    <fill><patternFill patternType="none"/></fill>
                    <fill><patternFill patternType="gray125"/></fill>
                  </fills>
                  <borders count="1"><border/></borders>
                  <cellStyleXfs count="1"><xf numFmtId="0" fontId="0" fillId="0" borderId="0"/></cellStyleXfs>
                  <cellXfs count="2">
                    <xf numFmtId="0" fontId="0" fillId="0" borderId="0" xfId="0"/>
                    <xf numFmtId="0" fontId="1" fillId="0" borderId="0" xfId="0" applyFont="1"/>
                  </cellXfs>
                  <cellStyles count="1"><cellStyle name="Normal" xfId="0" builtinId="0"/></cellStyles>
                </styleSheet>
                """;
        }

        private static string TaoWorksheetXml(DuLieuBaoCaoPhienChay baoCao)
        {
            var rows = new List<(IReadOnlyList<string?> Values, int Style)>
            {
                (new[] { "Báo cáo kiểm thử API Shop" }, 1),
                (new[] { "Thời điểm chạy", baoCao.ThoiDiemChay }, 0),
                (new[] { "Thời điểm kết thúc", baoCao.ThoiDiemKetThuc }, 0),
                (new[] { "Người thực hiện", baoCao.NguoiThucHien }, 0),
                (new[] { "Máy chạy", baoCao.MayChay }, 0),
                (new[] { "Hệ điều hành", baoCao.HeDieuHanh }, 0),
                (new[] { "Base URL", baoCao.BaseUrl }, 0),
                (new[] { "Chế độ chạy", baoCao.CheDoChay }, 0),
                (new[] { "Chế độ lỗi", baoCao.CheDoLoi }, 0),
                (new[] { "Tổng", baoCao.Tong.ToString(CultureInfo.InvariantCulture), "Đạt", baoCao.Dat.ToString(CultureInfo.InvariantCulture), "Không đạt", baoCao.KhongDat.ToString(CultureInfo.InvariantCulture), "Tỷ lệ", baoCao.TyLeDat.ToString(CultureInfo.InvariantCulture) + "%" }, 0),
                (Array.Empty<string?>(), 0),
                (new[]
                {
                    "STT", "Mã", "Nhóm", "Tên", "Kết quả", "Mong đợi", "Thực tế", "HTTP",
                    "ms", "Endpoint", "Thông điệp", "Request body", "Response"
                }, 1)
            };

            rows.AddRange(baoCao.KetQua.Select(item => ((IReadOnlyList<string?>)new[]
            {
                item.Stt.ToString(CultureInfo.InvariantCulture),
                item.Ma,
                item.Nhom,
                item.TenHienThi,
                DinhDangKetQuaKiemThu.TrangThaiHienThi(item.TrangThaiGoc),
                item.MaMongDoi,
                item.MaThucTe,
                item.HttpStatus?.ToString(CultureInfo.InvariantCulture),
                item.ThoiGianMs.ToString(CultureInfo.InvariantCulture),
                item.Endpoint,
                item.ThongDiep,
                item.RequestBodyJson,
                item.ResponseRutGon
            }, 0)));

            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            sb.AppendLine("<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\">");
            sb.AppendLine("<cols>");
            var widths = new[] { 8, 24, 16, 44, 14, 24, 24, 10, 10, 48, 64, 64, 64 };
            for (var i = 0; i < widths.Length; i++)
            {
                sb.AppendLine($"<col min=\"{i + 1}\" max=\"{i + 1}\" width=\"{widths[i]}\" customWidth=\"1\"/>");
            }

            sb.AppendLine("</cols><sheetData>");
            for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                var rowNumber = rowIndex + 1;
                var row = rows[rowIndex];
                sb.AppendLine($"<row r=\"{rowNumber}\">");
                for (var colIndex = 0; colIndex < row.Values.Count; colIndex++)
                {
                    sb.AppendLine(TaoCellXml(rowNumber, colIndex, row.Values[colIndex], row.Style));
                }

                sb.AppendLine("</row>");
            }

            if (baoCao.KetQua.Count > 0)
            {
                sb.AppendLine($"<autoFilter ref=\"A12:M{rows.Count}\"/>");
            }

            sb.AppendLine("</sheetData></worksheet>");
            return sb.ToString();
        }

        private static string TaoCellXml(int rowNumber, int colIndex, string? value, int style)
        {
            var styleAttr = style > 0 ? $" s=\"{style}\"" : "";
            var cellRef = TenCotExcel(colIndex) + rowNumber.ToString(CultureInfo.InvariantCulture);
            return $"<c r=\"{cellRef}\" t=\"inlineStr\"{styleAttr}><is><t>{Xml(value)}</t></is></c>";
        }

        private static string TenCotExcel(int colIndex)
        {
            var dividend = colIndex + 1;
            var columnName = "";
            while (dividend > 0)
            {
                var modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar('A' + modulo) + columnName;
                dividend = (dividend - modulo) / 26;
            }

            return columnName;
        }

        private static string Xml(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "";
            }

            var hopLe = new StringBuilder(value.Length);
            foreach (var ch in value)
            {
                if (ch is '\t' or '\n' or '\r' || ch >= ' ')
                {
                    hopLe.Append(ch);
                }
            }

            return System.Security.SecurityElement.Escape(hopLe.ToString()) ?? "";
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

    public sealed record KetQuaLuuBaoCao(string DuongDanHtml, string DuongDanJson, string DuongDanExcel);

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
