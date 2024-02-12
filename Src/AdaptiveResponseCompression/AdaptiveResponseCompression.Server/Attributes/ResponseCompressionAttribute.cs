using AdaptiveResponseCompression.Server.Enums;

namespace AdaptiveResponseCompression.Server.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class ResponseCompressionAttribute : Attribute
{
    public ResponseCompressionMethod Method { get; set; }

    public ResponseCompressionAttribute(ResponseCompressionMethod method)
    {
        Method = method;
    }
}
