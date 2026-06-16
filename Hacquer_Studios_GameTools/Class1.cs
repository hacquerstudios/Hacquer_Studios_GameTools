using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Text.Json;
#nullable enable
namespace Hacquer_Studios_GameTools
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
			/// Saves data as human‑readable JSON.
			/// </summary>
			Json,
			/// <summary>
			/// Saves data as compact binary.
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
		/// Provides diagnostic tools for the Sav save system.
		/// </summary>
		public class Tests
		{
			/// <summary>
			/// Defines the actions for Test
			/// </summary>
			public enum TestAction
			{
				/// <summary>
				/// Test the saving of the reflected data
				/// </summary>
				Save,
				/// <summary>
				/// test the loading of the reflected data
				/// </summary>
				Load
			}
			/// <summary>
			/// Runs a diagnostic check on the reflection‑based save system.
			/// This method scans all data classes in the current project,
			/// verifies that fields and properties are correctly detected,
			/// and ensures that members marked with <c>[SavIgnore]</c>
			/// are excluded. When performing a save test, the resulting
			/// JSON is written to the console. When performing a load test,
			/// the specified file is read and validated.
			/// </summary>
			/// <param name="action">
			/// Determines whether the diagnostic performs a save or load test.
			/// Defaults to <see cref="TestAction.Save"/>.
			/// </param>
			/// <param name="file">
			/// Optional file path used when performing a load test.
			/// </param>
			public void Test(TestAction action = TestAction.Save, string? file = null)
			{
				Console.WriteLine("=== Sav Diagnostic Test ===");

				// Only scan THIS project (not whole solution)
				var asm = typeof(Sav).Assembly;

				// Collect all data classes in the project
				var types = asm.GetTypes()
					.Where(t =>
						t.IsClass &&
						!t.IsAbstract &&
						!t.IsSubclassOf(typeof(Attribute)) &&
						t != typeof(Sav)) // exclude the Sav class itself
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

				Console.WriteLine("=== Test Complete ===");
			}
			private void RunSaveTest(List<Type> types)
			{
				Console.WriteLine("\n=== SAVE TEST ===");

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

					var fields = type.GetFields(flags);
					var props = type.GetProperties(flags);

					var includedFields = fields.Where(f => !Attribute.IsDefined(f, typeof(SavIgnoreAttribute)));
					var includedProps = props.Where(p => !Attribute.IsDefined(p, typeof(SavIgnoreAttribute)));

					Console.WriteLine("  Included Members:");
					foreach (var f in includedFields)
						Console.WriteLine($"    Field: {f.Name} ({f.FieldType.Name})");

					foreach (var p in includedProps)
						Console.WriteLine($"    Property: {p.Name} ({p.PropertyType.Name})");

					Console.WriteLine("  Ignored Members:");
					foreach (var f in fields.Where(f => Attribute.IsDefined(f, typeof(SavIgnoreAttribute))))
						Console.WriteLine($"    Field: {f.Name}");

					foreach (var p in props.Where(p => Attribute.IsDefined(p, typeof(SavIgnoreAttribute))))
						Console.WriteLine($"    Property: {p.Name}");

					// If we successfully created an instance, serialize it
					if (instance != null)
					{
						var json = JsonSerializer.Serialize(instance, new JsonSerializerOptions
						{
							WriteIndented = true
						});

						Console.WriteLine("\n  JSON Output:");
						Console.WriteLine(json);
					}
				}
			}
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
