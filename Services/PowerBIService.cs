using Microsoft.Identity.Web;
using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Rest;

namespace PowerBIExport.API.Services
{
    public class ExportedReport
    {
        public string ReportName { get; set; }
        public string ResourceFileExtension { get; set; }
        public Stream ReportStream { get; set; }
    }
    public class PowerBIService
    {
        public const string powerbiApiDefaultScope = "https://analysis.windows.net/powerbi/api/.default";
        private string urlPowerBiServiceApiRoot;
        private ITokenAcquisition tokenAcquisition;
        

        public PowerBIService(IConfiguration configuration, ITokenAcquisition tokenAcquisition) {
            this.urlPowerBiServiceApiRoot = configuration["PowerBi:ServiceRootUrl"];
            this.tokenAcquisition = tokenAcquisition;
            //this.workspaceId = new Guid(WorkspaceId);
            //this.reportId = new Guid(ReportId);
        }

        public async Task<ExportedReport> ExportReportInPNG(string WorkspaceId, string ReportId)
        {
            PowerBIClient pbiClient = GetPowerBiClient();
            var exportRequest = new ExportReportRequest
            {
                Format = FileFormat.PNG ,
                PowerBIReportConfiguration = new PowerBIReportExportConfiguration()
            };
            Guid pbi_workSpaceId = new Guid(WorkspaceId);
            Guid pbi_reportId = new Guid(ReportId);
            Export export = await pbiClient.Reports.ExportToFileInGroupAsync(pbi_workSpaceId, pbi_reportId, exportRequest);

            string exportId = export.Id;
            do
            {
                System.Threading.Thread.Sleep(3000);
                export = pbiClient.Reports.GetExportToFileStatusInGroup(pbi_workSpaceId, pbi_reportId, exportId);
            } while (export.Status != ExportState.Succeeded && export.Status != ExportState.Failed);

            if (export.Status == ExportState.Failed)
            {
                throw new Exception($"Export Failed");
            }

            if (export.Status == ExportState.Succeeded)
            {
                return new ExportedReport
                {
                    ReportName = export.ReportName,
                    ResourceFileExtension = export.ResourceFileExtension,
                    ReportStream = pbiClient.Reports.GetFileOfExportToFileInGroup(pbi_workSpaceId, pbi_reportId, exportId)
                };
            }

            return null;
        }
        private PowerBIClient GetPowerBiClient()
        {
            string accessToken = this.tokenAcquisition.GetAccessTokenForAppAsync(powerbiApiDefaultScope).Result;
            var tokenCredentials = new TokenCredentials(accessToken, "Bearer");
            return new PowerBIClient(new Uri(urlPowerBiServiceApiRoot), tokenCredentials);
        }
    }
}
