namespace Application.Contracts;

public interface IQrCodeService
{
    string GenerateSvgBase64(string content, int pixelsPerModule = 5);
    string GeneratePngBase64(string content, int pixelsPerModule = 20);
}
