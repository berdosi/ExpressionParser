using System;
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

        public int level
        {
            get
            {
                if (this.atom != null) return this.atom.level;
                else return this.subExpressions[0].level - 1;
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
        public EncapsulatedData Evaluate(Dictionary<string, EncapsulatedData> environment)
        {
            if (this.functionName != "")
            {
                // [subexpression[atom.functionCall]] [SubExpression] sequences are converted 
                // to SubExpressions with a functionName proprerty.
                return Library.Evaluate(functionName, parameterList);
            }
            if (this.atom != null)
            {
                return this.atom.Evaluate(environment);
            }
            else // there are operators and function calls to handle
            {
                // 1.   parse the funcion calls
                List<SubExpression> functionCallsParsed = new List<SubExpression>();

                for (int i = 0; i < subExpressions.Count; i++)
                {
                    if ((subExpressions[i].atom != null))
                    {
                        // if it is an atom.functionCall, replace it with a functioncall subExpression:
                        //  if the next subexpression contains an atom, the function doesn't take parameters
                        //  else the next subexpression evaluates to a paramlist.
                        // TODO : with functions with only one parameter, like random(1) , 
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
                // 2.   evaluate the operators. They are on the same level, so parse them in their order of precedence.
                //      note,
                //      - operators are just weird-looking function calls
                //      - if it starts with an operator 
                //          OR there is an operator to the left and a value to the right, 
                //          THEN it is unary (e.g. minus). 
                
                // 2.1. handle unary operators
                List<SubExpression> unaryOperatorsParsed = new List<SubExpression>();
                // while 
                //          there is an operator followed by an evaluable atom or subexpression
                //          AND preceded by another operator, 
                //      OR there is an operator at the start, 
                // treat it like an unary operator (replace it in the sequence with its evaluated value)

                // 2.2. Handle binary operators
                // for each operator in order of precedence
                // while there is an operator between two evaluables
                // replace them with their evaluated value

                throw new NotImplementedException("Cannot handle operators.");
            }
        }
    }
}
