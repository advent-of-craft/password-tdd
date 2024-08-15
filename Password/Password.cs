using LanguageExt;
using static System.Text.RegularExpressions.Regex;
using static Password.Password.Errors;

namespace Password
{
    public sealed class Password : IEquatable<Password>
    {
        private readonly string _value;

        private Password(string value) => _value = value;

        private static readonly Seq<Rule> Rules = Seq.create(
            new Rule("^.{8,}$", TooShort),
            new Rule(".*[A-Z].*", NoCapitalLetter),
            new Rule(".*[a-z].*", NoLowerLetter),
            new Rule(".*[0-9].*", NoNumber),
            new Rule(".*[.*#@$%&].*", NoSpecialCharacter),
            new Rule("^[a-zA-Z0-9.*#@$%&]+$", InvalidCharacter)
        );

        public static Either<Seq<ParsingError>, Password> Parse(string input)
            => Rules.Filter(rule => rule.IsNotMatching(input))
                .Map(rule => new ParsingError(rule.Reason))
                .Let(parsingErrors => ToEither(input, parsingErrors));

        private static Either<Seq<ParsingError>, Password> ToEither(string input, Seq<ParsingError> parsingErrors)
            => parsingErrors.IsEmpty ? new Password(input) : parsingErrors;

        #region Equality operators

        public override string ToString() => _value;

        public override int GetHashCode() => _value.GetHashCode();
        public static bool operator ==(Password password, Password other) => password.Equals(other);
        public static bool operator !=(Password password, Password other) => !(password == other);

        private int CompareTo(Password? other)
            => string.Compare(_value, other?._value, StringComparison.Ordinal);

        public bool Equals(Password? other) => _value.Equals(other!._value);
        public override bool Equals(object? obj) => obj is Password && Equals(_value);

        #endregion

        public static class Errors
        {
            public const string TooShort = "Too short";
            public const string NoCapitalLetter = "No capital letter";
            public const string NoLowerLetter = "No lower letter";
            public const string NoNumber = "No number";
            public const string NoSpecialCharacter = "No special character";
            public const string InvalidCharacter = "Invalid character";
        }
    }

    public sealed record ParsingError(string Reason);

    public sealed record Rule(string Pattern, string Reason)
    {
        public bool IsNotMatching(string input) => !IsMatch(input, Pattern);
    }
}