# DSL for C#

## Overview

A lightweight C# library designed for fast and flexible object-to-string formatting using a custom DSL syntax.

---

## Formatting with `dslInstruction`

### Syntax

```
[FunctionName] /param1:value1 /param2:"value with spaces" /param3:value3 ...
```

- Parameters start with `/` followed by the name and a colon
- Values containing spaces must be enclosed in double quotes
- Parameter names are case-sensitive
- Invalid function or parameter names will throw an `ArgumentException`

### Common Functions

| Function Name | Description |
|---------------|-------------|
| `fe` or `foreach` | Iterates over an `IEnumerable` or `IDictionary` and lists its contents |
| `basic`       | Default output method for formatting objects |

### Common Parameters

| Parameter Name | Example         | Description |
|----------------|-----------------|-------------|
| `end`          | `/end:\\n`      | Appends a string after each value. Use `Regex.Unescape()` to decode escape sequences |
| `tostring`     | `/tostring:F2`  | Applies formatting to items implementing `IFormattable`. Not applicable to dictionaries |

### IEnumerable-Specific Parameters

| Parameter Name       | Example                         | Description |
|----------------------|---------------------------------|-------------|
| `exclude-last-end`   | `/exclude-last-end:true`        | Omits the end string after the last item in the sequence. Applies to all `IEnumerable` types. |
| `last-concat-string` | `/last-concat-string:\" and \"` | Replace the connector between the last and second-to-last items with last_concat_string (falling back to end if not specified) |

### IDictionary-Specific Parameters

| Parameter Name | Example               | Description |
|----------------|-----------------------|-------------|
| `dicformat`    | `/dicformat:{0}=>{1}` | Format string for dictionary entries. `{0}` = key, `{1}` = value |
| `keyformat`    | `/keyformat:F2`       | Format string applied to dictionary keys |
| `valueformat`  | `/valueformat:F2`     | Format string applied to dictionary values |

---

### Functions Name
- `Format` – Formats an object into a string synchronously
- `FormatAsync` – Formats an object into a string asynchronously

## License

MIT License