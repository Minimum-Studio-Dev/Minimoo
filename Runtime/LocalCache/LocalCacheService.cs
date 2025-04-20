using System.Collections.Generic;
using UnityEngine;


namespace Minimoo.LocalCache
{
    /// <summary>
    /// 로컬 캐싱 데이터 저장 
    /// </summary>
    public class LocalCacheService : Singleton<LocalCacheService>
    {
        [SerializeField]
        private SerializedDictionary<string, object> keyValueStorage { get; set; } = new SerializedDictionary<string, object>();

        public void Set(string key, object value)
        {
            if (keyValueStorage.TryAdd(key, value) == false)
            {
                keyValueStorage[key] = value;
            }
        }

        public bool Get<T>(string key, out T taregtValue)
        {
            taregtValue = default(T);

            if (keyValueStorage.TryGetValue(key, out var value))
            {
                taregtValue = (T)value;
                return true;
            }

            return false;
        }
    }
}
