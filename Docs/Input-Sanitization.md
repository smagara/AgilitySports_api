# Input Validation & Data Normalization Implementation

## Overview

This implementation provides a two-layer approach to input security and data quality for single-line text fields:

1. **XSS Validation Service**: Blocks malicious content and prevents XSS attacks
2. **Input Validation Service**: Validates structured fields (height, weight, age, position) with specific format requirements
3. **Input Sanitization Service**: Normalizes and cleans legitimate data (whitespace, control characters, etc.)

**Note**: This sanitization is designed for single-line text fields (names, positions, team names, etc.). For multi-line text areas, a different approach would be needed.

This separation ensures that:
- **Security**: XSS validation blocks malicious content entirely
- **Data Quality**: Structured validation ensures proper formats
- **Data Consistency**: Normalization ensures clean, consistent data storage

## Architecture

```
Request → XSS Validation → Structured Field Validation → Data Normalization → Repository → Database
```

1. **XSS Validation**: Blocks malicious content and returns detailed feedback
2. **Structured Field Validation**: Validates format and range for specific fields
3. **Data Normalization**: Cleans and normalizes legitimate data (whitespace, control characters)
4. **Repository**: Processes validated and normalized data
5. **Database**: Stores clean, safe, and properly formatted data

## Why This Approach?

### The Problem with Traditional Sanitization
Traditional sanitization (HTML encoding) causes data corruption:
- `O'Connor` becomes `O&#39;Connor` in database
- `6'2"` becomes `6&#39;2&quot;` in database
- Legitimate punctuation gets stored as HTML entities

### The Solution: Validation + Normalization + Security
- **XSS Validation**: Blocks all HTML tags and dangerous patterns
- **Data Normalization**: Cleans whitespace, control characters, and Unicode issues
- **SQL Injection Prevention**: Removes malicious SQL patterns
- **Result**: Clean, secure data without corruption

## Normalization Levels

### None
- No normalization applied
- Used for fields that should be stored exactly as entered (e.g., Height)

### Minimal
- Basic whitespace normalization
- Unicode character normalization (curly quotes, smart apostrophes, etc.)
- SQL injection pattern removal
- Used for names and fields with legitimate punctuation

### Moderate (Default)
- Whitespace normalization
- Control character removal (including high control characters 0x80-0x9F)
- Unicode character normalization
- SQL injection pattern removal
- Used for most text fields (names, locations, etc.)

### Strict
- All moderate features plus:
- Unicode formatting character removal (zero-width spaces, invisible separators, etc.)
- Special character removal (preserves basic punctuation)
- Length truncation (50 characters max)
- Used for codes, positions, and other constrained fields

## Field-Specific Configuration

The service automatically applies different normalization levels based on field names:

| Field Type | Normalization Level | Examples |
|------------|-------------------|----------|
| Names | Minimal | FirstName, LastName, Name |
| Team Names | Moderate | Team |
| Positions | Strict | Position, FantasyPosition, PositionCategory |
| Numbers/Codes | Strict | Number, PlayerId, Age |
| Physical Attributes | None/Minimal | Height (None), Weight (Moderate) |
| Status Fields | Moderate | CurrentStatus |
| Location Fields | Moderate | College, BirthPlace, BirthCountry |
| Dates | Strict | DateOfBirth, BirthDateShortString |

## Features

### 1. Whitespace Normalization
- Removes extra spaces, tabs, and newlines
- Trims leading/trailing whitespace
- Example: `"  John   Doe  "` → `"John Doe"`

### 2. Control Character Removal
- Removes all control characters (0x00-0x1F, 0x7F) including newlines and tabs
- Removes high control characters (0x80-0x9F) that can cause database issues
- Example: `"John\x00Doe\x80\n\t"` → `"JohnDoe"`

