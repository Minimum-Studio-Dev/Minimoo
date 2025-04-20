namespace Minimoo
{
    public delegate void XEvent(params object[] param);

    public delegate void XEvent<T>(T param);

    public delegate void XEvent<T1, T2>(T1 param1, T2 param2);

    public delegate void XEvent<T1, T2, T3>(T1 param1, T2 param2, T3 param3);
}