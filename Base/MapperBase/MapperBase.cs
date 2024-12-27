
using AutoMapper;

namespace ManageLife.Base
{
    public class MapperBase : IMappeBase
    {
        private readonly IMapper _mapper;

        public MapperBase(Action<IMapperConfigurationExpression> config)
        {
            var mapperConfig = new MapperConfiguration(config);
            _mapper = mapperConfig.CreateMapper();
        }

        public TDestination MapTo<TDestination>(object source)
        {
            return _mapper.Map<object, TDestination>(source);
        }

        public TDestination MapTo<TSource, TDestination>(TSource source)
        {
            return _mapper.Map<TSource, TDestination>(source);
        }

        public IEnumerable<TDestination> MapToList<TDestination>(IEnumerable<object> source)
        {
            return _mapper.Map<IEnumerable<object>, IEnumerable<TDestination>>(source);
        }

        public IEnumerable<TDestination> MapToList<TSource, TDestination>(IEnumerable<TSource> source)
        {
            return _mapper.Map<IEnumerable<TSource>, IEnumerable<TDestination>>(source);
        }
    }
}