### 3. Unicode Character Normalization
- Converts curly quotes (`"` `"`) to straight quotes (`"`)
- Converts smart apostrophes (`'` `'`) to straight apostrophes (`'`)
- Converts em/en dashes (`—` `–`) to regular hyphens (`-`)
- Converts ellipsis (`…`) to three dots (`...`)
- Converts other problematic Unicode characters (degree symbols, etc.)
- Example: `"O'Connor's "smart" quote"` → `"O'Connor's "smart" quote"`

### 4. Unicode Formatting Character Removal (Strict Mode)
- Removes zero-width spaces, soft hyphens, invisible separators
- Removes bidirectional text control characters
- Removes other Unicode formatting characters that pollute databases
- Example: `"John\u200BDoe"` → `"JohnDoe"`

### 5. Special Character Filtering (Strict Mode)
- Removes special characters except alphanumeric, spaces, and basic punctuation
- **Preserves**: apostrophes (`'`), quotes (`"`), parentheses, commas, periods, hyphens
- Example: `"C@#$%^&*()"` → `"C"`

### 6. SQL Injection Prevention
- Removes common SQL keywords and patterns
- Prevents basic SQL injection attempts
- Example: `"John; DROP TABLE Users; --"` → `"John"`

### 7. Length Truncation (Strict Mode)
- Limits field length to 50 characters
- Prevents excessive data storage

## XSS Validation Service

The XSS Validation Service provides security by blocking malicious content:

### What It Blocks
- All HTML tags (`<`, `>`)
- Script tags and event handlers
- JavaScript protocol (`javascript:`)
- Encoded attack patterns
- CSS expressions and VBScript

### What It Allows
- Legitimate punctuation: `'`, `"`, `-`, `,`, `.`, `(`, `)`
- Normal text with apostrophes: `O'Connor`, `D'Angelo`
- Height measurements: `6'2"`, `5'8"`
- Team names with punctuation: `St. Louis`, `New York`

## Input Validation Service

The Input Validation Service handles structured fields that have specific format requirements and constraints:

