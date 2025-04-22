using UnityEngine;
using System;
using System.Net.Http;
using Minimoo.Attributes;
using Minimoo.Extensions;
using Cysharp.Threading.Tasks;

namespace Minimoo.LocalizationDatas
{
    [CreateAssetMenu(fileName = "Google Sheet CSV Data", menuName = "Minimoo/Localization/Google Sheet CSV Data")]
    public class GoogleSheetCSVData : CSVData
    {
        [SerializeField] private string _sheetUrl;
        private const string EXPORT_URL_FORMAT = "https://docs.google.com/spreadsheets/d/{0}/export?format=csv";

        [Button("Download Sheet")]
        public async void DownloadSheet()
        {
            if (string.IsNullOrEmpty(_sheetUrl))
            {
                D.Error("Google Sheet URL이 지정되지 않았습니다.");
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

        private async UniTask<bool> DownloadAndParseSheet()
        {
            try
            {
                var spreadsheetId = GetSpreadsheetId(_sheetUrl);
                var exportUrl = string.Format(EXPORT_URL_FORMAT, spreadsheetId);

                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync(exportUrl);
                    if (!response.IsSuccessStatusCode)
                    {
                        D.Error($"시트 다운로드 실패: {response.StatusCode}");
                        return false;
                    }

                    var csvData = await response.Content.ReadAsStringAsync();
                    ParseCSV(csvData);

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

        private static string GetSpreadsheetId(string url)
        {
            var startIndex = url.IndexOf("/d/") + 3;
            var endIndex = url.IndexOf("/", startIndex);
            if (endIndex == -1)
                endIndex = url.Length;
            return url.Substring(startIndex, endIndex - startIndex);
        }
    }
} 