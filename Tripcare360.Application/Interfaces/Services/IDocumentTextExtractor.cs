namespace Tripcare360.Application.Interfaces.Services;

public interface IDocumentTextExtractor
{
    /// <summary>Extracts plain text from a PDF stream. Returns an empty string for image-only PDFs.</summary>
    string ExtractText(Stream pdfStream);
}
