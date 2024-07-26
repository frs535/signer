using System.Diagnostics;
using System.Text;
using System.Text.Json;
using signer;
internal class Program
{
    public static async Task Main(string[] args)
    {
        var text = await File.ReadAllTextAsync(args[0]);
        var signs = JsonSerializer.Deserialize<SignParameters>(text);
        
        var client = new HttpClient();
        foreach (var sign in signs!.Signs)
        {
            var response = await client.GetAsync("https://markirovka.crpt.ru/api/v3/true-api/auth/key");
            if (!response.IsSuccessStatusCode)
            {
                await Console.Error.WriteLineAsync("Server https://markirovka.crpt.ru does not response");
                continue;
            }
     
            var info = JsonSerializer.Deserialize<RequestData>(await response.Content.ReadAsStringAsync());
            if (info==null)
            {
                await Console.Error.WriteLineAsync("No data from https://markirovka.crpt.ru/api/v3/true-api/auth/key");
                continue;
            }
            
            var tempFile = $"{signs.WorkingDirectory}/signData_{sign.Organisation}";
            await File.WriteAllTextAsync(tempFile, info.Data);
            //-nochain  
            var arguments = $"-signf -attached -cadesbes -cert -thumbprint {sign.Thumbprint} -dir {signs.WorkingDirectory} {tempFile}";
            var proc =Process.Start(signs.CryptcpPath, arguments);
            await proc.WaitForExitAsync();
            
            var requestData = new RequestData()
            {
                Uuid = info.Uuid,
                Data = await File.ReadAllTextAsync($"{tempFile}.sig")
            };
            
            var content = JsonSerializer.Serialize(requestData);
            var request = new HttpRequestMessage(HttpMethod.Post, "https://markirovka.crpt.ru/api/v3/true-api/auth/simpleSignIn");
            request.Content = new StringContent(content, Encoding.UTF8, "application/json");
            
            response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                await Console.Error.WriteLineAsync(await response.Content.ReadAsStringAsync());
                continue;
            }

            var testString = await response.Content.ReadAsStringAsync();
            Console.WriteLine(testString);
        }
        
        client.Dispose();
    }
}
