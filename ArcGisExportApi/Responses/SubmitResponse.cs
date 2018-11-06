namespace PGZ.UI.PrintService.Responses
{
    public sealed class SubmitResponse : ResponseStatus
    {
        public string Token;

        public SubmitResponse(string token)
        {
            StatusCode = ResponseStatusCode.OK;
            Token = token;
        }
    }
}