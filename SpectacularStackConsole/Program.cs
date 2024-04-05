using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using SpectacularStackAuth.Domain.Model;

Console.WriteLine("Hello, World!");

string CLIENT_ID = "b03ee56bb0f38ebef184";
string ACCESS_TOKEN = "";

try
{
    using (HttpClient httpClient = new HttpClient())
    {
      
        var deviceCodeRequest = new HttpRequestMessage(HttpMethod.Post, $"https://github.com/login/device/code?client_id={CLIENT_ID}&scope=read:user");

        var deviceCodeResponse = await httpClient.SendAsync(deviceCodeRequest);
        deviceCodeResponse.EnsureSuccessStatusCode();

        string deviceCodeResponseBody = await deviceCodeResponse.Content.ReadAsStringAsync();

        string[] keyValuePairs = deviceCodeResponseBody.Split('&');

        Dictionary<string, string> responseDict = new Dictionary<string, string>();

   
        foreach (string pair in keyValuePairs)
        {
            string[] parts = pair.Split('=');
            if (parts.Length == 2)
            {
                string key = parts[0];
                string value = HttpUtility.UrlDecode(parts[1]);
                responseDict[key] = value;
            }
        }

        var deviceVerification = new DeviceVerification();
        if (responseDict.ContainsKey("device_code"))
        {
            deviceVerification.device_code = responseDict["device_code"];
        }
        if (responseDict.ContainsKey("expires_in") && int.TryParse(responseDict["expires_in"], out int expiresIn))
        {
            deviceVerification.expires_in = expiresIn;
        }
        if (responseDict.ContainsKey("interval") && int.TryParse(responseDict["interval"], out int interval))
        {
            deviceVerification.interval = interval;
        }
        if (responseDict.ContainsKey("user_code"))
        {
            deviceVerification.user_code = responseDict["user_code"];
        }
        if (responseDict.ContainsKey("verification_uri"))
        {
            deviceVerification.verification_uri = responseDict["verification_uri"];
        }

        Console.WriteLine($"Please go to {deviceVerification.verification_uri} and enter the code: {deviceVerification.user_code}");

        bool success = false;

        while (!success)
        {
            await Task.Delay(5000); 

            var accessTokenRequest = new HttpRequestMessage(HttpMethod.Post, $"https://github.com/login/oauth/access_token?client_id={CLIENT_ID}&device_code={deviceVerification.device_code}&grant_type=urn:ietf:params:oauth:grant-type:device_code");

            var accessTokenResponse = await httpClient.SendAsync(accessTokenRequest);
            accessTokenResponse.EnsureSuccessStatusCode();

            string accessTokenResponseBody = await accessTokenResponse.Content.ReadAsStringAsync();

            if (!accessTokenResponseBody.StartsWith("error"))
            {

                string responseString = "access_token=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&scope=read%3Auser&token_type=bearer";

                string[] keyValPairs = responseString.Split('&');

                Dictionary<string, string> accessTokenDict = new Dictionary<string, string>();

                
                foreach (string pair in keyValPairs)
                {
                    string[] parts = pair.Split('=');
                    if (parts.Length == 2)
                    {
                        string key = parts[0];
                        string value = Uri.UnescapeDataString(parts[1]); 
                        accessTokenDict[key] = value;
                    }
                }

                if (accessTokenDict.ContainsKey("access_token"))
                {
                    string accessToken = accessTokenDict["access_token"];
                    Console.WriteLine($"Access Token: {accessToken}");
                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        success = true;
                        ACCESS_TOKEN = accessToken;
                    }
                }

                if (accessTokenDict.ContainsKey("scope"))
                {
                    string scope = accessTokenDict["scope"];
                    Console.WriteLine($"Scope: {scope}");
                }

                if (accessTokenDict.ContainsKey("token_type"))
                {
                    string tokenType = accessTokenDict["token_type"];
                    Console.WriteLine($"Token Type: {tokenType}");
                }

            }

        }

        Console.WriteLine("Authentication successful!");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Authentication failed: {ex.Message}");
}
