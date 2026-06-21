using ManageLife.Core;
using OfficeOpenXml;
using System.Reflection;

namespace ManageLife.Helpers
{
	public static class ExcelHelper
	{
		public static List<T> Import<T>(
			Stream excelStream,
			int sheetIndex = 0,
			bool hasHeader = true,
			int? startRowHeader = null,
			int? startRowData = null) where T : new()
		{
			using var package = new ExcelPackage(excelStream);
			var worksheet = package.Workbook.Worksheets[sheetIndex];
			return ImportFromWorksheet<T>(worksheet, hasHeader, startRowHeader, startRowData);
		}

		public static List<T> Import<T>(
			Stream excelStream,
			string sheetName,
			bool hasHeader = true,
			int? startRowHeader = null,
			int? startRowData = null) where T : new()
		{
			using var package = new ExcelPackage(excelStream);
			var worksheet = package.Workbook.Worksheets[sheetName];
			return ImportFromWorksheet<T>(worksheet, hasHeader, startRowHeader, startRowData);
		}

		private static List<T> ImportFromWorksheet<T>(
			ExcelWorksheet worksheet,
			bool hasHeader,
			int? startRowHeader,
			int? startRowData) where T : new()
		{
			var data = new List<T>();

			if (worksheet == null || worksheet.Dimension == null) return data;

			var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
			var colMapping = new Dictionary<int, PropertyInfo>();

			int headerRow = hasHeader ? (startRowHeader ?? worksheet.Dimension.Start.Row) : -1;
			int dataRow = startRowData ?? (hasHeader ? headerRow + 1 : worksheet.Dimension.Start.Row);

			if (hasHeader)
			{
				for (int col = worksheet.Dimension.Start.Column; col <= worksheet.Dimension.End.Column; col++)
				{
					var header = worksheet.Cells[headerRow, col].Text?.Trim();
					if (string.IsNullOrEmpty(header)) continue;

					var prop = properties.FirstOrDefault(p =>
						string.Equals(p.Name, header, StringComparison.OrdinalIgnoreCase));
					if (prop != null)
					{
						colMapping[col] = prop;
					}
				}
			}
			else
			{
				for (int col = worksheet.Dimension.Start.Column; col <= worksheet.Dimension.End.Column; col++)
				{
					if (col - 1 < properties.Length)
						colMapping[col] = properties[col - 1];
				}
			}

			for (int row = dataRow; row <= worksheet.Dimension.End.Row; row++)
			{
				var obj = new T();
				foreach (var map in colMapping)
				{
					var cellValue = worksheet.Cells[row, map.Key].Text;
					if (string.IsNullOrWhiteSpace(cellValue)) continue;

					try
					{
						object safeValue = Convert.ChangeType(cellValue, map.Value.PropertyType);
						map.Value.SetValue(obj, safeValue);
					}
					catch (Exception ex)
					{
						System.Diagnostics.Debug.WriteLine($"[ExcelHelper] Cannot convert '{cellValue}' to {map.Value.PropertyType.Name} (row {row}, col {map.Key}): {ex.Message}");
					}
				}
				data.Add(obj);
			}

			return data;
		}

		/// <summary>
		/// Đọc Excel thành list row, mỗi row là dictionary {header: cellValue}.
		/// Dùng cho các file có cột động (không biết trước số cột).
		/// </summary>
		public static List<Dictionary<string, string>> ImportAsRows(
			Stream excelStream,
			int sheetIndex = 0,
			int? startRowHeader = null,
			int? startRowData = null)
		{
			using var package = new ExcelPackage(excelStream);
			var ws = package.Workbook.Worksheets[sheetIndex];
			return ImportAsRowsFromWorksheet(ws, startRowHeader, startRowData);
		}

		public static List<Dictionary<string, string>> ImportAsRows(
			Stream excelStream,
			string sheetName,
			int? startRowHeader = null,
			int? startRowData = null)
		{
			using var package = new ExcelPackage(excelStream);
			var ws = package.Workbook.Worksheets[sheetName];
			return ImportAsRowsFromWorksheet(ws, startRowHeader, startRowData);
		}

		private static List<Dictionary<string, string>> ImportAsRowsFromWorksheet(
			ExcelWorksheet ws,
			int? startRowHeader,
			int? startRowData)
		{
			var result = new List<Dictionary<string, string>>();
			if (ws?.Dimension == null) return result;

			int headerRow = startRowHeader ?? ws.Dimension.Start.Row;
			int dataRow = startRowData ?? headerRow + 1;
			int lastCol = ws.Dimension.End.Column;
			int lastRow = ws.Dimension.End.Row;

			var headers = new Dictionary<int, string>();
			for (int col = ws.Dimension.Start.Column; col <= lastCol; col++)
			{
				var header = ws.Cells[headerRow, col].Text?.Trim();
				if (header.IsNotEmpty())
					headers[col] = header;
			}

			for (int row = dataRow; row <= lastRow; row++)
			{
				var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
				foreach (var (col, header) in headers)
				{
					var value = ws.Cells[row, col].Text?.Trim() ?? string.Empty;
					dict[header] = value;
				}
				result.Add(dict);
			}

			return result;
		}
	}
}
