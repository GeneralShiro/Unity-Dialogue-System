using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor.UIElements;

using UnityEditor.Experimental.GraphView;


namespace CustomEditors
{
	public abstract class BooleanComparisonNode : GraphNode
	{
		public enum ComparisonOperator
		{
			EQUAL_TO,
			NOT_EQUAL_TO,
			LESS_THAN,
			GREATER_THAN,
			LESS_OR_EQUAL_TO,
			GREATER_OR_EQUAL_TO
		}
		public EnumField operationEnumField {get; set;}

		public BooleanComparisonNode()
		{
			operationEnumField = new EnumField(ComparisonOperator.EQUAL_TO);
			mainContainer.Insert(1, operationEnumField);

			AddPort("Output Bool", typeof(bool), false, Port.Capacity.Multi);
		}
	}

	public class FloatComparisonNode : BooleanComparisonNode
	{
		public FloatComparisonNode() : base()
		{
			title = "Comparison Node (Float)";

			AddPort("Float 1", typeof(float));
			AddPort("Float 2", typeof(float));
		}
	}

	public class IntComparisonNode : BooleanComparisonNode
	{
		public IntComparisonNode() : base()
		{
			title = "Comparison Node (Int)";

			AddPort("Int 1", typeof(int));
			AddPort("Int 2", typeof(int));
		}
	}
}
