using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor.UIElements;

namespace CustomGraphEditors
{
	public abstract class BooleanNode : GraphNode
	{
		public enum BooleanOperation
		{
			EQUAL_TO,
			NOT_EQUAL_TO,
			LESS_THAN,
			GREATER_THAN,
			LESS_OR_EQUAL_TO,
			GREATER_OR_EQUAL_TO
		}

		public BooleanNode()
		{
			var opEnum = new EnumField(BooleanOperation.EQUAL_TO);
			mainContainer.Insert(1, opEnum);

			AddOutputPort("Output Bool", typeof(bool));
		}

		// TODO: move this to a monobehaviour obj
		protected bool EvaluateBoolean<T>(T value1, T value2) where T : IComparable<T>
		{
			/*
			switch (booleanOp)
			{
				case BooleanOperation.EQUAL_TO:
				{
					return (value1.CompareTo(value2) == 0);
				}

				case BooleanOperation.NOT_EQUAL_TO:
				{
					return (value1.CompareTo(value2) != 0);
				}

				case BooleanOperation.LESS_THAN:
				{
					return (value1.CompareTo(value2) == -1);
				}

				case BooleanOperation.GREATER_THAN:
				{
					return (value1.CompareTo(value2) == 1);
				}

				case BooleanOperation.LESS_OR_EQUAL_TO:
				{
					return (value1.CompareTo(value2) <= 0);
				}

				case BooleanOperation.GREATER_OR_EQUAL_TO:
				{
					return (value1.CompareTo(value2) >= 0);
				}
			}
*/
			return false;
		}
	}

	public class FloatBooleanNode : BooleanNode
	{
		public FloatBooleanNode() : base()
		{
			title = "Boolean Node (Float)";

			AddInputPort("Float 1", typeof(float));
			AddInputPort("Float 2", typeof(float));
		}
	}

	public class IntBooleanNode : BooleanNode
	{
		public IntBooleanNode() : base()
		{
			title = "Boolean Node (Int)";

			AddInputPort("Int 1", typeof(int));
			AddInputPort("Int 2", typeof(int));
		}
	}
}
