using Aspose.Cells;
using Microsoft.Extensions.Logging;
using RR.AI_Chat.Dto;
using System.Text;

namespace RR.AI_Chat.Service
{
    public interface IExcelService
    {
        List<DocumentExtractorDto> ExtractCsvText(byte[] bytes);
    }

    public class ExcelService(ILogger<ExcelService> logger) : IExcelService
    {
        private readonly ILogger<ExcelService> _logger = logger;

        public List<DocumentExtractorDto> ExtractCsvText(byte[] bytes)
        {
            ArgumentNullException.ThrowIfNull(bytes);

            using var memoryStream = new MemoryStream(bytes);
            var workbook = new Workbook(memoryStream);
            _logger.LogInformation("Starting text extraction from Excel document {Name}.", workbook.FileName);

            var worksheets = workbook.Worksheets;
            List<DocumentExtractorDto> dto = [];
            foreach (var worksheet in worksheets)
            {
                _logger.LogInformation("Processing worksheet: {WorksheetName}", worksheet.Name);
                var cells = worksheet.Cells;
                StringBuilder sb = new();
                for (int row = 0; row <= cells.MaxDataRow; row++)
                {
                    for (int col = 0; col <= cells.MaxDataColumn; col++)
                    {
                        var cell = cells[row, col];
                        if (col > 0)
                        {
                            sb.Append(",");
                        }
                        sb.Append(cell.StringValue);
                    }
                    sb.AppendLine();
                }

                dto.Add(new()
                {
                    PageNumber = worksheet.Index + 1,
                    PageText = sb.ToString()
                });
            }

            _logger.LogInformation("Completed text extraction from Excel document {Name}.", workbook.FileName);
            return dto;
        }
    }
}
