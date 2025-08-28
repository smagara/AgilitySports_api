using System.Text.RegularExpressions;

namespace AgilitySportsAPI.Services;

public interface IXssValidationService
{
    (bool IsValid, List<string> Violations) ValidateTextFields(object model, ILogger logger);
    bool ContainsXssPattern(string input);
}

public class XssValidationService : IXssValidationService
{
    private readonly List<Regex> _xssPatterns;

    public XssValidationService()
    {
        // Common XSS patterns to detect
        _xssPatterns = new List<Regex>
        {
            // Any < and > characters
            new Regex(@"<", RegexOptions.IgnoreCase),
            new Regex(@">", RegexOptions.IgnoreCase),

            // Script tags
            new Regex(@"<script[^>]*>.*?</script>", RegexOptions.IgnoreCase | RegexOptions.Singleline),
            new Regex(@"<script[^>]*>", RegexOptions.IgnoreCase),
            
            // Event handlers
            new Regex(@"on\w+\s*=", RegexOptions.IgnoreCase),
            
            // JavaScript protocol
            new Regex(@"javascript:", RegexOptions.IgnoreCase),
            
            // Data URLs
            new Regex(@"data:text/html", RegexOptions.IgnoreCase),
            
            // Encoded characters that could be used for XSS
            new Regex(@"&#x?[0-9a-f]+;", RegexOptions.IgnoreCase),
            new Regex(@"%[0-9a-f]{2}", RegexOptions.IgnoreCase),
            
            // Expression and eval
            new Regex(@"expression\s*\(", RegexOptions.IgnoreCase),
            new Regex(@"eval\s*\(", RegexOptions.IgnoreCase),
            
            // CSS expressions
            new Regex(@"expression\s*\([^)]*\)", RegexOptions.IgnoreCase),
            
            // VBScript
            new Regex(@"vbscript:", RegexOptions.IgnoreCase),
            
            // Encoded script tags
            new Regex(@"%3Cscript", RegexOptions.IgnoreCase),
            new Regex(@"%3cscript", RegexOptions.IgnoreCase),
            
            // Double encoded
            new Regex(@"%253Cscript", RegexOptions.IgnoreCase),
            new Regex(@"%253cscript", RegexOptions.IgnoreCase),

            // Any encoded < and > characters
            new Regex(@"&lt;", RegexOptions.IgnoreCase),
            new Regex(@"&gt;", RegexOptions.IgnoreCase)
        };
    }

    public (bool IsValid, List<string> Violations) ValidateTextFields(object model, ILogger logger)
    {
        var violations = new List<string>();
        var modelType = model.GetType();
        var properties = modelType.GetProperties();

        foreach (var property in properties)
        {
            // Only check string properties
            if (property.PropertyType == typeof(string))
            {
                var value = property.GetValue(model) as string;
                if (!string.IsNullOrEmpty(value))
                {
                    if (ContainsXssPattern(value))
                    {
                        var violation = $"XSS pattern detected in field '{property.Name}': {value}.  Update is rejected.";
                        violations.Add(violation);
                        logger.LogWarning("XSS Attempt Detected: {Violation}", violation);
                    }
                }
            }
        }

        return (!violations.Any(), violations);
    }

    public bool ContainsXssPattern(string input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        return _xssPatterns.Any(pattern => pattern.IsMatch(input));
    }
}

