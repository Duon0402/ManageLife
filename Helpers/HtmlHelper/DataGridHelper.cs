using ManageLife.Base;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManageLife.Helpers
{
	public static class DataGridHelper
	{
		public static IHtmlContent DataGrid<T>(this IHtmlHelper htmlHelper, IEnumerable<T> items, IEnumerable<string> columnNames, DataGridOptions? options = null)
		{
			var gridBuilder = new TagBuilder("div");
			gridBuilder.AddCssClass("data-grid-container");

			if (!string.IsNullOrWhiteSpace(options?.Id))
			{
				gridBuilder.Attributes.Add("id", options.Id);
			}

			#region Toolbar
			// TODO: Thêm searching, filtering, sorting
			var toolBarBuilder = new TagBuilder("div");
			toolBarBuilder.AddCssClass("d-flex justify-content-end mb-3");

			// Insert Button
			if (options?.AllowInsert == true)
			{
				var insertButton = new TagBuilder("button");
				insertButton.AddCssClass("btn btn-primary me-2");
				insertButton.InnerHtml.Append("Insert");
				// TODO: Thêm hàm xử lý Insert có thể tái sử dụng
				insertButton.Attributes.Add("onclick", "insertFunction()");
				toolBarBuilder.InnerHtml.AppendHtml(insertButton);
			}

			gridBuilder.InnerHtml.AppendHtml(toolBarBuilder);
			#endregion

			#region Table
			var tableContainer = new TagBuilder("div");
			tableContainer.AddCssClass("table-responsive");

			var tableBuilder = new TagBuilder("table");
			tableBuilder.AddCssClass("table table-bordered table-hover");

			// Header
			var theadBuilder = new TagBuilder("thead");
			theadBuilder.AddCssClass("table-secondary");

			var headerRowBuilder = new TagBuilder("tr");
			foreach (var columnName in columnNames)
			{
				var thBuilder = new TagBuilder("th");
				thBuilder.InnerHtml.Append(columnName);
				headerRowBuilder.InnerHtml.AppendHtml(thBuilder);
			}
			theadBuilder.InnerHtml.AppendHtml(headerRowBuilder);
			tableBuilder.InnerHtml.AppendHtml(theadBuilder);

			// Body
			var tbodyBuilder = new TagBuilder("tbody");

			// TODO: Thêm lựa chọn đặt tên cho cột
			foreach (var item in items)
			{
				var rowBuilder = new TagBuilder("tr");

				foreach (var columnName in columnNames)
				{
					var tdBuilder = new TagBuilder("td");
					var property = typeof(T).GetProperty(columnName);
					if (property != null)
					{
						var value = property.GetValue(item)?.ToString() ?? string.Empty;
						tdBuilder.InnerHtml.Append(value);
					}
					rowBuilder.InnerHtml.AppendHtml(tdBuilder);
				}

				tbodyBuilder.InnerHtml.AppendHtml(rowBuilder);
			}

			tableBuilder.InnerHtml.AppendHtml(tbodyBuilder);

			tableContainer.InnerHtml.AppendHtml(tableBuilder);
			#endregion

			gridBuilder.InnerHtml.AppendHtml(tableContainer);
			// TODO: Thêm paging
			return gridBuilder;
		}
	}
}
