using System.Net.Http;
using System.Text;
using System.Collections;
using System.Net.Http.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;


string headerCookie = string.Empty;
string baseUrl = "http://localhost:50/";
string filePath = "C:\\Users\\Vibin.KolothVerghese\\Downloads\\VibinTest.zip";
BeginCustomizationProcess();

void BeginCustomizationProcess()
{
  
    LoginRequestAsync().Wait();
    //GetPackagesAsync().Wait();
    //var result = ValidatePackageAsync().Result;
    ValidatePackageAsync().Wait();
    //UploadPackageAsync().Wait();
    //PublishPackageAsync().Wait();
    //PublishPackageEndAsync().Wait();
    //UnpublishAllAsync().Wait();
    LogoutRequestAsync().Wait();
    Console.ReadLine();
}

//Get HttpClient
HttpClient GetHttpClient(string url)
{
    var uri = new Uri(baseUrl);
    var client = new HttpClient
    {
        BaseAddress = new Uri($"{baseUrl}{url}")
    };
    return client;
}

//Login Request
async Task LoginRequestAsync()
{
    var client = GetHttpClient("entity/auth/login");
    using StringContent jsonContent = new(System.Text.Json.JsonSerializer.Serialize(new
    {
        name = "admin",
        password = "Vibin@Das@1980", //set the password
        company = "",
        branch = ""
    }), Encoding.UTF8,
        "application/json");

    var request = new HttpRequestMessage
    {
        Method = HttpMethod.Post,
        RequestUri = new Uri(client.BaseAddress?.AbsoluteUri),
        Content = jsonContent
    };
    var response = await client.SendAsync(request);
    response.EnsureSuccessStatusCode();
    headerCookie = response.Headers.FirstOrDefault(header => header.Key == "Set-Cookie").Value.Where(c => c.StartsWith(".ASPXAUTH")).ToList().FirstOrDefault(x => x.StartsWith(".ASPXAUTH"));
    Console.WriteLine(await response.Content.ReadAsStringAsync());

}

//Get Published Packages
async Task GetPublishedPackagesAsync()
{

    var client = GetHttpClient("CustomizationApi/getPublished");
    var request = new HttpRequestMessage
    {
        Method = HttpMethod.Post,
        RequestUri = new Uri(client.BaseAddress?.AbsoluteUri),
        Content = new StringContent("", null, "application/json")
    };
    request.Headers.Add("Cookie", headerCookie);
    var response = await client.SendAsync(request);
    response.EnsureSuccessStatusCode();
    Console.WriteLine(await response.Content.ReadAsStringAsync());
}

//Upload Package
async Task UploadPackageAsync()
{
    var client = GetHttpClient("CustomizationApi/import");
    using StringContent jsonContent = new(System.Text.Json.JsonSerializer.Serialize(new
    {
        projectLevel = "1",
        isReplaceIfExists = true,
        projectName = "ConstructionComplianceTesting",
        projectDescription = "Customization project for the Smart Fix company",
        projectContentBase64 = Convert.ToBase64String(File.ReadAllBytes(filePath))        
    }), Encoding.UTF8,
        "application/json");

    var request = new HttpRequestMessage
    {
        Method = HttpMethod.Post,
        RequestUri = new Uri(client?.BaseAddress.AbsoluteUri),
        Content = jsonContent

    };
    request.Headers.Add("Cookie", headerCookie);
    var response = await client.SendAsync(request);
    response.EnsureSuccessStatusCode();
    Console.WriteLine(await response.Content.ReadAsStringAsync());
}

//Validate Package
async Task ValidatePackageAsync()
{
    var client = GetHttpClient("CustomizationApi/PublishBegin");
    using StringContent jsonContent = new(System.Text.Json.JsonSerializer.Serialize(new
        {
            isMergeWithExistingPackages = false,
            isOnlyValidation = true,
            isOnlyDbUpdates = false,
            isReplayPreviouslyExecutedScripts = false,
            projectNames = new[] { "ConstructionComplianceTesting" },
            tenantMode = "Current"
        }), Encoding.UTF8,
        "application/json");

    var request = new HttpRequestMessage
    {
        Method = HttpMethod.Post,
        RequestUri = new Uri(client?.BaseAddress.AbsoluteUri),
        Content = jsonContent

    };
    request.Headers.Add("Cookie", headerCookie);
    var response = await client.SendAsync(request);
    response.EnsureSuccessStatusCode();
    Console.WriteLine(await response.Content.ReadAsStringAsync());
    WaitForPublishingCompletion();
}

