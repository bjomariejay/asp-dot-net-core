using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Net.Http;

namespace employeesAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HtmlSamplesController : ControllerBase
{
    private static readonly HttpClient HttpClient = new();

    /// <summary>
    /// Parses a tiny in-memory HTML snippet and returns the extracted heading text.
    /// </summary>
    [HttpGet("demo")]
    public ActionResult<IEnumerable<string>> Demo()
    {
        const string sampleHtml = """
            <html>
                <body>
                    <article>
                        <h1>Company Announcements</h1>
                        <p>Welcome aboard!</p>
                        <h2>Onboarding</h2>
                    </article>
                </body>
            </html>
            """;

        var document = new HtmlDocument();
        document.LoadHtml(sampleHtml);

        var headings = document.DocumentNode
            .SelectNodes("//h1 | //h2 | //p")?
            .Select(node => node.InnerText.Trim())
            .ToList()
            ?? new List<string>();

        return Ok(headings);
    }

    /// <summary>
    /// Fetches the supplied URL and extracts the &lt;title&gt; element.
    /// </summary>
    [HttpGet("title")]
    public async Task<ActionResult<string>> GetTitleAsync([FromQuery] string url, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return BadRequest("Provide a URL to inspect, e.g. /api/htmlsamples/title?url=https://example.com");
        }

        Uri uri;
        try
        {
            uri = new Uri(url);
        }
        catch (UriFormatException)
        {
            return BadRequest("The supplied URL is not valid.");
        }

        try
        {
            using var response = await HttpClient.GetAsync(uri, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, $"Request failed with status {(int)response.StatusCode}.");
            }

            var html = await response.Content.ReadAsStringAsync(cancellationToken);
            var document = new HtmlDocument();
            document.LoadHtml(html);

            var title = document.DocumentNode.SelectSingleNode("//p | //title")?.InnerText.Trim();
            if (string.IsNullOrEmpty(title))
            {
                return NotFound("The page did not contain a <title> tag.");
            }

            return Ok(title);
        }
        catch (HttpRequestException ex)
        {
            return Problem($"Could not download '{uri}': {ex.Message}");
        }
    }

    /// <summary>
    /// Downloads the provided URL and returns the full text content of the &lt;body&gt;.
    /// </summary>
    [HttpGet("bodyFromHtml")]
    public async Task<ActionResult<string>> GetBodyFromHtmlAsync([FromQuery] string url, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return BadRequest("Provide a URL to inspect, e.g. /api/htmlsamples/bodyFromHtml?url=https://example.com");
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return BadRequest("The supplied URL is not valid.");
        }

        try
        {
            using var response = await HttpClient.GetAsync(uri, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode,
                    $"Request failed with status {(int)response.StatusCode}.");
            }

            var html = await response.Content.ReadAsStringAsync(cancellationToken);
            var document = new HtmlDocument();
            document.LoadHtml(html);

            var bodyNode = document.DocumentNode.SelectSingleNode("//body");
            if (bodyNode is null)
            {
                return NotFound("The page did not contain a <body> tag.");
            }

            var textContent = HtmlEntity.DeEntitize(bodyNode.InnerText).Trim();
            return Ok(textContent);
        }
        catch (HttpRequestException ex)
        {
            return Problem($"Could not download '{uri}': {ex.Message}");
        }
    }
}
