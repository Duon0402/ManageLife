using AutoMapper;

namespace ManageLife.Core
{
    public static class MapperBase
    {
        private static IMapper _mapper = null!;

        internal static void Configure(IMapper mapper) => _mapper = mapper;

        public static TDestination MapTo<TDestination>(this object source)
            => _mapper.Map<TDestination>(source);

        public static TDestination MapTo<TSource, TDestination>(this TSource source)
            => _mapper.Map<TSource, TDestination>(source);

        public static void MapTo<TSource, TDestination>(this TSource source, TDestination destination)
            => _mapper.Map(source, destination);

        public static List<TDestination> MapToList<TDestination>(this IEnumerable<object> source)
            => _mapper.Map<List<TDestination>>(source);

        public static List<TDestination> MapToList<TSource, TDestination>(this IEnumerable<TSource> source)
            => _mapper.Map<List<TDestination>>(source);
    }
}
