using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace CustomSystem
{
    public abstract class NodeCondition
    {
        public bool IsOutputInversed { get; set; }

        public abstract bool Evaluate();
    }

    public class ComparisonCondition<T> : NodeCondition where T : IComparable<T>
    {
        public enum CompareOperator
        {
            EQUAL_TO,
            NOT_EQUAL_TO,
            LESS_THAN,
            GREATER_THAN,
            LESS_OR_EQUAL_TO,
            GREATER_OR_EQUAL_TO
        }
        private CompareOperator _operator;

        private ScriptableObject _leftObj;
        private ScriptableObject _rightObj;
        private string _leftPropertyName;
        private string _rightPropertyName;
        private T _leftOperand;
        private T _rightOperand;

        public ComparisonCondition(CompareOperator op, T leftOperand, T rightOperand)
        {
            _leftOperand = leftOperand;
            _rightOperand = rightOperand;
            _operator = op;
        }

        public ComparisonCondition(CompareOperator op, ScriptableObject leftObj, string leftPropertyName, T rightOperand)
        {
            _leftObj = leftObj;
            _leftPropertyName = leftPropertyName;
            _rightOperand = rightOperand;
            _operator = op;
        }

        public ComparisonCondition(CompareOperator op, T leftOperand, ScriptableObject rightObj, string rightPropertyName)
        {
            _rightObj = rightObj;
            _rightPropertyName = rightPropertyName;
            _leftOperand = leftOperand;
            _operator = op;
        }

        public ComparisonCondition(CompareOperator op, ScriptableObject leftObj, string leftPropertyName, ScriptableObject rightObj, string rightPropertyName)
        {
            _rightObj = rightObj;
            _rightPropertyName = rightPropertyName;
            _leftObj = leftObj;
            _leftPropertyName = leftPropertyName;
            _operator = op;
        }

        public override bool Evaluate()
        {
            T value1, value2;

            value1 = (_leftObj != null) ? (T)_leftObj.GetType().GetProperty(_leftPropertyName).GetValue(_leftObj) : _leftOperand;
            value2 = (_rightObj != null) ? (T)_rightObj.GetType().GetProperty(_rightPropertyName).GetValue(_rightObj) : _rightOperand;

            bool output = false;

            switch (Operator)
            {
                case CompareOperator.EQUAL_TO:
                    {
                        output = (value1.CompareTo(value2) == 0);
                        break;
                    }

                case CompareOperator.NOT_EQUAL_TO:
                    {
                        output = (value1.CompareTo(value2) != 0);
                        break;
                    }

                case CompareOperator.LESS_THAN:
                    {
                        output = (value1.CompareTo(value2) == -1);
                        break;
                    }

                case CompareOperator.GREATER_THAN:
                    {
                        output = (value1.CompareTo(value2) == 1);
                        break;
                    }

                case CompareOperator.LESS_OR_EQUAL_TO:
                    {
                        output = (value1.CompareTo(value2) <= 0);
                        break;
                    }

                case CompareOperator.GREATER_OR_EQUAL_TO:
                    {
                        output = (value1.CompareTo(value2) >= 0);
                        break;
                    }
            }

            return IsOutputInversed ? !output : output;
        }

        public CompareOperator Operator
        {
            get { return _operator; }
            protected set { _operator = value; }
        }
    }

    public class LogicCondition : NodeCondition
    {
        public enum LogicOperator
        {
            AND,
            OR
        }
        private LogicOperator _operator;
        private List<NodeCondition> _inputs;

        public LogicCondition(LogicOperator op)
        {
            _inputs = new List<NodeCondition>();
            _operator = op;
        }

        public void AddInput(NodeCondition condition)
        {
            _inputs.Add(condition);
        }

        public override bool Evaluate()
        {
            bool output = false;

            switch (Operator)
            {
                case LogicOperator.AND:
                    {
                        output = true;

                        foreach (NodeCondition condition in _inputs)
                        {
                            if (!condition.Evaluate())
                            {
                                output = false;
                                break;
                            }
                        }
                        break;
                    }

                case LogicOperator.OR:
                    {
                        output = false;

                        foreach (NodeCondition condition in _inputs)
                        {
                            if (condition.Evaluate())
                            {
                                output = true;
                                break;
                            }
                        }

                        break;
                    }
            }

            return IsOutputInversed ? !output : output;
        }

        public LogicOperator Operator
        {
            get { return _operator; }
            protected set { _operator = value; }
        }
    }

    public class AccessedBoolCondition : NodeCondition
    {
        private ScriptableObject _obj;
        private string _propertyName;

        public AccessedBoolCondition(ScriptableObject obj, string propertyName)
        {
            _obj = obj;
            _propertyName = propertyName;
        }

        public override bool Evaluate()
        {
            bool output = (bool)_obj.GetType().GetProperty(_propertyName).GetValue(_obj);
            
            return IsOutputInversed ? !output : output;
        }
    }
}
