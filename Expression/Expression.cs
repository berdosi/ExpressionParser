﻿using System;
using System.Text;
using System.Linq;
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
            Regex operatorCharacter = new Regex(@"^[+\-*/&,]|AND |OR |XOR |NOT |>=|<=|<|>|=");
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
        public override string ToString()
        {
            StringBuilder retValue = new StringBuilder("(Expression: ");
            foreach (Atom atom in Atoms) retValue.Append(atom.ToString());
            retValue.Append(")");
            return retValue.ToString();
        }
        public EncapsulatedData Evaluate(Dictionary<string, EncapsulatedData> environment) {
            // create list of SubExpressions.
            List<SubExpression> SubExpressions = new List<SubExpression>();
            foreach (Atom atom in Atoms)
            {
                SubExpressions.Add(new SubExpression(atom));
            }
            // TODO
            // "roll" it up : keep merging SubExpressions on the same level into separate SubExpressions, 
            // until there is only one top-level SE left. 
            //throw new NotImplementedException("grouping atoms on the same level into subexpressions is not implemented yet.");
            // Create a SubExpression and evaluate it.
            // Evaluate the top-level SubExpression (and it will evaluate its chilren)
            return new SubExpression(SubExpressions).Evaluate(environment);
        }
    }
}
