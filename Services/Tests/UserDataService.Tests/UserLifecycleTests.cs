using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using UserDataService.DTOs;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using UserAuthService.DTOs;
using Xunit.Abstractions;

namespace UserDataService.Tests
{
    public class UserLifecycleTests
    {
        private readonly ITestOutputHelper _output;
        private readonly HttpClient _authClient;
        private readonly HttpClient _dataClient;

        public UserLifecycleTests(ITestOutputHelper output)
        {
            _output = output;

            // 启动 UserAuthService
            var authFactory = new WebApplicationFactory<UserAuthService.Program>();
            _authClient = authFactory.CreateClient();

            // 启动 UserDataService
            var dataFactory = new WebApplicationFactory<UserDataService.Program>();
            _dataClient = dataFactory.CreateClient();
        }

        private readonly RegisterRequest _registerRequest = new RegisterRequest
        {
            Username = "delete_test_user",
            Password = "TestPass123!",
            Email = "delete_test_user@example.com",
            Phone = "13800001111"
        };

        [Fact(DisplayName = "User lifecycle: 注册 → 登录 → 获取ID → 获取信息 → 更新信息 → 注销账户 → 登录失败")]
        public async Task UserLifecycle_Flow_Should_Work_As_Expected()
        {
            // 1. 注册用户
            var registerResponse = await _authClient.PostAsJsonAsync("/api/auth/register", _registerRequest);
            registerResponse.EnsureSuccessStatusCode();
            _output.WriteLine("✅ 注册成功");

            // 2. 登录获取 Token
            var loginRequest = new LoginRequest
            {
                Identifier = _registerRequest.Username,
                Password = _registerRequest.Password
            };

            var loginResponse = await _authClient.PostAsJsonAsync("/api/auth/login", loginRequest);
            loginResponse.EnsureSuccessStatusCode();
            var loginJson = JObject.Parse(await loginResponse.Content.ReadAsStringAsync());
            var token = loginJson["data"]?.ToString();
            token.Should().NotBeNullOrEmpty();
            _output.WriteLine($"✅ 登录成功，Token: {token}");

            // 3. 获取用户ID
            _authClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var meResponse = await _authClient.GetAsync("/api/auth/me");
            meResponse.EnsureSuccessStatusCode();
            var meJson = JObject.Parse(await meResponse.Content.ReadAsStringAsync());
            var userId = meJson["data"]?["id"]?.ToString();
            var username = meJson["data"]?["username"]?.ToString();
            userId.Should().NotBeNullOrEmpty();
            _output.WriteLine($"✅ 当前用户ID: {userId}, 用户名: {username}");

            // 4. 获取用户信息（UserDataService）
            
            var updateUserInfoRequest = new UpdateUserInfoRequest {

                Username = "123456",

                Bio = "123456",

                Gender = "男",

                Birthday = new DateTime(2025, 7, 20),

                Location = "123456",

                Experience = 1,

                Level = 2,
            };
        
            _dataClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var updateInfoResponse = await _dataClient.PutAsJsonAsync($"/api/users/{userId}/profile", updateUserInfoRequest);
            var updateInfoResult = await updateInfoResponse.Content.ReadAsStringAsync();

            if (!updateInfoResponse.IsSuccessStatusCode)
            {
                _output.WriteLine($"❌ 更新用户资料失败: {(int)updateInfoResponse.StatusCode} {updateInfoResponse.ReasonPhrase}");
                _output.WriteLine($"返回内容: {updateInfoResult}");
            }
            else
            {
                _output.WriteLine($"✅ 用户更新成功: {updateInfoResult}");
            }

            
            
            // 5. 修改用户信息
            UpdateUserRequest updateUserRequest = new UpdateUserRequest
            {
                Username = "updated_username",
                Password = "NewPass123!",
                Email = "newemail@example.com",
                Phone = "19999999999"
            };
            _dataClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var updateResponse = await _dataClient.PutAsJsonAsync($"/api/users/{userId}/user", updateUserRequest);
            updateResponse.EnsureSuccessStatusCode();
            var updateResult = await updateResponse.Content.ReadAsStringAsync();
            _output.WriteLine($"✅ 用户更新成功: {updateResult}");
            
            // 6. 获取用户信息
            _dataClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var getResponse = await _dataClient.GetAsync($"/api/users/{userId}");
            getResponse.EnsureSuccessStatusCode();
            var getUserJson = JObject.Parse(await getResponse.Content.ReadAsStringAsync());
            _output.WriteLine($"✅ 查询用户成功: {getUserJson["data"]}");


            // 7. 注销用户（UserDataService）
            _dataClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var deleteResponse = await _dataClient.DeleteAsync($"/api/users/{userId}/delete");
            deleteResponse.EnsureSuccessStatusCode();
            _output.WriteLine($"✅ 注销响应: {await deleteResponse.Content.ReadAsStringAsync()}");

            // 8. 再次尝试登录
            var retryLogin = await _authClient.PostAsJsonAsync("/api/auth/login", loginRequest);
            retryLogin.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
            _output.WriteLine("✅ 注销后再次登录失败");
        }
    }

}
