using System.Text.Json.Nodes;
using HactechTest.ApiShopTesting.Core;
using HactechTest.ApiShopTesting.Seed;
using static HactechTest.ApiShopTesting.Core.HelperTC;

namespace HactechTest.ApiShopTesting.KichBan;

public static partial class BoKichBanApi
{
    private const string RewardImageUrl = "https://upload.wikimedia.org/wikipedia/commons/a/a9/Example.jpg";

    private static readonly IReadOnlySet<string> RewardSaiToken = Tap("9998", "9995", "HTTP_401", "HTTP_403");
    private static readonly IReadOnlySet<string> RewardOkHoacAiLoi = Tap("1000", "1005", "HTTP_500");
    private static readonly IReadOnlySet<string> RewardHistorySaiBody = Tap("1004", "1005", "HTTP_500");

    private static void ThemKichBanRewards(List<KichBanApi> ds)
    {
        ThemAddRewardProof(ds);
        ThemGetRewardProof(ds);
        ThemGetRewardHistory(ds);
        ThemCreateRewardAppeal(ds);
    }

    private static void ThemAddRewardProof(List<KichBanApi> ds)
    {
        Them(ds, "REWARD-ADD-01", "Rewards", "Gửi bằng chứng nhận thưởng bằng token sai",
            "Gọi POST /rewards/add_reward_proof bằng token hết hạn/sai định dạng, body có image_url và description hợp lệ.",
            ctx => Req(
                HttpMethod.Post,
                "/rewards/add_reward_proof",
                TaoBodyRewardProofHopLe("REWARD-ADD-01"),
                ctx.TokenSaiDinhDang),
            RewardSaiToken);

        Them(ds, "REWARD-ADD-02", "Rewards", "Gửi bằng chứng nhận thưởng hợp lệ",
            "Token hợp lệ, gửi image_url và description hợp lệ. Nếu API tạo proof thành công thì lưu reward_proof_seed và cập nhật wallet_seed khi ai_score = 1.",
            async ctx =>
            {
                var taiKhoan = await YeuCauTaiKhoanCoWalletAsync(ctx);
                var request = new YeuCauApi(
                    HttpMethod.Post,
                    "/rewards/add_reward_proof",
                    TaoBodyRewardProofHopLe("REWARD-ADD-02"),
                    await LayTokenCuaTaiKhoanAsync(ctx, taiKhoan));
                request.Tam["taiKhoan"] = taiKhoan;
                if (LayWalletCuaTaiKhoan(ctx, taiKhoan) is { } wallet)
                {
                    request.Tam["balanceBefore"] = wallet.Balance;
                    request.Tam["availableBalanceBefore"] = wallet.AvailableBalance;
                }

                return request;
            },
            RewardOkHoacAiLoi,
            KiemTraResponseAddRewardProof(),
            async (response, request, ctx) => await ctx.CapNhatDB.LuuRewardProofSauAddRewardAsync(response, request));

        Them(ds, "REWARD-ADD-03", "Rewards", "Gửi bằng chứng thiếu media url",
            "Token hợp lệ, body có description nhưng không có image_url/video_url.",
            async ctx => new YeuCauApi(
                HttpMethod.Post,
                "/rewards/add_reward_proof",
                Obj(("description", "Thiếu URL media")),
                await YeuCauTokenHopLeAsync(ctx)),
            SaiGiaTri);

        Them(ds, "REWARD-ADD-04", "Rewards", "Gửi bằng chứng media lỗi hoặc AI lỗi",
            "Token hợp lệ, image_url không ổn định/không tải được; service có thể trả 1000 kèm data.error hoặc 1005.",
            async ctx =>
            {
                var taiKhoan = await YeuCauTaiKhoanCoWalletAsync(ctx);
                var request = new YeuCauApi(
                    HttpMethod.Post,
                    "/rewards/add_reward_proof",
                    Obj(
                        ("image_url", "https://invalid.example.invalid/reward-proof-not-found.jpg"),
                        ("description", "Media lỗi để kiểm tra nhanh AI/service")),
                    await LayTokenCuaTaiKhoanAsync(ctx, taiKhoan));
                request.Tam["taiKhoan"] = taiKhoan;
                return request;
            },
            RewardOkHoacAiLoi,
            KiemTraResponseAddRewardProof(),
            async (response, request, ctx) => await ctx.CapNhatDB.LuuRewardProofSauAddRewardAsync(response, request));
    }

