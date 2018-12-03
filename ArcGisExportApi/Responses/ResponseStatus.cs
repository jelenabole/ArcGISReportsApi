using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PGZ.UI.PrintService.Responses
{
    public class ResponseStatus
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public ResponseStatusCode StatusCode { get; set; }
        public string ErrorDescription { get; set; }

        public ResponseStatus() { }

        public ResponseStatus(string errorMessage)
        {
            StatusCode = ResponseStatusCode.ERROR;
            ErrorDescription = errorMessage;
        }

        public ResponseStatus(string errorMessage, ResponseStatusCode statusCode)
        {
            StatusCode = ResponseStatusCode.ERROR;
            ErrorDescription = errorMessage;
            StatusCode = statusCode;
        }
    }

    public enum ResponseStatusCode
    {
        OK,
        ERROR,
        PENDING
    }
}