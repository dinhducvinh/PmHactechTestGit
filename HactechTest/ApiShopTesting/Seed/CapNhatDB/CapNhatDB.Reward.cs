using System.Text.Json.Nodes;
using HactechTest.ApiShopTesting.Core;
using Microsoft.Data.SqlClient;
using static HactechTest.ApiShopTesting.Core.HelperTC;

namespace HactechTest.ApiShopTesting.Seed;

public sealed partial class CapNhatDB
{
    private const decimal SoCoinThuongRewardMacDinh = 1000000m;

    internal async Task LuuRewardProofSauAddRewardAsync(PhanHoiApi response, YeuCauApi request)
    {
        if (!LaMaThanhCong(response) ||
            request.Tam["taiKhoan"] is not TaiKhoanSignupThanhCongSeed taiKhoan)
        {
            return;
        }

        var proof = LayRewardProofObject(response.Data);
        var rewardId = DocIntTuObject(proof, "id", "reward_id", "reward_id_server");
        if (rewardId is not > 0)
        {
            return;
        }

        var aiScore = DocIntTuObject(proof, "ai_score");
        var rewardCoin = proof is null ? null : DocDecimalTuNode(proof["reward_coin"]);
        var rewardProof = DuLieu.RewardProofSeed.FirstOrDefault(x => x.RewardIdServer == rewardId.Value);
        if (rewardProof is null)
        {
            rewardProof = new RewardProofSeed
            {
                RewardIdServer = rewardId.Value,
                TaoLuc = DateTimeOffset.Now
            };
            DuLieu.RewardProofSeed.Add(rewardProof);
        }

        rewardProof.TaiKhoanIdServer = taiKhoan.TaiKhoanIdServer;
        rewardProof.AiScore = aiScore;
        rewardProof.RewardCoin = rewardCoin;
        rewardProof.TrangThai = TaoTrangThaiRewardProof(aiScore);
        rewardProof.CapNhatLuc = DateTimeOffset.Now;
        rewardProof.GhiChu = "Tạo bằng API /rewards/add_reward_proof.";

        var coCapNhatWallet = CapNhatWalletSauReward(taiKhoan, aiScore, rewardCoin);
        if (coCapNhatWallet)
        {
            await LuuAsync(BangDuLieuSeed.RewardProof, BangDuLieuSeed.Wallet);
            return;
        }

        await LuuAsync(BangDuLieuSeed.RewardProof);
    }

    internal async Task DanhDauRewardProofDaKhieuNaiAsync(RewardProofSeed rewardProof)
    {
        rewardProof.TrangThai = "da_khieu_nai";
        rewardProof.CapNhatLuc = DateTimeOffset.Now;
        rewardProof.GhiChu = "Tạo appeal bằng API /rewards/create_reward_appeal.";
        await LuuAsync(BangDuLieuSeed.RewardProof);
    }

    private async Task LuuRewardProofAsync(SqlConnection connection, SqlTransaction transaction)
    {
        const string sql = """
            MERGE dbo.reward_proof_seed AS target
            USING (SELECT @reward_id_server AS reward_id_server) AS source
            ON target.reward_id_server = source.reward_id_server
            WHEN MATCHED THEN
                UPDATE SET tk_id_server = @tk_id_server,
                    ai_score = @ai_score,
                    reward_coin = @reward_coin,
                    trang_thai = @trang_thai,
                    tao_luc = @tao_luc,
                    cap_nhat_luc = @cap_nhat_luc,
                    ghi_chu = @ghi_chu
            WHEN NOT MATCHED THEN
                INSERT (reward_id_server, tk_id_server, ai_score, reward_coin, trang_thai, tao_luc, cap_nhat_luc, ghi_chu)
                VALUES (@reward_id_server, @tk_id_server, @ai_score, @reward_coin, @trang_thai, @tao_luc, @cap_nhat_luc, @ghi_chu);
            """;

        foreach (var item in DuLieu.RewardProofSeed)
        {
            if (item.RewardIdServer is not > 0 ||
                item.TaiKhoanIdServer is not > 0)
            {
                continue;
            }

            await using var command = TaoLenh(sql, connection, transaction);
            Them(command, "@reward_id_server", item.RewardIdServer);
            Them(command, "@tk_id_server", item.TaiKhoanIdServer);
            Them(command, "@ai_score", item.AiScore);
            Them(command, "@reward_coin", item.RewardCoin);
            Them(command, "@trang_thai", item.TrangThai);
            Them(command, "@tao_luc", item.TaoLuc ?? DateTimeOffset.Now);
            Them(command, "@cap_nhat_luc", item.CapNhatLuc);
            Them(command, "@ghi_chu", item.GhiChu);
            await command.ExecuteNonQueryAsync();
        }
    }

    private bool CapNhatWalletSauReward(
        TaiKhoanSignupThanhCongSeed taiKhoan,
        int? aiScore,
        decimal? rewardCoin)
    {
        if (aiScore != 1)
        {
            return false;
        }

        var soCoin = rewardCoin is > 0 ? rewardCoin.Value : SoCoinThuongRewardMacDinh;
        var wallet = DuLieu.WalletSeed.FirstOrDefault(x => x.TaiKhoanIdServer == taiKhoan.TaiKhoanIdServer);
        if (wallet is null)
        {
            return false;
        }

        var availableBefore = wallet.AvailableBalance ?? wallet.Balance;
        wallet.Balance += soCoin;
        wallet.AvailableBalance = availableBefore + soCoin;
        wallet.XacMinhLuc = DateTimeOffset.Now;
        wallet.TrangThai ??= "san_sang";
        wallet.GhiChu = "Cập nhật sau khi add_reward_proof được thưởng.";
        return true;
    }

    private static JsonObject? LayRewardProofObject(JsonNode? data)
    {
        if (data is not JsonObject obj)
        {
            return null;
        }

        return obj["proof"] as JsonObject ?? obj;
    }

    private static string TaoTrangThaiRewardProof(int? aiScore)
    {
        return aiScore switch
        {
            1 => "san_sang",
            0 => "khong_duoc_thuong",
            _ => "ai_loi"
        };
    }
}
