using OfficeOpenXml;

namespace ManageLife.Helpers
{
    public static class ExcelHelper
    {
        //TODO: Xây dựng ExcelHelper: Import, Export, ...
        public static List<T> Import<T>(Stream excelStream, int sheetIndex = 0, bool hasHeader = true) where T : new()
        {
            using var package = new ExcelPackage(excelStream);
            var worksheet = package.Workbook.Worksheets[sheetIndex];
            return ImportFromWorksheet<T>(worksheet, hasHeader);
        }

        public static List<T> Import<T>(Stream excelStream, string sheetName, bool hasHeader = true) where T : new()
        {
            using var package = new ExcelPackage(excelStream);
            var worksheet = package.Workbook.Worksheets[sheetName];
            return ImportFromWorksheet<T>(worksheet, hasHeader);
        }

        private static List<T> ImportFromWorksheet<T>(ExcelWorksheet worksheet, bool hasHeader) where T : new()
        {
            var list = new List<T>();

            return list;
        }
    }
}
