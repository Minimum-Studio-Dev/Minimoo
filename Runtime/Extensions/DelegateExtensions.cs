namespace Minimoo.Extensions
{
    /// <summary>
    /// 델리게이트와 이벤트 관련 확장 메소드를 제공합니다.
    /// </summary>
    public static class DelegateExtensions
    {
        /// <summary>
        /// 파라미터가 없는 이벤트를 호출합니다.
        /// </summary>
        /// <param name="eventAction">호출할 이벤트</param>
        public static void InvokeEvent(this System.Action eventAction)
        {
            if (eventAction == null)
            {
                //D.Warn("Attempt to invoke an event with no subscribers");
                return;
            }

            var delegateList = eventAction.GetInvocationList();

            if (delegateList != null)
            {
                for (int i = 0; i < delegateList.Length; i++)
                {
                    delegateList[i].Method.Invoke(delegateList[i].Target, null);
                }
            }
        }

        /// <summary>
        /// 하나의 파라미터를 가진 이벤트를 호출합니다.
        /// </summary>
        /// <typeparam name="T">파라미터 타입</typeparam>
        /// <param name="eventAction">호출할 이벤트</param>
        /// <param name="param">전달할 파라미터</param>
        public static void InvokeEvent<T>(this System.Action<T> eventAction, T param)
        {
            if (eventAction == null)
            {
                //D.Warn("Attempt to invoke an event with no subscribers");
                return;
            }

            var delegateList = eventAction.GetInvocationList();

            if (delegateList != null)
            {
                var paramArray = new object[] { param };

                for (int i = 0; i < delegateList.Length; i++)
                {
                    delegateList[i].Method.Invoke(delegateList[i].Target, paramArray);
                }
            }
        }

        /// <summary>
        /// 두 개의 파라미터를 가진 이벤트를 호출합니다.
        /// </summary>
        /// <typeparam name="T1">첫 번째 파라미터 타입</typeparam>
        /// <typeparam name="T2">두 번째 파라미터 타입</typeparam>
        /// <param name="eventAction">호출할 이벤트</param>
        /// <param name="param1">첫 번째 파라미터</param>
        /// <param name="param2">두 번째 파라미터</param>
        public static void InvokeEvent<T1, T2>(this System.Action<T1, T2> eventAction, T1 param1, T2 param2)
        {
            if (eventAction == null)
            {
                //D.Warn("Attempt to invoke an event with no subscribers");
                return;
            }

            var delegateList = eventAction.GetInvocationList();

            if (delegateList != null)
            {
                var paramArray = new object[] { param1, param2 };

                for (int i = 0; i < delegateList.Length; i++)
                {
                    delegateList[i].Method.Invoke(delegateList[i].Target, paramArray);
                }
            }
        }

        /// <summary>
        /// 세 개의 파라미터를 가진 이벤트를 호출합니다.
        /// </summary>
        /// <typeparam name="T1">첫 번째 파라미터 타입</typeparam>
        /// <typeparam name="T2">두 번째 파라미터 타입</typeparam>
        /// <typeparam name="T3">세 번째 파라미터 타입</typeparam>
        /// <param name="eventAction">호출할 이벤트</param>
        /// <param name="param1">첫 번째 파라미터</param>
        /// <param name="param2">두 번째 파라미터</param>
        /// <param name="param3">세 번째 파라미터</param>
        public static void InvokeEvent<T1, T2, T3>(this System.Action<T1, T2, T3> eventAction, T1 param1, T2 param2, T3 param3)
        {
            if (eventAction == null)
            {
                //D.Warn("Attempt to invoke an event with no subscribers");
                return;
            }

            var delegateList = eventAction.GetInvocationList();

            if (delegateList != null)
            {
                var paramArray = new object[] { param1, param2, param3 };

                for (int i = 0; i < delegateList.Length; i++)
                {
                    delegateList[i].Method.Invoke(delegateList[i].Target, paramArray);
                }
            }
        }

        /// <summary>
        /// 가변 파라미터를 가진 이벤트를 호출합니다.
        /// </summary>
        /// <param name="eventAction">호출할 이벤트</param>
        /// <param name="param">전달할 파라미터 배열</param>
        public static void InvokeEvent(this XEvent eventAction, params object[] param)
        {
            if (eventAction == null)
            {
                //D.Warn("Attempt to invoke an event with no subscribers");
                return;
            }

            var delegateList = eventAction.GetInvocationList();

            if (delegateList != null)
            {
                object[] paramArray = { param };

                for (int i = 0; i < delegateList.Length; i++)
                {
                    delegateList[i].Method.Invoke(delegateList[i].Target, paramArray);
                }
            }
        }

        /// <summary>
        /// 제네릭 타입의 이벤트를 호출합니다.
        /// </summary>
        /// <typeparam name="T">파라미터 타입</typeparam>
        /// <param name="eventAction">호출할 이벤트</param>
        /// <param name="param">전달할 파라미터</param>
        public static void InvokeEvent<T>(this XEvent<T> eventAction, T param)
        {
            if (eventAction == null)
            {
                //D.Warn("Attempt to invoke an event with no subscribers");
                return;
            }

            var delegateList = eventAction.GetInvocationList();

            if (delegateList != null)
            {
                object[] paramArray = { param };

                for (int i = 0; i < delegateList.Length; i++)
                {
                    delegateList[i].Method.Invoke(delegateList[i].Target, paramArray);
                }
            }
        }

        /// <summary>
        /// 두 개의 제네릭 파라미터를 가진 이벤트를 호출합니다.
        /// </summary>
        /// <typeparam name="T1">첫 번째 파라미터 타입</typeparam>
        /// <typeparam name="T2">두 번째 파라미터 타입</typeparam>
        /// <param name="eventAction">호출할 이벤트</param>
        /// <param name="param1">첫 번째 파라미터</param>
        /// <param name="param2">두 번째 파라미터</param>
        public static void InvokeEvent<T1, T2>(this XEvent<T1, T2> eventAction, T1 param1, T2 param2)
        {
            if (eventAction == null)
            {
                //D.Warn("Attempt to invoke an event with no subscribers");
                return;
            }

            var delegateList = eventAction.GetInvocationList();

            if (delegateList != null)
            {
                var paramArray = new object[] { param1, param2 };

                for (int i = 0; i < delegateList.Length; i++)
                {
                    delegateList[i].Method.Invoke(delegateList[i].Target, paramArray);
                }
            }
        }

        /// <summary>
        /// 세 개의 제네릭 파라미터를 가진 이벤트를 호출합니다.
        /// </summary>
        /// <typeparam name="T1">첫 번째 파라미터 타입</typeparam>
        /// <typeparam name="T2">두 번째 파라미터 타입</typeparam>
        /// <typeparam name="T3">세 번째 파라미터 타입</typeparam>
        /// <param name="eventAction">호출할 이벤트</param>
        /// <param name="param1">첫 번째 파라미터</param>
        /// <param name="param2">두 번째 파라미터</param>
        /// <param name="param3">세 번째 파라미터</param>
        public static void InvokeEvent<T1, T2, T3>(this XEvent<T1, T2, T3> eventAction, T1 param1, T2 param2, T3 param3)
        {
            if (eventAction == null)
            {
                //D.Warn("Attempt to invoke an event with no subscribers");
                return;
            }

            var delegateList = eventAction.GetInvocationList();

            if (delegateList != null)
            {
                var paramArray = new object[] { param1, param2, param3 };

                for (int i = 0; i < delegateList.Length; i++)
                {
                    delegateList[i].Method.Invoke(delegateList[i].Target, paramArray);
                }
            }
        }
    }
}