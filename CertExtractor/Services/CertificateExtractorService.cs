using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using CertExtractor.Models;
using Windows.Storage;

namespace CertExtractor.Services;

public class CertificateExtractorService
{
    public List<CertificateModel> ExtractCertificatesFromFile(string filePath)
    {
        var extractedCerts = new List<CertificateModel>();
        try
        {
            var cert = new X509Certificate2(filePath);

            using var chain = new X509Chain
            {
                ChainPolicy =
                {
                    RevocationMode = X509RevocationMode.NoCheck,
                    VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority
                }
            };

            chain.Build(cert);

            if (chain.ChainElements.Count == 0) return extractedCerts;

            // End-Entity Certificate
            var endEntityCert = chain.ChainElements[0].Certificate;
            extractedCerts.Add(CreateModel(endEntityCert, CertificateLevel.EndEntity));

            // Intermediate and Root Certificates
            for (int i = 1; i < chain.ChainElements.Count; i++)
            {
                var chainElementCert = chain.ChainElements[i].Certificate;
                bool isRoot = (chainElementCert.Subject == chainElementCert.Issuer);
                extractedCerts.Add(CreateModel(chainElementCert, isRoot ? CertificateLevel.Root : CertificateLevel.Intermediate));
            }
        }
        catch (CryptographicException)
        {
            // File is not a valid certificate or not signed
        }
        catch (Exception)
        {
            // Other potential errors (e.g., file access)
        }

        return extractedCerts;
    }

    public async Task<List<string>> SaveCertificatesAsync(IEnumerable<CertificateModel> certificates, StorageFolder outputFolder)
    {
        var logMessages = new List<string>();

        foreach (var certModel in certificates)
        {
            try
            {
                var file = await outputFolder.CreateFileAsync(certModel.FileName, CreationCollisionOption.ReplaceExisting);
                var base64String = Convert.ToBase64String(certModel.RawData, Base64FormattingOptions.InsertLineBreaks);

                var pemString = new StringBuilder();
                pemString.AppendLine("-----BEGIN CERTIFICATE-----");
                pemString.AppendLine(base64String);
                pemString.AppendLine("-----END CERTIFICATE-----");

                await FileIO.WriteTextAsync(file, pemString.ToString());
                logMessages.Add($"SUCCESS: Saved '{file.Name}' to '{outputFolder.Name}'.");
            }
            catch (Exception ex)
            {
                logMessages.Add($"ERROR: Failed to save '{certModel.FileName}'. Reason: {ex.Message}");
            }
        }
        return logMessages;
    }

    private CertificateModel CreateModel(X509Certificate2 cert, CertificateLevel level)
    {
        string commonName = cert.GetNameInfo(X509NameType.SimpleName, false);
        if (string.IsNullOrWhiteSpace(commonName))
        {
            // Fallback to Subject if CN is not available
            commonName = cert.Subject.Split(',')[0].Replace("CN=", "").Trim();
        }

        string expirationDate = cert.NotAfter.ToString("yyyy-MM-dd");
        string fileName = $"{level}_{SanitizeFileName(commonName)}_{expirationDate}.cer";

        return new CertificateModel
        {
            Level = level,
            RawData = cert.RawData,
            FileName = fileName
        };
    }

    private string SanitizeFileName(string name)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return new string(name.Where(c => !invalidChars.Contains(c)).ToArray()).Replace(" ", "_");
    }
}
