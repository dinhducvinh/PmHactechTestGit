using System.Text.Json;

namespace ApiTest.Services
{
    /// <summary>
    /// Định nghĩa tĩnh 27 test case cho 7 API thuộc nhóm "Xác thực & Tài khoản" theo
    /// "tài liệu mô tả các API.md". Đường dẫn HTTP đặt theo tên API ("/<api_name>") để
    /// đồng nhất với tên trong tài liệu - người dùng có thể chỉnh ở mục Path nếu API team
    /// triển khai dưới prefix khác.
    /// </summary>
    public static class AuthCatalog
    {
        public const string TenBoSuuTap = "UAV API";
        public const string TenModule = "Xác thực & Tài khoản";

        // ---- Đường dẫn cho từng API (đổi nếu API team triển khai khác) ----
        private const string PathSignup                  = "/signup";
        private const string PathLogin                   = "/login";
        private const string PathLogout                  = "/logout";
        private const string PathCreateCodeResetPassword = "/create_code_reset_password";
        private const string PathCheckCodeResetPassword  = "/check_code_reset_password";
        private const string PathResetPassword           = "/reset_password";
        private const string PathChangePassword          = "/change_password";

        public static IReadOnlyList<TestCaseDefinition> TatCa => _tatCa.Value;

        private static readonly Lazy<IReadOnlyList<TestCaseDefinition>> _tatCa =
            new(() =>
            {
                var ds = new List<TestCaseDefinition>();
                ds.AddRange(SignupTestCases());
                ds.AddRange(LoginTestCases());
                ds.AddRange(LogoutTestCases());
                ds.AddRange(CreateCodeResetPasswordTestCases());
                ds.AddRange(CheckCodeResetPasswordTestCases());
                ds.AddRange(ResetPasswordTestCases());
                ds.AddRange(ChangePasswordTestCases());
                return ds;
            });

        // ====================================================================
        // 1) signup
        // ====================================================================
        private static IEnumerable<TestCaseDefinition> SignupTestCases()
        {
            yield return new TestCaseDefinition
            {
                Id = "signup_TC1",
                ApiName = "signup",
                DisplayName = "signup TC1 - Đăng ký thành công với SĐT chưa đăng ký",
                HttpMethod = "POST",
                PathTemplate = PathSignup,
                ExpectedAppCode = "1000",
                Description = "SĐT chưa đăng ký + password hợp lệ + uuid hợp lệ → 1000 OK.",
                PrepareAsync = async (ctx, ct) =>
                {
                    var acc = await ctx.SeedStore.LayMotTaiKhoanChuaDangKyAsync(ct);
                    if (acc == null)
                    {
                        return new TestRequestPlan
                        {
                            SkipExecution = true,
                            SkipReason = "Bảng mồi không còn tài khoản chua_dang_ky."
                        };
                    }
                    ctx.WorkingAccount = acc;
                    return new TestRequestPlan
                    {
                        JsonBody = new
                        {
                            phonenumber = acc.PhoneNumber,
                            password = acc.Password,
                            uuid = ctx.Config.TestData.DefaultUuid
                        }
                    };
                },
                PostCheckAsync = async (ctx, resp, ct) =>
                {
                    var serverId = LayChuoi(resp.Data, "id");
                    if (string.IsNullOrEmpty(serverId))
                    {
                        return ValidationOutcome.Fail("Response thiếu data.id sau khi đăng ký.");
                    }
                    if (ctx.WorkingAccount != null)
                    {
                        await ctx.SeedStore.DanhDauDaDangKyAsync(ctx.WorkingAccount, serverId, ct);
                    }
                    return ValidationOutcome.Ok($"Đã sync data.id={serverId} vào bảng mồi.");
                }
            };

            yield return new TestCaseDefinition
            {
                Id = "signup_TC2",
                ApiName = "signup",
                DisplayName = "signup TC2 - SĐT đã đăng ký",
                HttpMethod = "POST",
                PathTemplate = PathSignup,
                ExpectedAppCode = "1004",
                Description = "SĐT đã đăng ký + password hợp lệ → 1004 Parameter value is invalid.",
                PrepareAsync = async (ctx, ct) =>
                {
                    var acc = await EnsureRegisteredAccountAsync(ctx, ct);
                    if (acc == null)
                    {
                        return new TestRequestPlan
                        {
                            SkipExecution = true,
                            SkipReason = "Không có tài khoản đã đăng ký để test."
                        };
                    }
                    return new TestRequestPlan
                    {
                        JsonBody = new
                        {
                            phonenumber = acc.PhoneNumber,
                            password = ctx.Config.TestData.DefaultPassword,
                            uuid = ctx.Config.TestData.DefaultUuid
                        }
                    };
                }
            };

            yield return new TestCaseDefinition
            {
                Id = "signup_TC3",
                ApiName = "signup",
                DisplayName = "signup TC3 - SĐT sai định dạng",
                HttpMethod = "POST",
                PathTemplate = PathSignup,
                ExpectedAppCode = "1004",
                Description = "phonenumber=12345 (sai định dạng) → 1004.",
                PrepareAsync = (ctx, _) => Task.FromResult(new TestRequestPlan
                {
                    JsonBody = new
                    {
                        phonenumber = "12345",
                        password = ctx.Config.TestData.DefaultPassword,
                        uuid = ctx.Config.TestData.DefaultUuid
                    }
                })
            };

            yield return new TestCaseDefinition
            {
                Id = "signup_TC4",
                ApiName = "signup",
                DisplayName = "signup TC4 - Mật khẩu sai định dạng (< 6 ký tự)",
                HttpMethod = "POST",
                PathTemplate = PathSignup,
                ExpectedAppCode = "1004",
                Description = "password='abc12' (quá ngắn) → 1004.",
                PrepareAsync = async (ctx, ct) =>
                {
                    var acc = await ctx.SeedStore.LayMotTaiKhoanChuaDangKyAsync(ct);
                    var phone = acc?.PhoneNumber ?? SinhSoDienThoaiHopLe(ctx);
                    return new TestRequestPlan
                    {
                        JsonBody = new
                        {
                            phonenumber = phone,
                            password = "abc12",
                            uuid = ctx.Config.TestData.DefaultUuid
                        }
                    };
                }
            };

            yield return new TestCaseDefinition
            {
                Id = "signup_TC5",
                ApiName = "signup",
                DisplayName = "signup TC5 - Bỏ trống SĐT và mật khẩu",
                HttpMethod = "POST",
                PathTemplate = PathSignup,
                ExpectedAppCode = "1002",
                Description = "phonenumber='', password='' → 1002 Parameter is not enough.",
                PrepareAsync = (ctx, _) => Task.FromResult(new TestRequestPlan
                {
                    JsonBody = new
                    {
                        phonenumber = "",
                        password = "",
                        uuid = ctx.Config.TestData.DefaultUuid
                    }
                })
            };
        }

