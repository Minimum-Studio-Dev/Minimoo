namespace Minimoo.Error
{
    public class BaseError
    {
        public int Code { get; set; }

        public BaseError(int code)
        {
            Code = code;
        }

        public static implicit operator BaseError(int code)
        {
            return new BaseError(code);
        }

        public static implicit operator int(BaseError error)
        {
            return error.Code;
        }
    }

    public static class ErrorCode
    {
        public static readonly BaseError SUCCESS = new BaseError(0);
        public static readonly BaseError SYSTEM_ERROR = new BaseError(1);
        public static readonly BaseError INVALID_SESSION_KEY = new BaseError(2);
        public static readonly BaseError LOGIN_SESSION_IS_NULL = new BaseError(3);
        public static readonly BaseError BAD_REQUEST = new BaseError(4);
        public static readonly BaseError BAD_PARAMETER = new BaseError(5);
        public static readonly BaseError ALREADY_REGISTERD_RECEIPT = new BaseError(6);
        public static readonly BaseError HAS_BINDING_ACCOUNT = new BaseError(7);
        public static readonly BaseError NOT_SUPPORTED_GAME_VERSION = new BaseError(8);
        public static readonly BaseError ALREADY_PURCHASED_PRODUCT = new BaseError(9);
        public static readonly BaseError SERVER_IN_MAINTENANCE = new BaseError(10);
    }
}