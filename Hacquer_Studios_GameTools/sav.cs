using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;


#nullable enable

namespace Hacquer_Studios_GameTools.SavSystem
{
	///<summary>
	///public container class for save load logic 
	///using reflection to collect varibles then save as a json or birnay
	///</summary>
	public class Sav
	{
		/// <summary>
		/// Specifies the format used when saving or loading data.
		/// Supports JSON (readable) and Binary (compact).
		/// </summary>
		public enum Format
		{
			/// <summary>
			/// Tells the Sav to use Json
			/// </summary>
			Json,
			/// <summary>
			/// Tells the Sav to use binary
			/// </summary>
			Binary
		}

		/// <summary>
		/// Marks a field or property to be ignored by the Sav save system.
		/// </summary>
		[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
		public sealed class SavIgnoreAttribute : Attribute
		{
		}

		/// <summary>
		/// Provides diagnostic and reflection‑based testing tools for the Sav save system.
		/// This class can scan assemblies, detect data classes, list included/ignored members,
		/// and perform save/load validation.
		/// </summary>
		public class Tests
		{
			/// <summary>
			/// Defines the type of diagnostic test to perform.
			/// </summary>
			public enum TestAction
			{
				/// <summary>
				/// Performs a reflection scan and outputs JSON for all detected data classes.
				/// </summary>
				Save,

				/// <summary>
				/// Attempts to load JSON into all detected data classes to validate deserialization.
				/// </summary>
				Load
			}

			// ======================================================================
			// OVERLOAD 1 — DEFAULT BEHAVIOR (CALLER ASSEMBLY)
			// ======================================================================

			/// <summary>
			/// Overload 1: Runs the reflection test on the caller's assembly.
			/// Use this when Sav is imported into another project (e.g., a game),
			/// and you want to automatically scan that project's data classes.
			/// </summary>
			/// <param name="action">The diagnostic action to perform (Save or Load).</param>
			/// <param name="file">Optional file path used when performing a Load test.</param>
			public void Test(TestAction action = TestAction.Save, string? file = null)
			{
				Console.WriteLine("=== Sav Diagnostic Test ===");

				var asm = Assembly.GetCallingAssembly();
				RunTest(action, asm, file);

				Console.WriteLine("=== Test Complete ===");
			}

			// ======================================================================
			// OVERLOAD 2 — SPECIFIED ASSEMBLY
			// ======================================================================

			/// <summary>
			/// Overload 2: Runs the reflection test on a specified assembly.
			/// Use this when you want full control over which assembly is scanned,
			/// such as in unit tests or editor tooling.
			/// </summary>
			/// <param name="action">The diagnostic action to perform (Save or Load).</param>
			/// <param name="assemblyToScan">The assembly to reflect and analyze.</param>
			public void Test(TestAction action, Assembly assemblyToScan)
			{
				Console.WriteLine("=== Sav Diagnostic Test ===");

				RunTest(action, assemblyToScan, null);

				Console.WriteLine("=== Test Complete ===");
			}

			// ======================================================================
			// SHARED INTERNAL LOGIC
			// ======================================================================

			/// <summary>
			/// Internal shared logic used by all overloads.
			/// Performs reflection scanning and dispatches to Save or Load routines.
			/// </summary>
			private void RunTest(TestAction action, Assembly asm, string? file)
			{
				var types = asm.GetTypes()
					.Where(t =>
						t.IsClass &&
						!t.IsAbstract &&
						!t.IsSubclassOf(typeof(Attribute)) &&
						t != typeof(Sav) &&
						t != typeof(Tests) &&
						t.DeclaringType != typeof(Tests) &&
						!t.Name.StartsWith("<")) // exclude compiler‑generated types
					.ToList();

				Console.WriteLine($"Found {types.Count} data classes.");

				switch (action)
				{
					case TestAction.Save:
						RunSaveTest(types);
						break;

					case TestAction.Load:
						RunLoadTest(types, file);
						break;
				}
			}

			// ======================================================================
			// SAVE TEST
			// ======================================================================

			/// <summary>
			/// Performs a save‑diagnostic test by instantiating each data class,
			/// listing included/ignored members, and outputting a single combined JSON file.
			/// </summary>
			private void RunSaveTest(List<Type> types)
			{
				Console.WriteLine("\n=== SAVE TEST ===");

				var allData = new Dictionary<string, object?>();

				foreach (var type in types)
				{
					Console.WriteLine($"\nType: {type.FullName}");

					object? instance = null;

					try
					{
						instance = Activator.CreateInstance(type);
					}
					catch
					{
						Console.WriteLine("  Could not instantiate (no default constructor).");
					}

					BindingFlags flags =
						BindingFlags.Public |
						BindingFlags.NonPublic |
						BindingFlags.Instance |
						BindingFlags.Static;

					// REAL fields only (no backing fields, no compiler‑generated)
					var includedFields = type.GetFields(flags)
						.Where(f =>
							!f.Name.Contains("k__BackingField") &&
							!Attribute.IsDefined(f, typeof(Sav.SavIgnoreAttribute)) &&
							!Attribute.IsDefined(f, typeof(CompilerGeneratedAttribute)))
						.ToList();

					// REAL properties only
					var includedProps = type.GetProperties(flags)
						.Where(p => !Attribute.IsDefined(p, typeof(Sav.SavIgnoreAttribute)))
						.ToList();

					Console.WriteLine("  Included Members:");
					foreach (var f in includedFields)
						Console.WriteLine($"    Field: {f.Name} ({f.FieldType.Name})");

					foreach (var p in includedProps)
						Console.WriteLine($"    Property: {p.Name} ({p.PropertyType.Name})");

					Console.WriteLine("  Ignored Members:");
					foreach (var f in type.GetFields(flags)
						.Where(f => Attribute.IsDefined(f, typeof(Sav.SavIgnoreAttribute))))
						Console.WriteLine($"    Field: {f.Name}");

					foreach (var p in type.GetProperties(flags)
						.Where(p => Attribute.IsDefined(p, typeof(Sav.SavIgnoreAttribute))))
						Console.WriteLine($"    Property: {p.Name}");

					if (instance != null)
						allData[type.Name] = instance;
				}

				Console.WriteLine("\n=== COMBINED JSON OUTPUT ===");

				var combinedJson = JsonSerializer.Serialize(allData, new JsonSerializerOptions
				{
					WriteIndented = true
				});

				Console.WriteLine(combinedJson);
			}

			// ======================================================================
			// LOAD TEST
			// ======================================================================

			/// <summary>
			/// Performs a load‑diagnostic test by attempting to deserialize JSON
			/// into each detected data class.
			/// </summary>
			private void RunLoadTest(List<Type> types, string? file)
			{
				Console.WriteLine("\n=== LOAD TEST ===");

				if (string.IsNullOrWhiteSpace(file) || !File.Exists(file))
				{
					Console.WriteLine("No valid file provided for load test.");
					return;
				}

				var json = File.ReadAllText(file);

				foreach (var type in types)
				{
					Console.WriteLine($"\nAttempting to load into: {type.FullName}");

					try
					{
						var obj = JsonSerializer.Deserialize(json, type);

						if (obj != null)
							Console.WriteLine("  Load successful.");
						else
							Console.WriteLine("  Load returned null.");
					}
					catch (Exception ex)
					{
						Console.WriteLine($"  Load failed: {ex.Message}");
					}
				}
			}
		}



	}
}