        // ====================================================================
        // 2) login
        // ====================================================================
        private static IEnumerable<TestCaseDefinition> LoginTestCases()
        {
            yield return new TestCaseDefinition
            {
                Id = "login_TC1",
                ApiName = "login",
                DisplayName = "login TC1 - Đăng nhập thành công",
                HttpMethod = "POST",
                PathTemplate = PathLogin,
                ExpectedAppCode = "1000",
                Description = "SĐT đã đăng ký + đúng mật khẩu → 1000 + token.",
                PrepareAsync = async (ctx, ct) =>
                {
                    var acc = await EnsureRegisteredAccountAsync(ctx, ct);
                    if (acc == null)
                    {
                        return new TestRequestPlan
                        {
                            SkipExecution = true,
                            SkipReason = "Không bootstrap được tài khoản đã đăng ký."
                        };
                    }
                    ctx.WorkingAccount = acc;
                    return new TestRequestPlan
                    {
                        JsonBody = new
                        {
                            phonenumber = acc.PhoneNumber,
                            password = acc.Password
                        }
                    };
                },
                PostCheckAsync = (ctx, resp, _) =>
                {
                    var token = LayChuoi(resp.Data, "token");
                    if (string.IsNullOrEmpty(token))
                    {
                        return Task.FromResult(ValidationOutcome.Fail("Response thiếu data.token."));
                    }
                    ctx.CurrentLoginToken = token;
                    return Task.FromResult(ValidationOutcome.Ok("Đã capture token cho các test sau."));
                }
            };

            yield return new TestCaseDefinition
            {
                Id = "login_TC2",
                ApiName = "login",
                DisplayName = "login TC2 - SĐT chưa đăng ký",
                HttpMethod = "POST",
                PathTemplate = PathLogin,
                ExpectedAppCode = "9995",
                Description = "SĐT chưa đăng ký + mật khẩu hợp lệ → 9995 User is not validated.",
                PrepareAsync = async (ctx, ct) =>
                {
                    var acc = await ctx.SeedStore.LayMotTaiKhoanChuaDangKyAsync(ct);
                    var phone = acc?.PhoneNumber ?? SinhSoDienThoaiHopLe(ctx);
                    return new TestRequestPlan
                    {
                        JsonBody = new
                        {
                            phonenumber = phone,
                            password = ctx.Config.TestData.DefaultPassword
                        }
                    };
                }
            };

            yield return new TestCaseDefinition
            {
                Id = "login_TC3",
                ApiName = "login",
                DisplayName = "login TC3 - SĐT sai định dạng",
                HttpMethod = "POST",
                PathTemplate = PathLogin,
                ExpectedAppCode = "1004",
                Description = "phonenumber=09999999999999 (quá dài) → 1004.",
                PrepareAsync = (ctx, _) => Task.FromResult(new TestRequestPlan
                {
                    JsonBody = new
                    {
                        phonenumber = "09999999999999",
                        password = ctx.Config.TestData.DefaultPassword
                    }
                })
            };

            yield return new TestCaseDefinition
            {
                Id = "login_TC4",
                ApiName = "login",
                DisplayName = "login TC4 - Bỏ trống thông tin bắt buộc",
                HttpMethod = "POST",
                PathTemplate = PathLogin,
                ExpectedAppCode = "1002",
                Description = "phonenumber='', password='' → 1002 Parameter is not enough.",
                PrepareAsync = (_, _) => Task.FromResult(new TestRequestPlan
                {
                    JsonBody = new { phonenumber = "", password = "" }
                })
            };
        }