    private static void ThemGetRewardProof(List<KichBanApi> ds)
    {
        Them(ds, "REWARD-PROOF-01", "Rewards", "Lấy chi tiết reward proof bằng token sai",
            "Gọi POST /rewards/get_reward_proof bằng token sai, body có reward_id.",
            ctx => Req(
                HttpMethod.Post,
                "/rewards/get_reward_proof",
                Obj(("reward_id", LayRewardIdDangCoHoacMacDinh(ctx))),
                ctx.TokenSaiDinhDang),
            RewardSaiToken);

        Them(ds, "REWARD-PROOF-02", "Rewards", "Lấy chi tiết reward proof của current user",
            "Token hợp lệ của user tạo proof, reward_id lấy từ reward_proof_seed.",
            async ctx =>
            {
                var cap = LayRewardProofBatKy(ctx);
                var request = new YeuCauApi(
                    HttpMethod.Post,
                    "/rewards/get_reward_proof",
                    Obj(("reward_id", IdBatBuoc(cap.RewardProof.RewardIdServer, "reward_proof_seed.reward_id_server"))),
                    await LayTokenCuaTaiKhoanAsync(ctx, cap.TaiKhoan));
                request.Tam["rewardProof"] = cap.RewardProof;
                return request;
            },
            Ok,
            KiemTraChiTietRewardProof());

        Them(ds, "REWARD-PROOF-03", "Rewards", "Lấy reward proof không tồn tại",
            "Token hợp lệ, reward_id không tồn tại trên server.",
            async ctx => new YeuCauApi(
                HttpMethod.Post,
                "/rewards/get_reward_proof",
                Obj(("reward_id", LayRewardIdKhongTonTai(ctx))),
                await YeuCauTokenHopLeAsync(ctx)),
            SaiGiaTri);

        Them(ds, "REWARD-PROOF-04", "Rewards", "Lấy reward proof của user khác",
            "Token hợp lệ của user A, reward_id thuộc user B.",
            async ctx =>
            {
                var cap = LayRewardProofBatKy(ctx);
                var taiKhoanKhac = LayTaiKhoanKhacRewardProof(ctx, cap.RewardProof);
                return new YeuCauApi(
                    HttpMethod.Post,
                    "/rewards/get_reward_proof",
                    Obj(("reward_id", IdBatBuoc(cap.RewardProof.RewardIdServer, "reward_proof_seed.reward_id_server"))),
                    await LayTokenCuaTaiKhoanAsync(ctx, taiKhoanKhac));
            },
            SaiGiaTri);

        Them(ds, "REWARD-PROOF-05", "Rewards", "Lấy reward proof thiếu reward_id",
            "Token hợp lệ, body thiếu reward_id theo DTO @IsInt.",
            async ctx => new YeuCauApi(
                HttpMethod.Post,
                "/rewards/get_reward_proof",
                Obj(),
                await YeuCauTokenHopLeAsync(ctx)),
            ThieuThamSo);
    }

