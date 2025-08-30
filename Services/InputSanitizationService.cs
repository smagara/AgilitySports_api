using System.Text.RegularExpressions;

namespace AgilitySportsAPI.Services;

    public interface IInputSanitizationService
    {
        T SanitizeModel<T>(T model, ILogger logger) where T : class;
        string SanitizeString(string input, SanitizationLevel level = SanitizationLevel.Moderate);
        string SanitizeSqlInjection(string input);
        string NormalizeWhitespace(string input);
    }

public enum SanitizationLevel
{
    None,       // No sanitization, rely on Validation only
    Minimal,    // Basic cleanup only (whitespace normalization)
    Moderate,   // Whitespace + control character removal
    Strict      // Whitespace + control characters + length limits
}

public class InputSanitizationService : IInputSanitizationService
{
    private readonly ILogger<InputSanitizationService> _logger;
    private readonly Dictionary<string, SanitizationLevel> _fieldSanitizationLevels;

    public InputSanitizationService(ILogger<InputSanitizationService> logger)
    {
        _logger = logger;
        
        // Configure sanitization levels for different field types
        // Note: Security validation is handled by XssValidationService
        // This service only handles data normalization
        _fieldSanitizationLevels = new Dictionary<string, SanitizationLevel>
        {
            // Names - minimal normalization to preserve legitimate punctuation
            { "FirstName", SanitizationLevel.Minimal },
            { "LastName", SanitizationLevel.Minimal },
            { "Name", SanitizationLevel.Minimal },
            
            // Team names - moderate normalization
            { "Team", SanitizationLevel.Moderate },
            
            // Positions - strict normalization
            { "Position", SanitizationLevel.Strict },
            { "FantasyPosition", SanitizationLevel.Strict },
            { "PositionCategory", SanitizationLevel.Strict },
            
            // Numbers and codes - strict
            { "Number", SanitizationLevel.Strict },
            { "PlayerId", SanitizationLevel.Strict },
            { "playerID", SanitizationLevel.Strict },
            
            // Physical attributes - minimal to preserve legitimate punctuation
            { "Height", SanitizationLevel.None },
            { "Weight", SanitizationLevel.Moderate },
            { "Age", SanitizationLevel.Strict },
            
            // Status fields - moderate
            { "CurrentStatus", SanitizationLevel.Moderate },
            { "CurrentStatusColor", SanitizationLevel.Strict },
            
            // Location fields - moderate
            { "College", SanitizationLevel.Moderate },
            { "BirthPlace", SanitizationLevel.Moderate },
            { "BirthCountry", SanitizationLevel.Moderate },
            
            // Dates - strict
            { "DateOfBirth", SanitizationLevel.Strict },
            { "BirthDateShortString", SanitizationLevel.Strict },
            
            // Handedness - strict
            { "Handed", SanitizationLevel.Strict },
            { "Drafted", SanitizationLevel.Strict }
        };
    }

    public T SanitizeModel<T>(T model, ILogger logger) where T : class
    {
        if (model == null)
            return model!;

        var modelType = model.GetType();
        var properties = modelType.GetProperties();

        foreach (var property in properties)
        {
            if (property.PropertyType == typeof(string))
            {
                var originalValue = property.GetValue(model) as string;
                if (!string.IsNullOrEmpty(originalValue))
                {
                    // Determine sanitization level for this field
                    var sanitizationLevel = GetSanitizationLevel(property.Name);
                    
                    // Sanitize the value (data normalization only, not security)
                    var sanitizedValue = SanitizeString(originalValue, sanitizationLevel);
                    
                    // Set the sanitized value back to the property
                    property.SetValue(model, sanitizedValue);
                    
                    // Log if significant changes were made
                    if (originalValue != sanitizedValue)
                    {
                        logger.LogInformation("Normalized field '{FieldName}': '{Original}' -> '{Sanitized}'", 
                            property.Name, originalValue, sanitizedValue);
                    }
                }
            }
        }

        return model;
    }

    public string SanitizeString(string input, SanitizationLevel level = SanitizationLevel.Moderate)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var sanitized = input;

