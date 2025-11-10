using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace EsepWebhook
{
    public class Function
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest input, ILambdaContext context)
        {
            try
            {
                var body = JsonConvert.DeserializeObject<JObject>(input.Body);
                var issueUrl = body["issue"]?["html_url"]?.ToString() ?? "No URL found";

                var message = new { text = $"Issue Created: {issueUrl}" };
                var slackUrl = Environment.GetEnvironmentVariable("SLACK_URL");

                var content = new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(slackUrl, content);

                context.Logger.LogInformation($"Sent to Slack: {response.StatusCode}");

                return new APIGatewayProxyResponse
                {
                    StatusCode = 200,
                    Body = JsonConvert.SerializeObject(new { message = "Sent to Slack" })
                };
            }
            catch (Exception ex)
            {
                context.Logger.LogError($"Error: {ex.Message}");
                return new APIGatewayProxyResponse
                {
                    StatusCode = 500,
                    Body = $"Error: {ex.Message}"
                };
            }
        }
    }
}
