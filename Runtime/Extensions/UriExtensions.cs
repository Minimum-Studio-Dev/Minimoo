using System;
using System.Collections.Generic;
using System.Linq;

namespace Minimoo.Extensions
{
    /// <summary>
    /// URI 관련 확장 메소드를 제공합니다.
    /// </summary>
    public static class UriExtensions
    {
        /// <summary>
        /// URI의 경로 부분을 문자열로 반환합니다.
        /// </summary>
        /// <param name="self">대상 URI</param>
        /// <returns>경로 문자열</returns>
        public static string GetPath(this Uri self) => string.Join(string.Empty, self.Segments.Skip(1));

        /// <summary>
        /// URI의 쿼리 파라미터를 설정합니다.
        /// </summary>
        /// <param name="self">대상 URI</param>
        /// <param name="query">설정할 쿼리 파라미터</param>
        /// <returns>쿼리 파라미터가 설정된 새로운 URI</returns>
        public static Uri SetQuery(this Uri self, IEnumerable<KeyValuePair<string, string>> query)
        {
            if (query == null || !query.Any())
                return self;

            var originalQuery = self.GetQuery();
            foreach (var (key, value) in query)
            {
                originalQuery.Set(key, Uri.EscapeDataString(value));
            }

            var queryString = string.Join('&', originalQuery.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));
            return new Uri($"{self.Scheme}://{self.Host}{self.LocalPath}?{queryString}");
        }

        /// <summary>
        /// URI의 쿼리 파라미터를 Dictionary로 반환합니다.
        /// </summary>
        /// <param name="self">대상 URI</param>
        /// <returns>쿼리 파라미터 Dictionary</returns>
        public static Dictionary<string, string> GetQuery(this Uri self) =>
            string.IsNullOrEmpty(self.Query)
                ? new Dictionary<string, string>()
                    : self.Query.Split('?')[1].Split('&')
                        .Select(x => x.Split('='))
                        .ToDictionary(x => x[0], x => x[1]);

#pragma warning disable ERP022
        /// <summary>
        /// URI의 쿼리 파라미터를 지정된 타입으로 파싱을 시도합니다.
        /// </summary>
        /// <typeparam name="T">파싱할 타입</typeparam>
        /// <param name="self">대상 URI</param>
        /// <param name="key">파싱할 쿼리 파라미터 키</param>
        /// <param name="value">파싱된 값</param>
        /// <returns>파싱 성공 여부</returns>
        public static bool TryParseQuery<T>(this Uri self, string key, out T value)
        {
            var query = self.GetQuery();
            if (query == null || !query.TryGetValue(key, out var value_))
            {
                value = default;
                return false;
            }

            var type = typeof(T);
            if (type.IsEnum)
            {
                try
                {
                    value = (T)Enum.Parse(type, value_);
                    return true;
                }
                catch
                {
                    try
                    {
                        value = (T)Enum.ToObject(type, int.Parse(value_));
                        return true;
                    }
                    catch
                    {
                        value = default;
                        return false;
                    }
                }
            }
            else
            {
                try
                {
                    value = Type.GetTypeCode(type) switch
                    {
                        TypeCode.Byte => (T)Convert.ChangeType(byte.Parse(value_), type),
                        TypeCode.SByte => (T)Convert.ChangeType(sbyte.Parse(value_), type),
                        TypeCode.UInt16 => (T)Convert.ChangeType(ushort.Parse(value_), type),
                        TypeCode.UInt32 => (T)Convert.ChangeType(uint.Parse(value_), type),
                        TypeCode.UInt64 => (T)Convert.ChangeType(ulong.Parse(value_), type),
                        TypeCode.Int16 => (T)Convert.ChangeType(short.Parse(value_), type),
                        TypeCode.Int32 => (T)Convert.ChangeType(int.Parse(value_), type),
                        TypeCode.Int64 => (T)Convert.ChangeType(long.Parse(value_), type),
                        TypeCode.Decimal => (T)Convert.ChangeType(decimal.Parse(value_), type),
                        TypeCode.Double => (T)Convert.ChangeType(double.Parse(value_), type),
                        TypeCode.Single => (T)Convert.ChangeType(float.Parse(value_), type),
                        TypeCode.Boolean => (T)Convert.ChangeType(bool.Parse(value_), type),
                        TypeCode.Char => (T)Convert.ChangeType(value_, type),
                        TypeCode.DateTime => (T)Convert.ChangeType(value_, type),
                        TypeCode.DBNull => (T)Convert.ChangeType(value_, type),
                        TypeCode.Empty => (T)Convert.ChangeType(value_, type),
                        TypeCode.Object => (T)Convert.ChangeType(value_, type),
                        TypeCode.String => (T)Convert.ChangeType(value_, type),
                        _ => (T)Convert.ChangeType(value_, type),
                    };
                    return true;
                }
                catch
                {
                    value = default;
                    return false;
                }
            }
        }
#pragma warning restore ERP022
    }
}
