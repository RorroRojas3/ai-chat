using Aspose.Cells;
using Microsoft.Extensions.Logging;
using RR.AI_Chat.Dto;
using RR.AI_Chat.Service.Common.Interface;
using System.Text;

namespace RR.AI_Chat.Service
{
    public class ExcelService(ILogger<ExcelService> logger) : IFileService
    {
        private readonly ILogger<ExcelService> _logger = logger;

        public List<DocumentExtractorDto> ExtractText(byte[] bytes, string fileName)
        {
            ArgumentNullException.ThrowIfNull(bytes);
            ArgumentNullException.ThrowIfNull(fileName);

            using var memoryStream = new MemoryStream(bytes);
            var workbook = new Workbook(memoryStream);
            _logger.LogInformation("Starting text extraction from Excel document {FileName}.", fileName);

            var worksheets = workbook.Worksheets;
            List<DocumentExtractorDto> dto = [];
            foreach (var worksheet in worksheets)
            {
                _logger.LogInformation("Processing worksheet: {FileName}", fileName);
                var cells = worksheet.Cells;
                StringBuilder sb = new();
                for (int row = 0; row <= cells.MaxDataRow; row++)
                {
                    for (int col = 0; col <= cells.MaxDataColumn; col++)
                    {
                        var cell = cells[row, col];
                        if (col > 0)
                        {
                            sb.Append(' ');
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

            _logger.LogInformation("Completed text extraction from Excel document {FileName}.", fileName);
            return dto;
        }
    }
}
