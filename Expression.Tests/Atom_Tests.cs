using Xunit;
using Automation;
using System;
using System.Collections.Generic;

namespace Automation.UnitTests
{
	public class Expression_Atoms_Tests
	{
		private string testExpressionString1 = "2334+(1+2)";
		private readonly Expression testExpression1;

		public Expression_Atoms_Tests()
		{
			testExpression1 = new Expression(testExpressionString1);
		}

		[Fact]
		public void canBeCreated()
		{
			//_expression = new Automation.Expression("[variable]");
			try
			{
				Expression _expression = new Expression(testExpressionString1);
				Assert.True(true, "Object is initialized for " + testExpressionString1);
			}
			catch (System.Exception ex)
			{
				Assert.True(false, "Cannot initialize object." + ex.Message + ex.StackTrace);
			}
		}

		[Fact]
		public void atomsDetected()
		{
			Assert.True(5 == testExpression1.Atoms.Count, testExpressionString1 + ": There are 5 atoms.");
		}

		[Fact]
		public void atomTypesCorrect()
		{
			Assert.True(testExpression1.Atoms[0].type == AtomType.Number, "1st atom's type is Number.");
			Assert.True(testExpression1.Atoms[1].type == AtomType.Operator, "2nd atom's type is Operator.");
			Assert.True(testExpression1.Atoms[2].type == AtomType.Number, "3rd atom's type is Number.");
			Assert.True(testExpression1.Atoms[3].type == AtomType.Operator, "4th atom's type is Operator.");
			Assert.True(testExpression1.Atoms[4].type == AtomType.Number, "5th atom's type is Number.");
		}

		[Fact]
		public void levelsCorrect()
		{
			Assert.True(testExpression1.Atoms[0].level == 0, "1st atom's level is 0.");
			Assert.True(testExpression1.Atoms[1].level == 0, "2nd atom's level is 0.");
			Assert.True(testExpression1.Atoms[2].level == 1, "3rd atom's level is 1.");
			Assert.True(testExpression1.Atoms[3].level == 1, "4th atom's level is 1.");
			Assert.True(testExpression1.Atoms[4].level == 1, "5th atom's level is 1.");
		}

		[Fact]
		public void contentStringsCorrect()
		{
			Assert.True(testExpression1.Atoms[0].content == "2334", "1st atom's content is \"2334\".");
			Assert.True(testExpression1.Atoms[1].content == "+", "2nd atom's content is \"+\".");
			Assert.True(testExpression1.Atoms[2].content == "1", "3rd atom's content is \"1\".");
			Assert.True(testExpression1.Atoms[3].content == "+", "4th atom's content is \"+\".");
			Assert.True(testExpression1.Atoms[4].content == "2", "5th atom's content is \"2\".");
		}
	}
}