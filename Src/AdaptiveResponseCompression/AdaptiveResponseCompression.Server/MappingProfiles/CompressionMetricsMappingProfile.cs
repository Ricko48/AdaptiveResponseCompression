using AdaptiveResponseCompression.Server.Dtos;
using AdaptiveResponseCompression.Server.Models;
using AutoMapper;

namespace AdaptiveResponseCompression.Server.MappingProfiles;

internal class CompressionMetricsMappingProfile : Profile
{
    public CompressionMetricsMappingProfile()
    {
        CreateMap<MetricsCreateUpdateDto, CompressionMetrics>();
    }
}
