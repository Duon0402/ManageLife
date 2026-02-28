namespace ManageLife.Base
{
    public class KeyValueModel
    {
        public KeyValueModel()
        {

        }

        public KeyValueModel(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; set; } = null!;
        public string Value { get; set; } = null!;
    }

    public class KeyValueModel<TKey, TValue>
    {
        public KeyValueModel()
        {

        }

        public KeyValueModel(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }

        public TKey Key { get; set; } = default!;
        public TValue Value { get; set; } = default!;
    }
}
