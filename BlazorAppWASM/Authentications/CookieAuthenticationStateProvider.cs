using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace BlazorAppWASM.Authentications
{
    public class CookieAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient httpClient;

        public CookieAuthenticationStateProvider(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }
        public async override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity());

            try
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, "manage/info");
                requestMessage.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
                var userResponse = await httpClient.SendAsync(requestMessage);
                if (userResponse.IsSuccessStatusCode)
                {
                    var userJson = await userResponse.Content.ReadAsStringAsync();
                    var jsonSOptions = new JsonSerializerOptions { AllowTrailingCommas = true, PropertyNameCaseInsensitive = true };
                    var userInfo = JsonSerializer.Deserialize<UserInfo>(userJson, jsonSOptions);
                    if (userInfo != null)
                    {
                        var claims = new List<Claim>
                    {
                        new(ClaimTypes.Name, userInfo.Email!),
                        new(ClaimTypes.Email, userInfo.Email!)
                    };
                        var id = new ClaimsIdentity(claims, nameof(CookieAuthenticationStateProvider));
                        user = new ClaimsPrincipal(id);
                    }
                }
                return new AuthenticationState(user);
            }
            catch { return new AuthenticationState(user); }
        }

        public async Task LoginAndGetAuthenticationState(LoginModel model)
        {
            //model.Email = "fred@gmail.com"; model.Password = "Fred@123";
            var jsonContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "login?useCookies=true")
            {
                Content = jsonContent
            };
            requestMessage.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            var userResponse = await httpClient.SendAsync(requestMessage);
            if (userResponse.IsSuccessStatusCode)
                NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            else
                return;
        }
    }
}
