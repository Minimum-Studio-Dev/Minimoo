namespace Minimoo.Network.WebSockets
{
	public class WebSocketErrorCode
	{
		//    1005
		//    1005 is a reserved value and MUST NOT be set as a status code in a
		//    Close control frame by an endpoint.  It is designated for use in
		//    applications expecting a status code to indicate that no status
		//    code was actually present.
		public static readonly int KICKED_OUT_NO_REASON = 1005;
		//    1006
		//    1006 is a reserved value and MUST NOT be set as a status code in a
		//    Close control frame by an endpoint.  It is designated for use in
		//    applications expecting a status code to indicate that the
		//    connection was closed abnormally, e.g., without sending or
		//    receiving a Close control frame.
		public static readonly int KICKED_OUT_ABNORMAL = 1006;
	}
}