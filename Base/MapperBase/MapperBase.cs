using AutoMapper;

namespace ManageLife.Base
{
    public static class MapperBase
    {
        private static IMapper _mapper;
        public static void Configure(IMapper mapper)
        {
            _mapper = mapper;
        }

        public static TDestination MapTo<TDestination>(this object source)
        {
            if (source == null) return default;
            return _mapper.Map<TDestination>(source);
        }

        public static List<TDestination> MapToList<TDestination>(this IEnumerable<object> source)
        {
            if (source == null) return null;
            return _mapper.Map<List<TDestination>>(source);
        }
    }
}
