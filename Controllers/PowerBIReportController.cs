using Microsoft.AspNetCore.Mvc;
using PowerBIExport.API.Services;

namespace PowerBIExport.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PowerBIReportController : Controller
    {
        private PowerBIService powerBiService;
        public PowerBIReportController(PowerBIService powerBiService) { 
            this.powerBiService = powerBiService;
        }   
        [HttpPost]
        [Route("ExportToPNG")]
        public async Task<IActionResult> ExportReportToPNG([FromQuery] string WorkSpaceId, [FromQuery] string ReportId)
        {
            // Access and process the input values here
            var exportedReport = await powerBiService.ExportReportInPNG(WorkSpaceId, ReportId);
            exportedReport.ReportStream.Flush();            
            var file = new FileStreamResult(exportedReport.ReportStream, "image/png");
            file.FileDownloadName = exportedReport.ReportName + exportedReport.ResourceFileExtension;
            return file;            
        }
    }
}
