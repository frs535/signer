namespace signer;

public class SignParameters
{
    public string WorkingDirectory { get; set; }
    public string CryptcpPath { get; set; }
    public List<Sign> Signs { get; set; } = [];
}

public class Sign
{
    public string Thumbprint { get; set; }
    public string Organisation { get; set; }
    public string Password { get; set; }
}