namespace ManageLife.Base
{
    public class FormViewModel
    {
        public FormViewModel()
        {
            FormModels = new List<FormModel<object>>();
        }

        public List<FormModel<object>> FormModels { get; set; }
    }

    public class FormModel<T>
    {
        public string FieldName { get; set; }
        public T? FieldValue { get; set; }
        public string FieldType { get; set; }
    }

    public class FormOptions
    {

    }
}