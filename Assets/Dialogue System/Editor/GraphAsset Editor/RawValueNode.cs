
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;


namespace CustomEditors
{
    public class IntValueNode : GraphNode
    {
        public IntegerField _intField;

        public IntValueNode()
        {
            styleSheets.Add(Resources.Load<StyleSheet>("RawValueNodeStyle"));
            AddToClassList("rawValNode");
            AddToClassList("rawIntNode");

            tooltip = "Raw integer value";

            title = "Int";
            titleContainer.RemoveFromHierarchy();

            _intField = new IntegerField();
            inputContainer.Add(_intField);

            AddPort("", typeof(int), false, Port.Capacity.Multi, "value-int-output");
        }
    }

    public class FloatValueNode : GraphNode
    {
        public FloatField _floatField;

        public FloatValueNode()
        {
            styleSheets.Add(Resources.Load<StyleSheet>("RawValueNodeStyle"));
            AddToClassList("rawValNode");
            AddToClassList("rawFloatNode");

            tooltip = "Raw floating-point value";

            title = "Float";
            titleContainer.RemoveFromHierarchy();

            _floatField = new FloatField();
            inputContainer.Add(_floatField);

            AddPort("", typeof(float), false, Port.Capacity.Multi, "value-float-output");
        }
    }
}