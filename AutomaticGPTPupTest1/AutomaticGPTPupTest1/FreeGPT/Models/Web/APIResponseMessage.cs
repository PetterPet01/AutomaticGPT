using System.Diagnostics.CodeAnalysis;

namespace AutomaticGPTPupTest1.FreeGPT.Models.Web
{
    public class APIResponseMessage<T>
    {
        public bool IsErrorOccurred { get; set; }
        public ResponseError? Error { get; set; }
        [MemberNotNullWhen(false, nameof(IsErrorOccurred))]
        public T? Response { get; set; }
        public APIResponseMessage(bool isErrorOccurred, ResponseError? error, T? response)
        {
            IsErrorOccurred = isErrorOccurred;
            Error = error;
            Response = response;
        }
    }
}
