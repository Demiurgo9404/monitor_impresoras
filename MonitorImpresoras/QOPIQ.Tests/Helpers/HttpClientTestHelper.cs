using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;

namespace QOPIQ.Tests.Helpers
{
    public static class HttpClientTestHelper
    {
        public static Mock<HttpMessageHandler> CreateMockHttpMessageHandler(
            HttpStatusCode statusCode = HttpStatusCode.OK,
            string responseContent = "{}",
            string expectedUrl = null,
            HttpMethod expectedMethod = null,
            string expectedContent = null)
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var httpMethod = expectedMethod ?? HttpMethod.Get;

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        (expectedUrl == null || req.RequestUri.ToString().Contains(expectedUrl)) &&
                        (expectedMethod == null || req.Method == httpMethod) &&
                        (expectedContent == null || (req.Content != null && 
                         req.Content.ReadAsStringAsync().Result.Contains(expectedContent)))
                    ),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent(responseContent)
                })
                .Verifiable();

            return handlerMock;
        }

        public static IHttpClientFactory CreateMockHttpClientFactory(Mock<HttpMessageHandler> handlerMock)
        {
            var httpClient = new HttpClient(handlerMock.Object);
            var mockFactory = new Mock<IHttpClientFactory>();
            mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);
            return mockFactory.Object;
        }
    }
}
