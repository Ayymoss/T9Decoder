using System.Globalization;
using System.Text.Json;

namespace PhoneDecoder;

public static class PhoneDecoder
{
    public static async Task Main()
    {
        // Encode
        const string textString = "This is a test string to figure out.";
        var encodedString = string.Empty;
        var phoneLookup = new Dictionary<int, string>
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

        foreach (var letter in textString.ToUpper())
        {
            var letterIndex = phoneLookup.FirstOrDefault(x => x.Value.Contains(letter)).Key;

            if (letterIndex == 0 && letter != ' ')
            {
                continue;
            }

            encodedString = string.Concat(encodedString, letterIndex.ToString());
        }

        Console.WriteLine($"Clean String: {textString}\nEncoded String: {encodedString}\nFormat: (WordIndex:GuessIndex: FoundWord)");

        // Word List
        const string fileName = "EnglishWords.json";
        await using var openStream = File.OpenRead(fileName);
        var dictWords = await JsonSerializer.DeserializeAsync<DictModel>(openStream);

        // Decode
        var cultureInfo = new CultureInfo("en-US", false).TextInfo;
        var resultIndex = 0;
        var wordIndex = 0;
        
        var words = encodedString.Split('0').Select(numberGroup =>
            numberGroup.Select(numberChar => phoneLookup[int.Parse(numberChar.ToString())]).ToList()).ToList();
        
        foreach (var result in words.Select(word => PermutateList(word)))
        {
            Parallel.ForEach(result, validWord =>
            {
                if (!dictWords.EnglishWords.Contains(validWord.ToLower())) return;
                Console.WriteLine($"{resultIndex}:{wordIndex}: {cultureInfo.ToTitleCase(validWord.ToLower())}");
                Interlocked.Increment(ref wordIndex);
            });
            
            wordIndex = 0;
            resultIndex++;
        }
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
}
