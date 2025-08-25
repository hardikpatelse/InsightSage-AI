# Slug Generation Implementation

## Overview
This implementation adds automatic slug generation to the `AIModel` and `ChatMode` entity classes. When the `Name` property is set, the `Slug` property is automatically generated following specific formatting rules.

## Implementation Details

### Rules for Slug Generation
1. **Lowercase Conversion**: All letters are converted to lowercase
2. **Space Replacement**: All spaces are replaced with underscores (`_`)
3. **Whitespace Handling**: Tabs, newlines, and carriage returns are also replaced with underscores
4. **Trimming**: Leading and trailing whitespace is removed
5. **Empty Input**: If the name is null, empty, or only whitespace, the slug will be empty

### Classes Updated
- `InsightSage.Shared.Models.Entities.AIModel`
- `InsightSage.Shared.Models.Entities.ChatMode`

## Example Usage

### AIModel Examples
```csharp
var model = new AIModel { Name = "GPT-4 Turbo Model" };
// model.Slug will be: "gpt-4_turbo_model"

var model2 = new AIModel { Name = "Claude 3 Haiku" };
// model2.Slug will be: "claude_3_haiku"

var model3 = new AIModel { Name = "OpenAI GPT Model" };
// model3.Slug will be: "openai_gpt_model"
```

### ChatMode Examples
```csharp
var mode = new ChatMode { Name = "Creative Writing Mode" };
// mode.Slug will be: "creative_writing_mode"

var mode2 = new ChatMode { Name = "Code Assistant" };
// mode2.Slug will be: "code_assistant"

var mode3 = new ChatMode { Name = "Technical SUPPORT Mode" };
// mode3.Slug will be: "technical_support_mode"
```

## Implementation Pattern

Both classes use the same implementation pattern:

1. **Private Backing Field**: A private `_name` field stores the actual name value
2. **Property Setter**: The `Name` property setter automatically updates the `Slug` when changed
3. **Static Helper Method**: A private `GenerateSlugFromName` method handles the conversion logic
4. **Consistent Behavior**: Both classes behave identically for slug generation

## Code Structure

```csharp
public class AIModel : BaseEntity
{
    private string _name = string.Empty;
    
    public required string Name 
    { 
        get => _name;
        set
        {
            _name = value;
            Slug = GenerateSlugFromName(value);
        }
    }
    
    public string Slug { get; set; } = string.Empty;
    
    private static string GenerateSlugFromName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;

        return name.Trim()
                  .ToLowerInvariant()
                  .Replace(" ", "_")
                  .Replace("\t", "_")
                  .Replace("\n", "_")
                  .Replace("\r", "_");
    }
}
```

## Benefits

1. **Automatic Generation**: No need to manually set slugs when creating or updating entities
2. **Consistency**: All slugs follow the same formatting rules
3. **URL-Friendly**: Generated slugs are safe for use in URLs and database queries
4. **Maintainability**: Changes to name automatically update the slug
5. **Validation-Ready**: Can be used for unique constraints and lookups

## Testing

Comprehensive unit tests have been created to verify:
- ? Basic slug generation from names
- ? Space replacement with underscores
- ? Case conversion to lowercase
- ? Handling of empty/whitespace names
- ? Tab and newline character replacement
- ? Automatic updates when name changes

## Thread Safety

The implementation is thread-safe for single-instance operations. However, if multiple threads are modifying the same entity instance simultaneously, external synchronization may be required.

## Database Considerations

- The `Slug` property should be indexed for fast lookups
- Consider adding unique constraints on slug fields if appropriate
- Existing entities in the database will need their slugs populated through migration or update scripts

## Future Enhancements

Potential improvements could include:
- More sophisticated slug generation (handling special characters, numbers)
- Duplicate slug handling (appending numbers if slug already exists)
- Custom slug generation rules per entity type
- Slug history tracking for SEO purposes