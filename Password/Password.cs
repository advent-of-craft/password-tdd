using LanguageExt;
using static System.Text.RegularExpressions.Regex;

namespace Password
{
    public sealed class Password : IEquatable<Password>
    {
        private readonly string _value;

        private Password(string value) => _value = value;

        private static readonly Seq<Rule> Rules = Seq.create(
            new Rule("^.{8,}$", "Too short"),
            new Rule(".*[A-Z].*", "No capital letter"),
            new Rule(".*[a-z].*", "No lower letter"),
            new Rule(".*[0-9].*", "No number"),
            new Rule(".*[.*#@$%&].*", "No special character"),
            new Rule("^[a-zA-Z0-9.*#@$%&]+$", "Invalid character")
        );

        public static Either<ParsingError, Password> Parse(string input)
            => ParseWithMultipleErrors(input)
                .MapLeft(errors => errors.Head());

        public static Either<Seq<ParsingError>, Password> ParseWithMultipleErrors(string input)
            => Rules.Filter(rule => rule.IsNotMatching(input))
                .Map(rule => new ParsingError(rule.Reason))
                .Let(parsingErrors => ToEither(input, parsingErrors.ToSeq()));

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
    }

    public sealed record ParsingError(string Reason);

    public sealed record Rule(string Pattern, string Reason)
    {
        public bool IsNotMatching(string input) => !IsMatch(input, Pattern);
    }
}