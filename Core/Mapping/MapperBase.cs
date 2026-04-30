using AutoMapper;

namespace ManageLife.Core
{
	public static class MapperBase
	{
		private static IMapper? _mapper;

		public static void Configure(IMapper mapper)
		{
			_mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
		}

		private static void EnsureConfigured()
		{
			if (_mapper == null)
				throw new InvalidOperationException("MapperBase is not configured. Call MapperBase.Configure() first.");
		}

		public static TDestination MapTo<TDestination>(this object source)
		{
			EnsureConfigured();
			return source == null ? default! : _mapper!.Map<TDestination>(source);
		}

		public static TDestination MapTo<TSource, TDestination>(this TSource source)
		{
			EnsureConfigured();
			return source == null ? default! : _mapper!.Map<TSource, TDestination>(source);
		}

        public static void MapTo<TSource, TDestination>(this TSource source, TDestination destination)
        {
            EnsureConfigured();
            if (source != null && destination != null)
                _mapper!.Map(source, destination);
        }

        public static List<TDestination> MapToList<TDestination>(this IEnumerable<object> source)
		{
			EnsureConfigured();
			return source == null ? new List<TDestination>() : _mapper!.Map<List<TDestination>>(source);
		}

		public static List<TDestination> MapToList<TSource, TDestination>(this IEnumerable<TSource> source)
		{
			EnsureConfigured();
			return source == null ? new List<TDestination>() : _mapper!.Map<List<TDestination>>(source);
		}
	}
}
