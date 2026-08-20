namespace Puentes.Orchestrator.Services;

public static class KnownPersonNameMatcher
{
    public static bool IsSimilarWord(string heard, string known)
    {
        if (heard.Equals(known, StringComparison.Ordinal))
        {
            return true;
        }

        var maximumLength = Math.Max(heard.Length, known.Length);
        if (maximumLength < 5 || Math.Abs(heard.Length - known.Length) > 2)
        {
            return false;
        }

        var distance = LevenshteinDistance(heard, known);
        if (maximumLength <= 5)
        {
            return heard[0] == known[0] && distance == 1;
        }

        var endingLength = Math.Min(3, Math.Min(heard.Length, known.Length));
        var hasCompatibleEnding = heard.EndsWith(
            known[^endingLength..], StringComparison.Ordinal) ||
            known.EndsWith(heard[^endingLength..], StringComparison.Ordinal);

        return hasCompatibleEnding && distance <= 3 &&
            1d - (double)distance / maximumLength >= 0.62d;
    }

    private static int LevenshteinDistance(string left, string right)
    {
        var previous = Enumerable.Range(0, right.Length + 1).ToArray();
        var current = new int[right.Length + 1];
        for (var leftIndex = 1; leftIndex <= left.Length; leftIndex++)
        {
            current[0] = leftIndex;
            for (var rightIndex = 1; rightIndex <= right.Length; rightIndex++)
            {
                var substitutionCost = left[leftIndex - 1] ==
                    right[rightIndex - 1] ? 0 : 1;
                current[rightIndex] = Math.Min(
                    Math.Min(current[rightIndex - 1] + 1,
                        previous[rightIndex] + 1),
                    previous[rightIndex - 1] + substitutionCost);
            }
            (previous, current) = (current, previous);
        }
        return previous[right.Length];
    }
}
