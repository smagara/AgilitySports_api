# XSS Protection Implementation

## Overview

This API has been enhanced with comprehensive XSS (Cross-Site Scripting) protection for all text fields in roster operations. The implementation prevents malicious content from being saved to the database and provides detailed logging of attempted attacks.

## Features

### 1. XSS Pattern Detection
The system detects various XSS attack patterns including:
- Any tags, including script or html, with <> characters, even if encoded
- Event handlers (`onclick`, `onload`, `onerror`, etc.)
- JavaScript protocol (`javascript:`)
- Data URLs (`data:text/html`)
- Encoded characters (`&#x3C;`, `%3C`, etc.)
- CSS expressions (`expression()`)
- VBScript protocol (`vbscript:`)

### 2. Comprehensive Logging
- All XSS attempts are logged with detailed information
- Request details including IP address, user agent, and headers are captured
- Violations are logged with specific field names and detected patterns

### 3. Helpful Error Responses
When XSS is detected, the API returns a 400 Bad Request with:
- Clear error message indicating XSS attempt
- User-friendly explanation
- Detailed list of violations for debugging

## Implementation Details

### Services
- **XssValidationService**: Core validation logic with regex pattern matching
- **XssLoggingMiddleware**: Request logging middleware for monitoring

### Protected Endpoints
All POST and PUT operations for roster management are protected:

#### NBA
- `POST /api/nba/roster` - Create player
- `PUT /api/nba/roster` - Update player

#### NFL
- `POST /api/nfl/roster` - Create player
- `PUT /api/nfl/roster` - Update player

#### NHL
- `POST /api/nhl/roster` - Create player
- `PUT /api/nhl/roster` - Update player

### Protected Fields
All string properties in roster models are validated:
- FirstName, LastName
- Team, TeamName
- Position, Number
- Height, Weight
- College
- BirthPlace, BirthCountry
- And all other string fields

## Testing

Use the provided `Test/XSS-Test.http` file to test the XSS protection:

```bash
# Test with script tag
POST /api/nba/roster
{
  "FirstName": "<script>alert('XSS')</script>",
  "LastName": "Mouse"
}

# Expected Response: 400 Bad Request
{
  "error": "XSS attempt detected",
  "message": "The request contains potentially malicious content and has been blocked for security reasons.",
  "details": ["XSS pattern detected in field 'FirstName': <script>alert('XSS')</script>"]
}
```

## Logging

XSS attempts are logged with the following information:
- Timestamp
- IP Address
- User Agent
- Request Method and Path
- Specific violations detected
- Field names containing malicious content

Example log entry:
```
WARN: XSS attempt blocked in NBA roster creation. Violations: XSS pattern detected in field 'FirstName': <script>alert('XSS')</script>
```

## Configuration

The XSS protection is automatically enabled for all roster operations. No additional configuration is required.

## Security Considerations

1. **Pattern Updates**: The regex patterns are regularly updated to catch new XSS vectors
2. **Performance**: Validation is performed before database operations to prevent unnecessary load
3. **Logging**: All attempts are logged for security monitoring and analysis
4. **User Experience**: Clear error messages help legitimate users understand why their request was rejected

## Future Enhancements

Potential improvements could include:
- Rate limiting for repeated XSS attempts
- IP-based blocking for persistent attackers
- Integration with security monitoring systems
- Customizable validation rules per field type

