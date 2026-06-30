using AutoMapper;
using AutoMapper.QueryableExtensions;
//using CQS.Services.Contract;
using CQSAirborne.Services.Contract;
using System.Linq;

namespace CQS.Services.Implementation
{
    public class AutomapperDataMapper : IDataMapper
    {
        private readonly IMapper _mapper;

        public AutomapperDataMapper(IMapper mapper)
        {
            _mapper = mapper;
        }

        public TDestination Map<TSource, TDestination>(TSource source)
            where TSource : class
            where TDestination : class
        {
            return _mapper.Map<TSource, TDestination>(source);
        }

        public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
            where TSource : class
            where TDestination : class
        {
            return _mapper.Map(source, destination);
        }

        public IQueryable<TDestination> Project<TSource, TDestination>(IQueryable<TSource> sources)
            where TSource : class
            where TDestination : class
        {
            return sources.ProjectTo<TDestination>(_mapper.ConfigurationProvider);
        }
    }

}
