using System.Collections.Generic;
namespace Minimoo.Extensions
{
    /// <summary>
    /// Dictionary 관련 확장 메소드를 제공합니다.
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Dictionary에 키-값 쌍을 추가하거나 업데이트합니다.
        /// 키가 존재하면 값을 업데이트하고, 존재하지 않으면 새로 추가합니다.
        /// </summary>
        /// <typeparam name="TKey">키의 타입</typeparam>
        /// <typeparam name="TValue">값의 타입</typeparam>
        /// <param name="self">대상 Dictionary</param>
        /// <param name="key">추가하거나 업데이트할 키</param>
        /// <param name="value">설정할 값</param>
        public static void Set<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key, TValue value)
        {
            if (self.ContainsKey(key))
            {
                self[key] = value;
            }
            else
            {
                self.Add(key, value);
            }
        }

        /// <summary>
        /// Dictionary에서 값을 가져와 지정된 타입으로 캐스팅을 시도합니다.
        /// </summary>
        /// <typeparam name="TKey">키의 타입</typeparam>
        /// <typeparam name="TValue">캐스팅할 값의 타입</typeparam>
        /// <param name="self">대상 Dictionary</param>
        /// <param name="key">찾을 키</param>
        /// <param name="value">캐스팅된 값</param>
        /// <returns>캐스팅 성공 여부</returns>
        public static bool TryCastValue<TKey, TValue>(this IDictionary<TKey, object> self, TKey key, out TValue value)
        {
            value = default;

            if (!self.TryGetValue(key, out var obj))
                return false;

            if (obj is not TValue)
                return false;

            value = (TValue)obj;
            return true;
        }
    }
}