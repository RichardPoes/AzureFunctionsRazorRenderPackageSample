using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Razor.Templating.Core;
using RRPS.FrontEnd.Views;

namespace RRPS.FunctionApp;

public class SendRazorTemplate
{
    private readonly ILogger<SendRazorTemplate> _logger;

    public SendRazorTemplate(ILogger<SendRazorTemplate> log)
    {
        _logger = log;
    }

    [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
    [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "The **Name** parameter")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/html", bodyType: typeof(string), Description = "The OK response")]
    [FunctionName("SendRazorTemplate")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "GET", "POST", Route = null)]
        HttpRequest req)
    {
        string name = req.Query["name"];

        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic data = JsonConvert.DeserializeObject(requestBody);
        name ??= data?.name;

        var model = new PocTemplateModel
        {
            Name = name
        };
        var htmlContent = await RazorTemplateEngine.RenderAsync("~/PocTemplate.cshtml", model);

        _logger.LogInformation("C# HTTP trigger function processed a request.");
        return new ContentResult { Content = htmlContent, ContentType = "text/html" };
    }
}