//Publish Package
async Task PublishPackageAsync()
{
    var client = GetHttpClient("CustomizationApi/PublishBegin");
    using StringContent jsonContent = new(System.Text.Json.JsonSerializer.Serialize(new
    {
        isMergeWithExistingPackages = false,
        isOnlyValidation = false,
        isOnlyDbUpdates = false,
        isReplayPreviouslyExecutedScripts = false,
        projectNames = new[] { "ConstructionComplianceTesting" },
        tenantMode = "Current"
    }), Encoding.UTF8,
        "application/json");

    var request = new HttpRequestMessage
    {
        Method = HttpMethod.Post,
        RequestUri = new Uri(client?.BaseAddress.AbsoluteUri),
        Content = jsonContent

    };
    request.Headers.Add("Cookie", headerCookie);
    var response = await client.SendAsync(request);
    response.EnsureSuccessStatusCode();
    Console.WriteLine(await response.Content.ReadAsStringAsync());
}

//Publish Package End
async Task<Root> PublishPackageEndAsync()
{
        var client = GetHttpClient("CustomizationApi/publishEnd");
        Root val = null;
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(client?.BaseAddress.AbsoluteUri),
            Content = new StringContent("", null, "application/json")
        };
        request.Headers.Add("Cookie", headerCookie);
        using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
        {
            response.EnsureSuccessStatusCode();
            val = await response.Content.ReadFromJsonAsync<Root>();
        }
        return val;
}


void WaitForPublishingCompletion(int millisecondsInterval = 2000)
{
    while (true)
    {
        var processResult = PublishPackageEndAsync().Result;
        if (processResult.log.Count > 0)
        {
            Console.WriteLine(processResult.log.LastOrDefault()?.message);
        }

        if (processResult.isCompleted)
        {
            return;
        }
        else if (processResult.isFailed)
        {
            StringBuilder log = new StringBuilder();
            throw new Exception(log.ToString());
        }
        else
        {
            Thread.Sleep(millisecondsInterval);
        }
    }
}


async Task UnpublishAllAsync()
{
    var client = GetHttpClient("CustomizationApi/unpublishAll");
    using StringContent jsonContent = new(System.Text.Json.JsonSerializer.Serialize(new {
        tenantMode = "All"
    }), Encoding.UTF8,
        "application/json");

    var request = new HttpRequestMessage
    {
        Method = HttpMethod.Post,
        RequestUri = new Uri(client?.BaseAddress.AbsoluteUri),
        Content = jsonContent
    };
    request.Headers.Add("Cookie", headerCookie);
    using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
    {
        response.EnsureSuccessStatusCode();
        Console.WriteLine(await response.Content.ReadAsStringAsync());
    }
}

//Logout Request
async Task LogoutRequestAsync()
{
    var client = GetHttpClient("entity/auth/logout");
    using StringContent jsonContent = new(System.Text.Json.JsonSerializer.Serialize(new 
        {
            name = "admin",
            password = "", //set the password
            company = "",
            branch = ""
        }), Encoding.UTF8,
        "application/json");
    var request = new HttpRequestMessage
    {
        Method = HttpMethod.Post,
        RequestUri = new Uri(client?.BaseAddress.AbsoluteUri),
        Content = jsonContent
    };
    var response = await client.SendAsync(request);
    response.EnsureSuccessStatusCode();
    Console.WriteLine(await response.Content.ReadAsStringAsync());
}

// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
public class Log
{
    //public DateTime timestamp { get; set; }
    public string logType { get; set; }
    public string message { get; set; }
}

public class Root
{
    public bool isCompleted { get; set; }
    public bool isFailed { get; set; }
    public List<Log> log { get; set; }
}



