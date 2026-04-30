using ManageLife.Core;

namespace ManageLife.ViewModels
{
    public class TranslationViewModel
    {
        public TranslationViewModel()
        {
            Languages = new();
        }

        public List<KeyValueModel> Languages { get; set; }
    }
}
