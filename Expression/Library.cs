using System;
using System.Collections.Generic;

namespace Automation
{
	public static class Library {
        public static Dictionary<string, Func<EncapsulatedData, EncapsulatedData>> UnaryOperators
            = new Dictionary<string, Func<EncapsulatedData, EncapsulatedData>>
            {
                {   "NOT", 
                    (operand) => { // first can be NULL. The **second** parameter is negated.
                        if (operand.type == DataType.Boolean)
                            return new EncapsulatedData(!operand.booleanData);
                        throw new NotImplementedException(String.Format("Cannot negate '{0}'.", operand));
                    }
                },
                {   "-", 
                    (operand) => { // first can be NULL. The **second** parameter is negated.
                        if (operand.type == DataType.Number)
                            return new EncapsulatedData(- operand.numberData);
                        throw new NotImplementedException(String.Format("Cannot negate '{0}'.", operand));
                    }
                },
            };
        public static Dictionary<string, Func<EncapsulatedData, EncapsulatedData, EncapsulatedData>> Operators
            = new Dictionary<string, Func<EncapsulatedData, EncapsulatedData, EncapsulatedData>>
            {
                {   "/", 
                    (first, second) => {
                        if ((first.type == DataType.Number) && (second.type == DataType.Number))
                            return new EncapsulatedData(first.numberData / second.numberData);
                        throw new NotImplementedException(String.Format("Cannot divide '{0}' with '{1}'.", first, second));
                    }
                },
                {   "*", 
                    (first, second) => {
                        if ((first.type == DataType.Number) && (second.type == DataType.Number))
                            return new EncapsulatedData(first.numberData * second.numberData);
                        throw new NotImplementedException(String.Format("Cannot multiply '{0}' with '{1}'.", first, second));
                    }
                },
                {   "+", 
                    (first, second) => {
                        if ((first.type == DataType.Number) && (second.type == DataType.Number))
                            return new EncapsulatedData(first.numberData + second.numberData);
                        throw new NotImplementedException(String.Format("Cannot add '{0}' and '{1}'.", first, second));
                    }
                },
                {   "-", 
                    (first, second) => {
                        if ((first.type == DataType.Number) && (second.type == DataType.Number))
                            return new EncapsulatedData(first.numberData - second.numberData);
                        throw new NotImplementedException(String.Format("Cannot substract '{1}' from '{0}'.", first, second));
                    }
                },
                {   "<=", 
                    (first, second) => {
                        if ((first.type == DataType.Number) && (second.type == DataType.Number))
                            return new EncapsulatedData(first.numberData - second.numberData);
                        throw new NotImplementedException(String.Format("Cannot substract '{1}' from '{0}'.", first, second));
                    }
                },
                {   ">=", 
                    (first, second) => {
                        if ((first.type == DataType.Number) && (second.type == DataType.Number))
                            return new EncapsulatedData(first.numberData - second.numberData);
                        throw new NotImplementedException(String.Format("Cannot substract '{1}' from '{0}'.", first, second));
                    }
                },
                {   "<", 
                    (first, second) => {
                        if ((first.type == DataType.Number) && (second.type == DataType.Number))
                            return new EncapsulatedData(first.numberData - second.numberData);
                        throw new NotImplementedException(String.Format("Cannot substract '{1}' from '{0}'.", first, second));
                    }
                },
                {   ">", 
                    (first, second) => {
                        if ((first.type == DataType.Number) && (second.type == DataType.Number))
                            return new EncapsulatedData(first.numberData > second.numberData);
                        // TODO implement for dates
                        throw new NotImplementedException(String.Format("Cannot substract '{1}' from '{0}'.", first, second));
                    }
                },
                {   "=", 
                    (first, second) => {
                        if ((first.type == DataType.Number) && (second.type == DataType.Number))
                            return new EncapsulatedData(first.numberData == second.numberData);
                        if ((first.type == DataType.Boolean) && (second.type == DataType.Boolean))
                            return new EncapsulatedData(first.booleanData == second.booleanData);
                        if ((first.type == DataType.DateTime) && (second.type == DataType.DateTime))
                            return new EncapsulatedData(first.dateTimeData == second.dateTimeData);
                        throw new NotImplementedException(String.Format("Cannot substract '{1}' from '{0}'.", first, second));
                    }
                },
                {   "AND", 
                    (first, second) => {
                        if ((first.type == DataType.Number) && (second.type == DataType.Number))
                            return new EncapsulatedData(first.numberData - second.numberData);
                        throw new NotImplementedException(String.Format("Cannot substract '{1}' from '{0}'.", first, second));
                    }
                },
                {   "OR", 
                    (first, second) => {
                        if ((first.type == DataType.Number) && (second.type == DataType.Number))
                            return new EncapsulatedData(first.numberData - second.numberData);
                        throw new NotImplementedException(String.Format("Cannot substract '{1}' from '{0}'.", first, second));
                    }
                },
                {   "XOR", 
                    (first, second) => {
                        if ((first.type == DataType.Number) && (second.type == DataType.Number))
                            return new EncapsulatedData(first.numberData - second.numberData);
                        throw new NotImplementedException(String.Format("Cannot substract '{1}' from '{0}'.", first, second));
                    }
                },
                {   "&", 
                    (first, second) => {
                        return new EncapsulatedData(first.ToString() + second.ToString());
                    }
                },
            };
        private static Dictionary<string, Func<EncapsulatedData, EncapsulatedData>> FunctionLibrary 
            = new Dictionary<string, Func<EncapsulatedData, EncapsulatedData>>
            {
                { 
                    "example",
                    inputData => { return new EncapsulatedData("a"); } },
                { 
                    "Chr",
                    inputData => {
                        return new EncapsulatedData(Char.ConvertFromUtf32(Convert.ToInt32(inputData.numberData)));
                    }
                },
            };

		public static EncapsulatedData Evaluate(string functionName)
        {
            return FunctionLibrary[functionName](null);
        }
        public static EncapsulatedData Evaluate(string functionName, EncapsulatedData parameters)
        {
            // NOTE parameters.type is not always parameterList. 
            // SubExpressions following single-parameter functions evaluate to EncapsulatedData with a different type.
            return FunctionLibrary[functionName](parameters);
        }
    }
}
