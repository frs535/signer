using System.Diagnostics;
using System.Net;
using System.Text.Json;
using signer;
internal class Program
{
    public static async Task Main(string[] args)
    {
        var text = File.ReadAllText(args[0]);
        var signs = JsonSerializer.Deserialize<SignParameters>(text);

        foreach (var sign in signs!.Signs)
        {
            var client = new HttpClient();
            var response = await client.GetAsync("https://markirovka.crpt.ru/api/v3/true-api/auth/key");
            if (response.StatusCode != HttpStatusCode.Accepted && response.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine(await response.Content.ReadAsStringAsync());
                continue;
            }
     
            var info = JsonSerializer.Deserialize<RequestData>(await response.Content.ReadAsStringAsync());
            if (info==null)
            {
                Console.WriteLine("No data from https://markirovka.crpt.ru/api/v3/true-api/auth/key");
                continue;
            }

            var tempFile = Path.GetTempFileName();
            //var tempFile = signs.WorkingDirectory + @"/test.txt";
            await File.WriteAllTextAsync(tempFile, info.Data);
            
            var arguments = $"-signf -nochain -attached -thumbprint {sign.Thumbprint} -dir {signs.WorkingDirectory} {tempFile}";
            var proc =Process.Start(signs.CryptcpPath, arguments);
            await proc.WaitForExitAsync();
        }
    }
}
