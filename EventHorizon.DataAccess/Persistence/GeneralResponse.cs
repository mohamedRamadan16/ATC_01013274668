using System.Diagnostics.Eventing.Reader;
using System.Net;

namespace EventHorizon.DataAccess.Persistence;

public class GeneralResponse
{
    public HttpStatusCode statusCode { get; set; }
    public bool isSuccess { get; set; } = true;
    public List<string>? Errors { get; set; }
    public object? Result { get; set; } = null!;
}
