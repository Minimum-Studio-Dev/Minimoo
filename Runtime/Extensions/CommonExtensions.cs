using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Minimoo.Extensions
{
    public static class CommonExtensions
    {
        /// <summary>
        /// 열거형의 다음 값을 반환합니다. 마지막 값인 경우 첫 번째 값을 반환합니다.
        /// </summary>
        /// <typeparam name="T">열거형 타입</typeparam>
        /// <param name="src">현재 열거형 값</param>
        /// <returns>다음 열거형 값</returns>
        public static T Next<T>(this T src) where T : struct
        {
            if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

            T[] Arr = (T[])Enum.GetValues(src.GetType());
            int j = Array.IndexOf<T>(Arr, src) + 1;
            return (Arr.Length == j) ? Arr[0] : Arr[j];
        }

        /// <summary>
        /// 객체의 깊은 복사본을 생성합니다.
        /// </summary>
        /// <typeparam name="T">복사할 객체의 타입</typeparam>
        /// <param name="obj">복사할 객체</param>
        /// <returns>복사된 새 객체</returns>
        public static T DeepClone<T>(this T obj)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, obj);
                stream.Position = 0;

                return (T)formatter.Deserialize(stream);
            }
        }

        /// <summary>
        /// 객체를 지정된 타입으로 캐스팅을 시도합니다.
        /// </summary>
        /// <typeparam name="T">캐스팅할 타입</typeparam>
        /// <param name="target">캐스팅할 객체</param>
        /// <param name="result">캐스팅 결과</param>
        /// <returns>캐스팅 성공 여부</returns>
        public static bool TryCast<T>(this object target, out T result)
        {
            try
            {
                result = default;

                if (target == null)
                {
                    return false;
                }

                Type targetType = target.GetType();
                Type resultType = typeof(T);

                // 기본 타입 간의 변환 시도
                if (resultType.IsPrimitive || resultType == typeof(string) || resultType == typeof(decimal))
                {
                    result = (T)Convert.ChangeType(target, resultType);
                    return true;
                }

                if (target is T targetResult)
                {
                    result = targetResult;
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                result = default;
                D.Warn($"Failed to cast {target?.GetType().Name ?? "null"} to {typeof(T).Name}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 현재 게임오브젝트를 제외한 형제 게임오브젝트들 중에서 지정된 컴포넌트를 찾습니다.
        /// </summary>
        /// <typeparam name="T">찾을 컴포넌트 타입</typeparam>
        /// <param name="target">현재 게임오브젝트</param>
        /// <returns>찾은 컴포넌트 또는 null</returns>
        public static T GetComponentInSiblings<T>(this GameObject target) where T : class
        {
            T result = null;

            Transform parentObject = target.transform.parent;
            int siblingCount = parentObject.transform.childCount;
            for (int i = 0; i < siblingCount; i++)
            {
                Transform currentChild = parentObject.GetChild(i);
                if (currentChild == target.transform) continue;

                if (currentChild.TryGetComponent(out T tryGetComponentResult))
                {
                    result = tryGetComponentResult;
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// 숫자의 서수 접미사(1st, 2nd, 3rd 등)를 반환합니다.
        /// </summary>
        /// <param name="num">숫자</param>
        /// <returns>서수 접미사 (st, nd, rd, th)</returns>
        public static string GetOrdinalSuffix(this int num)
        {
            string number = num.ToString();
            if (number.EndsWith("11")) return "th";
            if (number.EndsWith("12")) return "th";
            if (number.EndsWith("13")) return "th";
            if (number.EndsWith("1")) return "st";
            if (number.EndsWith("2")) return "nd";
            if (number.EndsWith("3")) return "rd";
            return "th";
        }

        /// <summary>
        /// 문자열을 기반으로 고유한 색상을 생성합니다.
        /// 같은 문자열은 항상 같은 색상을 반환합니다.
        /// </summary>
        /// <param name="str">색상을 생성할 문자열</param>
        /// <returns>생성된 색상</returns>
        public static Color RandomColorFromString(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return Color.clear;
            }

            var hash = 0;
            var chars = str.ToCharArray();
            for (var i = 0; i < chars.Length; i++)
            {
                hash = chars[i] + ((hash << 5) - hash);
            }

            var rgb = new int[3];
            for (var i = 0; i < 3; i++)
            {
                rgb[i] = (hash >> (i * 8)) & 255;
            }

            return new Color((float)rgb[0] / 255f, (float)rgb[1] / 255f, (float)rgb[2] / 255f);
        }
    }
}