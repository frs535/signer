namespace signer;

public class SignParameters
{
    public string WorkingDirectory { get; set; } = string.Empty;
    public string CryptcpPath { get; set; } = string.Empty;

    public string HttpServer1C { get; set; } = string.Empty;
    public List<Sign> Signs { get; set; } = [];
}

public class Sign
{
    public string Thumbprint { get; set; } = string.Empty;
    public string Organisation { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}