        // ====================================================================
        // 3) logout
        // ====================================================================
        private static IEnumerable<TestCaseDefinition> LogoutTestCases()
        {
            yield return new TestCaseDefinition
            {
                Id = "logout_TC1",
                ApiName = "logout",
                DisplayName = "logout TC1 - Đăng xuất thành công",
                HttpMethod = "POST",
                PathTemplate = PathLogout,
                ExpectedAppCode = "1000",
                Description = "Token hợp lệ → 1000 OK; token sau đó bị vô hiệu hóa.",
                PrepareAsync = async (ctx, ct) =>
                {
                    var token = await EnsureLoginTokenAsync(ctx, ct);
                    if (string.IsNullOrEmpty(token))
                    {
                        return new TestRequestPlan
                        {
                            SkipExecution = true,
                            SkipReason = "Không lấy được token đăng nhập để logout."
                        };
                    }
                    return new TestRequestPlan
                    {
                        JsonBody = new { token = token }
                    };
                },
                PostCheckAsync = (ctx, _, _) =>
                {
                    ctx.CurrentLoginToken = null;
                    return Task.FromResult(ValidationOutcome.Ok("Đã clear token sau logout."));
                }
            };
        }

        // ====================================================================
        // 4) create_code_reset_password
        // ====================================================================
        private static IEnumerable<TestCaseDefinition> CreateCodeResetPasswordTestCases()
        {
            yield return new TestCaseDefinition
            {
                Id = "create_code_reset_password_TC1",
                ApiName = "create_code_reset_password",
                DisplayName = "create_code_reset_password TC1 - Gửi OTP thành công",
                HttpMethod = "POST",
                PathTemplate = PathCreateCodeResetPassword,
                ExpectedAppCode = "1000",
                Description = "SĐT đã đăng ký → 1000 + hệ thống tạo OTP gửi qua SMS.",
                PrepareAsync = async (ctx, ct) =>
                {
                    var acc = await EnsureRegisteredAccountAsync(ctx, ct);
                    if (acc == null)
                    {
                        return new TestRequestPlan
                        {
                            SkipExecution = true,
                            SkipReason = "Không có tài khoản đã đăng ký."
                        };
                    }
                    ctx.WorkingAccount = acc;
                    return new TestRequestPlan
                    {
                        JsonBody = new { phonenumber = acc.PhoneNumber }
                    };
                }
            };

            yield return new TestCaseDefinition
            {
                Id = "create_code_reset_password_TC2",
                ApiName = "create_code_reset_password",
                DisplayName = "create_code_reset_password TC2 - SĐT sai định dạng",
                HttpMethod = "POST",
                PathTemplate = PathCreateCodeResetPassword,
                ExpectedAppCode = "1004",
                Description = "phonenumber=12345 → 1004.",
                PrepareAsync = (_, _) => Task.FromResult(new TestRequestPlan
                {
                    JsonBody = new { phonenumber = "12345" }
                })
            };

            yield return new TestCaseDefinition
            {
                Id = "create_code_reset_password_TC3",
                ApiName = "create_code_reset_password",
                DisplayName = "create_code_reset_password TC3 - SĐT chưa đăng ký",
                HttpMethod = "POST",
                PathTemplate = PathCreateCodeResetPassword,
                ExpectedAppCode = "9995",
                Description = "SĐT hợp lệ nhưng chưa đăng ký → 9995.",
                PrepareAsync = async (ctx, ct) =>
                {
                    var acc = await ctx.SeedStore.LayMotTaiKhoanChuaDangKyAsync(ct);
                    var phone = acc?.PhoneNumber ?? SinhSoDienThoaiHopLe(ctx);
                    return new TestRequestPlan
                    {
                        JsonBody = new { phonenumber = phone }
                    };
                }
            };
        }

