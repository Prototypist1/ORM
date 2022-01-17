using System;
using System.Collections.Generic;
using System.Text;

namespace ORM
{
    class SqlModel
    {
    }

    class QueryModel { 
    
    
    }

    // equals
    // not equals
    // greater
    // greater or eqaul
    // less then
    // less than or equal
    // in

    // case when

    // sum 
    // max 
    // min 
    // count

    // and 
    // or
    // not

    // +
    // -
    // %
    // /
    // *

    // except
    // union
    // intersect
    // -- 👆 these are going to be a bit hard to use, they request the two sub queries match types
    // so no anonymous types. 
    // they are kind of advanced features anyway

    public class EqualOperator : ISqlValue<bool> {
        readonly ISqlCode left, right;

        public EqualOperator(ISqlCode left, ISqlCode right)
        {
            this.left = left ?? throw new ArgumentNullException(nameof(left));
            this.right = right ?? throw new ArgumentNullException(nameof(right));
        }

        public string ToCode() => $"({left.ToCode()} = {right.ToCode()})";
    }

    public class NotEqualOperator : ISqlValue<bool> {

        readonly ISqlCode left, right;
        public string ToCode() => $"({left.ToCode()} <> {right.ToCode()})";
    }

    public class GreaterThanOperator : ISqlValue<bool>
    {
        readonly ISqlNumericValue left, right;
        public GreaterThanOperator(ISqlNumericValue left, ISqlNumericValue right)
        {
            this.left = left ?? throw new ArgumentNullException(nameof(left));
            this.right = right ?? throw new ArgumentNullException(nameof(right));
        }

        public string ToCode() => $"({left.ToCode()} > {right.ToCode()})";
    }

    public class LessThanOperator : ISqlValue<bool>
    {
        readonly ISqlNumericValue left, right;
        public string ToCode() => $"({left.ToCode()} < {right.ToCode()})";
    }

    public class GreaterThanOrEqualOperator : ISqlValue<bool>
    {
        readonly ISqlNumericValue left, right;
        public string ToCode() => $"({left.ToCode()} >= {right.ToCode()})";
    }

    public class LessThanOrEqualOperator : ISqlValue<bool>
    {
        readonly ISqlNumericValue left, right;
        public string ToCode() => $"({left.ToCode()} <= {right.ToCode()})";
    }

    public class InOperator : ISqlValue<bool>
    {
        readonly ISqlCode left, right;
        public string ToCode() => $"({left.ToCode()} IN {right.ToCode()})";
    }

    public class CaseWhenOperator<T> : ISqlValue<T>
    {
        public string ToCode()
        {
            throw new NotImplementedException();
        }
    }

    public class SumOperator : ISqlNumericValue
    {
        readonly SqlCollection<ISqlNumericValue> primary;
        public string ToCode() => $"SUM({primary.ToCode()})";
    }

    public class MaxOperator : ISqlNumericValue
    {
        readonly SqlCollection<ISqlNumericValue> primary;
        public string ToCode() => $"MAX({primary.ToCode()})";

    }
    public class MinOperator : ISqlNumericValue
    {
        readonly SqlCollection<ISqlNumericValue> primary;
        public string ToCode() => $"MIN({primary.ToCode()})";
    }
    public class CountOperator : ISqlNumericValue
    {

        readonly ISqlCollection<ISqlCode> primary;

        public CountOperator(ISqlCollection<ISqlCode> primary)
        {
            this.primary = primary ?? throw new ArgumentNullException(nameof(primary));
        }

        public string ToCode() => $"COUNT({primary.ToCode()})";
    }
    public class NotOperator : ISqlValue<bool> {

        readonly ISqlValue<bool> primary;
        public string ToCode() => $"(NOT {primary.ToCode()})";
    }

    public class AndOperator : ISqlValue<bool>
    {
        readonly ISqlValue<bool> left, right;
        public string ToCode() => $"({left.ToCode()} AND {right.ToCode()})";
    }

    public class OrOperator : ISqlValue<bool> {
        readonly ISqlValue<bool> left, right;
        public string ToCode() => $"({left.ToCode()} OR {right.ToCode()})";
    }

    public class AddOperator : ISqlNumericValue
    {
        readonly ISqlNumericValue left, right;
        public string ToCode() => $"({left.ToCode()} + {right.ToCode()})";

    }

    public class SubtractOperator : ISqlNumericValue
    {
        readonly ISqlNumericValue left, right;
        public string ToCode() => $"({left.ToCode()} - {right.ToCode()})";

    }

    public class MultiplyOperator : ISqlNumericValue
    {
        readonly ISqlNumericValue left, right;
        public string ToCode() => $"({left.ToCode()} * {right.ToCode()})";

    }

    public class DivideOperator : ISqlNumericValue
    {

        readonly ISqlNumericValue left, right;
        public string ToCode() => $"({left.ToCode()} / {right.ToCode()})";
    }

    public class RemainderOperator : ISqlNumericValue
    {

        readonly ISqlNumericValue left, right;
        public string ToCode() => $"({left.ToCode()} % {right.ToCode()})";
    }
}