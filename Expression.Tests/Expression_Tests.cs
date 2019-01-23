using Xunit;
using Automation;
using System;
using System.Collections.Generic;

namespace Automation.UnitTests
{
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
		[Fact]
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
		[Fact]
		public void multipleNestedParen()
		{
			Assert.Equal((new Expression("2 * ((2 + 1))")).Evaluate(environment), new EncapsulatedData((Decimal)6));
			Assert.Equal((new Expression("2 * ((2 + 1)) * 2")).Evaluate(environment), new EncapsulatedData((Decimal)12));
			Assert.Equal((new Expression("((2 + 1)) * 2")).Evaluate(environment), new EncapsulatedData((Decimal)6));
		}
	}
	public class Functions_Tests
	{
		private Dictionary<string, EncapsulatedData> environment = new Dictionary<string, EncapsulatedData>(){
			{ "[one]", new EncapsulatedData((Decimal)1) },
			{ "[just a string]", new EncapsulatedData("test string") },
		};
		public Functions_Tests()
		{

		}
		[Fact]
		public void simpleFunction()
		{
			Assert.Equal(new Expression("example()").Evaluate(environment), new EncapsulatedData("a"));
		}
		[Fact]
		public void simpleFunctionAndOperator()
		{
			Assert.Equal(new Expression("\"a\" & example()").Evaluate(environment), new EncapsulatedData("aa"));
		}
		[Fact]
		public void simpleFunctionWithParameter()
		{
			Assert.Equal(new Expression("Chr(32)").Evaluate(environment), new EncapsulatedData(" "));
			Assert.Equal(new Expression("Chr(32) & \"a\"").Evaluate(environment), new EncapsulatedData(" a"));
			Assert.Equal(new Expression("Chr(31 + 1)").Evaluate(environment), new EncapsulatedData(" "));
			Assert.Equal(new Expression("Chr((31 + 1))").Evaluate(environment), new EncapsulatedData(" "));
			Assert.Equal(new Expression("Chr(2 * (15 + 1))").Evaluate(environment), new EncapsulatedData(" "));
			Assert.Equal(new Expression("Chr(((15 + 1) * 2))").Evaluate(environment), new EncapsulatedData(" "));
		}
		[Fact]
		public void twoParamFunction()
		{
			Assert.Equal(
				new Expression(
					"exampleConcat(\"a\",\"b\")").Evaluate(environment),
				new EncapsulatedData("ab"));
		}
		[Fact]
		public void multiParamFunction()
		{
			Assert.Equal(
				new Expression(
					"exampleConcatThree(\"a\",\"b\",\"c\")").Evaluate(environment),
				new EncapsulatedData("abc"));
		}
	}
}