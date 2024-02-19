using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using NSubstitute;

namespace UnAd.Functions.Tests.Integration;

internal static class TestHelpers {
    public static HttpRequestData CreateRequest(Stream bodyStream, Stream responseStream, HttpHeadersCollection? headers) {
        var context = Substitute.For<FunctionContext>();
        var request = Substitute.For<HttpRequestData>(context);
        request.Body.Returns(bodyStream);
        request.Headers.Returns(headers ?? []);
        var response = Substitute.For<HttpResponseData>(context);
        response.Headers.Returns([]);
        response.StatusCode.Returns(System.Net.HttpStatusCode.OK);
        response.Body.Returns(responseStream);
        request.CreateResponse().Returns(response);
        return request;
    }
}