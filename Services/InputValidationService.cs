using System.Text.RegularExpressions;

namespace AgilitySportsAPI.Services;

public interface IInputValidationService
{
    (bool IsValid, List<string> ValidationErrors) ValidateModel<T>(T model, ILogger logger) where T : class;
    (bool IsValid, string ErrorMessage) ValidateHeight(string input);
    (bool IsValid, string ErrorMessage) ValidateWeight(string input);
    (bool IsValid, string ErrorMessage) ValidateAge(string input);
    (bool IsValid, string ErrorMessage) ValidatePlayerNumber(string input);
    (bool IsValid, string ErrorMessage) ValidatePosition(string input);
    (bool IsValid, string ErrorMessage) ValidatePositionForSport(string input, string sport);
    (bool IsValid, string ErrorMessage) ValidateHanded(string input);
}

public class InputValidationService : IInputValidationService
{
    private readonly ILogger<InputValidationService> _logger;

    public InputValidationService(ILogger<InputValidationService> logger)
    {
        _logger = logger;
    }

    public (bool IsValid, List<string> ValidationErrors) ValidateModel<T>(T model, ILogger logger) where T : class
    {
        var validationErrors = new List<string>();
        
        if (model == null)
        {
            validationErrors.Add("Model cannot be null");
            return (false, validationErrors);
        }

        var modelType = model.GetType();
        var properties = modelType.GetProperties();

        foreach (var property in properties)
        {
            if (property.PropertyType == typeof(string))
            {
                var value = property.GetValue(model) as string;
                if (!string.IsNullOrEmpty(value))
                {
                    var (isValid, errorMessage) = ValidateField(property.Name, value);
                    if (!isValid)
                    {
                        validationErrors.Add($"Field '{property.Name}': {errorMessage}");
                    }
                }
            }
        }

        return (!validationErrors.Any(), validationErrors);
    }

    private (bool IsValid, string ErrorMessage) ValidateField(string fieldName, string value)
    {
        return fieldName.ToLower() switch
        {
            "height" => ValidateHeight(value),
            "weight" => ValidateWeight(value),
            "age" => ValidateAge(value),
            "number" => ValidatePlayerNumber(value),
            "position" => ValidatePosition(value),
            "fantasyposition" => ValidatePosition(value),
            "handed" => ValidateHanded(value),
            _ => (true, string.Empty) // No validation for other fields
        };
    }

    public (bool IsValid, string ErrorMessage) ValidateHeight(string input)
    {
        if (string.IsNullOrEmpty(input))
            return (true, string.Empty);

        // Pattern: feet'inches" (e.g., 5'8", 6'9")
        var heightPattern = @"^(\d+)\'?(\d{1,2})\""?$";
        var match = Regex.Match(input.Trim(), heightPattern);
        
        if (!match.Success)
        {
            return (false, "Height must be in format: feet'inches\" (e.g., 5'8\", 6'9\")");
        }

        var feet = int.Parse(match.Groups[1].Value);
        var inches = int.Parse(match.Groups[2].Value);
        
        // Validate ranges (reasonable height limits)
        if (feet < 4 || feet > 8)
        {
            return (false, "Height feet must be between 4 and 8");
        }
        
        if (inches < 0 || inches > 11)
        {
            return (false, "Height inches must be between 0 and 11");
        }

        return (true, string.Empty);
    }

    public (bool IsValid, string ErrorMessage) ValidateWeight(string input)
    {
        if (string.IsNullOrEmpty(input))
            return (true, string.Empty);

        // Extract only numbers
        var numbers = Regex.Replace(input.Trim(), @"[^0-9]", "");
        
        if (string.IsNullOrEmpty(numbers))
        {
            return (false, "Weight must contain numeric values");
        }

        var weight = int.Parse(numbers);
        
        // Validate reasonable weight range (50-400 lbs)
        if (weight < 50 || weight > 400)
        {
            return (false, "Weight must be between 50 and 400");
        }

        return (true, string.Empty);
    }

