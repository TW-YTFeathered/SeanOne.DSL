# Update Log

## **Beta**

### **beta 0.3.0**

*Focus:Create and refine `GUIDE.md` documentation*

- Added asynchronous (`async`) methods to support non-blocking operations
- Added `GUIDE.md` to complement `README.md` with extended documentation
- Renamed parameters to follow a more consistent and syntactically appropriate naming style, except for `tostring`, which directly invokes C#'s `ToString` method
- Replaced generic `throw` with specific exception type for clearer error semantics

### **beta 0.2.0**

*Focus:Bug fixs*

- Create a Git repository and push it to GitHub
- Renamed `namespace`, `class`, and `entry point` to align with `DSL` naming conventions
- Wrote Markdown files
- Refined the approach
- Fixed the issue where a parameter value was incorrectly interpreted as a parameter name
- Fixed the bug where `\r`, `\n`, etc. are incorrectly treated as whitespace

### **beta 0.1.0**

*Focus:Initial file creation*

- Extracted formatting module from an unpublished project and repurposed it as a DSL formatting tool
