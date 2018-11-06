namespace PGZ.UI.PrintService.Responses
{
    public sealed class DocumentResponse : ResponseStatus
    {
        public byte[] Document;
        public string Format;
        public string Filename;

        public DocumentResponse()
        {
            StatusCode = ResponseStatusCode.PENDING;
        }

        public string GetMimeTypeByFormat()
        {
            if (Format.ToLower() == "pdf")
            {
                return "application/pdf";
            } else
            {
                return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            }
        }
    }
}