using AdaptiveResponseCompression.Server.Models;
using Microsoft.AspNetCore.Http;

namespace AdaptiveResponseCompression.Server.Services.Interfaces;

internal interface ICompressionService
{
    Task CompressBodyAsync(HttpContext context, ResponseCompressionModel compressionModel);
}