using System.Diagnostics;
using System.Globalization;
using System.Text.Json;

namespace PhoneDecoder;

public static class PhoneDecoder
{
    private static readonly Dictionary<int, string> PhoneLookup = new()
    {
        {0, " "},
        {1, "1"},
        {2, "ABC"},
        {3, "DEF"},
        {4, "GHI"},
        {5, "JKL"},
        {6, "MNO"},
        {7, "PQRS"},
        {8, "TUV"},
        {9, "WXYZ"}
    };

    public static async Task Main()
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        
        // Word List
        var encodedWordList = new DictModel();
        const string fileName = "EnglishWords.json";
        await using var openStream = File.OpenRead(fileName);
        var dictWords = await JsonSerializer.DeserializeAsync<DictModel>(openStream);

        // Encode
        const string textString = "A random test string of varying length, and a random large word... superseding";
        var encodedString = EncodeString(textString);

        foreach (var word in dictWords.EnglishWords)
        {
            encodedWordList.EnglishWordsEncoded.Add(word, EncodeString(word));
        }
        
        Console.WriteLine($"Processing Time: {stopwatch.ElapsedMilliseconds:N0}ms");
        Console.WriteLine($"Clean String: {textString}\nEncoded String: {encodedString}\nFormat: (WordIndex: FoundWord)");
        
        // Decode
        var wordIndex = 0;
        var words = encodedString.Split('0');

        foreach (var word in words)
        {
            var keys = encodedWordList.EnglishWordsEncoded
                .Where(kvp => kvp.Value == word)
                .Select(kvp => kvp.Key)
                .ToList();
            
            if (keys.Count == 0)
            {
                keys.Add("Word could not be found!");
            }
            
            Console.WriteLine($"{wordIndex}: {string.Join(", ", keys).ToLower()}");
            wordIndex++;
        }

        Console.WriteLine($"Processing Time: {stopwatch.ElapsedMilliseconds:N0}ms");
    }

    private static string EncodeString(string textString)
    {
        var encodedString = string.Empty;

        foreach (var letter in textString.ToUpper())
        {
            var letterIndex = PhoneLookup.FirstOrDefault(x => x.Value.Contains(letter)).Key;

            if (letterIndex == 0 && letter != ' ' && letter != '-')
            {
                continue;
            }

            encodedString = string.Concat(encodedString, letterIndex.ToString());
        }

        return encodedString;
    }

    private static IEnumerable<string> PermutateList(IReadOnlyList<string> remaining, string textSoFar = "")
    {
        if (remaining.Count == 0)
        {
            yield return textSoFar;
        }
        else
        {
            foreach (var result in remaining[0]
                         .SelectMany(c => PermutateList(remaining.Skip(1).ToList(), textSoFar + c)))
            {
                yield return result;
            }
        }
    }
}

public class DictModel
{
    public List<string> EnglishWords { get; set; }
    public Dictionary<string, string> EnglishWordsEncoded { get; set; } = new();
}