    public (bool IsValid, string ErrorMessage) ValidateAge(string input)
    {
        if (string.IsNullOrEmpty(input))
            return (true, string.Empty);

        // Extract only numbers
        var numbers = Regex.Replace(input.Trim(), @"[^0-9]", "");
        
        if (string.IsNullOrEmpty(numbers))
        {
            return (false, "Age must contain numeric values");
        }

        var age = int.Parse(numbers);
        
        // Validate reasonable age range (16-80)
        if (age < 16 || age > 80)
        {
            return (false, "Age must be between 16 and 80");
        }

        return (true, string.Empty);
    }

    public (bool IsValid, string ErrorMessage) ValidatePlayerNumber(string input)
    {
        if (string.IsNullOrEmpty(input))
            return (true, string.Empty);

        // Extract only numbers
        var numbers = Regex.Replace(input.Trim(), @"[^0-9]", "");
        
        if (string.IsNullOrEmpty(numbers))
        {
            return (false, "Player number must contain numeric values");
        }

        var number = int.Parse(numbers);
        
        // Validate reasonable player number range (0-99)
        if (number < 0 || number > 99)
        {
            return (false, "Player number must be between 0 and 99");
        }

        return (true, string.Empty);
    }

    public (bool IsValid, string ErrorMessage) ValidatePosition(string input)
    {
        if (string.IsNullOrEmpty(input))
            return (true, string.Empty);

        // Position codes from your database by sport
        var validPositions = new[]
        {
            // NBA
            "C", "F", "G", "PF", "PG", "SF", "SG",
            // NFL
            "C", "CB", "DB", "DE", "DL", "DT", "FB", "G", "ILB", "K", "LB", "LS", "NT", "OL", "OLB", "OT", "P", "QB", "RB", "S", "TE", "WR",
            // NHL
            "C", "D", "F", "G", "LW", "RW"
        };

        var cleanInput = input.Trim().ToUpper();
        
        if (!validPositions.Contains(cleanInput))
        {
            return (false, $"Position '{input}' is not a valid position code. Valid codes: {string.Join(", ", validPositions)}");
        }

        return (true, string.Empty);
    }

    public (bool IsValid, string ErrorMessage) ValidatePositionForSport(string input, string sport)
    {
        if (string.IsNullOrEmpty(input))
            return (true, string.Empty);

        var sportUpper = sport?.Trim().ToUpper();
        var cleanInput = input.Trim().ToUpper();

        var validPositions = sportUpper switch
        {
            "NBA" => new[] { "C", "F", "G", "PF", "PG", "SF", "SG" },
            "NFL" => new[] { "C", "CB", "DB", "DE", "DL", "DT", "FB", "G", "ILB", "K", "LB", "LS", "NT", "OL", "OLB", "OT", "P", "QB", "RB", "S", "TE", "WR" },
            "NHL" => new[] { "C", "D", "F", "G", "LW", "RW" },
            _ => new string[0] // Unknown sport
        };

        if (validPositions.Length == 0)
        {
            return (false, $"Unknown sport '{sport}'. Valid sports: NBA, NFL, NHL");
        }

        if (!validPositions.Contains(cleanInput))
        {
            return (false, $"Position '{input}' is not valid for {sport}. Valid {sport} positions: {string.Join(", ", validPositions)}");
        }

        return (true, string.Empty);
    }

    public (bool IsValid, string ErrorMessage) ValidateHanded(string input)
    {
        if (string.IsNullOrEmpty(input))
            return (true, string.Empty);

        var cleanInput = input.Trim().ToUpper();
        
        // Valid handed values from your UI
        var validHandedValues = new[] { "L", "R", "B" };
        
        if (!validHandedValues.Contains(cleanInput))
        {
            return (false, $"Handed value '{input}' is not valid. Valid values: {string.Join(", ", validHandedValues)} (Left, Right, Both)");
        }

        return (true, string.Empty);
    }
}