        // ====================================================================
        // 5) check_code_reset_password
        // ====================================================================
        private static IEnumerable<TestCaseDefinition> CheckCodeResetPasswordTestCases()
        {
            yield return new TestCaseDefinition
            {
                Id = "check_code_reset_password_TC1",
                ApiName = "check_code_reset_password",
                DisplayName = "check_code_reset_password TC1 - Xác minh OTP thành công",
                HttpMethod = "POST",
                PathTemplate = PathCheckCodeResetPassword,
                ExpectedAppCode = "1000",
                Description = "SĐT đã đăng ký + reset_code hợp lệ → 1000 + reset_token.",
                PrepareAsync = async (ctx, ct) =>
                {
                    var acc = await EnsureRegisteredAccountAsync(ctx, ct);
                    if (acc == null)
                    {
                        return new TestRequestPlan
                        {
                            SkipExecution = true,
                            SkipReason = "Không có tài khoản đã đăng ký."
                        };
                    }
                    ctx.WorkingAccount = acc;

                    var createResp = await GoiCreateCodeAsync(ctx, acc.PhoneNumber, ct);
                    var otp = await ctx.OtpProvider.LayOtpAsync(acc.PhoneNumber, createResp, ct);
                    if (string.IsNullOrEmpty(otp))
                    {
                        return new TestRequestPlan
                        {
                            SkipExecution = true,
                            SkipReason = "Không cấu hình được nguồn OTP (TestData.OtpStrategy)."
                        };
                    }
                    return new TestRequestPlan
                    {
                        JsonBody = new { phonenumber = acc.PhoneNumber, reset_code = otp }
                    };
                },
                PostCheckAsync = (ctx, resp, _) =>
                {
                    var rt = LayChuoi(resp.Data, "reset_token");
                    if (string.IsNullOrEmpty(rt))
                    {
                        return Task.FromResult(ValidationOutcome.Fail("Response thiếu data.reset_token."));
                    }
                    ctx.CurrentResetToken = rt;
                    return Task.FromResult(ValidationOutcome.Ok("Đã capture reset_token cho test reset_password."));
                }
            };

            yield return new TestCaseDefinition
            {
                Id = "check_code_reset_password_TC2",
                ApiName = "check_code_reset_password",
                DisplayName = "check_code_reset_password TC2 - Mã xác thực không đúng",
                HttpMethod = "POST",
                PathTemplate = PathCheckCodeResetPassword,
                ExpectedAppCode = "9993",
                Description = "Đúng SĐT + reset_code sai → 9993 Code verify is incorrect.",
                PrepareAsync = async (ctx, ct) =>
                {
                    var acc = await EnsureRegisteredAccountAsync(ctx, ct);
                    if (acc == null)
                    {
                        return new TestRequestPlan
                        {
                            SkipExecution = true,
                            SkipReason = "Không có tài khoản đã đăng ký."
                        };
                    }
                    await GoiCreateCodeAsync(ctx, acc.PhoneNumber, ct);
                    return new TestRequestPlan
                    {
                        JsonBody = new { phonenumber = acc.PhoneNumber, reset_code = "000000" }
                    };
                }
            };

            yield return new TestCaseDefinition
            {
                Id = "check_code_reset_password_TC3",
                ApiName = "check_code_reset_password",
                DisplayName = "check_code_reset_password TC3 - Mã đã sử dụng trước đó",
                HttpMethod = "POST",
                PathTemplate = PathCheckCodeResetPassword,
                ExpectedAppCode = "9993",
                Description = "Verify thành công 1 lần rồi gọi lại với cùng code → 9993.",
                PrepareAsync = async (ctx, ct) =>
                {
                    var acc = await EnsureRegisteredAccountAsync(ctx, ct);
                    if (acc == null)
                    {
                        return new TestRequestPlan
                        {
                            SkipExecution = true,
                            SkipReason = "Không có tài khoản đã đăng ký."
                        };
                    }
                    var createResp = await GoiCreateCodeAsync(ctx, acc.PhoneNumber, ct);
                    var otp = await ctx.OtpProvider.LayOtpAsync(acc.PhoneNumber, createResp, ct);
                    if (string.IsNullOrEmpty(otp))
                    {
                        return new TestRequestPlan
                        {
                            SkipExecution = true,
                            SkipReason = "Không cấu hình được nguồn OTP (TestData.OtpStrategy)."
                        };
                    }
                    // verify một lần để otp bị vô hiệu hóa
                    await ctx.Runner.ThucThiAsync("POST", ctx.BaseUrl, PathCheckCodeResetPassword,
                        new TestRequestPlan { JsonBody = new { phonenumber = acc.PhoneNumber, reset_code = otp } }, ct);
                    return new TestRequestPlan
                    {
                        JsonBody = new { phonenumber = acc.PhoneNumber, reset_code = otp }
                    };
                }
            };

            yield return new TestCaseDefinition
            {
                Id = "check_code_reset_password_TC4",
                ApiName = "check_code_reset_password",
                DisplayName = "check_code_reset_password TC4 - Mã đã hết hạn",
                HttpMethod = "POST",
                PathTemplate = PathCheckCodeResetPassword,
                ExpectedAppCode = "9993",
                Description = "Mã hết TTL → 9993. (Test này cần TTL ngắn ở môi trường test.)",
                PrepareAsync = async (ctx, ct) =>
                {
                    var acc = await EnsureRegisteredAccountAsync(ctx, ct);
                    if (acc == null)
                    {
                        return new TestRequestPlan
                        {
                            SkipExecution = true,
                            SkipReason = "Không có tài khoản đã đăng ký."
                        };
                    }
                    // Dùng OTP chưa từng được tạo để mô phỏng hết hạn (server thường gộp 2 case lại làm 9993).
                    return new TestRequestPlan
                    {
                        JsonBody = new { phonenumber = acc.PhoneNumber, reset_code = "999999" }
                    };
                }
            };
        }

