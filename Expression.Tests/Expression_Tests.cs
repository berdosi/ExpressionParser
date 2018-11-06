using Xunit;
using Automation;

namespace Automation.UnitTests {
	public class Expression_Tests {
		private Expression _expression;

		public Expression_Tests() {
		}

		[Fact]
		public void canBeCreated() {	
			//_expression = new Automation.Expression("[variable]");
			try
			{
				_expression = new Expression("[variable]");
				Assert.True(true, "Object is initialized.");		
			}
			catch (System.Exception)
			{
				Assert.True(false, "Cannot initialize object.");
			}
		}
	}
}