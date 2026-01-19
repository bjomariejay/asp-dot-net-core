using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using JsonException = System.Text.Json.JsonException;

namespace employeesAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JsonSamplesController : ControllerBase
{
    /// <summary>
    /// Accepts any JSON payload and returns a pretty-printed version using Json.NET.
    /// </summary>
    [HttpGet("normalize")]
    public IActionResult NormalizeSample()
    {
        const string samplePayload = """
        { "Skills": ["C#", "SQL", "React"] }
        """;
        var sampleToken = JsonConvert.DeserializeObject(samplePayload);
        var normalized = JsonConvert.SerializeObject(sampleToken, Formatting.Indented);

        return Ok(new
        {
            instructions = "POST your own JSON payload to this endpoint to receive a pretty-printed response.",
            endpoint = Url.ActionLink(nameof(Normalize), "JsonSamples") ?? "/api/jsonsamples/normalize",
            payload = samplePayload,
            normalized
        });
    }

    //test data = { "Skills": ["C#", "SQL", "React"] }
    [HttpPost("normalize")]
    public ActionResult<string> Normalize([FromBody] JsonElement payload)
    {
        var raw = payload.GetRawText();
        var token = JsonConvert.DeserializeObject(raw);
        var formatted = JsonConvert.SerializeObject(token, Formatting.Indented);
        return Content(formatted, "application/json");
    }


  // sample 1
  // {
  //  "json": "{ \"employee\": { \"email\": \"ada@example.com\" } }",
  //  "path": "$.employee.email"
  //}

  // sample 2
  //   {
  //  "json": "[\"C#\", \"SQL\", \"React\"]",
  //  "path": "$[0]"
  //}

    /// <summary>
    /// Evaluates the requested JSONPath expression against the provided JSON string.
    /// </summary>
    [HttpPost("extract")]
    public ActionResult<string> ExtractValue([FromBody] JsonPathRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Json))
        {
            return BadRequest("Provide the JSON payload to inspect.");
        }

        if (string.IsNullOrWhiteSpace(request.Path))
        {
            return BadRequest("Provide a JSONPath expression, e.g. $.employee.email");
        }

        try
        {
            var token = JToken.Parse(request.Json);
            var result = token.SelectToken(request.Path);
            if (result is null)
            {
                return NotFound("The JSONPath expression did not match any value.");
            }

            return Ok(result.Type == JTokenType.String ? result.Value<string>() : result.ToString(Formatting.None));
        }
        catch (JsonReaderException ex)
        {
            return BadRequest($"Invalid JSON: {ex.Message}");
        }
        catch (JsonException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    public sealed class JsonPathRequest
    {
        public string Json { get; init; } = string.Empty;
        public string Path { get; init; } = string.Empty;
    }
}
