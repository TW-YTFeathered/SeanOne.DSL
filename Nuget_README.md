# SeanOne.DSL

A lightweight and efficient C# library for fast object-to-string formatting using a simple DSL syntax.

## Example Usage

```csharp
using SeanOne.DSL;

List<int> ints = Enumerable.Range(0, 10).ToList();

DslFormatter.Format(ints, "fe /tostring:F2 /end:\\n /exclude-last-end:true");

/* Output:
0.00
1.00
2.00
3.00
4.00
5.00
6.00
7.00
8.00
9.00
*/
```

## Documentation

For complete documentation and more examples, see the [README.md](https://github.com/TW-YTFeathered/SeanOne.DSL/blob/master/README.md) file.
For more code example, see the [GUIDE.md](https://github.com/TW-YTFeathered/SeanOne.DSL/blob/master/GUIDE.md) file.

## GitHub Repository

[SeanOne.DSL GitHub](https://github.com/TW-YTFeathered/SeanOne.DSL.git)

## License

MIT License
