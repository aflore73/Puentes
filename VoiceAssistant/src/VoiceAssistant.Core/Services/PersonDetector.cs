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
            
            if (lower.Length >= 5)
                _personAliases[lower.Substring(0, 5)] = person;
            
            if (lower.Length >= 4)
                _personAliases[lower.Substring(0, 4)] = person;
            
            if (person.ToLower() == "ezequiel")
            {
                _personAliases["equi"] = person;
                _personAliases["eze"] = person;
                _personAliases["ezeq"] = person;
                _personAliases["ezqu"] = person;
                _personAliases["equ"] = person;
            }
            
            if (person.ToLower() == "alejandro")
            {
                _personAliases["aleja"] = person;
                _personAliases["alej"] = person;
                _personAliases["alex"] = person;
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
            if (alias.Key.Length >= 4 && textLower.Contains(alias.Key))
                return alias.Value;
        }
        
        return null;
    }
}