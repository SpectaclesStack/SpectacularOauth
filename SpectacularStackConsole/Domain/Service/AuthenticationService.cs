using Newtonsoft.Json;
using SpectacularStackAuth.Domain.Model;
using System;
using System.Net.Http;
using System.Text;

namespace SpectacularStackAuth.Domain.Service
{
    public class AuthenticationService
    {
        public static readonly string BASE_URL = "";
        public static readonly string CLIENT_ID = "b03ee56bb0f38ebef184";
        public static string ACCESS_TOKEN = "";
        private static bool authenticated = false;
        private static bool exiting = false;

        public async Task AuthenticateAsync()
        {
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    var postContent = new StringContent("", Encoding.UTF8, "application/json");

                    var post = new HttpRequestMessage(HttpMethod.Post, $"https://github.com/login/device/code?client_id={CLIENT_ID}&scope=read:user")
                    {
                        Content = postContent
                    };
                    post.Headers.Add("Accept", "application/json");

                    var response = await httpClient.SendAsync(post);
                    response.EnsureSuccessStatusCode();

                    string responseBody = await response.Content.ReadAsStringAsync();



                    DeviceVerification deviceVerification = JsonConvert.DeserializeObject<DeviceVerification>(responseBody);

                    bool success = false;
                    while (!success)
                    {
                        await Task.Delay(5000);
                        var postRequest = new HttpRequestMessage(HttpMethod.Post, $"https://github.com/login/oauth/access_token?client_id={CLIENT_ID}&device_code={deviceVerification.device_code}&grant_type=urn:ietf:params:oauth:grant-type:device_code")
                        {
                            Content = postContent
                        };
                        postRequest.Headers.Add("Accept", "application/json");

                        response = await httpClient.SendAsync(postRequest);
                        response.EnsureSuccessStatusCode();

                        responseBody = await response.Content.ReadAsStringAsync();

                        Token accessToken = JsonConvert.DeserializeObject<Token>(responseBody);

                        if (!string.IsNullOrEmpty(accessToken.AccessToken))
                        {
                            success = true;
                            ACCESS_TOKEN = accessToken.AccessToken;
                            authenticated = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Authentication failed.", ex);
            }
        }
    }
}
