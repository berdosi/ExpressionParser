using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Automation
{
    public class Expression
    {
        public List<Atom> Atoms { get; }

        private void parseGeneral(ref string input, ref int level, ref bool inString)
        {
            Regex parenOpen = new Regex(@"^\(");
            Regex parenClose = new Regex(@"^\)");

            Regex whiteSpace = new Regex(@"^\s+");

            Regex numeric = new Regex(@"^[0-9\\\.]+");
            Regex variable = new Regex(@"^\[[^\]]+\]");
            Regex operatorCharacter = new Regex(@"^[+\-*/&,]");
            Regex functionCall = new Regex(@"^[a-zA-Z]+");
            Regex stringStart = new Regex("^\"");

            if (whiteSpace.IsMatch(input))
            {
                input = input.Remove(0, whiteSpace.Match(input).Value.Length);
            }
            else if (parenOpen.IsMatch(input))
            {
                level++;
                input = input.Remove(0, 1);
            }
            else if (parenClose.IsMatch(input))
            {
                level--;
                input = input.Remove(0, 1);
            }
            else if (numeric.IsMatch(input))
            {
                string contentString = numeric.Match(input).Value;
                Atom currentAtom = new Atom(AtomType.Number, contentString, level);
                this.Atoms.Add(currentAtom);
                input = input.Remove(0, contentString.Length);
            }
            else if (variable.IsMatch(input))
            {
                string contentString = variable.Match(input).Value;
                Atom currentAtom = new Atom(AtomType.Variable, contentString, level);
                this.Atoms.Add(currentAtom);
                input = input.Remove(0, contentString.Length);
            }
            else if (operatorCharacter.IsMatch(input))
            {
                string contentString = operatorCharacter.Match(input).Value;
                Atom currentAtom = new Atom(AtomType.Operator, contentString, level);
                this.Atoms.Add(currentAtom);
                input = input.Remove(0, contentString.Length);
            }
            else if (functionCall.IsMatch(input))
            {
                string contentString = functionCall.Match(input).Value;
                Atom currentAtom = new Atom(AtomType.FunctionCall, contentString, level);
                this.Atoms.Add(currentAtom);
                input = input.Remove(0, contentString.Length);
            }
            else if (stringStart.IsMatch(input))
            {
                Atom currentAtom = new Atom(AtomType.String, "", level);
                this.Atoms.Add(currentAtom);
                input = input.Remove(0, 1);
                inString = true;
            }
        }
        private void parseInString(ref string input, ref bool inString)   
        {
            Regex inStringDoubleQuote = new Regex("^\"\"");
            Regex inStringNoQuote = new Regex("^[^\"]+");
            Regex inStringFinalQuote = new Regex("^\"");

            if (inStringDoubleQuote.IsMatch(input))
            {
                var atom = Atoms[Atoms.Count -1];
                atom.content += "\"";
                input = input.Remove(0, 2);
            }
            else if (inStringNoQuote.IsMatch(input))
            {
                var atom = Atoms[Atoms.Count -1];
                atom.content += inStringNoQuote.Match(input).Value;
                input = input.Remove(0, inStringNoQuote.Match(input).Value.Length);
            }
            else if (inStringFinalQuote.IsMatch(input))
            {
                inString = false;
                input = input.Remove(0, 1);
            }

        }
        public Expression(string inputString)
        {
            this.Atoms = new List<Atom>();
            string input = inputString;
            bool inString = false;
            int level = 0;

            // create list of Atoms
            while ((input != ""))
            {
                if (!inString) parseGeneral(ref input, ref level, ref inString);
                else parseInString(ref input, ref inString);
            }
        }
        public EncapsulatedData Evaluate() {
            
            // create list of SubExpressions.
            foreach (Atom atom in Atoms)
            {
                List<SubExpression> SubExpressions = new List<SubExpression>();
                SubExpressions.Add(new SubExpression(atom));
            }
            // "roll" it up : keep merging SubExpressions on the same level into separate SubExpressions, 
            // until there is only one top-level SE left. 
            
            // Evaluate the top-level SubExpression (and it will evaluate its chilren)
            
            throw new NotImplementedException();
        }
    }
    public enum AtomType
    {
        FunctionCall,
        Number,
        Operator,
        String,
        Variable,
    }
    public enum DataType
    {
        Boolean,
        Date,
        DateTime,
        Number,
        ParameterList,
        String,
    }
    public class Atom
    {
        public string content;
        public readonly int level;
        public readonly AtomType type;

        public Atom(AtomType type, string content, int level)
        {
            this.level = level;
            this.content = content;
            this.type = type;
        }
        public EncapsulatedData Evaluate(Dictionary<string, EncapsulatedData> environment)
        {
            switch (this.type)
            {
                case AtomType.Variable:
                    // the environment should only contain the in-scope variables.
                    // the names contain the brackets around them.
                    return environment[this.content];
                case AtomType.String:
                    return new EncapsulatedData(this.content);
                case AtomType.Number:
                    return new EncapsulatedData(Decimal.Parse(this.content));
                default:
                    // AtomType.FunctionCall, AtomType.Operator shouldn't happen
                    throw new Exception("Syntax error: invalid item type.");
            }
        }
    }
    public class SubExpression
    {
        /*
         a SubExpression collects Atoms or SubExpressions on the same level.
         The most atomic SubExpression only contains an Atom, and has the same level as the atom it contains.
         Pun intended.
         A complex SubExpression contains a list of SubExpressions on the same level, and has a lower level than them.
         (e.g. in "fun(1,2,3)" 1,2,3 are on level1, and fun is on level0. 
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
                // [functionCall] [SubExpression] sequences are converted 
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
                //      note, the operators are just weird-looking function calls
                throw new NotImplementedException("Cannot handle operators.");
            }
        }
    }
    public static class Library {
		public static EncapsulatedData Evaluate(string functionName)
        {
            throw new NotImplementedException();
        }
        public static EncapsulatedData Evaluate(string functionName, EncapsulatedData parameters)
        {
            // NOTE parameters.type is not always parameterList. 
            // SubExpressions following single-parameter functions evaluate to EncapsulatedData with a different type.
            // if the 
            throw new NotImplementedException();
        }
    }
    public class EncapsulatedData
    {
        public DataType type { get; }
        public String stringData { get; }
        public Decimal numberData { get; }
        public DateTime dateTimeData { get; }
        public Boolean booleanData { get; }
        public List<EncapsulatedData> parameterList { get; }

        public EncapsulatedData(String data)
        {
            type = DataType.String;
            stringData = data;
        }
        public EncapsulatedData(Decimal data)
        {
            type = DataType.Number;
            numberData = data;
        }
        public EncapsulatedData(DateTime data)
        {
            type = DataType.DateTime;
            dateTimeData = data;
        }
        public EncapsulatedData(Boolean data)
        {
            type = DataType.String;
            booleanData = data;
        }
        public EncapsulatedData(List<EncapsulatedData> data)
        {
            type = DataType.ParameterList;
            parameterList = data;
        }
    }
}
