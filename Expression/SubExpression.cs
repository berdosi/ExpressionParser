using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace Automation
{
	public class SubExpression
	{
		/*
		 a SubExpression collects Atoms or SubExpressions on the same level.
		 The most atomic SubExpression only contains an Atom, and has the same level as the atom it contains.
		 Pun intended.
		 A complex SubExpression contains a list of SubExpressions on the same level, and has a lower level than them.
		 (e.g. in "fun(1,2,3)" 1,2,3 are on level1, and fun is functioncall atom on level0.
				1,2,3 are converted to a ParameterList on level0,
				then fun" is called with it.

		 SubExpressions can be evaluated. The Expression is evaulated to the result of its SubExpressions' recursive evaluation.
		 */

		private readonly Atom atom = null;
		private readonly List<SubExpression> subExpressions;
		private readonly string functionName = "";
		private readonly EncapsulatedData parameterList;
		private readonly EncapsulatedData EvaluatedValue;

		public int level
		{
			get
			{
				if (IsAtom) return atom.level;
				return subExpressions[0].level - 1;
			}
		}
		private bool IsAtom { get { return atom != null; } }
		private bool IsFunctionCall { get { return functionName != ""; } }
		private bool IsEvaluable
		{
			get
			{
				return (IsAtom && (atom.type != AtomType.Operator))
						|| IsFunctionCall
						|| subExpressions != null
						|| EvaluatedValue != null;
			}
		}
		private bool IsOperator
		{
			get
			{
				if (IsAtom)
				{
					return this.atom.type == AtomType.Operator;
				}
				return false;
			}
		}

		public SubExpression(Atom atom)
		{
			this.atom = atom;
		}
		public SubExpression(List<SubExpression> subExpressions)
		{
			this.subExpressions = subExpressions;
		}
		public SubExpression(string functionName, EncapsulatedData parameterList)
		{
			// if it is initialized with a string and EncapsulatedData, it is a functioncall which was parsed.
			this.functionName = functionName;
			this.parameterList = parameterList;
		}
		public SubExpression(EncapsulatedData Evaluated)
		{
			this.EvaluatedValue = Evaluated;
		}
		public override string ToString()
		{
			if (IsAtom) return String.Format("(atom: {0})", atom.ToString());
			if (IsFunctionCall) return String.Format("(functionCall: {0}({1}))", functionName, parameterList.ToString());
			if (EvaluatedValue != null) return EvaluatedValue.ToString();

			StringBuilder returnValue = new StringBuilder("(subList ");

			foreach (SubExpression sub in subExpressions)
			{
				returnValue.Append(sub.ToString());
			}
			returnValue.Append(")");
			return returnValue.ToString();
		}
		public EncapsulatedData Evaluate(Dictionary<string, EncapsulatedData> environment)
		{
			if (EvaluatedValue != null) return EvaluatedValue;
			if (IsFunctionCall)
			{
				// [subexpression[atom.functionCall]] [SubExpression] sequences are converted
				// to SubExpressions with a functionName proprerty.
				return Library.Evaluate(functionName, parameterList);
			}
			if (IsAtom)
			{
				return this.atom.Evaluate(environment);
			}
			else // there are operators and function calls to handle
			{
				// 1.   parse the funcion calls
				List<SubExpression> functionCallsParsed = ParseFunctionCalls(environment);

				// 2.   evaluate the operators. They are on the same level, so parse them in their order of precedence.
				//      note,
				//      - operators are just weird-looking function calls
				//      - if it starts with an operator
				//          OR there is an operator to the left and a value to the right,
				//          THEN it is unary (e.g. minus).

				// 2.1. handle unary operators
				LinkedList<SubExpression> unaryOperatorsParsed = ParseUnaryOperators(functionCallsParsed, environment);

				// 2.2. Handle binary operators
				SubExpression binaryOperatorsParsed = ParseBinaryOperators(unaryOperatorsParsed, environment);
				// for each operator in order of precedence
				// while there is an operator between two evaluables
				// replace them with their evaluated value
				return binaryOperatorsParsed.EvaluatedValue;
			}
		}

		internal List<SubExpression> ParseFunctionCalls(Dictionary<string, EncapsulatedData> environment)
		{
			List<SubExpression> functionCallsParsed = new List<SubExpression>();

			for (int i = 0; i < subExpressions.Count; i++)
			{
				if ((subExpressions[i].atom != null))
				{
					// if it is an atom.functionCall, replace it with a functioncall subExpression:
					//  if the next subexpression contains an atom, the function doesn't take parameters
					//  else the next subexpression evaluates to a paramlist.
					// TODO (DONE): with functions with only one parameter, like random(1) ,
					// 1 should evaluate to a ParamList containing one item.
					// otherwise just add it to functionCallsParsed.
					Atom currentAtom = subExpressions[i].atom;
					AtomType atomType = subExpressions[i].atom.type;
					if (atomType == AtomType.FunctionCall)
					{
						// when there is a functionCall atom
						// it is either followed by an operator, like "random() + 1"
						//      - and it is a function without parameters
						// or it is followed by a subExpression evaluating to a parameterList,
						// like in random(1, 12 + 1) + 1
						//      [functionCall] [subExpression -> parameterlist] [operator] [number]
						//      1, 12 + 1 is evaluated to a parameterlist of (1, 13),
						//      because addition is performed first.
						//      parentheticals, like in "1, 12 + (1 / 2)" are treated
						//      on their own level as  1 / 2 will be a separate subExpression
						if (i == (subExpressions.Count - 1))
						{
							// this is the case when a zero parameter function closes the subexpression:
							// 1 + random()
							functionCallsParsed.Add(new SubExpression(currentAtom.content, null));
						}
						else if (subExpressions[i + 1].atom != null) // TODO check if there is i+1
						{
							// TODO NOTE it should be IsEvaluable instead.
							if (subExpressions[i + 1].atom.type != AtomType.Operator)
							{
								functionCallsParsed.Add(
										new SubExpression(currentAtom.content,
										subExpressions[i + 1].atom.Evaluate(environment)));
								i++; // skip the next item
							}
							else
							{
								// if the next atom is an operator, it is a zero-argument function like random() + 1
								functionCallsParsed.Add(new SubExpression(currentAtom.content, null));
							}
						}
						else if (subExpressions[i + 1].subExpressions != null)
						{
							// TODO check if the next atom is NOT a subExpression
							functionCallsParsed.Add(
									new SubExpression(
											currentAtom.content,
											subExpressions[i + 1].Evaluate(environment)));
							i++; // skip the next item
						}
						else throw new Exception("Unexpected token.");
					}
					else
					{
						functionCallsParsed.Add(subExpressions[i]);
					}
				}
				else // at this point it cannot be a functioncall expression
				{
					functionCallsParsed.Add(subExpressions[i]);
				}
			}
			return functionCallsParsed;
		}
		internal LinkedList<SubExpression> ParseUnaryOperators(List<SubExpression> functionCallsParsed, Dictionary<string, EncapsulatedData> environment)
		{
			LinkedList<SubExpression> unaryOperatorsParsed = new LinkedList<SubExpression>();

			// TODO review

			// while
			//          there is an operator followed by an evaluable atom or subexpression
			//          AND preceded by another operator,
			//      OR there is an operator at the start,
			// treat it like an unary operator (replace it in the sequence with its evaluated value)

			for (int i = functionCallsParsed.Count - 1; i >= 0; i--)
			{
				if (i > 1)
				{
					if (functionCallsParsed[i].IsEvaluable)
					{
						if (functionCallsParsed[i - 1].IsOperator)
						{
							if (functionCallsParsed[i - 2].IsOperator)
							{ // Operator1 Operator2 Evaluable --> Operator1 Evaluated
								unaryOperatorsParsed.AddFirst(
										new SubExpression(
												Library.UnaryOperators[
														functionCallsParsed[i - 1].atom.content](
														functionCallsParsed[i].Evaluate(environment))));
								i = i - 1;
							}
							else // Evaluable Operator Evaluable --> Evaluable Operator Evaluated
							{
								unaryOperatorsParsed.AddFirst(
										new SubExpression(
												functionCallsParsed[i].Evaluate(environment)));
							}
						}
						else // TODO check. two evaluables after each other may be an error.
						{
							unaryOperatorsParsed.AddFirst(
									new SubExpression(
											functionCallsParsed[i].Evaluate(environment)));
						}
					}
					else // if it is an Operator, leave it alone.
					{
						unaryOperatorsParsed.AddFirst(functionCallsParsed[i]);
					}
				}
				else if (i == 1)
				{
					if (functionCallsParsed[i].IsEvaluable)
					{
						if (functionCallsParsed[i - 1].IsOperator)
						{
							unaryOperatorsParsed.AddFirst(
									new SubExpression(
											Library.UnaryOperators[
													functionCallsParsed[i - 1].atom.content](
													functionCallsParsed[i].Evaluate(environment))));
							i = i - 1;
						}
						else // TODO : check: two evaluables after each other
						{
							unaryOperatorsParsed.AddFirst(
									new SubExpression(
											functionCallsParsed[i].Evaluate(environment)));
						}
					}
					else
					{
						if (functionCallsParsed[i - 1].IsOperator && functionCallsParsed[i].IsOperator)
						{
							unaryOperatorsParsed.First.Value =
									new SubExpression(
											Library.UnaryOperators[
													functionCallsParsed[i].atom.content](
													unaryOperatorsParsed.First.Value.Evaluate(environment)));
						}
						else
						{
							unaryOperatorsParsed.AddFirst(
									functionCallsParsed[i]);
						}
					}
				}
				else // i == 0
				{
					if (functionCallsParsed[i].IsOperator)
					{
						if (unaryOperatorsParsed.First.Value.IsEvaluable)
						{
							unaryOperatorsParsed.First.Value =
									new SubExpression(
											Library.UnaryOperators[
													functionCallsParsed[i].atom.content](
													unaryOperatorsParsed.First.Value.Evaluate(environment)));
						}
						else
						{
							throw new Exception("parse error.");
						}
					}
					else
					{
						if (functionCallsParsed[i].IsEvaluable)
						{
							unaryOperatorsParsed.AddFirst(
									new SubExpression(
														functionCallsParsed[i].Evaluate(environment)));
						}
						else
						{
							throw new Exception("parse error.");
						}
					}
				}
			}
			return unaryOperatorsParsed;
		}
		internal SubExpression ParseBinaryOperators(LinkedList<SubExpression> unaryOperatorsParsed, Dictionary<string, EncapsulatedData> environment)
		{
			// for each operator in order of precedence
			// while there is an operator between two evaluables
			// replace them with their evaluated value
			SubExpression[] ParsingOperator = unaryOperatorsParsed.ToArray();
			foreach (KeyValuePair<string, Func<EncapsulatedData, EncapsulatedData, EncapsulatedData>> operatorEntry in Library.Operators)
			{
				SubExpression Current = null;
				SubExpression BeforeCurrent = null;
				SubExpression BeforeBeforeCurrent = null;
				SubExpression[] ParsedOperator = { };
				for (int i = 0, j = 0; i < ParsingOperator.Length; i++, j++)
				{
					Current = ParsingOperator[i];
					// process the Evaluable Operator Evaluable pattern here.
					if ((BeforeCurrent != null) && (BeforeCurrent.IsOperator) && (BeforeCurrent.atom?.content == operatorEntry.Key))
					{
						ParsedOperator = ParsedOperator.Take(ParsedOperator.Count() - 2).ToArray(); // remove the last two elements
						BeforeCurrent = new SubExpression(
										operatorEntry.Value(
												BeforeBeforeCurrent.EvaluatedValue,
												Current.EvaluatedValue));
						j = j - 2;
						Array.Resize(ref ParsedOperator, j + 1);
						ParsedOperator[j] = BeforeCurrent;
						BeforeBeforeCurrent = null;
					}
					else
					{
						BeforeBeforeCurrent = BeforeCurrent != null ? BeforeCurrent : null;
						BeforeCurrent = Current;
						Array.Resize(ref ParsedOperator, j + 1);
						ParsedOperator[j] = Current;
					}
				}
				ParsingOperator = ParsedOperator; // save the current evaluation state 
			}
			return ParsingOperator[0];
		}
	}
}
