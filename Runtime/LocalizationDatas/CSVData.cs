using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Net.Http;
using Minimoo.Attributes;
using Minimoo.Extensions;
using Cysharp.Threading.Tasks;

namespace Minimoo.LocalizationDatas
{
    [CreateAssetMenu(fileName = "CSVData", menuName = "Minimoo/Localization/CSV Data")]
    public class CSVData : ScriptableObject
    {
        [SerializeField] private TextAsset _textAsset;
        [SerializeField] private string _sheetUrl;
        [SerializeField] protected List<CSVRow> _rows = new List<CSVRow>();
        [SerializeField] protected string[] _headers;

        private const string EXPORT_URL_FORMAT = "https://docs.google.com/spreadsheets/d/{0}/export?format=csv";

        public string[] Headers => _headers;
        public int RowCount => _rows.Count;

        private void OnEnable()
        {
            if (_textAsset != null)
            {
                ParseCSV(_textAsset.text);
            }
        }

        [Button("Parse TextAsset")]
        public void ParseTextAsset()
        {
            if (_textAsset == null)
            {
                D.Error("TextAsset이 지정되지 않았습니다.");
                return;
            }

            ParseCSV(_textAsset.text);
            D.Log($"CSV 데이터 파싱이 완료되었습니다. (총 {_rows.Count}행)");
        }

        [Button("Download Sheet")]
        public void DownloadSheet()
        {
            if (string.IsNullOrEmpty(_sheetUrl))
            {
                D.Error("Google Sheet URL이 지정되지 않았습니다.");
                return;
            }

            var task = DownloadAndParseSheet();
            if (task.Result)
            {
                D.Log($"Google Sheet 데이터 파싱이 완료되었습니다. (총 {_rows.Count}행)");
            }
        }

        public void SetTextAsset(TextAsset textAsset)
        {
            _textAsset = textAsset;
            ParseCSV(textAsset.text);
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

        protected void ParseCSV(string csvText)
        {
            _rows.Clear();

            try
            {
                var reader = new StringReader(csvText);
                string headerLine = reader.ReadLine();
                if (string.IsNullOrEmpty(headerLine))
                {
                    D.Error("CSV 헤더가 비어있습니다.");
                    return;
                }

                // 헤더 파싱
                _headers = ParseCSVLine(headerLine);
                if (_headers == null || _headers.Length == 0)
                {
                    D.Error("CSV 헤더를 파싱할 수 없습니다.");
                    return;
                }

                // 데이터 파싱
                string line;
                var currentRow = new List<string>();
                var currentField = new System.Text.StringBuilder();
                var inQuotes = false;
                int lineNumber = 1;
                CSVRow currentCSVRow = null;

                while ((line = reader.ReadLine()) != null)
                {
                    lineNumber++;

                    // 빈 줄이나 주석 건너뛰기
                    if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
                        continue;

                    foreach (char c in line)
                    {
                        if (c == '"')
                        {
                            inQuotes = !inQuotes;
                            currentField.Append(c);
                        }
                        else if (c == ',' && !inQuotes)
                        {
                            currentRow.Add(currentField.ToString());
                            currentField.Clear();
                        }
                        else
                        {
                            currentField.Append(c);
                        }
                    }

                    // 줄의 마지막이 아닌 경우 줄바꿈 추가
                    if (inQuotes)
                    {
                        currentField.Append("\n");
                        continue;
                    }

                    // 마지막 필드 추가
                    currentRow.Add(currentField.ToString());
                    currentField.Clear();

                    // 행 데이터 처리
                    if (currentRow.Count > 0)
                    {
                        ProcessRow(currentRow.ToArray(), lineNumber);
                        currentRow.Clear();
                    }
                }

                // 마지막 행이 따옴표로 끝나지 않은 경우 처리
                if (currentRow.Count > 0 || currentField.Length > 0)
                {
                    if (currentField.Length > 0)
                    {
                        currentRow.Add(currentField.ToString());
                    }
                    ProcessRow(currentRow.ToArray(), lineNumber);
                }
            }
            catch (Exception e)
            {
                D.Error($"CSV 파싱 중 오류 발생: {e.Message}");
            }
        }

        protected void ProcessRow(string[] values, int lineNumber)
        {
            if (values.Length == 0) return;

            var key = CleanupValue(values[0]);
            var row = new CSVRow(key);

            for (int i = 0; i < _headers.Length; i++)
            {
                var value = i < values.Length ? CleanupValue(values[i]) : string.Empty;
                row.SetValue(_headers[i], value);
            }

            _rows.Add(row);

            if (values.Length != _headers.Length)
            {
                D.Warn($"헤더와 데이터의 열 수가 일치하지 않습니다. (행: {lineNumber}, 헤더: {_headers.Length}, 데이터: {values.Length})");
            }
        }

        protected string CleanupValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            // 따옴표로 둘러싸인 값 처리
            if (value.StartsWith("\"") && value.EndsWith("\""))
            {
                // 앞뒤 따옴표 제거
                value = value.Substring(1, value.Length - 2);
                // 이스케이프된 따옴표 처리
                value = value.Replace("\"\"", "\"");
            }

            return value.Trim();
        }

        protected string[] ParseCSVLine(string line)
        {
            var values = new List<string>();
            var currentValue = new System.Text.StringBuilder();
            var inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                var c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        currentValue.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    values.Add(CleanupValue(currentValue.ToString()));
                    currentValue.Clear();
                }
                else
                {
                    currentValue.Append(c);
                }
            }

            values.Add(CleanupValue(currentValue.ToString()));
            return values.ToArray();
        }

        public CSVRow GetRow(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= _rows.Count)
            {
                D.Error($"유효하지 않은 행 인덱스입니다: {rowIndex}");
                return null;
            }
            return _rows[rowIndex];
        }

        public CSVRow FindRowByKey(string key)
        {
            return _rows.FirstOrDefault(row => row.Key == key);
        }

        public string GetValue(int rowIndex, string columnName)
        {
            var row = GetRow(rowIndex);
            return row?.GetValue(columnName);
        }

        public T GetValue<T>(int rowIndex, string columnName) where T : struct
        {
            var row = GetRow(rowIndex);
            return row != null ? row.GetValue<T>(columnName) : default;
        }

        public List<CSVRow> GetAllRows()
        {
            return _rows;
        }
    }
}