        // ====================================================================
        // 6) reset_password
        // ====================================================================
        private static IEnumerable<TestCaseDefinition> ResetPasswordTestCases()
        {
            yield return new TestCaseDefinition
            {
                Id = "reset_password_TC1",
                ApiName = "reset_password",
                DisplayName = "reset_password TC1 - Đặt lại mật khẩu thành công",
                HttpMethod = "POST",
                PathTemplate = PathResetPassword,
                ExpectedAppCode = "1000",
                Description = "Có reset_token hợp lệ + password mới hợp lệ → 1000.",
                PrepareAsync = async (ctx, ct) =>
                {
                    var (acc, resetToken, skipReason) = await EnsureValidResetTokenAsync(ctx, ct);
                    if (acc == null || resetToken == null)
                    {
                        return new TestRequestPlan
                        {
                            SkipExecution = true,
                            SkipReason = skipReason ?? "Không lấy được reset_token."
                        };
                    }
                    ctx.WorkingAccount = acc;
                    var matKhauMoi = SinhMatKhauMoi();
                    ctx.Variables["last_new_password"] = matKhauMoi;
                    return new TestRequestPlan
                    {
                        JsonBody = new
                        {
                            phonenumber = acc.PhoneNumber,
                            password = matKhauMoi,
                            reset_token = resetToken
                        }
                    };
                },
                PostCheckAsync = async (ctx, _, ct) =>
                {
                    if (ctx.WorkingAccount != null && ctx.Variables.TryGetValue("last_new_password", out var pw))
                    {
                        await ctx.SeedStore.CapNhatMatKhauAsync(ctx.WorkingAccount, pw, ct);
                    }
                    return ValidationOutcome.Ok("Đã sync mật khẩu mới vào bảng mồi.");
                }
            };

            yield return new TestCaseDefinition
            {
                Id = "reset_password_TC2",
                ApiName = "reset_password",
                DisplayName = "reset_password TC2 - Mật khẩu mới không hợp lệ",
                HttpMethod = "POST",
                PathTemplate = PathResetPassword,
                ExpectedAppCode = "1004",
                Description = "Có reset_token hợp lệ nhưng password mới sai chính sách → 1004.",
                PrepareAsync = async (ctx, ct) =>
                {
                    var (acc, resetToken, skipReason) = await EnsureValidResetTokenAsync(ctx, ct);
                    if (acc == null || resetToken == null)
                    {
                        return new TestRequestPlan
                        {
                            SkipExecution = true,
                            SkipReason = skipReason ?? "Không lấy được reset_token."
                        };
                    }
                    return new TestRequestPlan
                    {
                        JsonBody = new
                        {
                            phonenumber = acc.PhoneNumber,
                            password = "ab",
                            reset_token = resetToken
                        }
                    };
                }
            };

            yield return new TestCaseDefinition
            {
                Id = "reset_password_TC3",
                ApiName = "reset_password",
                DisplayName = "reset_password TC3 - Thiếu mật khẩu mới",
                HttpMethod = "POST",
                PathTemplate = PathResetPassword,
                ExpectedAppCode = "1002",
                Description = "Có reset_token hợp lệ nhưng password để trống → 1002.",
                PrepareAsync = async (ctx, ct) =>
                {
                    var (acc, resetToken, skipReason) = await EnsureValidResetTokenAsync(ctx, ct);
                    if (acc == null || resetToken == null)
                    {
                        return new TestRequestPlan
                        {
                            SkipExecution = true,
                            SkipReason = skipReason ?? "Không lấy được reset_token."
                        };
                    }
                    return new TestRequestPlan
                    {
                        JsonBody = new
                        {
                            phonenumber = acc.PhoneNumber,
                            password = "",
                            reset_token = resetToken
                        }
                    };
                }
            };

            yield return new TestCaseDefinition
            {
                Id = "reset_password_TC4",
                ApiName = "reset_password",
                DisplayName = "reset_password TC4 - Thiếu reset_token",
                HttpMethod = "POST",
                PathTemplate = PathResetPassword,
                ExpectedAppCode = "1002",
                Description = "reset_token để trống → 1002.",
                PrepareAsync = async (ctx, ct) =>
                {
                    var acc = await EnsureRegisteredAccountAsync(ctx, ct);
                    if (acc == null)
                    {
                        return new TestRequestPlan
                        {
                            SkipExecution = true,
                            SkipReason = "Không có tài khoản đã đăng ký."
                        };
                    }
                    return new TestRequestPlan
                    {
                        JsonBody = new
                        {
                            phonenumber = acc.PhoneNumber,
                            password = SinhMatKhauMoi(),
                            reset_token = ""
                        }
                    };
                }
            };

            yield return new TestCaseDefinition
            {
                Id = "reset_password_TC5",
                ApiName = "reset_password",
                DisplayName = "reset_password TC5 - reset_token không hợp lệ",
                HttpMethod = "POST",
                PathTemplate = PathResetPassword,
                ExpectedAppCode = "9998",
                Description = "reset_token sai/hết hạn → 9998 Token is invalid.",
                PrepareAsync = async (ctx, ct) =>
                {
                    var acc = await EnsureRegisteredAccountAsync(ctx, ct);
                    if (acc == null)
                    {
                        return new TestRequestPlan
                        {
                            SkipExecution = true,
                            SkipReason = "Không có tài khoản đã đăng ký."
                        };
                    }
                    return new TestRequestPlan
                    {
                        JsonBody = new
                        {
                            phonenumber = acc.PhoneNumber,
                            password = SinhMatKhauMoi(),
                            reset_token = "invalid_reset_token_xxx"
                        }
                    };
                }
            };
        }

