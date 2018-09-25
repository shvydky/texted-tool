# texted-tool
This simple .NET Core CLI tool allows to search and replace string in text file.

## Installation

Tool can be installed from Nuget:

```
dotnet tool install -g texted-tool
```

## Using 
```
Usage: texted-tool [arguments] [options]

Arguments:
  SourceFile        The name of source text file.
  DestFile          The name of destination text file.
  Pattern           A RegEx pattern that needs to be replaced in text file.
  Value             String Value

Options:
  -if|--if-changed  This option indicate that tool chould update target file only if changed (slow).
  -?|-h|--help      Show help information
  ```
