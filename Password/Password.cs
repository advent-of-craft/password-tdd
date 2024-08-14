using LanguageExt;
using static System.Text.RegularExpressions.Regex;

namespace Password
{
    public sealed class Password : IEquatable<Password>
    {
        private readonly string _value;

        private Password(string value) => _value = value;

        private static readonly Dictionary<string, string> Rules =
            new()
            {
                {".{8,}", "Too short"},
                {".*[A-Z].*", "No capital letter"},
                {".*[a-z].*", "No lower letter"},
                {".*[0-9].*", "No number"},
                {".*[.*#@$%&].*", "No special character"},
                {"^[a-zA-Z0-9.*#@$%&]+$", "Invalid character"}
            };

        public static Either<ParsingError, Password> Parse(string input)
            => ParseWithMultipleErrors(input)
                .MapLeft(errors => errors.Head());

        public static Either<Seq<ParsingError>, Password> ParseWithMultipleErrors(string input)
            => Rules.Filter(rule => !IsMatch(input, rule.Key))
                .Map(rule => new ParsingError(rule.Value))
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

        public int CompareTo(object? other)
            => other switch
            {
                null => 1,
                Password password => CompareTo(password),
                _ => throw new ArgumentException("must be of type Snafu")
            };

        public bool Equals(Password? other) => _value.Equals(other!._value);
        public override bool Equals(object? obj) => obj is Password && Equals(_value);

        #endregion
    }

    public sealed record ParsingError(string Reason);
}