        // ====================================================================
        // 7) change_password
        // ====================================================================
        private static IEnumerable<TestCaseDefinition> ChangePasswordTestCases()
        {
            yield return new TestCaseDefinition
            {
                Id = "change_password_TC1",
                ApiName = "change_password",
                DisplayName = "change_password TC1 - Đổi mật khẩu thành công",
                HttpMethod = "POST",
                PathTemplate = PathChangePassword,
                ExpectedAppCode = "1000",
                Description = "Token + đúng password hiện tại + new_password hợp lệ → 1000.",
                PrepareAsync = async (ctx, ct) =>
                {
                    var (acc, token, skip) = await EnsureLoginTokenWithAccountAsync(ctx, ct);
                    if (acc == null || token == null)
                    {
                        return new TestRequestPlan { SkipExecution = true, SkipReason = skip };
                    }
                    ctx.WorkingAccount = acc;
                    var matKhauMoi = SinhMatKhauMoi();
                    ctx.Variables["last_new_password"] = matKhauMoi;
                    return new TestRequestPlan
                    {
                        JsonBody = new
                        {
                            token = token,
                            password = acc.Password,
                            new_password = matKhauMoi
                        }
                    };
                },
                PostCheckAsync = async (ctx, _, ct) =>
                {
                    if (ctx.WorkingAccount != null && ctx.Variables.TryGetValue("last_new_password", out var pw))
                    {
                        await ctx.SeedStore.CapNhatMatKhauAsync(ctx.WorkingAccount, pw, ct);
                    }
                    return ValidationOutcome.Ok("Đã sync mật khẩu mới vào bảng mồi.");
                }
            };

            yield return new TestCaseDefinition
            {
                Id = "change_password_TC2",
                ApiName = "change_password",
                DisplayName = "change_password TC2 - Mật khẩu hiện tại không chính xác",
                HttpMethod = "POST",
                PathTemplate = PathChangePassword,
                ExpectedAppCode = "1004",
                Description = "Token hợp lệ nhưng password hiện tại sai → 1004.",
                PrepareAsync = async (ctx, ct) =>
                {
                    var (_, token, skip) = await EnsureLoginTokenWithAccountAsync(ctx, ct);
                    if (token == null)
                    {
                        return new TestRequestPlan { SkipExecution = true, SkipReason = skip };
                    }
                    return new TestRequestPlan
                    {
                        JsonBody = new
                        {
                            token = token,
                            password = "wrong_password_xxx",
                            new_password = SinhMatKhauMoi()
                        }
                    };
                }
            };

            yield return new TestCaseDefinition
            {
                Id = "change_password_TC3",
                ApiName = "change_password",
                DisplayName = "change_password TC3 - Mật khẩu mới không hợp lệ",
                HttpMethod = "POST",
                PathTemplate = PathChangePassword,
                ExpectedAppCode = "1004",
                Description = "Token + đúng password + new_password='ab' (quá ngắn) → 1004.",
                PrepareAsync = async (ctx, ct) =>
                {
                    var (acc, token, skip) = await EnsureLoginTokenWithAccountAsync(ctx, ct);
                    if (acc == null || token == null)
                    {
                        return new TestRequestPlan { SkipExecution = true, SkipReason = skip };
                    }
                    return new TestRequestPlan
                    {
                        JsonBody = new
                        {
                            token = token,
                            password = acc.Password,
                            new_password = "ab"
                        }
                    };
                }
            };

            yield return new TestCaseDefinition
            {
                Id = "change_password_TC4",
                ApiName = "change_password",
                DisplayName = "change_password TC4 - Thiếu password hoặc new_password",
                HttpMethod = "POST",
                PathTemplate = PathChangePassword,
                ExpectedAppCode = "1002",
                Description = "Token hợp lệ + password='' + new_password='' → 1002.",
                PrepareAsync = async (ctx, ct) =>
                {
                    var (_, token, skip) = await EnsureLoginTokenWithAccountAsync(ctx, ct);
                    if (token == null)
                    {
                        return new TestRequestPlan { SkipExecution = true, SkipReason = skip };
                    }
                    return new TestRequestPlan
                    {
                        JsonBody = new
                        {
                            token = token,
                            password = "",
                            new_password = ""
                        }
                    };
                }
            };

            yield return new TestCaseDefinition
            {
                Id = "change_password_TC5",
                ApiName = "change_password",
                DisplayName = "change_password TC5 - Token không hợp lệ",
                HttpMethod = "POST",
                PathTemplate = PathChangePassword,
                ExpectedAppCode = "9998",
                Description = "Token sai/hết hạn → 9998 Token is invalid.",
                PrepareAsync = async (ctx, ct) =>
                {
                    var acc = await EnsureRegisteredAccountAsync(ctx, ct);
                    return new TestRequestPlan
                    {
                        JsonBody = new
                        {
                            token = "invalid_token_xxx",
                            password = acc?.Password ?? ctx.Config.TestData.DefaultPassword,
                            new_password = SinhMatKhauMoi()
                        }
                    };
                }
            };
        }