### Height Field Validation
- **Format**: Must match pattern `feet'inches"` (e.g., 5'8", 6'9")
- **Range**: 4-8 feet, 0-11 inches
- **Examples**:
  - ✅ Valid: `"5'8""`, `"6'9""`, `"7'0""`
  - ❌ Invalid: `"6 feet 2 inches"`, `"180cm"`, `"9'0""`

### Weight Field Validation
- **Format**: Must contain numeric values
- **Range**: 50-400
- **Examples**:
  - ✅ Valid: `"185"`, `"200"`, `"150"`
  - ❌ Invalid: `"185 lbs"`, `"500"`, `"abc"`

### Age Field Validation
- **Format**: Must contain numeric values
- **Range**: 16-80
- **Examples**:
  - ✅ Valid: `"25"`, `"35"`, `"45"`
  - ❌ Invalid: `"25 years old"`, `"15"`, `"100"`

### Position Field Validation
- **Format**: Must be a valid position code for the sport
- **Valid Codes by Sport**:
  - **NBA**: C, F, G, PF, PG, SF, SG
  - **NFL**: C, CB, DB, DE, DL, DT, FB, G, ILB, K, LB, LS, NT, OL, OLB, OT, P, QB, RB, S, TE, WR
  - **NHL**: C, D, F, G, LW, RW
- **Examples**:
  - ✅ Valid NBA: `"PG"`, `"C"`, `"SF"`
  - ✅ Valid NFL: `"QB"`, `"WR"`, `"CB"`
  - ✅ Valid NHL: `"C"`, `"LW"`, `"D"`
  - ❌ Invalid: `"XYZ"`, `"Point Guard"`, `"123"`
  - ❌ Cross-sport: `"QB"` in NBA roster

### Player Number Validation
- **Format**: Must contain numeric values
- **Range**: 0-99
- **Examples**:
  - ✅ Valid: `"23"`, `"0"`, `"99"`
  - ❌ Invalid: `"100"`, `"abc"`, `"23A"`

### Handed Field Validation
- **Format**: Must be one of the predefined values
- **Valid Values**: L (Left), R (Right), B (Both)
- **Examples**:
  - ✅ Valid: `"L"`, `"R"`, `"B"`
  - ❌ Invalid: `"X"`, `"Left"`, `"Right"`, `"Both"`

## Usage Examples

### Complete Validation & Normalization Flow
```csharp
// In endpoint
// 1. XSS Validation (Security)
var (isValid, violations) = xssValidator.ValidateTextFields(roster, logger);
if (!isValid) return Results.BadRequest(...);

// 2. Structured Field Validation (Data Quality)
var (isValidStructured, validationErrors) = validator.ValidateModel(roster, logger);
if (!isValidStructured) return Results.BadRequest(...);

// 3. Data Normalization (Data Consistency)
var normalizedRoster = sanitizer.SanitizeModel(roster, logger);
var result = await repo.Create(normalizedRoster, logger);
```

### Individual Validation
```csharp
// Validate specific fields
var (isValidHeight, heightError) = validator.ValidateHeight("5'8\"");
var (isValidWeight, weightError) = validator.ValidateWeight("185");
var (isValidPosition, positionError) = validator.ValidatePosition("PG");

// Sport-specific position validation
var (isValidNbaPosition, nbaError) = validator.ValidatePositionForSport("PG", "NBA");
var (isValidNflPosition, nflError) = validator.ValidatePositionForSport("QB", "NFL");
var (isValidNhlPosition, nhlError) = validator.ValidatePositionForSport("C", "NHL");

// Handed validation
var (isValidHanded, handedError) = validator.ValidateHanded("L");
```

### Custom Normalization
```csharp
// Normalize individual strings
var cleanName = sanitizer.SanitizeString("  John   Doe  ", SanitizationLevel.Minimal);
var cleanCode = sanitizer.SanitizeString("ABC@123", SanitizationLevel.Strict);
```

## Logging

The service logs normalization activities:
- Information level: When fields are normalized
- Warning level: When significant changes are made
- Debug level: Detailed normalization steps

Example log entry:
```
[Information] Normalized field 'FirstName': '  John   Doe  ' -> 'John Doe'
```

## Testing

Use the provided test file `Test/Input-Sanitization-Test.http` to verify:

1. **Whitespace Normalization**: Extra spaces are removed
2. **Control Character Removal**: Non-printable characters are stripped
3. **Strict Field Validation**: Special characters are removed from strict fields
4. **Length Truncation**: Long strings are truncated in strict mode
5. **Punctuation Preservation**: Legitimate punctuation is preserved

## Security Benefits

1. **Defense in Depth**: Multiple layers of protection
2. **Data Integrity**: Ensures consistent, clean data without corruption
3. **XSS Prevention**: Validation blocks script injection entirely
4. **SQL Injection Prevention**: Removes malicious SQL patterns
5. **Buffer Overflow Prevention**: Length limits prevent attacks
6. **Audit Trail**: Comprehensive logging for security monitoring

## Data Quality Benefits

1. **No Data Corruption**: Legitimate punctuation is preserved
2. **Consistent Formatting**: Whitespace is normalized
3. **Clean Storage**: Control characters are removed
4. **Proper Validation**: Structured fields are properly validated

## Configuration

To add new field types or modify normalization levels:

```csharp
// In InputSanitizationService constructor
_fieldSanitizationLevels.Add("NewField", SanitizationLevel.Moderate);
```

## Best Practices

1. **Always use XSS validation first**: Security validation runs before normalization
2. **Log normalization activities**: Monitor for unusual patterns
3. **Test thoroughly**: Use provided test cases
4. **Review field configurations**: Ensure appropriate normalization levels
5. **Monitor logs**: Watch for repeated normalization patterns

## Integration Points

- **Endpoints**: All POST/PUT operations use validation and normalization
- **Models**: Automatically processes all string properties
- **Logging**: Integrates with existing logging infrastructure
- **Dependency Injection**: Registered as scoped service
