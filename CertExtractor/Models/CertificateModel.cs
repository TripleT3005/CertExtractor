namespace CertExtractor.Models;

public enum CertificateLevel
{
    EndEntity,
    Intermediate,
    Root
}

public class CertificateModel
{
    public required string FileName { get; set; }
    public required byte[] RawData { get; set; }
    public CertificateLevel Level { get; set; }
}
