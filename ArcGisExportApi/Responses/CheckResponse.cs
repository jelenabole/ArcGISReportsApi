namespace PGZ.UI.PrintService.Responses
{
    public sealed class CheckResponse : ResponseStatus
    {
        public string DownloadUrlDocX;
        public string DownloadUrlPdf;

        public CheckResponse(string downloadUrlDocX, string downloadUrlPdf)
        {
            StatusCode = ResponseStatusCode.OK;
            DownloadUrlDocX = downloadUrlDocX;
            DownloadUrlPdf = downloadUrlPdf;
        }
    }
}