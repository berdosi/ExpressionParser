using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Automation
{
    public class Expression
    {
        private List<Atom> Atoms;
        public Expression(string inputString)
        {
            string input = inputString;
            bool inString = false;
            Regex inStringDoubleQuote = new Regex("^\"\"");
            Regex inStringNoQuote = new Regex("^[^\"]+");
            Regex finalQuote = new Regex("^\"");
            int level = 0;
            return;
            while (input != "") {
                if (!inString) {
                } else {
                    if (inStringDoubleQuote.IsMatch(input)) {
                        var atom = Atoms[Atoms.Count -1];
                        atom.contentString += "\"";
                        input = input.Substring(2);
                    } else if (inStringNoQuote.IsMatch(input)) {
                        var atom = Atoms[Atoms.Count -1];
                        atom.contentString += inStringNoQuote.Match(input).Value;
                        input = input.Substring(inStringNoQuote.Match(input).Value.Length);
                    } else if (finalQuote.IsMatch(input)) {
                        inString = false;
                        input = input.Substring(1);
                    }
                }
            }
        }
    }
    enum AtomType
    {
        FunctionCall,
        Number,
        Operator,
        Paren_Close,
        Paren_Open,
        String,
        Variable,
    }
    class Atom
    {
        public string contentString;
        public Atom(AtomType type, string content) {

        }
    }
}
