using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Minimoo.Attributes;
using Minimoo.Extensions;

namespace Minimoo.LocalizationDatas
{
    public abstract class CSVData : ScriptableObject
    {
        [SerializeField] protected List<CSVRow> _rows = new List<CSVRow>();
        [SerializeField] protected string[] _headers;

        public string[] Headers => _headers;
        public int RowCount => _rows.Count;

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