        // ====================================================================
        // Helper - bootstrap dữ liệu phụ thuộc giữa các API
        // ====================================================================
        private static async Task<SeedAccount?> EnsureRegisteredAccountAsync(TestContext ctx, CancellationToken ct)
        {
            var acc = await ctx.SeedStore.LayMotTaiKhoanDaDangKyAsync(ct);
            if (acc != null) return acc;

            // Bootstrap: signup một tài khoản unregistered
            var unreg = await ctx.SeedStore.LayMotTaiKhoanChuaDangKyAsync(ct);
            if (unreg == null) return null;

            var resp = await ctx.Runner.ThucThiAsync("POST", ctx.BaseUrl, PathSignup, new TestRequestPlan
            {
                JsonBody = new
                {
                    phonenumber = unreg.PhoneNumber,
                    password = unreg.Password,
                    uuid = ctx.Config.TestData.DefaultUuid
                }
            }, ct);

            if (resp.AppCode == "1000")
            {
                var serverId = LayChuoi(resp.Data, "id");
                await ctx.SeedStore.DanhDauDaDangKyAsync(unreg, serverId, ct);
                unreg.Status = SeedAccountStatus.DaDangKy;
                return unreg;
            }
            return null;
        }

        private static async Task<string?> EnsureLoginTokenAsync(TestContext ctx, CancellationToken ct)
        {
            if (!string.IsNullOrEmpty(ctx.CurrentLoginToken)) return ctx.CurrentLoginToken;

            var (_, token, _) = await EnsureLoginTokenWithAccountAsync(ctx, ct);
            return token;
        }

