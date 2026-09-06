using Microsoft.AspNetCore.Mvc;
using ThessPharmacies.Api.Services;

namespace ThessPharmacies.Api.Controllers;

[ApiController]
[Route("api/imports")]
public sealed class PharmacyImportsController : ControllerBase
{
    private readonly DutyImportService _importService;

    private const long MaxFileSize = 10 * 1024 * 1024;

    public PharmacyImportsController(
        DutyImportService importService)
    {
        _importService = importService;
    }

    [HttpPost("parse")]
    [RequestSizeLimit(MaxFileSize)]
    public async Task<ActionResult<ParseResponse>> Parse(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("A PDF file is required.");
        }

        if (!string.Equals(
                Path.GetExtension(file.FileName),
                ".pdf",
                StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Only PDF files are supported.");
        }

        var tempFilePath = Path.Combine(
            Path.GetTempPath(),
            $"{Guid.NewGuid():N}.pdf");

        try
        {
            await using (var stream = System.IO.File.Create(tempFilePath))
            {
                await file.CopyToAsync(
                    stream,
                    cancellationToken);
            }

            var result = await _importService.ImportAsync(
                tempFilePath,
                file.FileName,
                cancellationToken);

            var response = new ParseResponse
            {
                Success = result.IsValid,
                FileName = file.FileName,
                DutyDate = result.DutyDate,
                AreaGroup = result.Area,
                DaytimeCount = result.DaytimeCount,
                OvernightCount = result.OvernightCount,
                AfterMidnightCount = result.AfterMidnightCount,
                Total = result.Pharmacies.Count,
                Warnings = result.Warnings.ToList(),
                Pharmacies = result.Pharmacies
                    .Select(x => new PharmacyResponse
                    {
                        DutyDate = x.DutyDate,
                        Area = x.Area,
                        Name = x.Name,
                        Address = x.Address,
                        Phone = x.Phone,
                        DutyType = x.DutyType.ToString()
                    })
                    .ToList()
            };

            return Ok(response);
        }
        catch (FormatException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (OperationCanceledException)
        {
            return BadRequest("The import was cancelled.");
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    error = "An error occurred while importing the PDF.",
                    details = ex.Message
                });
        }
        finally
        {
            if (System.IO.File.Exists(tempFilePath))
            {
                System.IO.File.Delete(tempFilePath);
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
}