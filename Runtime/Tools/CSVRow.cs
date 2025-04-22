using UnityEngine;
using System;
using System.Collections.Generic;

namespace Minimoo.Tools
{
    [Serializable]
    public class CSVRow
    {
        [SerializeField] private string _key;
        [SerializeField] private SerializedDictionary<string, string> _values = new SerializedDictionary<string, string>();

        public string Key => _key;
        public SerializedDictionary<string, string> Values => _values;

        public CSVRow() { }

        public CSVRow(string key)
        {
            _key = key;
        }

        public void SetValue(string columnName, string value)
        {
            _values[columnName] = value;
        }

        public string GetValue(string columnName)
        {
            return _values.TryGetValue(columnName, out var value) ? value : string.Empty;
        }

        public T GetValue<T>(string columnName) where T : struct
        {
            var value = GetValue(columnName);
            if (string.IsNullOrEmpty(value))
                return default;

            try
            {
                return (T)System.Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                D.Error($"값을 {typeof(T)}로 변환할 수 없습니다: {value}");
                return default;
            }
        }
    }
}