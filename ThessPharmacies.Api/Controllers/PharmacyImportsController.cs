using Microsoft.AspNetCore.Mvc;
using ThessPharmacies.Parser;

namespace ThessPharmacies.Api.Controllers;

[ApiController]
[Route("api/imports")]
public class PharmacyImportsController : ControllerBase
{
    private readonly FsthPdfParser _parser;

    public PharmacyImportsController(
        FsthPdfParser parser)
    {
        _parser = parser;
    }

    [HttpPost("parse")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<ParseResponse>> Parse(
        IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(
                new
                {
                    error = "A PDF file is required."
                });
        }

        if (!string.Equals(
                Path.GetExtension(file.FileName),
                ".pdf",
                StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(
                new
                {
                    error = "Only PDF files are supported."
                });
        }

        var temporaryFile =
            Path.Combine(
                Path.GetTempPath(),
                $"{Guid.NewGuid():N}.pdf");

        try
        {
            await using (var stream =
                         new FileStream(
                             temporaryFile,
                             FileMode.CreateNew,
                             FileAccess.Write,
                             FileShare.None))
            {
                await file.CopyToAsync(stream);
            }

            var result =
                _parser.Parse(temporaryFile);

            var response =
                new ParseResponse
                {
                    Success =
                        result.IsValid,

                    FileName =
                        file.FileName,

                    DutyDate =
                        result.DutyDate,

                    AreaGroup =
                        result.AreaGroup,

                    DaytimeCount =
                        result.DaytimeCount,

                    OvernightCount =
                        result.OvernightCount,

                    AfterMidnightCount =
                        result.AfterMidnightCount,

                    Total =
                        result.Pharmacies.Count,

                    Warnings =
                        result.Warnings.ToList(),

                    Pharmacies =
                        result.Pharmacies
                            .Select(x =>
                                new PharmacyResponse
                                {
                                    DutyDate =
                                        x.DutyDate,

                                    Area =
                                        x.Area,

                                    Name =
                                        x.Name,

                                    Address =
                                        x.Address,

                                    Phone =
                                        x.Phone,

                                    DutyType =
                                        x.DutyType.ToString()
                                })
                            .ToList()
                };

            return Ok(response);
        }
        catch (FormatException ex)
        {
            return BadRequest(
                new
                {
                    error = ex.Message
                });
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    error =
                        "An unexpected error occurred while parsing the PDF.",
                    details =
                        ex.Message
                });
        }
        finally
        {
            if (System.IO.File.Exists(
                    temporaryFile))
            {
                System.IO.File.Delete(
                    temporaryFile);
            }
        }
    }
}

public sealed class ParseResponse
{
    public bool Success { get; init; }

    public string FileName { get; init; } = "";

    public DateOnly DutyDate { get; init; }

    public string AreaGroup { get; init; } = "";

    public int DaytimeCount { get; init; }

    public int OvernightCount { get; init; }

    public int AfterMidnightCount { get; init; }

    public int Total { get; init; }

    public List<string> Warnings { get; init; } = new();

    public List<PharmacyResponse> Pharmacies { get; init; } = new();
}

public sealed class PharmacyResponse
{
    public DateOnly DutyDate { get; init; }

    public string Area { get; init; } = "";

    public string Name { get; init; } = "";

    public string Address { get; init; } = "";

    public string Phone { get; init; } = "";

    public string DutyType { get; init; } = "";
}