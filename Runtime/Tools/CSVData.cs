using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Minimoo.Tools
{
    public class CSVData : ScriptableObject
    {
        [SerializeField] private TextAsset _textAsset;
        
        private const char QUOTE_CHAR = '\"';
        private const char COMMA_CHAR = ',';
        private const char CR_CHAR = '\r';
        private const char LF_CHAR = '\n';
        private static readonly string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
        private static readonly char[] TRIM_CHARS = { '\"' };

        private List<string[]> _data;
        private Dictionary<string, int> _headerIndexMap;
        private bool _hasHeader = true;

        private void OnEnable()
        {
            LoadData();
        }

        private void LoadData()
        {
            if (_textAsset == null)
            {
                Debug.LogError($"TextAsset가 설정되지 않았습니다: {name}");
                return;
            }

            _data = Parse(_textAsset.text);
            _headerIndexMap = new Dictionary<string, int>();

            if (_hasHeader && _data.Count > 0)
            {
                var headers = _data[0];
                for (var i = 0; i < headers.Length; i++)
                {
                    _headerIndexMap[headers[i]] = i;
                }
            }
        }

        public void SetTextAsset(TextAsset textAsset)
        {
            _textAsset = textAsset;
            LoadData();
        }

        private static List<string[]> Parse(string data)
        {
            // 먼저 정규식으로 라인을 분리
            var lines = Regex.Split(data, LINE_SPLIT_RE);
            if (lines.Length <= 0) return new List<string[]>();

            var result = new List<string[]>();
            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line)) continue;

                var currentRow = new List<string>();
                var currentValue = new System.Text.StringBuilder();
                var inQuotes = false;
                var i = 0;

                while (i < line.Length)
                {
                    var c = line[i];

                    if (c == QUOTE_CHAR)
                    {
                        if (inQuotes && i + 1 < line.Length && line[i + 1] == QUOTE_CHAR)
                        {
                            currentValue.Append(QUOTE_CHAR);
                            i++;
                        }
                        else
                        {
                            inQuotes = !inQuotes;
                        }
                    }
                    else if (c == COMMA_CHAR && !inQuotes)
                    {
                        currentRow.Add(currentValue.ToString().Trim(TRIM_CHARS));
                        currentValue.Clear();
                    }
                    else
                    {
                        currentValue.Append(c);
                    }

                    i++;
                }

                if (currentValue.Length > 0 || currentRow.Count > 0)
                {
                    currentRow.Add(currentValue.ToString().Trim(TRIM_CHARS));
                    result.Add(currentRow.ToArray());
                }
            }

            return result;
        }

        public int RowCount => _hasHeader ? _data?.Count - 1 ?? 0 : _data?.Count ?? 0;

        public string[] GetRow(int rowIndex)
        {
            if (_data == null || rowIndex < 0 || rowIndex >= RowCount)
            {
                Debug.LogError($"잘못된 행 인덱스입니다: {rowIndex}");
                return null;
            }

            return _data[_hasHeader ? rowIndex + 1 : rowIndex];
        }

        public string GetValue(int rowIndex, int columnIndex)
        {
            var row = GetRow(rowIndex);
            if (row == null || columnIndex < 0 || columnIndex >= row.Length)
            {
                Debug.LogError($"잘못된 열 인덱스입니다: {columnIndex}");
                return null;
            }

            return row[columnIndex];
        }

        public string GetValue(int rowIndex, string columnName)
        {
            if (!_hasHeader)
            {
                Debug.LogError("헤더가 없는 CSV에서는 열 이름으로 값을 가져올 수 없습니다.");
                return null;
            }

            if (!_headerIndexMap.TryGetValue(columnName, out var columnIndex))
            {
                Debug.LogError($"열을 찾을 수 없습니다: {columnName}");
                return null;
            }

            return GetValue(rowIndex, columnIndex);
        }

        public T GetValue<T>(int rowIndex, int columnIndex) where T : struct
        {
            var value = GetValue(rowIndex, columnIndex);
            if (value == null) return default;

            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                Debug.LogError($"값을 {typeof(T)}로 변환할 수 없습니다: {value}");
                return default;
            }
        }

        public T GetValue<T>(int rowIndex, string columnName) where T : struct
        {
            var value = GetValue(rowIndex, columnName);
            if (value == null) return default;

            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                Debug.LogError($"값을 {typeof(T)}로 변환할 수 없습니다: {value}");
                return default;
            }
        }

        public string[] GetHeaders()
        {
            if (!_hasHeader)
            {
                Debug.LogError("헤더가 없는 CSV입니다.");
                return null;
            }

            return _data?[0];
        }

        public List<string[]> GetAllRows()
        {
            if (_data == null) return new List<string[]>();
            var startIndex = _hasHeader ? 1 : 0;
            return _data.GetRange(startIndex, RowCount);
        }

        public Dictionary<string, object>[] GetDictionaryData()
        {
            if (_data == null || !_hasHeader) return null;

            var headers = GetHeaders();
            var result = new Dictionary<string, object>[RowCount];

            for (var i = 0; i < RowCount; i++)
            {
                var row = GetRow(i);
                var entry = new Dictionary<string, object>();

                for (var j = 0; j < headers.Length && j < row.Length; j++)
                {
                    var value = row[j];
                    if (int.TryParse(value, out var n))
                    {
                        entry[headers[j]] = n;
                    }
                    else if (float.TryParse(value, out var f))
                    {
                        entry[headers[j]] = f;
                    }
                    else
                    {
                        entry[headers[j]] = value;
                    }
                }
                result[i] = entry;
            }

            return result;
        }
    }
}
