namespace ManageLife.Base
{
    public class DataSourceOptions<T>
    {
        public IEnumerable<T>? Data { get; private set; }

        public Func<T, Task<T>>? OnInsert { get; private set; }
        public Func<T, Task<T>>? OnUpdate { get; private set; }
        public Func<T, Task<bool>>? OnDelete { get; private set; }

        public string? LoadUrl { get; private set; }
        public string? InsertUrl { get; private set; }
        public string? UpdateUrl { get; private set; }
        public string? DeleteUrl { get; private set; }

        public DataSourceOptions<T> LoadData(IEnumerable<T> data)
        {
            Data = data;
            return this;
        }

        public DataSourceOptions<T> LoadData(string url)
        {
            if (url.IsEmpty())
                throw new ArgumentNullException(nameof(url), "LoadUrl không được để trống.");

            LoadUrl = url;
            return this;
        }

        public DataSourceOptions<T> Insert(string url)
        {
            if (url.IsEmpty())
                throw new ArgumentNullException(nameof(url), "InsertUrl không được để trống.");

            InsertUrl = url;
            return this;
        }

        public DataSourceOptions<T> Update(string url)
        {
            if (url.IsEmpty())
                throw new ArgumentNullException(nameof(url), "UpdateUrl không được để trống.");

            UpdateUrl = url;
            return this;
        }

        public DataSourceOptions<T> Delete(string url)
        {
            if (url.IsEmpty())
                throw new ArgumentNullException(nameof(url), "DeleteUrl không được để trống.");

            DeleteUrl = url;
            return this;
        }

        public DataSourceOptions<T> Insert(Func<T, Task<T>> insert)
        {
            OnInsert = insert;
            return this;
        }

        public DataSourceOptions<T> Update(Func<T, Task<T>> update)
        {
            OnUpdate = update;
            return this;
        }

        public DataSourceOptions<T> Delete(Func<T, Task<bool>> delete)
        {
            OnDelete = delete;
            return this;
        }
    }
}
