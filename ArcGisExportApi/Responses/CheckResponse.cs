namespace PGZ.UI.PrintService.Responses
{
    public sealed class CheckResponse : ResponseStatus
    {
        public string DownloadUrl;

        public CheckResponse(string downloadUrl)
        {
            StatusCode = ResponseStatusCode.OK;
            DownloadUrl = downloadUrl;
        }
    }
}