using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace signer;
internal static class Program
{
    public static async Task Main(string[] args)
    {
        var text = await File.ReadAllTextAsync(args[0]);
        var signs = JsonSerializer.Deserialize<SignParameters>(text);

        HttpResponseMessage response;
        HttpRequestMessage request;
        string content;
        List<SignResult> result = [];
        
        var client = new HttpClient();
        foreach (var sign in signs!.Signs)
        {
            response = await client.GetAsync("https://markirovka.crpt.ru/api/v3/true-api/auth/key");
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
              
            var arguments = string.IsNullOrEmpty(sign.Password)?
                $"-signf -attached -cadesbes -cert -thumbprint {sign.Thumbprint} -dir {signs.WorkingDirectory} {tempFile}"
                : $"-signf -attached -cadesbes -cert -thumbprint {sign.Thumbprint} -dir {signs.WorkingDirectory} {tempFile} -pin {sign.Password}";
            
            var proc =Process.Start(signs.CryptcpPath, arguments);
            await proc.WaitForExitAsync();
            File.Delete(tempFile);
            
            var requestData = new RequestData()
            {
                Uuid = info.Uuid,
                Data = await File.ReadAllTextAsync($"{tempFile}.sig")
            };
            
            File.Delete($"{tempFile}.sig");
            
            content = JsonSerializer.Serialize(requestData);
            request = new HttpRequestMessage(HttpMethod.Post, "https://markirovka.crpt.ru/api/v3/true-api/auth/simpleSignIn");
            request.Content = new StringContent(content, Encoding.UTF8, "application/json");
            
            response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                await Console.Error.WriteLineAsync(await response.Content.ReadAsStringAsync());
                continue;
            }

            content = await response.Content.ReadAsStringAsync();
            var token = JsonSerializer.Deserialize<CRPTResult>(content);
            result.Add(new SignResult
            {
                Organisation = sign.Organisation,
                Token = token?.Token?? string.Empty
            });
        }
        
        content = JsonSerializer.Serialize(result);
        request = new HttpRequestMessage(HttpMethod.Post, signs.HttpServer1C);
        request.Content = new StringContent(content, Encoding.UTF8, "application/json");
        
        Console.WriteLine(content);

        //response = await client.SendAsync(request);
        
        client.Dispose();
    }
}