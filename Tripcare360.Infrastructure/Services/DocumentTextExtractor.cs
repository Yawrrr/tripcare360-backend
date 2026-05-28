using Microsoft.Extensions.Logging;
using Tripcare360.Application.Interfaces.Services;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace Tripcare360.Infrastructure.Services;

public class DocumentTextExtractor(ILogger<DocumentTextExtractor> logger) : IDocumentTextExtractor
{
    public string ExtractText(Stream pdfStream)
    {
        try
        {
            using var doc = PdfDocument.Open(pdfStream);
            var builder = new System.Text.StringBuilder();
            foreach (Page page in doc.GetPages())
                builder.AppendLine(page.Text);
            return builder.ToString().Trim();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "[DocumentExtractor] Failed to extract text from PDF stream");
            return string.Empty;
        }
    }
}
