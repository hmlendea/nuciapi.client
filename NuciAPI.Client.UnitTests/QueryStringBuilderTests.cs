using System;
using System.Collections.Generic;
using NUnit.Framework;
using NuciAPI.Client;

namespace NuciAPI.Client.UnitTests
{
    [TestFixture]
    public class QueryStringBuilderTests
    {
        [Test]
        public void GivenNullObject_WhenBuild_ThenReturnsEmptyDictionary()
            => Assert.That(QueryStringBuilder.Build(null), Is.Empty);

        [Test]
        public void GivenSimpleObject_WhenBuild_ThenReturnsCorrectKeyValuePairs()
        {
            object obj = new { Name = "test", Value = 42 };

            Dictionary<string, string> result = QueryStringBuilder.Build(obj);

            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result["name"], Is.EqualTo("test"));
            Assert.That(result["value"], Is.EqualTo("42"));
        }

        [Test]
        public void GivenObjectWithNullProperty_WhenBuild_ThenSkipsNullProperty()
        {
            object obj = new { Name = "test", Value = (int?)null };

            Dictionary<string, string> result = QueryStringBuilder.Build(obj);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result["name"], Is.EqualTo("test"));
        }

        [Test]
        public void GivenNestedObject_WhenBuild_ThenFlattensWithDotNotation()
        {
            object obj = new
            {
                User = new { Name = "john", Age = 30 },
                Active = true
            };

            Dictionary<string, string> result = QueryStringBuilder.Build(obj);

            Assert.That(result, Has.Count.EqualTo(3));
            Assert.That(result["user.name"], Is.EqualTo("john"));
            Assert.That(result["user.age"], Is.EqualTo("30"));
            Assert.That(result["active"], Is.EqualTo("True"));
        }

        [Test]
        public void GivenGuidProperty_WhenBuild_ThenConvertsToString()
        {
            Guid guid = Guid.NewGuid();
            object obj = new { Id = guid };

            Dictionary<string, string> result = QueryStringBuilder.Build(obj);

            Assert.That(result["id"], Is.EqualTo(guid.ToString()));
        }

        [Test]
        public void GivenDateTimeProperty_WhenBuild_ThenConvertsToString()
        {
            DateTime date = new(2024, 1, 15, 10, 30, 0);
            object obj = new { CreatedAt = date };

            Dictionary<string, string> result = QueryStringBuilder.Build(obj);

            Assert.That(result["createdAt"], Is.EqualTo(date.ToString()));
        }

        [Test]
        public void GivenDateTimeOffsetProperty_WhenBuild_ThenConvertsToString()
        {
            DateTimeOffset date = new(2024, 1, 15, 10, 30, 0, TimeSpan.Zero);
            object obj = new { CreatedAt = date };

            Dictionary<string, string> result = QueryStringBuilder.Build(obj);

            Assert.That(result["createdAt"], Is.EqualTo(date.ToString()));
        }

        [Test]
        public void GivenDecimalProperty_WhenBuild_ThenConvertsToString()
        {
            object obj = new { Price = 19.99m };

            Dictionary<string, string> result = QueryStringBuilder.Build(obj);

            Assert.That(result["price"], Is.EqualTo("19.99"));
        }

        [Test]
        public void GivenPropertyWithJsonPropertyNameAttribute_WhenBuild_ThenUsesAttributeName()
        {
            TestClassWithJsonProperty obj = new() { CustomName = "value" };

            Dictionary<string, string> result = QueryStringBuilder.Build(obj);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result["custom_name"], Is.EqualTo("value"));
        }

        [Test]
        public void GivenComplexObject_WhenBuild_ThenUsesCamelCaseForPropertyNames()
        {
            object obj = new { FirstName = "John", LastName = "Doe" };

            Dictionary<string, string> result = QueryStringBuilder.Build(obj);

            Assert.That(result["firstName"], Is.EqualTo("John"));
            Assert.That(result["lastName"], Is.EqualTo("Doe"));
        }

        [Test]
        public void GivenEmptyObject_WhenBuild_ThenReturnsEmptyDictionary()
        {
            object obj = new { };

            Dictionary<string, string> result = QueryStringBuilder.Build(obj);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GivenObjectWithBooleanProperty_WhenBuild_ThenConvertsToString()
        {
            object obj = new { IsActive = true, IsDeleted = false };

            Dictionary<string, string> result = QueryStringBuilder.Build(obj);

            Assert.That(result["isActive"], Is.EqualTo("True"));
            Assert.That(result["isDeleted"], Is.EqualTo("False"));
        }

        private sealed class TestClassWithJsonProperty
        {
            [System.Text.Json.Serialization.JsonPropertyName("custom_name")]
            public string CustomName { get; set; }
        }
    }
}