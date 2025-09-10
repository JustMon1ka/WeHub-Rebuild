using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReportService.Services;
using ReportService.DTOs;

namespace ReportService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] 
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateReport([FromBody] CreateReportDto createReportDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _reportService.CreateReportAsync(createReportDto);
                return CreatedAtAction(nameof(CreateReport), new { reportId = result.ReportId }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")] 
        public async Task<IActionResult> GetReports()
        {
            var reports = await _reportService.GetReportsAsync();
            return Ok(reports);
        }

        [HttpPut("{reportId}")]
        [Authorize(Roles = "Admin")] 
        public async Task<IActionResult> UpdateReport(int reportId, [FromBody] UpdateReportDto updateReportDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _reportService.UpdateReportAsync(reportId, updateReportDto);
            if (result == null)
            {
                return NotFound("举报记录不存在");
            }

            return Ok(result);
        }
    }
}