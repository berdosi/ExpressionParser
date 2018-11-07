using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Automation
{
    public class Expression
    {
        public List<Atom> Atoms { get; }

        public Expression(string inputString)
        {
            this.Atoms = new List<Atom>();
            string input = inputString;
            bool inString = false;
            int level = 0;
            
            Regex parenOpen = new Regex(@"^\(");
            Regex parenClose = new Regex(@"^\)");

            Regex whiteSpace = new Regex(@"^\s+");

            Regex numeric = new Regex(@"^[0-9\\\.]+");
            Regex variable = new Regex(@"^\[[^\]]+\]");
            Regex operatorCharacter = new Regex(@"^[+\-*/&,]");
            Regex functionCall = new Regex(@"^[a-zA-Z]+");
            Regex stringStart = new Regex("^\"");

            Regex inStringDoubleQuote = new Regex("^\"\"");
            Regex inStringNoQuote = new Regex("^[^\"]+");
            Regex inStringFinalQuote = new Regex("^\"");

            Regex whiteSpaceOnly = new Regex(@"^\s+$");
            
            // create list of Atoms
            while ((input != ""))
            {
                if (!inString)
                {
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
                else { // inString = true
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
            }
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
    }
    public class SubExpression
    {
        /*
         a SubExpression collects Atoms or SubExpressions on the same level.
         The most atomic SubExpression only contains an Atom, and has the same level as the atom it contains.
         Pun intended.
         A complex SubExpression contains a list of SubExpressions on the same level, and has a lower level than them.

         SubExpressions can be evaluated. The Expression is evaulated to the result of its SubExpressions' recursive evaluation.
         */

        private Atom atom = null;
        private List<SubExpression> subExpressions;

        public SubExpression(Atom atom)
        {
            this.atom = atom;
        }
        public SubExpression(List<SubExpression> subExpressions)
        {
            this.subExpressions = subExpressions;
        }
        public EncapsulatedData Evaluate(Dictionary<string, EncapsulatedData> environment)
        {
            if (this.atom != null)
            {
                switch (this.atom.type)
                {
                    case AtomType.Variable:
                        // the environment should only contain the in-scope variables.
                        // the names contain the brackets around them.
                        return environment[this.atom.content];
                    case AtomType.String:
                        return new EncapsulatedData(this.atom.content);
                    case AtomType.Number:
                        return new EncapsulatedData(Decimal.Parse(this.atom.content));
                    default:
                        // AtomType.FunctionCall, AtomType.Operator shouldn't happen
                        throw new Exception("Syntax error: invalid item type.");
                }
            }
            else // there are operators to handle
            {
                throw new NotImplementedException("Cannot handle operators.");
                // 1.   parse the funcion calls
                // 2.   evaluate the operators. They are on the same level, so parse them in their order of precedence.
                //      note, the operators are just weird-looking function calls
            }
        }
    }
    public class EncapsulatedData
    {
        DataType type;
        String stringData;
        Decimal numberData;
        DateTime dateTimeData;
        Boolean booleanData;
        List<EncapsulatedData> parameterList;

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
