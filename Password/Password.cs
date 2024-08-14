using System.Text.RegularExpressions;
using LanguageExt;
using static LanguageExt.Option<Password.ParsingError>;
using static LanguageExt.Seq;

namespace Password
{
    public sealed class Password : IEquatable<Password>
    {
        private readonly string _value;

        private Password(string value) => _value = value;

        private static readonly Seq<Func<string, Option<ParsingError>>> Rules =
            create<Func<string, Option<ParsingError>>>(
                input => Match(input, ".{8,}", "Too short"),
                input => Match(input, ".*[A-Z].*", "No capital letter"),
                input => Match(input, ".*[a-z].*", "No lower letter"),
                input => Match(input, ".*[0-9].*", "No number"),
                input => Match(input, ".*[.*#@$%&].*", "No special character"),
                input => Match(input, "^[a-zA-Z0-9.*#@$%&]+$", "Invalid character")
            );

        public static Either<ParsingError, Password> Parse(string input) =>
            ParseWithMultipleErrors(input).MapLeft(errors => errors.Head());

        private static Option<ParsingError> Match(string input, string regex, string reason)
            => !Regex.Match(input, regex).Success
                ? new ParsingError(reason)
                : None;

        public static Either<Seq<ParsingError>, Password> ParseWithMultipleErrors(string input)
            => Rules.Bind(f => f(input))
                .ToSeq()
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