    private static void ThemGetRewardHistory(List<KichBanApi> ds)
    {
        Them(ds, "REWARD-HISTORY-01", "Rewards", "Lấy lịch sử reward bằng token sai",
            "Gọi POST /rewards/get_reward_history bằng token sai, body index/count hợp lệ.",
            ctx => Req(
                HttpMethod.Post,
                "/rewards/get_reward_history",
                Obj(("index", 1), ("count", 10)),
                ctx.TokenSaiDinhDang),
            RewardSaiToken);

        Them(ds, "REWARD-HISTORY-02", "Rewards", "Lấy lịch sử reward của current user",
            "Token hợp lệ của user đã có reward proof, index = 1 và count = 10.",
            async ctx =>
            {
                var cap = LayRewardProofBatKy(ctx);
                var request = new YeuCauApi(
                    HttpMethod.Post,
                    "/rewards/get_reward_history",
                    Obj(("index", 1), ("count", 10)),
                    await LayTokenCuaTaiKhoanAsync(ctx, cap.TaiKhoan));
                request.Tam["rewardProof"] = cap.RewardProof;
                return request;
            },
            Ok,
            KiemTraRewardHistory());

        Them(ds, "REWARD-HISTORY-03", "Rewards", "Lấy lịch sử reward sai body phân trang",
            "Token hợp lệ nhưng index/count sai kiểu; DTO hiện chưa validate chặt nên có thể trả 1005.",
            async ctx => new YeuCauApi(
                HttpMethod.Post,
                "/rewards/get_reward_history",
                Obj(("index", "abc"), ("count", "xyz")),
                await YeuCauTokenHopLeAsync(ctx)),
            RewardHistorySaiBody);
    }

    private static void ThemCreateRewardAppeal(List<KichBanApi> ds)
    {
        Them(ds, "REWARD-APPEAL-01", "Rewards", "Tạo appeal reward bằng token sai",
            "Gọi POST /rewards/create_reward_appeal bằng token sai, body có reward_id và reason hợp lệ.",
            ctx => Req(
                HttpMethod.Post,
                "/rewards/create_reward_appeal",
                TaoBodyRewardAppeal(LayRewardIdDangCoHoacMacDinh(ctx)),
                ctx.TokenSaiDinhDang),
            RewardSaiToken);

        Them(ds, "REWARD-APPEAL-02", "Rewards", "Tạo appeal reward hợp lệ",
            "Token hợp lệ của user tạo proof, reward_id tồn tại và ai_score != 1.",
            async ctx =>
            {
                var cap = LayRewardProofDeAppeal(ctx);
                var request = new YeuCauApi(
                    HttpMethod.Post,
                    "/rewards/create_reward_appeal",
                    TaoBodyRewardAppeal(IdBatBuoc(cap.RewardProof.RewardIdServer, "reward_proof_seed.reward_id_server")),
                    await LayTokenCuaTaiKhoanAsync(ctx, cap.TaiKhoan));
                request.Tam["rewardProof"] = cap.RewardProof;
                return request;
            },
            Ok,
            KiemTraRewardAppeal(),
            async (_, request, ctx) =>
            {
                if (request.Tam["rewardProof"] is RewardProofSeed rewardProof)
                {
                    await ctx.CapNhatDB.DanhDauRewardProofDaKhieuNaiAsync(rewardProof);
                }
            });

        Them(ds, "REWARD-APPEAL-03", "Rewards", "Tạo appeal reward_id không tồn tại",
            "Token hợp lệ, reward_id không tồn tại trên server.",
            async ctx => new YeuCauApi(
                HttpMethod.Post,
                "/rewards/create_reward_appeal",
                TaoBodyRewardAppeal(LayRewardIdKhongTonTai(ctx)),
                await YeuCauTokenHopLeAsync(ctx)),
            SaiGiaTri);

        Them(ds, "REWARD-APPEAL-04", "Rewards", "Tạo appeal reward của user khác",
            "Token hợp lệ của user A, reward_id thuộc user B.",
            async ctx =>
            {
                var cap = LayRewardProofBatKy(ctx);
                var taiKhoanKhac = LayTaiKhoanKhacRewardProof(ctx, cap.RewardProof);
                return new YeuCauApi(
                    HttpMethod.Post,
                    "/rewards/create_reward_appeal",
                    TaoBodyRewardAppeal(IdBatBuoc(cap.RewardProof.RewardIdServer, "reward_proof_seed.reward_id_server")),
                    await LayTokenCuaTaiKhoanAsync(ctx, taiKhoanKhac));
            },
            SaiGiaTri);
    }

