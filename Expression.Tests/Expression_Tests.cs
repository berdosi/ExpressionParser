using Xunit;
using Automation;

namespace Automation.UnitTests {
	public class Expression_Tests {

		public Expression_Tests() {
		}

		[Fact]
		public void canBeCreated() {	
			//_expression = new Automation.Expression("[variable]");
			try
			{
				Expression _expression = new Expression("2334+(1+2)");
				Assert.True(true, "Object is initialized.");		
			}
			catch (System.Exception ex)
			{
				Assert.True(false, "Cannot initialize object." + ex.Message + ex.StackTrace);
			}
		}
	}
}