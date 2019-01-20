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
	public class Expression_ASTElements_Tests
	{
		public Expression_ASTElements_Tests()
		{

		}
	}
	public class Expression_FuncionParser_Tests
	{
		public Expression_FuncionParser_Tests()
		{

		}
		[Fact]
		public void testInternals()
		{
			string location = "";
			try
			{
				location = "initializes with an encapsulated data item";
				SubExpression subExpression = new SubExpression(new EncapsulatedData("example string"));
				location = "can parse unaries";
				LinkedList<SubExpression> unaryParsed = subExpression.ParseUnaryOperators(new List<SubExpression>(), new Dictionary<string, EncapsulatedData>());

				Assert.True(true, "Didn't fail with empties. ");
			}
			catch (Exception e)
			{
				Assert.True(false, String.Format("Fails with empties {1} {0}", (e != null) ? e.Message : "", location));
			}
		}
	}
	public class Expression_Evaluation_Tests
	{
		private Dictionary<string, EncapsulatedData> emptyEnvironment = new Dictionary<string, EncapsulatedData>();
		public Expression_Evaluation_Tests()
		{
		}
		[Fact]
		public void canAddTwoNumbers()
		{
			string testExpressionString1 = "1 + 1";
			Expression testExpression1 = new Expression(testExpressionString1);
			try
			{
				EncapsulatedData result = testExpression1.Evaluate(emptyEnvironment); 
				Assert.True(true, String.Format("Valid expression '{0}' is evaluated.", testExpressionString1));
				Assert.True(result.Equals(new EncapsulatedData((Decimal)2)), String.Format("Equals 2.", testExpressionString1));
			}
			catch (Exception e)
			{
				Assert.True(
					false,
					String.Format(
						"Evaluating valid expression '{0}' should not throw. ({1})",
						testExpressionString1,
						e.Message));
			}
		}
		public void canCombine()
		{
			string testExpressionString1 = "2 * 3 + 1 + 4/2";
			Expression testExpression1 = new Expression(testExpressionString1);
			try
			{
				EncapsulatedData result = testExpression1.Evaluate(emptyEnvironment); 
				Assert.True(result.Equals(new EncapsulatedData((Decimal)9)), String.Format("Equals 7.", testExpressionString1));
			}
			catch (Exception e)
			{
				Assert.True(false, String.Format("Evaluating valid expression '{0}' should not throw.", testExpressionString1));
			}
		}
		[Fact]
		public void evaluatesSimple()
		{
			Expression testExpression1 = new Expression("1");
			try
			{
				EncapsulatedData retValue = testExpression1.Evaluate(emptyEnvironment);
				//if (retValue != null)				Console.WriteLine(retValue.ToString());
				//else Console.WriteLine("nullvalue");
				Assert.True(true, String.Format("Valid expression '{0}' is evaluated.", "1"));
			}
			catch (Exception e)
			{
				//Console.WriteLine("--->" + e.Message);
				Assert.True(false, String.Format("Evaluating valid expression '{0}' should not throw.", "1"));
			}
		}
		[Fact]
		public void evaluateUnaries()
		{
			Assert.Equal((new Expression("-1")).Evaluate(emptyEnvironment), new EncapsulatedData((Decimal)(-1)));
			Assert.Equal((new Expression("--1")).Evaluate(emptyEnvironment), new EncapsulatedData((Decimal)(1)));
			Assert.Equal((new Expression("---1")).Evaluate(emptyEnvironment), new EncapsulatedData((Decimal)(-1)));
		}
		[Fact]
		public void evaluateCombinedUnaryBinary()
		{
			Assert.Equal((new Expression("1--1")).Evaluate(emptyEnvironment), new EncapsulatedData((Decimal)(2)));
			Assert.Equal((new Expression("1---1")).Evaluate(emptyEnvironment), new EncapsulatedData((Decimal)(0)));
			Assert.Equal((new Expression("1/--1")).Evaluate(emptyEnvironment), new EncapsulatedData((Decimal)(1)));
		}
	}
	public class Environment_Tests
	{
		private Dictionary<string, EncapsulatedData> environment = new Dictionary<string, EncapsulatedData>(){
			{ "[one]", new EncapsulatedData((Decimal)1) },
			{ "[just a string]", new EncapsulatedData("test string") },
		};
		public Environment_Tests()
		{
		}
		[Fact]
		public void simpleEvalNumbers()
		{
			Assert.Equal((new Expression("[one]")).Evaluate(environment), new EncapsulatedData((Decimal)1));
			Assert.Equal((new Expression("[one]+[one]")).Evaluate(environment), new EncapsulatedData((Decimal)2));
		}
		[Fact]
		public void simpleEvalStrings()
		{
			Assert.Equal((new Expression("[just a string]")).Evaluate(environment), new EncapsulatedData("test string"));
		}		
	}
	public class Parentheticals_Tests
	{
		private Dictionary<string, EncapsulatedData> environment = new Dictionary<string, EncapsulatedData>(){
			{ "[one]", new EncapsulatedData((Decimal)1) },
			{ "[just a string]", new EncapsulatedData("test string") },
		};

		public Parentheticals_Tests()
		{

		}

		[Fact]
		public void simpleParen()
		{
			Assert.Equal((new Expression("2 * (2 + 1)")).Evaluate(environment), new EncapsulatedData((Decimal)6));
		}
	}
}