    private static Dictionary<string, object?> TaoBodyRewardProofHopLe(string maTestCase)
    {
        return Obj(
            ("image_url", RewardImageUrl),
            ("description", $"Bằng chứng nhận thưởng {maTestCase} {DateTimeOffset.Now:yyyyMMddHHmmssfff}"));
    }

    private static Dictionary<string, object?> TaoBodyRewardAppeal(int rewardId)
    {
        return Obj(
            ("reward_id", rewardId),
            ("reason", "AI đánh giá sai bằng chứng"));
    }

    private static Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task<KetQuaKiemTraThem>> KiemTraResponseAddRewardProof()
    {
        return (response, _, _) =>
        {
            if (!LaMaThanhCong(response))
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

            var proof = LayRewardProofObject(response.Data);
            if (proof is null)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "data của add_reward_proof không có proof object."));
            }

            if (HelperTC.DocIntTuObject(proof, "id", "reward_id", "reward_id_server") is not > 0)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "proof thiếu id."));
            }

            var aiScore = HelperTC.DocIntTuObject(proof, "ai_score");
            var rewardCoin = DocDecimalTuNode(proof["reward_coin"]);
            if (aiScore == 1 && rewardCoin != 1000000m)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, $"ai_score = 1 thì reward_coin phải bằng 1000000, thực tế {rewardCoin?.ToString() ?? "null"}."));
            }

            if (aiScore == 0 && rewardCoin != 0m)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, $"ai_score = 0 thì reward_coin phải bằng 0, thực tế {rewardCoin?.ToString() ?? "null"}."));
            }

            return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
        };
    }

    private static Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task<KetQuaKiemTraThem>> KiemTraChiTietRewardProof()
    {
        return (response, request, _) =>
        {
            if (!LaMaThanhCong(response))
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

            if (response.Data is not JsonObject data)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "data của get_reward_proof không phải object."));
            }

            if (request.Tam["rewardProof"] is RewardProofSeed rewardProof)
            {
                var id = HelperTC.DocIntTuObject(data, "id", "reward_id", "reward_id_server");
                if (id != rewardProof.RewardIdServer)
                {
                    return Task.FromResult(new KetQuaKiemTraThem(false, $"data.id phải bằng {rewardProof.RewardIdServer}, thực tế {id?.ToString() ?? "null"}."));
                }
            }

            foreach (var truong in new[] { "user", "appeals" })
            {
                if (!data.ContainsKey(truong))
                {
                    return Task.FromResult(new KetQuaKiemTraThem(false, $"data thiếu trường `{truong}`."));
                }
            }

            return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
        };
    }

    private static Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task<KetQuaKiemTraThem>> KiemTraRewardHistory()
    {
        return (response, _, _) =>
        {
            if (!LaMaThanhCong(response))
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

            if (response.Data is not JsonArray)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "data của get_reward_history không phải mảng RewardProof[]."));
            }

            return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
        };
    }

    private static Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task<KetQuaKiemTraThem>> KiemTraRewardAppeal()
    {
        return (response, _, _) =>
        {
            if (!LaMaThanhCong(response))
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

            if (response.Data is not JsonObject data)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "data của create_reward_appeal không phải object."));
            }

            var status = DocChuoiTuObject(data, "status");
            if (!string.Equals(status, "pending", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, $"appeal.status phải bằng pending, thực tế {status ?? "null"}."));
            }

            return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
        };
    }

    private static JsonObject? LayRewardProofObject(JsonNode? data)
    {
        if (data is not JsonObject obj)
        {
            return null;
        }

        return obj["proof"] as JsonObject ?? obj;
    }
}
