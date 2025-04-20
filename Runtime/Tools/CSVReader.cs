using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Minimoo.Tools
{
    public class CSVReader
    {
        private static readonly string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
        private static readonly char[] TRIM_CHARS = { '\"' };

        private readonly List<string[]> _data;
        private readonly Dictionary<string, int> _headerIndexMap;
        private readonly bool _hasHeader;

        public CSVReader(string csvText, bool hasHeader = true)
        {
            _data = Parse(csvText);
            _hasHeader = hasHeader;
            _headerIndexMap = new Dictionary<string, int>();

            if (hasHeader && _data.Count > 0)
            {
                var headers = _data[0];
                for (var i = 0; i < headers.Length; i++)
                {
                    _headerIndexMap[headers[i]] = i;
                }
            }
        }

        public static CSVReader FromFile(string filePath, bool hasHeader = true)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError($"CSV 파일을 찾을 수 없습니다: {filePath}");
                return null;
            }

            var csvText = File.ReadAllText(filePath);
            return new CSVReader(csvText, hasHeader);
        }

        private static List<string[]> Parse(string data)
        {
            var result = new List<string[]>();
            var currentRow = new List<string>();
            var currentValue = new System.Text.StringBuilder();
            var inQuotes = false;
            var i = 0;

            while (i < data.Length)
            {
                var c = data[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < data.Length && data[i + 1] == '"')
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
                    currentRow.Add(currentValue.ToString());
                    currentValue.Clear();
                }
                else if ((c == '\r' || c == '\n') && !inQuotes)
                {
                    currentRow.Add(currentValue.ToString());
                    currentValue.Clear();
                    result.Add(currentRow.ToArray());
                    currentRow.Clear();

                    // Skip the next character if it's part of a CRLF pair
                    if (c == '\r' && i + 1 < data.Length && data[i + 1] == '\n')
                    {
                        i++;
                    }
                }
                else
                {
                    currentValue.Append(c);
                }

                i++;
            }

            // Add the last value and row if they exist
            if (currentValue.Length > 0 || currentRow.Count > 0)
            {
                currentRow.Add(currentValue.ToString());
                result.Add(currentRow.ToArray());
            }

            return result;
        }

        public int RowCount => _hasHeader ? _data.Count - 1 : _data.Count;

        public string[] GetRow(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= RowCount)
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

        public T GetValue<T>(int rowIndex, int columnIndex, T defaultValue = default)
        {
            var value = GetValue(rowIndex, columnIndex);
            if (value == null) return defaultValue;

            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                Debug.LogError($"값을 {typeof(T)}로 변환할 수 없습니다: {value}");
                return defaultValue;
            }
        }

        public T GetValue<T>(int rowIndex, string columnName, T defaultValue = default)
        {
            var value = GetValue(rowIndex, columnName);
            if (value == null) return defaultValue;

            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                Debug.LogError($"값을 {typeof(T)}로 변환할 수 없습니다: {value}");
                return defaultValue;
            }
        }

        public string[] GetHeaders()
        {
            if (!_hasHeader)
            {
                Debug.LogError("헤더가 없는 CSV입니다.");
                return null;
            }

            return _data[0];
        }

        public List<string[]> GetAllRows()
        {
            var startIndex = _hasHeader ? 1 : 0;
            return _data.GetRange(startIndex, RowCount);
        }

        // 기존 Read 메서드와의 호환성을 위한 정적 메서드
        public static List<Dictionary<string, object>> Read(string data)
        {
            var reader = new CSVReader(data);
            var result = new List<Dictionary<string, object>>();
            var headers = reader.GetHeaders();

            for (var i = 0; i < reader.RowCount; i++)
            {
                var row = reader.GetRow(i);
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
                result.Add(entry);
            }

            return result;
        }
    }
}