        private static async Task<(SeedAccount? Account, string? Token, string? SkipReason)>
            EnsureLoginTokenWithAccountAsync(TestContext ctx, CancellationToken ct)
        {
            var acc = await EnsureRegisteredAccountAsync(ctx, ct);
            if (acc == null) return (null, null, "Không có tài khoản đã đăng ký để login.");

            var resp = await ctx.Runner.ThucThiAsync("POST", ctx.BaseUrl, PathLogin, new TestRequestPlan
            {
                JsonBody = new { phonenumber = acc.PhoneNumber, password = acc.Password }
            }, ct);

            if (resp.AppCode != "1000")
            {
                return (acc, null, $"login để bootstrap thất bại, code={resp.AppCode}.");
            }
            var token = LayChuoi(resp.Data, "token");
            if (string.IsNullOrEmpty(token))
            {
                return (acc, null, "Response login bootstrap thiếu data.token.");
            }
            ctx.CurrentLoginToken = token;
            return (acc, token, null);
        }

        private static async Task<(SeedAccount? Account, string? ResetToken, string? SkipReason)>
            EnsureValidResetTokenAsync(TestContext ctx, CancellationToken ct)
        {
            var acc = await EnsureRegisteredAccountAsync(ctx, ct);
            if (acc == null) return (null, null, "Không có tài khoản đã đăng ký.");

            var createResp = await GoiCreateCodeAsync(ctx, acc.PhoneNumber, ct);
            if (createResp.AppCode != "1000")
            {
                return (acc, null, $"create_code_reset_password thất bại, code={createResp.AppCode}.");
            }

            var otp = await ctx.OtpProvider.LayOtpAsync(acc.PhoneNumber, createResp, ct);
            if (string.IsNullOrEmpty(otp))
            {
                return (acc, null, "Không lấy được OTP (TestData.OtpStrategy).");
            }

            var checkResp = await ctx.Runner.ThucThiAsync("POST", ctx.BaseUrl, PathCheckCodeResetPassword,
                new TestRequestPlan { JsonBody = new { phonenumber = acc.PhoneNumber, reset_code = otp } }, ct);
            if (checkResp.AppCode != "1000")
            {
                return (acc, null, $"check_code_reset_password thất bại, code={checkResp.AppCode}.");
            }
            var rt = LayChuoi(checkResp.Data, "reset_token");
            if (string.IsNullOrEmpty(rt))
            {
                return (acc, null, "Response check_code thiếu data.reset_token.");
            }
            ctx.CurrentResetToken = rt;
            return (acc, rt, null);
        }

        private static Task<ApiResponse> GoiCreateCodeAsync(TestContext ctx, string phone, CancellationToken ct)
        {
            return ctx.Runner.ThucThiAsync("POST", ctx.BaseUrl, PathCreateCodeResetPassword,
                new TestRequestPlan { JsonBody = new { phonenumber = phone } }, ct);
        }

        // ----------------------------------------------------------------
        private static string SinhSoDienThoaiHopLe(TestContext ctx)
        {
            var prefix = string.IsNullOrWhiteSpace(ctx.Config.TestData.PhoneNumberPrefix)
                ? "099"
                : ctx.Config.TestData.PhoneNumberPrefix;
            var rnd = new Random().Next(1000000, 9999999);
            var so = rnd.ToString().PadLeft(10 - prefix.Length, '0');
            return prefix + so;
        }

        private static string SinhMatKhauMoi()
        {
            return "Pwd" + DateTime.UtcNow.Ticks.ToString().Substring(0, 6);
        }

        private static string? LayChuoi(JsonElement? element, string property)
        {
            if (element == null) return null;
            var el = element.Value;
            if (el.ValueKind != JsonValueKind.Object) return null;
            if (!el.TryGetProperty(property, out var prop)) return null;
            return prop.ValueKind switch
            {
                JsonValueKind.String => prop.GetString(),
                JsonValueKind.Number => prop.GetRawText(),
                JsonValueKind.Null => null,
                _ => prop.GetRawText()
            };
        }
    }
}
