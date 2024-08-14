using FluentAssertions;
using FluentAssertions.LanguageExt;
using LanguageExt;
using LanguageExt.UnsafeValueAccess;
using Xunit;

namespace Password.Tests
{
    public class PasswordTests
    {
        [Theory]
        [InlineData("P@ssw0rd")]
        [InlineData("Advent0fCraft&")]
        public void Success_For_A_Valid_Password_Parsed_With_Multiple_Errors(string password)
            => Password.Parse(password)
                .Should()
                .BeRight(p => p.ToString().Should().Be(password));

        [Fact]
        public void Value_Equality()
        {
            const string input = "P@ssw0rd";
            var password = Password.Parse(input).ValueUnsafe();
            var other = Password.Parse(input).ValueUnsafe();

            password.Equals(other).Should().BeTrue();
            (password == other).Should().BeTrue();
            (password != other).Should().BeFalse();
        }

        public class FailWhen
        {
            [Theory]
            [InlineData("P@ssw^rd1", "Invalid character")]
            [InlineData("aa", "Too short", "No capital letter", "No number", "No special character")]
            [InlineData("xxxxxxx", "Too short", "No capital letter", "No number", "No special character")]
            [InlineData("adventofcraft", "No capital letter", "No number", "No special character")]
            [InlineData("p@ssw0rd", "No capital letter")]
            [InlineData("ADVENTOFCRAFT", "No lower letter", "No number", "No special character")]
            [InlineData("P@SSW0RD", "No lower letter")]
            [InlineData("Adventofcraft", "No number", "No special character")]
            [InlineData("P@sswOrd", "No number")]
            [InlineData("Adventof09craft", "No special character")]
            [InlineData("PAssw0rd", "No special character")]
            [InlineData("Advent@of9Craft/", "Invalid character")]
            [InlineData("", "Too short", "No capital letter", "No lower letter", "No number", "No special character",
                "Invalid character")]
            public void Password_Should_Failed_To_Parse(string password, params string[] reasons)
                => Password.Parse(password)
                    .Should()
                    .BeLeft(errors => ErrorsAreEquivalent(reasons, errors));

            private static void ErrorsAreEquivalent(string[] reasons, Seq<ParsingError> errors)
                => errors.Should().BeEquivalentTo(reasons.Select(c => new ParsingError(c)));
        }
    }
}