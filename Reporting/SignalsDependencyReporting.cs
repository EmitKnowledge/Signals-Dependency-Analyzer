using OfficeOpenXml;
using Signals.Dependency.Analyzer.Atoms;

namespace Signals.Dependency.Analyzer.Reporting
{
    public class SignalsDependencyReporting
    {
        static SignalsDependencyReporting()
        {
            ExcelPackage.License.SetNonCommercialOrganization("EMIT KNOWLEDGE LLC.");
        }

        /// <summary>
        /// Exports the dependencies of the SignalsProcess as an Excel file, showing dependencies from leafs to top.
        /// </summary>
        /// <summary>
        /// Exports the dependencies of the SignalsProcess as an Excel file, showing dependencies from leafs to top.
        /// </summary>
        public string Export(List<SignalsProcess> processes)
        {
            // prepare the Excel package and worksheet
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Process Dependencies");

            // set column headers
            worksheet.Cells[1, 1].Value = "Signals Process Name";
            worksheet.Cells[1, 2].Value = "Type";
            worksheet.Cells[1, 3].Value = "File Path";
            worksheet.Cells[1, 4].Value = "Line";

            // make headers bold and set autofilter
            worksheet.Cells[1, 1, 1, 4].Style.Font.Bold = true;
            worksheet.Cells[1, 1, 1, 4].AutoFilter = true;
            
            // start adding data from row 2 onwards
            int row = 2;

            foreach (var process in processes)
            {
                // flatten the process hierarchy from leaf to root (reverse order of GetAllProcesses)
                var allProcesses = process.FlattenDesc();

                foreach (var p in allProcesses)
                {
                    worksheet.Cells[row, 1].Value = new string('—', p.Depth) + p.Name;
                    worksheet.Cells[row, 2].Value = p.Type?.Value;
                    worksheet.Cells[row, 3].Value = p.FilePath;
                    worksheet.Cells[row, 4].Value = p.Line;

                    // highlight the current process in bold if it matches the root process
                    if (string.Compare(p.FilePath, process.FilePath) == 0)
                    {
                        worksheet.Cells[row, 1].Style.Font.Bold = true;
                    }

                    row++;
                }

                row++;
            }

            // autofit columns
            worksheet.Cells.AutoFitColumns();

            // save the package to the file
            var fileName = $"signals_process_dependencies_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
            File.WriteAllBytes(filePath, package.GetAsByteArray());
            return filePath;
        }
    }
}
