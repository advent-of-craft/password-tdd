using FluentAssertions;
using FluentAssertions.LanguageExt;
using LanguageExt;
using LanguageExt.UnsafeValueAccess;
using Xunit;
using static Password.Password;
using static Password.Password.Errors;

namespace Password.Tests
{
    public class PasswordTests
    {
        [Theory]
        [InlineData("P@ssw0rd")]
        [InlineData("Advent0fCraft&")]
        public void Success_For_A_Valid_Password_Parsed_With_Multiple_Errors(string password)
            => Parse(password)
                .Should()
                .BeRight(p => p.ToString().Should().Be(password));

        [Fact]
        public void Value_Equality()
        {
            const string input = "P@ssw0rd";
            var password = Parse(input).ValueUnsafe();
            var other = Parse(input).ValueUnsafe();

            password.Equals(other).Should().BeTrue();
            (password == other).Should().BeTrue();
            (password != other).Should().BeFalse();
        }

        public class FailWhen
        {
            [Theory]
            [InlineData("P@ssw^rd1", InvalidCharacter)]
            [InlineData("aa", TooShort, NoCapitalLetter, NoNumber, NoSpecialCharacter)]
            [InlineData("xxxxxxx", TooShort, NoCapitalLetter, NoNumber, NoSpecialCharacter)]
            [InlineData("adventofcraft", NoCapitalLetter, NoNumber, NoSpecialCharacter)]
            [InlineData("p@ssw0rd", NoCapitalLetter)]
            [InlineData("ADVENTOFCRAFT", NoLowerLetter, NoNumber, NoSpecialCharacter)]
            [InlineData("P@SSW0RD", NoLowerLetter)]
            [InlineData("Adventofcraft", NoNumber, NoSpecialCharacter)]
            [InlineData("P@sswOrd", NoNumber)]
            [InlineData("Adventof09craft", NoSpecialCharacter)]
            [InlineData("PAssw0rd", NoSpecialCharacter)]
            [InlineData("Advent@of9Craft/", InvalidCharacter)]
            [InlineData("", TooShort, NoCapitalLetter, NoLowerLetter, NoNumber, NoSpecialCharacter,
                InvalidCharacter)]
            public void Password_Should_Failed_To_Parse(string password, params string[] reasons)
                => Parse(password)
                    .Should()
                    .BeLeft(errors => ErrorsAreEquivalent(reasons, errors));

            private static void ErrorsAreEquivalent(string[] reasons, Seq<ParsingError> errors)
                => errors.Should().BeEquivalentTo(reasons.Select(c => new ParsingError(c)));
        }
    }
}