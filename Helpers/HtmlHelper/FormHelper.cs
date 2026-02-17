using ManageLife.Base;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManageLife.Helpers
{
	public static class FormHelper
	{
		public static IHtmlContent Form<T>(this IHtmlHelper htmlHelper, FormViewOptions? options = null)
		{
			var formContainer = new TagBuilder("div");
			formContainer.AddCssClass("form-container");

			if (string.IsNullOrWhiteSpace(options?.Id))
			{
				formContainer.Attributes.Add("id", options?.Id);
			}

			var formBuilder = new TagBuilder("form");

			return formContainer;
		}
	}
}
