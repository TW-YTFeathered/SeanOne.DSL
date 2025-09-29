# DSL for C#

## Overview

A lightweight C# library designed for fast and flexible object-to-string formatting using a custom DSL syntax.

---

## Formatting with `dslInstruction`

### Syntax

```text
[FunctionName] /param1:value1 /param2:"value with spaces" /param3:value3 ...
```

- Parameters start with `/` followed by the name and a colon
- Values containing spaces must be enclosed in double quotes
- Parameter names are case-sensitive

### Common Functions

| Function Name | Description |
|---------------|-------------|
| `fe` or `foreach` | Iterates over an `IEnumerable` or `IDictionary` and lists its contents |
| `basic`       | Default output method for formatting objects |

### Common Parameters

| Parameter Name | Example         | Description |
|----------------|-----------------|-------------|
| `end`          | `/end:\\n`      | Appends a string after each value. |
| `tostring`     | `/tostring:F2`  | Applies formatting to items implementing `IFormattable`. Not applicable to dictionaries. Use C#'s `ToString()` method. |

### IEnumerable-Specific Parameters

| Parameter Name       | Example                         | Description |
|----------------------|---------------------------------|-------------|
| `exclude-last-end`   | `/exclude-last-end:true`        | Omits the end string after the last item in the sequence. Applies to all `IEnumerable` types. |
| `final-pair-separator` | `/final-pair-separator:" and "` | Replaces the separator between the last two items in the sequence. Falls back to `end` if not specified. |

### IDictionary-Specific Parameters

| Parameter Name | Example               | Description |
|----------------|-----------------------|-------------|
| `dict-format`    | `/dict-format:{0}=>{1}` | Format string for dictionary entries: `{0}` represents the key, `{1}` represents the value. |
| `key-format`    | `/key-format:F2`       | Format string applied to dictionary keys |
| `value-format`  | `/value-format:F2`     | Format string applied to dictionary values |

---

### Functions Name

- `Format` – Formats an object into a string synchronously
- `FormatAsync` – Formats an object into a string asynchronously

## More Examples

For more detailed examples and advanced usage, see the [complete guide](https://github.com/TW-YTFeathered/SeanOne.DSL/blob/master/GUIDE.md).

## License

MIT License
