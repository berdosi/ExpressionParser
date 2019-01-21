using System;
using System.Collections.Generic;

namespace Automation
{
	public class EncapsulatedData
    {
        public DataType type { get; }
        public String stringData { get; }
        public Decimal numberData { get; }
        public DateTime dateTimeData { get; }
        public Boolean booleanData { get; }
        public LinkedList<EncapsulatedData> parameterList { get; }

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
        public EncapsulatedData(LinkedList<EncapsulatedData> data)
        {
            type = DataType.ParameterList;
            parameterList = data;
        }

        public override string ToString()
        {
            Dictionary<DataType, Func<string>> rendererLookup = new Dictionary<DataType, Func<string>>
            {
                { DataType.Boolean,         () => booleanData.ToString() },
                { DataType.Date,            () => dateTimeData.ToString() },
                { DataType.DateTime,        () => dateTimeData.ToString() },
                { DataType.Number,          () => numberData.ToString() },
                { DataType.ParameterList,   () => parameterList.ToString() },
                { DataType.String,          () => stringData },
            };
            try
            {
                return rendererLookup[this.type]();
            }
            catch
            {
                return "Cannot render value as a string.";
            }
        }
        public override bool Equals(object obj)
        {            
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            
            if (this.type != ((EncapsulatedData)obj).type) return false;
            switch (this.type)
            {
                case DataType.Number:
                    return this.numberData == ((EncapsulatedData)obj).numberData;
                case DataType.String:
                    return this.stringData == ((EncapsulatedData)obj).stringData;
                case DataType.Boolean:
                    return this.booleanData == ((EncapsulatedData)obj).booleanData;
                case DataType.DateTime:
                    return this.dateTimeData == ((EncapsulatedData)obj).dateTimeData;
                case DataType.Date:
                    throw new NotImplementedException("Separate Date type not implemented.");
                case DataType.ParameterList:
                    return System.Linq.Enumerable.SequenceEqual(
                        this.parameterList,
                        ((EncapsulatedData)obj).parameterList);
                default:
                    throw new NotImplementedException("Datatype not implemented for comparison.");
            }
        }
        
        // override object.GetHashCode
        public override int GetHashCode()
        {
            // TODO: write your implementation of GetHashCode() here
            int typeCode = this.type.GetHashCode();
            int valueCode;
            switch (this.type)
            {
                case DataType.Number:
                    valueCode = numberData.GetHashCode();
                    break;
                case DataType.String:
                    valueCode = stringData.GetHashCode();
                    break;
                case DataType.Boolean:
                    valueCode = booleanData.GetHashCode();
                    break;
                case DataType.DateTime:
                    valueCode = dateTimeData.GetHashCode();
                    break;
                case DataType.Date:
                    throw new NotImplementedException("Date type not implemented.");
                case DataType.ParameterList:
                    valueCode = parameterList.GetHashCode();
                    break;
                default:
                    valueCode = 0;
                    break;
            }
            return typeCode ^ valueCode;
        }
    }
}
