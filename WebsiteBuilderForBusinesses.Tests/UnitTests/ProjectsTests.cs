using WebsiteBuilderForBusinesses.Core.Models;

namespace WebsiteBuilderForBusinesses.Tests.UnitTests
{
    public class ProjectsTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Create_WithValidData_ReturnsSuccess()
        {
            var id = Guid.NewGuid();
            var name = "Test Project";
            var dateOpen = DateTime.UtcNow;
            var textHtml = "<html><body>Test</body></html>";

            var result = Projects.Create(id, name, dateOpen, textHtml);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Value, Is.Not.Null);
            });
        }

        [Test]
        public void Create_WithValidData_PropertiesAreSetCorrectly()
        {
            var id = Guid.NewGuid();
            var name = "My Project";
            var dateOpen = new DateTime(2024, 1, 15, 10, 30, 0);
            var textHtml = "<div>Content</div>";

            var result = Projects.Create(id, name, dateOpen, textHtml);

            Assert.Multiple(() =>
            {
                Assert.That(result.Value.Id, Is.EqualTo(id));
                Assert.That(result.Value.Name, Is.EqualTo(name));
                Assert.That(result.Value.DateOpen, Is.EqualTo(dateOpen));
                Assert.That(result.Value.TextHtml, Is.EqualTo(textHtml));
            });
        }

        [Test]
        public void Create_WithEmptyTextHtml_ReturnsSuccess()
        {
            var id = Guid.NewGuid();
            var name = "Project";
            var dateOpen = DateTime.UtcNow;
            var textHtml = string.Empty;

            var result = Projects.Create(id, name, dateOpen, textHtml);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Value.TextHtml, Is.EqualTo(string.Empty));
            });
        }

        [Test]
        public void Create_WithEmptyGuid_ReturnsFailure()
        {
            var id = Guid.Empty;
            var name = "Project";
            var dateOpen = DateTime.UtcNow;
            var textHtml = "<html></html>";

            var result = Projects.Create(id, name, dateOpen, textHtml);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Error, Is.EqualTo("Поле Id не должно быть пустым"));
            });
        }

        [Test]
        public void Create_WithEmptyGuid_ValueIsNull()
        {
            var id = Guid.Empty;
            var name = "Project";
            var dateOpen = DateTime.UtcNow;
            var textHtml = "<html></html>";

            var result = Projects.Create(id, name, dateOpen, textHtml);

            Assert.That(result.Value, Is.Null);
        }

        [Test]
        public void Create_WithEmptyName_ReturnsFailure()
        {
            var id = Guid.NewGuid();
            var name = string.Empty;
            var dateOpen = DateTime.UtcNow;
            var textHtml = "<html></html>";

            var result = Projects.Create(id, name, dateOpen, textHtml);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Error, Is.EqualTo("Название не должно быть пустым"));
            });
        }
    }
}
