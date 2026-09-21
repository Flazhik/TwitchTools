using System;
using System.Linq;
using System.Text.RegularExpressions;
using ThornClient.Core.ConfigurableElements;

namespace TwitchTools.Utils;

internal static class InterfaceHintsFactory
{
    public static InterfaceHints SentenceCaseEnumSubstitutions(Type enumType)
    {
        if (enumType == null)
            throw new ArgumentNullException(nameof(enumType));

        if (!enumType.IsEnum)
            throw new ArgumentException("Provided type must be an enum", nameof(enumType));

        return new InterfaceHints
        {
            EnumSubstitutions = Enum.GetNames(enumType)
                .ToDictionary(static name => name, static name => name.ToSentenceCase())
        };
    }
        
    private static string ToSentenceCase(this string input) {
        if (string.IsNullOrEmpty(input)) {
            return input;
        }

        var result = Regex.Replace(input, @"(?<!^)(?=[A-Z][a-z])|(?<=[a-z0-9])(?=[A-Z])", " ");
        result = result.ToLowerInvariant();
        return char.ToUpperInvariant(result[0]) + result.Substring(1);
    }
}