namespace ManageLife.Base
{
    public interface IMappeBase
    {
        TDestination MapTo<TDestination>(object source);
        TDestination MapTo<TSource, TDestination>(TSource source);
        public IEnumerable<TDestination> MapToList<TDestination>(IEnumerable<object> source);
        IEnumerable<TDestination> MapToList<TSource, TDestination>(IEnumerable<TSource> source);
    }
}
