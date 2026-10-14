using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Text.Json.Serialization;
using NuciAPI.Client.UnitTests.Helpers;

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

        [Test]
        public void GivenObjectWithSpecialCharacters_WhenBuild_ThenEscapesValues()
        {
            object obj = new { Name = "test value", Description = "a&b=c" };

            Dictionary<string, string> result = QueryStringBuilder.Build(obj);

            Assert.That(result["name"], Is.EqualTo("test value"));
            Assert.That(result["description"], Is.EqualTo("a&b=c"));
        }

        [Test]
        public void GivenDeeplyNestedObject_WhenBuild_ThenFlattensCorrectly()
        {
            object obj = new
            {
                User = new
                {
                    Profile = new { FirstName = "John", LastName = "Doe" },
                    Settings = new { Theme = "dark", Notifications = true }
                }
            };

            Dictionary<string, string> result = QueryStringBuilder.Build(obj);

            Assert.That(result, Has.Count.EqualTo(4));
            Assert.That(result["user.profile.firstName"], Is.EqualTo("John"));
            Assert.That(result["user.profile.lastName"], Is.EqualTo("Doe"));
            Assert.That(result["user.settings.theme"], Is.EqualTo("dark"));
            Assert.That(result["user.settings.notifications"], Is.EqualTo("True"));
        }

        [Test]
        public void GivenObjectWithLongProperty_WhenBuild_ThenConvertsToString()
        {
            object obj = new { Id = 123456789012345L };

            Dictionary<string, string> result = QueryStringBuilder.Build(obj);

            Assert.That(result["id"], Is.EqualTo("123456789012345"));
        }

        [Test]
        public void GivenObjectWithDoubleProperty_WhenBuild_ThenConvertsToString()
        {
            object obj = new { Price = 19.99d };

            Dictionary<string, string> result = QueryStringBuilder.Build(obj);

            Assert.That(result["price"], Is.EqualTo("19.99"));
        }

        [Test]
        public void GivenObjectWithFloatProperty_WhenBuild_ThenConvertsToString()
        {
            object obj = new { Price = 19.99f };

            Dictionary<string, string> result = QueryStringBuilder.Build(obj);

            Assert.That(result["price"], Is.EqualTo("19.99"));
        }

        [Test]
        public void GivenObjectWithByteProperty_WhenBuild_ThenConvertsToString()
        {
            object obj = new { Value = (byte)255 };

            Dictionary<string, string> result = QueryStringBuilder.Build(obj);

            Assert.That(result["value"], Is.EqualTo("255"));
        }

        [Test]
        public void GivenObjectWithShortProperty_WhenBuild_ThenConvertsToString()
        {
            object obj = new { Value = (short)32767 };

            Dictionary<string, string> result = QueryStringBuilder.Build(obj);

            Assert.That(result["value"], Is.EqualTo("32767"));
        }

        [Test]
        public void GivenObjectWithUIntProperty_WhenBuild_ThenConvertsToString()
        {
            object obj = new { Value = 4294967295u };

            Dictionary<string, string> result = QueryStringBuilder.Build(obj);

            Assert.That(result["value"], Is.EqualTo("4294967295"));
        }

        [Test]
        public void GivenObjectWithULongProperty_WhenBuild_ThenConvertsToString()
        {
            object obj = new { Value = 18446744073709551615ul };

            Dictionary<string, string> result = QueryStringBuilder.Build(obj);

            Assert.That(result["value"], Is.EqualTo("18446744073709551615"));
        }

        [Test]
        public void GivenObjectWithCharProperty_WhenBuild_ThenConvertsToString()
        {
            object obj = new { Value = 'A' };

            Dictionary<string, string> result = QueryStringBuilder.Build(obj);

            Assert.That(result["value"], Is.EqualTo("A"));
        }
    }
}