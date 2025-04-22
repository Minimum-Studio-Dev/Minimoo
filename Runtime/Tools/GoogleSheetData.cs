using UnityEngine;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using System.Collections.Generic;

namespace Minimoo.Tools
{
    public class GoogleSheetData : ScriptableObject
    {
        [SerializeField] private string _sheetUrl;
        [SerializeField] private string _cacheFileName;
        [SerializeField] private bool _isDownloaded;
        [SerializeField] private string _lastDownloadTime;

        private CSVData _csvData;

        private const string CACHE_DIRECTORY = "Assets/StreamingAssets/SheetCache";
        private const string CACHE_EXTENSION = ".json";
        private const string EXPORT_URL_FORMAT = "https://docs.google.com/spreadsheets/d/{0}/export?format=csv";

        public string SheetUrl => _sheetUrl;
        public string CacheFileName => _cacheFileName;
        public bool IsDownloaded => _isDownloaded;
        public string LastDownloadTime => _lastDownloadTime;

        private void OnEnable()
        {
            LoadCachedData();
        }

        public void Initialize(string sheetUrl, string cacheFileName)
        {
            _sheetUrl = sheetUrl;
            _cacheFileName = cacheFileName;
            _isDownloaded = false;
            _lastDownloadTime = string.Empty;
        }

        public async Task<bool> EnsureDataDownloaded()
        {
            if (_isDownloaded && File.Exists(GetCachePath()))
            {
                LoadCachedData();
                return true;
            }

            return await DownloadAndCacheSheet();
        }

        private void LoadCachedData()
        {
            var cachePath = GetCachePath();
            if (!File.Exists(cachePath))
            {
                Debug.LogWarning($"캐시 파일을 찾을 수 없습니다: {cachePath}");
                return;
            }

            try
            {
                var jsonData = File.ReadAllText(cachePath);
                var sheetData = JsonUtility.FromJson<SheetData>(jsonData);
                var textAsset = new TextAsset(sheetData.CSV);
                _csvData = CreateInstance<CSVData>();
                _csvData.SetTextAsset(textAsset);
            }
            catch (Exception e)
            {
                Debug.LogError($"캐시 데이터 로드 실패: {e.Message}");
            }
        }

        private async Task<bool> DownloadAndCacheSheet()
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
                        Debug.LogError($"시트 다운로드 실패: {response.StatusCode}");
                        return false;
                    }

                    var csvData = await response.Content.ReadAsStringAsync();

                    // 캐시 디렉토리 생성
                    if (!Directory.Exists(CACHE_DIRECTORY))
                    {
                        Directory.CreateDirectory(CACHE_DIRECTORY);
                    }

                    // CSV 데이터를 JSON으로 변환하여 저장
                    var cachePath = GetCachePath();
                    var jsonData = JsonUtility.ToJson(new SheetData { CSV = csvData });
                    File.WriteAllText(cachePath, jsonData);

                    _isDownloaded = true;
                    _lastDownloadTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    // CSV 데이터 로드
                    var textAsset = new TextAsset(csvData);
                    _csvData = CreateInstance<CSVData>();
                    _csvData.SetTextAsset(textAsset);

#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(this);
                    UnityEditor.AssetDatabase.SaveAssets();
#endif

                    Debug.Log($"시트 데이터가 성공적으로 캐시되었습니다: {cachePath}");
                    return true;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"시트 데이터 다운로드 실패: {e.Message}");
                return false;
            }
        }

        public int RowCount => _csvData?.RowCount ?? 0;

        public string[] GetRow(int rowIndex) => _csvData?.GetRow(rowIndex);

        public string GetValue(int rowIndex, int columnIndex) => _csvData?.GetValue(rowIndex, columnIndex);

        public string GetValue(int rowIndex, string columnName) => _csvData?.GetValue(rowIndex, columnName);

        public T GetValue<T>(int rowIndex, int columnIndex) where T : struct => 
            _csvData != null ? _csvData.GetValue<T>(rowIndex, columnIndex) : default;

        public T GetValue<T>(int rowIndex, string columnName) where T : struct => 
            _csvData != null ? _csvData.GetValue<T>(rowIndex, columnName) : default;

        public string[] GetHeaders() => _csvData?.GetHeaders();

        public List<string[]> GetAllRows() => _csvData?.GetAllRows() ?? new List<string[]>();

        public Dictionary<string, object>[] GetDictionaryData() => _csvData?.GetDictionaryData();

        private string GetCachePath()
        {
            return Path.Combine(CACHE_DIRECTORY, _cacheFileName + CACHE_EXTENSION);
        }

        private static string GetSpreadsheetId(string url)
        {
            var startIndex = url.IndexOf("/d/") + 3;
            var endIndex = url.IndexOf("/", startIndex);
            if (endIndex == -1)
                endIndex = url.Length;
            return url.Substring(startIndex, endIndex - startIndex);
        }

        [Serializable]
        private class SheetData
        {
            public string CSV;
        }
    }
}