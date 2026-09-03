using Microsoft.ML;
using VoiceAssistant.Core.Interfaces;

namespace VoiceAssistant.Core.Services;

public class PersonDetector
{
    private readonly List<string> _knownPeople;
    private readonly Dictionary<string, string> _personAliases;

    public PersonDetector(List<string> knownPeople)
    {
        _knownPeople = knownPeople;
        _personAliases = new Dictionary<string, string>();
        
        foreach (var person in knownPeople)
        {
            var lower = person.ToLower();
            
            if (lower.Length >= 3)
                _personAliases[lower.Substring(0, 3)] = person;
            
            if (lower.Length >= 4)
                _personAliases[lower.Substring(0, 4)] = person;
            
            var consonants = new string(lower.Where(c => !"aeiou".Contains(c)).ToArray());
            if (consonants.Length >= 3)
                _personAliases[consonants.Substring(0, Math.Min(4, consonants.Length))] = person;
            
            if (person.ToLower() == "ezequiel")
            {
                _personAliases["equi"] = person;
                _personAliases["eze"] = person;
                _personAliases["ezeq"] = person;
                _personAliases["ezqu"] = person;
                _personAliases["equ"] = person;
                _personAliases["ez"] = person;
            }
            
            if (person.ToLower() == "alejandro")
            {
                _personAliases["ale"] = person;
                _personAliases["alej"] = person;
                _personAliases["aleja"] = person;
                _personAliases["alex"] = person;
                _personAliases["al"] = person;
            }
        }
    }

    public string DetectPerson(string text)
    {
        var textLower = text.ToLower();
        
        foreach (var person in _knownPeople)
        {
            if (textLower.Contains(person.ToLower()))
                return person;
        }
        
        foreach (var alias in _personAliases.OrderByDescending(a => a.Key.Length))
        {
            if (textLower.Contains(alias.Key))
                return alias.Value;
        }
        
        var words = textLower.Split(new char[] { ' ', ',', '.', ';', ':', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var word in words)
        {
            if (word.Length < 2) continue;
            
            foreach (var person in _knownPeople)
            {
                var personLower = person.ToLower();
                
                if (IsSimilar(word, personLower))
                    return person;
            }
        }
        
        return null;
    }

    private bool IsSimilar(string word, string personName)
    {
        if (word.Length < 2 || personName.Length < 2) return false;
        
        var maxLength = Math.Min(word.Length, personName.Length);
        var matches = 0;
        
        for (int i = 0; i < maxLength; i++)
        {
            if (word[i] == personName[i])
                matches++;
            else
                break;
        }
        
        var similarity = (double)matches / maxLength;
        
        return similarity >= 0.5;
    }
}