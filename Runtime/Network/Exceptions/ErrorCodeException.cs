using System;
using Minimoo.Common.Enum;
using Minimoo.Error;

namespace Minimoo.Network.Exceptions
{
    public class ErrorCodeException : ApplicationException
    {
        public BaseError Code = ErrorCode.SUCCESS;

        public ErrorCodeException()
            : base() { }

        public ErrorCodeException(BaseError code)
        {
            Code = code;
        }

        public ErrorCodeException(BaseError code, string message)
            : base(message)
        {
            Code = code;
        }

        public ErrorCodeException(string message, Exception inner)
            : base(message, inner)
        {

        }

        public ErrorCodeException(BaseError code, string message, Exception inner)
            : base(message, inner)
        {
            Code = code;
        }
    }
}
