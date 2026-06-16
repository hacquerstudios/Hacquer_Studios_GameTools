# Hacquer Studios GameTools (NuGet package)

This package provides a small toolkit targeted at game developers using .NET. Its primary feature is a reflection-based save/load helper along with developer utilities useful during design and debugging.

Package details
---------------
- Package id: Hacquer_Studios_GameTools
- Current version: 1.0.0
- Target framework: net10.0
- Includes XML documentation and a packaged README for NuGet consumers

Install
-------
From the dotnet CLI:

```powershell
dotnet add <YourProject> package Hacquer_Studios_GameTools --version 1.0.0
```

Or using NuGet Package Manager:

PM> Install-Package Hacquer_Studios_GameTools -Version 1.0.0

Quick usage
-----------
Add the namespace and use the provided helpers. The API surface is intentionally small:

- Hacquer_Studios_GameTools.Sav — main container class
- Hacquer_Studios_GameTools.Sav.Format — enum (Json, Binary)
- Hacquer_Studios_GameTools.Sav.SavIgnoreAttribute — attribute to exclude members from reflection save
- Hacquer_Studios_GameTools.Sav.Tests — developer-facing diagnostics for the save system

Example (console):

```csharp
using Hacquer_Studios_GameTools;

class Program
{
	static void Main()
	{
		// Run a quick diagnostic save test that prints discovered types and JSON
		var tester = new Sav.Tests();
		tester.Test();
	}
}
```

Notes
-----
- The library uses System.Text.Json for JSON serialization and includes a binary option placeholder. Review the source if you need custom converters or binary formats.
- Members marked with [SavIgnore] will be excluded from reflection-based save/load routines.
- XML documentation is included in the package for IntelliSense support.

Compatibility
-------------
The package targets .NET 10 (net10.0). Ensure your project targets a compatible framework or a higher version that supports net10.0 packages.

License
-------
This project includes a LICENSE.md in the package. See that file for license terms.

Support
-------
Open issues or feature requests on the GitHub repository:  
https://github.com/hacquerstudios/Hacquer_Studios_GameTools

If you're new to GitHub, you can create an issue by opening the link above, selecting the **Issues** tab, and choosing **New issue**.

