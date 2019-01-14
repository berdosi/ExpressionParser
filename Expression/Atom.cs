using System;
using System.Collections.Generic;

namespace Automation
{
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
        public override string ToString()
        {
            return String.Format("(Atom<{0}@{2}> {1})", type, content, level);
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
}
