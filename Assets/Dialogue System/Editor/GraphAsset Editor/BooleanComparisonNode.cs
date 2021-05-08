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

			AddPort("Output", typeof(bool), false, Port.Capacity.Multi, "bool-compare-output");
		}
	}

	public class FloatComparisonNode : BooleanComparisonNode
	{
		public FloatComparisonNode() : base()
		{
			title = "Compare (Float)";

			AddPort("Float 1", typeof(float), true, Port.Capacity.Single, "bool-compare-input-1");
			AddPort("Float 2", typeof(float), true, Port.Capacity.Single, "bool-compare-input-2");
		}
	}

	public class IntComparisonNode : BooleanComparisonNode
	{
		public IntComparisonNode() : base()
		{
			title = "Compare (Int)";

			AddPort("Int 1", typeof(int), true, Port.Capacity.Single, "bool-compare-input-1");
			AddPort("Int 2", typeof(int), true, Port.Capacity.Single, "bool-compare-input-2");
		}
	}
}
