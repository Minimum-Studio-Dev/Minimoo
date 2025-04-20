using System;
using System.Collections;
using UnityEngine;
using Minimoo;

namespace Minimoo.Extensions
{
    /// <summary>
    /// 코루틴을 생성하고 관리하기 위한 확장 메소드를 제공합니다.
    /// </summary>
    public static class MNMCoroutineExtensions
    {
        /// <summary>
        /// IEnumerator를 MNMCoroutine으로 변환합니다.
        /// </summary>
        /// <param name="generator">변환할 IEnumerator</param>
        /// <param name="owner">코루틴을 실행할 MonoBehaviour 컴포넌트</param>
        /// <returns>생성된 MNMCoroutine 인스턴스</returns>
        public static MNMCoroutine ToCoroutine(this IEnumerator generator, MonoBehaviour owner)
        {
            return new MNMCoroutine(generator, owner);
        }
    }
}