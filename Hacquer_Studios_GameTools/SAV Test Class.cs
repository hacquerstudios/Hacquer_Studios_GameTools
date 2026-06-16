using static Hacquer_Studios_GameTools.SavSystem.Sav;
using System.Collections.Generic;
#pragma warning disable CS1591
namespace Hacquer_Studios_GameTools.Tests
{
	public class TestPlayerData
	{
		// Basic fields
		public string Name { get; set; } = "TestPlayer";
		public int Level { get; set; } = 5;
		public float Health { get; set; } = 87.5f;

		// Nested object
		public InventoryData Inventory { get; set; } = new InventoryData();

		// List example
		public List<string> CompletedQuests { get; set; } = new()
		{
			"Tutorial",
			"FindTheDog",
			"OpenTheGate"
		};

		// Ignored member
		[SavIgnore]
		public string DebugInfo { get; set; } = "This should not be saved.";

		// A field instead of a property
		public bool IsHardcoreMode;

		// Default constructor required for reflection-based creation
		public TestPlayerData() { }
	}

	public class InventoryData
	{
		public int Gold { get; set; } = 123;
		public List<string> Items { get; set; } = new()
		{
			"Sword",
			"Health Potion",
			"Rope"
		};

		[SavIgnore]
		public string InternalNotes { get; set; } = "Not for serialization.";

		public InventoryData() { }
	}
}
#pragma warning restore CS1591