        switch (level)
        {
            case SanitizationLevel.None:
                sanitized = input; // No normalization applied
                break;

            case SanitizationLevel.Minimal:
                sanitized = NormalizeWhitespace(sanitized);
                sanitized = NormalizeUnicodeCharacters(sanitized);
                sanitized = SanitizeSqlInjection(sanitized);
                break;

            case SanitizationLevel.Moderate:
                sanitized = NormalizeWhitespace(sanitized);
                sanitized = RemoveControlCharacters(sanitized);
                sanitized = NormalizeUnicodeCharacters(sanitized);
                sanitized = SanitizeSqlInjection(sanitized);
                break;

            case SanitizationLevel.Strict:
                sanitized = NormalizeWhitespace(sanitized);
                sanitized = RemoveControlCharacters(sanitized);
                sanitized = RemoveSpecialCharacters(sanitized);
                sanitized = NormalizeUnicodeCharacters(sanitized);
                sanitized = SanitizeSqlInjection(sanitized);
                sanitized = TruncateIfNeeded(sanitized, 50); // Limit length for strict fields
                break;
        }

        return sanitized;
    }

    public string NormalizeWhitespace(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        // Normalize whitespace - replace multiple spaces/tabs with single space
        var normalized = Regex.Replace(input, @"\s+", " ");
        
        // Trim leading and trailing whitespace
        return normalized.Trim();
    }

    private string RemoveControlCharacters(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        // Remove all control characters (0x00-0x1F, 0x7F) including newlines and tabs
        // This includes: null, bell, backspace, form feed, escape, newlines, tabs, etc.
        var sanitized = Regex.Replace(input, @"[\x00-\x1F\x7F]", "");
        
        // Also remove high control characters (0x80-0x9F) which can cause database issues
        sanitized = Regex.Replace(sanitized, @"[\x80-\x9F]", "");
        
        return sanitized;
    }

    private string RemoveSpecialCharacters(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        // Remove Unicode control characters and formatting characters
        var sanitized = input;
        
        // Remove Unicode control characters (U+0000 to U+001F, U+007F to U+009F) including newlines and tabs
        sanitized = Regex.Replace(sanitized, @"[\u0000-\u001F\u007F-\u009F]", "");
        
        // Remove Unicode formatting characters that can pollute databases
        // This includes: zero-width spaces, soft hyphens, invisible separators, etc.
        sanitized = Regex.Replace(sanitized, @"[\u200B-\u200F\u2028-\u202F\u205F-\u206F\uFEFF]", "");
        
        // Remove bidirectional text control characters
        sanitized = Regex.Replace(sanitized, @"[\u202A-\u202E\u2066-\u2069]", "");
        
        // Remove other problematic Unicode characters
        sanitized = Regex.Replace(sanitized, @"[\u2060\u2061\u2062\u2063\u2064]", "");
        
        return sanitized;
    }

    private string NormalizeUnicodeCharacters(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var sanitized = input;
        
        // Replace curly quotes with straight quotes
        sanitized = sanitized.Replace("\u201C", "\"").Replace("\u201D", "\"");  // Smart quotes to straight quotes
        sanitized = sanitized.Replace("\u2018", "'").Replace("\u2019", "'");  // Smart apostrophes to straight apostrophes
        
        // Replace em dashes and en dashes with regular hyphens
        sanitized = sanitized.Replace("\u2014", "-").Replace("\u2013", "-");  // Em dash and en dash to hyphen
        
        // Replace ellipsis with three dots
        sanitized = sanitized.Replace("\u2026", "...");
        
        // Replace other common Unicode characters that might cause issues
        sanitized = sanitized.Replace("\u00B0", " degrees");  // Degree symbol
        sanitized = sanitized.Replace("\u00D7", "x");         // Multiplication sign
        sanitized = sanitized.Replace("\u00F7", "/");         // Division sign
        
        return sanitized;
    }

    public string SanitizeSqlInjection(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        // Remove or escape common SQL injection patterns
        var patterns = new[]
        {
            @"(\b(SELECT|INSERT|UPDATE|DELETE|DROP|CREATE|ALTER|EXEC|EXECUTE|UNION|SCRIPT)\b)",
            @"(--|/\*|\*/|;|xp_|sp_)",
            @"(\b(OR|AND)\b\s+\d+\s*=\s*\d+)",
            @"(\b(OR|AND)\b\s+['""]\w+['""]\s*=\s*['""]\w+['""])"
        };

        var sanitized = input;
        foreach (var pattern in patterns)
        {
            sanitized = Regex.Replace(sanitized, pattern, "", RegexOptions.IgnoreCase);
        }

        return sanitized;
    }

    private string TruncateIfNeeded(string input, int maxLength)
    {
        if (string.IsNullOrEmpty(input) || input.Length <= maxLength)
            return input;

        return input.Substring(0, maxLength);
    }

    private SanitizationLevel GetSanitizationLevel(string fieldName)
    {
        return _fieldSanitizationLevels.TryGetValue(fieldName, out var level) 
            ? level 
            : SanitizationLevel.Moderate; // Default to moderate if field not configured
    }
}
