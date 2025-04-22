using UnityEngine;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Minimoo.Attributes;
using Minimoo.Extensions;
using Cysharp.Threading.Tasks;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace Minimoo.LocalizationDatas
{
    [CreateAssetMenu(fileName = "Google Sheet CSV Data", menuName = "Minimoo/Localization/Google Sheet CSV Data")]
    public class GoogleSheetCSVData : CSVData
    {
        [SerializeField] private string _sheetUrl;
        [SerializeField] private TextAsset _serviceAccountKey;
        private const string SHEETS_API_URL = "https://sheets.googleapis.com/v4/spreadsheets/{0}/values/{1}?key={2}";

        [Button("Download Sheet")]
        public async void DownloadSheet()
        {
            if (string.IsNullOrEmpty(_sheetUrl))
            {
                D.Error("Google Sheet URL이 지정되지 않았습니다.");
                return;
            }

            if (_serviceAccountKey == null)
            {
                D.Error("서비스 계정 키 파일이 지정되지 않았습니다.");
                return;
            }

            try
            {
                var success = await DownloadAndParseSheet();
                if (success)
                {
                    D.Log($"Google Sheet 데이터 파싱이 완료되었습니다. (총 {_rows.Count}행)");
                }
            }
            catch (Exception e)
            {
                D.Error($"시트 다운로드 중 오류 발생: {e.Message}");
            }
        }

        public void SetSheetUrl(string sheetUrl)
        {
            _sheetUrl = sheetUrl;
        }

        public void SetServiceAccountKey(TextAsset serviceAccountKey)
        {
            _serviceAccountKey = serviceAccountKey;
        }

        private async UniTask<bool> DownloadAndParseSheet()
        {
            try
            {
                var spreadsheetId = GetSpreadsheetId(_sheetUrl);
                var serviceAccount = JsonSerializer.Deserialize<ServiceAccountInfo>(_serviceAccountKey.text);
                var token = await GetAccessToken(serviceAccount);

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    
                    var range = "A1:ZZ"; // 전체 범위를 가져옵니다
                    var url = string.Format(SHEETS_API_URL, spreadsheetId, range, serviceAccount.ApiKey);
                    
                    var response = await client.GetAsync(url);
                    if (!response.IsSuccessStatusCode)
                    {
                        D.Error($"시트 다운로드 실패: {response.StatusCode}");
                        return false;
                    }

                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var sheetData = JsonSerializer.Deserialize<SheetResponse>(jsonResponse);
                    
                    if (sheetData?.Values == null || sheetData.Values.Count == 0)
                    {
                        D.Error("시트에서 데이터를 찾을 수 없습니다.");
                        return false;
                    }

                    // CSV 형식으로 변환
                    var csvBuilder = new StringBuilder();
                    foreach (var row in sheetData.Values)
                    {
                        csvBuilder.AppendLine(string.Join(",", row.Select(cell => $"\"{cell}\"")));
                    }

                    ParseCSV(csvBuilder.ToString());

#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(this);
                    UnityEditor.AssetDatabase.SaveAssets();
#endif

                    return true;
                }
            }
            catch (Exception e)
            {
                D.Error($"시트 데이터 다운로드 실패: {e.Message}");
                return false;
            }
        }

        private async UniTask<string> GetAccessToken(ServiceAccountInfo serviceAccount)
        {
            var now = DateTime.UtcNow;
            var claims = new[]
            {
                new Claim("iss", serviceAccount.ClientEmail),
                new Claim("scope", "https://www.googleapis.com/auth/spreadsheets.readonly"),
                new Claim("aud", "https://oauth2.googleapis.com/token"),
                new Claim("exp", new DateTimeOffset(now.AddHours(1)).ToUnixTimeSeconds().ToString()),
                new Claim("iat", new DateTimeOffset(now).ToUnixTimeSeconds().ToString())
            };

            var privateKey = serviceAccount.PrivateKey.Replace("\\n", "\n");
            var key = new RSACryptoServiceProvider();
            key.ImportFromPem(privateKey);

            var token = new JwtSecurityToken(
                claims: claims,
                signingCredentials: new SigningCredentials(
                    new RsaSecurityKey(key),
                    SecurityAlgorithms.RsaSha256
                )
            );

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.WriteToken(token);

            using (var client = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer"),
                    new KeyValuePair<string, string>("assertion", jwt)
                });

                var response = await client.PostAsync("https://oauth2.googleapis.com/token", content);
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var tokenInfo = JsonSerializer.Deserialize<TokenResponse>(jsonResponse);

                return tokenInfo.AccessToken;
            }
        }

        private static string GetSpreadsheetId(string url)
        {
            var startIndex = url.IndexOf("/d/") + 3;
            var endIndex = url.IndexOf("/", startIndex);
            if (endIndex == -1)
                endIndex = url.Length;
            return url.Substring(startIndex, endIndex - startIndex);
        }

        private class ServiceAccountInfo
        {
            public string Type { get; set; }
            public string ProjectId { get; set; }
            public string PrivateKeyId { get; set; }
            public string PrivateKey { get; set; }
            public string ClientEmail { get; set; }
            public string ClientId { get; set; }
            public string AuthUri { get; set; }
            public string TokenUri { get; set; }
            public string AuthProviderX509CertUrl { get; set; }
            public string ClientX509CertUrl { get; set; }
            public string ApiKey { get; set; }
        }

        private class TokenResponse
        {
            public string AccessToken { get; set; }
            public int ExpiresIn { get; set; }
            public string TokenType { get; set; }
        }

        private class SheetResponse
        {
            public List<List<string>> Values { get; set; }
        }
    }
} 