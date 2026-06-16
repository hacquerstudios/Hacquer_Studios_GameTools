using System.Reflection;
using Hacquer_Studios_GameTools.SavSystem;
using Hacquer_Studios_GameTools.Tests; // <-- This is where SAV Test Class lives
using Xunit;

namespace Save_Reflection_Test
{
	public class SavTests
	{
		[Fact]
		public void SaveTestPlayerData()
		{
			var tester = new Sav.Tests();

			// Tell Sav to scan the assembly that contains SAV Test Class
			Assembly assemblyToScan = typeof(TestPlayerData).Assembly;

			tester.Test(Sav.Tests.TestAction.Save, assemblyToScan);
		}
	}
}
