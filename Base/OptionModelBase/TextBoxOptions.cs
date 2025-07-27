namespace ManageLife.Base
{
	public class TextBoxOptions
	{
		public string Name { get; set; }
		public string? Id { get; set; }
		public string? Placeholder { get; set; }
		public string? Value { get; set; }
		public string CssClass { get; set; } = "form-control";

		public string? Lable { get; set; }

		public TextBoxOptions()
		{
			Name = string.Empty;
			Id = string.Empty;
			Placeholder = string.Empty;
			Value = string.Empty;
		}

		public TextBoxOptions(string name)
		{
			Name = name;
		}

		public TextBoxOptions(string name, string? id)
		{
			Name = name;
			if (string.IsNullOrEmpty(id))
			{
				Id = id;
			}
		}

		public TextBoxOptions(string name, string id, string? value)
		{
			Name = name;
			Id = id;
			if (string.IsNullOrEmpty(value))
			{
				Value = value;
			}
		}
	}
}
