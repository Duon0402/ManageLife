using ManageLife.Base;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManageLife.Helpers
{
	public static class TextBoxHelper
	{
		public static IHtmlContent TextBox(this IHtmlHelper htmlHelper, TextBoxOptions options)
		{
			var textBoxBuilder = new TagBuilder("div");

			// Label
			var labelBuilder = new TagBuilder("label");
			labelBuilder.Attributes.Add("for", options.Name);
			labelBuilder.InnerHtml.Append(options.Lable ?? options.Name);
			// TODO: Để tạm, xây dựng 1 LabelOptions riêng
			labelBuilder.AddCssClass("form-label");
			textBoxBuilder.InnerHtml.AppendHtml(labelBuilder);

			// Input
			var inputBuilder = new TagBuilder("input");
			inputBuilder.Attributes.Add("type", "text");
			inputBuilder.Attributes.Add("name", options.Name);

			if (!string.IsNullOrWhiteSpace(options.Id))
			{
				inputBuilder.Attributes.Add("id", options.Id);
			}

			if (!string.IsNullOrWhiteSpace(options.Placeholder))
			{
				inputBuilder.Attributes.Add("placeholder", options.Placeholder);
			}

			if (!string.IsNullOrWhiteSpace(options.Value))
			{
				inputBuilder.Attributes.Add("value", options.Value);
			}

			if (!string.IsNullOrWhiteSpace(options.CssClass))
			{
				inputBuilder.AddCssClass(options.CssClass);
			}

			textBoxBuilder.InnerHtml.AppendHtml(inputBuilder);

			return textBoxBuilder;
		}
	}
}
