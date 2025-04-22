using UnityEngine;
using System;
using System.Linq;
using System.Text;
using Minimoo.Attributes;
using Minimoo.Extensions;
using Cysharp.Threading.Tasks;
using UnityEngine.Localization;
using UnityEngine.Localization.Metadata;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

namespace Minimoo.LocalizationDatas
{
    [CreateAssetMenu(fileName = "Google Sheet CSV Data", menuName = "Minimoo/Localization/Google Sheet CSV Data")]
    public class GoogleSheetCSVData : CSVData
    {
        [SerializeField] private string _sheetUrl;
        [SerializeField] private TextAsset _credentials;
        [SerializeField] private string _sheetRange = "A1:ZZ";

        private SheetsService _sheetsService;
        private string _spreadsheetId;

        [Button("Download Sheet")]
        public async void DownloadSheet()
        {
            if (string.IsNullOrEmpty(_sheetUrl))
            {
                D.Error("Google Sheet URL이 지정되지 않았습니다.");
                return;
            }

            if (_credentials == null)
            {
                D.Error("Google 서비스 계정 인증 파일이 지정되지 않았습니다.");
                return;
            }

            try
            {
                await InitializeGoogleSheetsService();
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

        private async UniTask InitializeGoogleSheetsService()
        {
            if (_sheetsService != null) return;

            var credential = GoogleCredential.FromJson(_credentials.text)
                .CreateScoped(SheetsService.Scope.SpreadsheetsReadonly);

            _sheetsService = new SheetsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Minimoo Localization"
            });

            _spreadsheetId = GetSpreadsheetId(_sheetUrl);
        }

        private async UniTask<bool> DownloadAndParseSheet()
        {
            try
            {
                var request = _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, _sheetRange);
                var response = await request.ExecuteAsync();

                if (response?.Values == null || response.Values.Count == 0)
                {
                    D.Error("시트에서 데이터를 찾을 수 없습니다.");
                    return false;
                }

                // CSV 형식으로 변환
                var csvBuilder = new StringBuilder();
                foreach (var row in response.Values)
                {
                    var rowData = row.Select(cell => $"\"{cell?.ToString() ?? ""}\"");
                    csvBuilder.AppendLine(string.Join(",", rowData));
                }

                ParseCSV(csvBuilder.ToString());

#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
                UnityEditor.AssetDatabase.SaveAssets();
#endif

                return true;
            }
            catch (Exception e)
            {
                D.Error($"시트 데이터 다운로드 실패: {e.Message}");
                return false;
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

        public void SetSheetUrl(string sheetUrl)
        {
            _sheetUrl = sheetUrl;
        }

        public void SetCredentials(TextAsset credentials)
        {
            _credentials = credentials;
        }

        public void SetSheetRange(string range)
        {
            _sheetRange = range;
        }
    }
}