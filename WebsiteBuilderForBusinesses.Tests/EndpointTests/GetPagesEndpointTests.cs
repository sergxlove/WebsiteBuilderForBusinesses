using System.Net;
using WebsiteBuilderForBusinesses.API;

namespace WebsiteBuilderForBusinesses.Tests.EndpointTests
{
    public class GetPagesEndpointTests
    {
        private CustomWebApplicationFactory _factory = null!;
        private HttpClient _client = null!;

        [SetUp]
        public void SetUp()
        {
            _factory = new CustomWebApplicationFactory();
            _client = _factory.CreateClient();
        }

        [TearDown]
        public async Task TearDown()
        {
            _client?.Dispose();
            if (_factory != null)
            {
                await _factory.DisposeAsync();
            }
        }

        [Test]
        public async Task GetRoot_ReturnsLoginPageHtml()
        {
            var response = await _client.GetAsync("/");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("text/html"));
        }

        [Test]
        public async Task GetLoginPage_ReturnsLoginPageHtml()
        {
            var response = await _client.GetAsync("/page/login");

            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("text/html"));
            });
        }

        [Test]
        public async Task GetIndex_WithoutAuth_ReturnsUnauthorized()
        {
            var response = await _client.GetAsync("/index");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task GetRegPage_WithoutAuth_ReturnsUnauthorized()
        {
            var response = await _client.GetAsync("/page/reg");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task GetProjectsPage_WithoutAuth_ReturnsUnauthorized()
        {
            var response = await _client.GetAsync("/page/projects");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task GetAdminPage_WithoutAuth_ReturnsUnauthorized()
        {
            var response = await _client.GetAsync("/page/admin");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task GetError401_ReturnsError401Page()
        {
            var response = await _client.GetAsync("/error/401");

            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("text/html"));
            });
        }

        [Test]
        public async Task GetError403_ReturnsError403Page()
        {
            var response = await _client.GetAsync("/error/403");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task GetError404_ReturnsError404Page()
        {
            var response = await _client.GetAsync("/error/404");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }
    }
}
