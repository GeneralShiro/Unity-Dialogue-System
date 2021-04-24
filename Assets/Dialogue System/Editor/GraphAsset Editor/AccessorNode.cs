using System.Collections;
using System.Collections.Generic;

using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

using CustomSystem;

namespace CustomEditors
{
    /// <summary>
    /// Base class for getting a variable from a serialized ScriptableObject asset.
    /// </summary>
    public abstract class AccessorNode : GraphNode
    {
        public ObjectField _objectField;
        public PopupField<string> _popupField;
        public SerializedPropertyType _targetPropertyType;
        public List<string> _popupList;


        public AccessorNode()
        {
            styleSheets.Add(Resources.Load<StyleSheet>("AccessorNodeStyle"));
            AddToClassList("accessorNode");

            _targetPropertyType = SerializedPropertyType.Generic;

            _objectField = new ObjectField()
            {
                objectType = typeof(ScriptableObject),
                allowSceneObjects = false
            };
            inputContainer.Add(_objectField);

            _popupList = new List<string>();
            _popupList.Add("");

            _popupField = new PopupField<string>(_popupList, 0);
            inputContainer.Add(_popupField);
        }

        public void SetPopupList(SerializedPropertyType desiredType)
        {
            _popupList.Clear();

            if (_objectField.value == null)
            {
                _popupList.Add("None");
            }
            else
            {
                SerializedObject obj = new SerializedObject(_objectField.value);
                SerializedProperty property = obj.GetIterator();
                property.Next(true);

                if (property != null)
                {
                    while (property.NextVisible(false))
                    {
                        if (desiredType == property.propertyType)
                        {
                            _popupList.Add(property.name);
                        }
                    }
                }

                if (_popupList.Count == 0)
                {
                    _popupList.Add("None");
                }

            }
        }

        public void InitializeFromData(AccessorNodeData data)
        {
            _objectField.value = data._scriptableObj;
            SetPopupList(_targetPropertyType);

            if (data._chosenPropertyString != "")
            {
                _popupField.value = data._chosenPropertyString;
            }

            NodeGuid = data._nodeGuid;
            SetPosition(new Rect(data._nodePosition, Vector2.zero));
        }
    }

    public class IntGetterNode : AccessorNode
    {
        public IntGetterNode()
        {
            title = "Get (Int)";
            _targetPropertyType = SerializedPropertyType.Integer;

            // add output port
            AddPort("", typeof(int), false, Port.Capacity.Multi, "accessor-int-output");

            _objectField.RegisterValueChangedCallback(x =>
            {
                SetPopupList(_targetPropertyType);
            });

            SetPopupList(_targetPropertyType);
        }
    }

    public class FloatGetterNode : AccessorNode
    {
        public FloatGetterNode()
        {
            title = "Get (Float)";
            _targetPropertyType = SerializedPropertyType.Float;

            // add output port
            AddPort("", typeof(float), false, Port.Capacity.Multi, "accessor-float-output");

            _objectField.RegisterValueChangedCallback(x =>
            {
                SetPopupList(_targetPropertyType);
            });

            SetPopupList(_targetPropertyType);
        }
    }

    public class BoolGetterNode : AccessorNode
    {
        public BoolGetterNode()
        {
            title = "Get (Bool)";
            _targetPropertyType = SerializedPropertyType.Boolean;

            // add output port
            AddPort("", typeof(bool), false, Port.Capacity.Multi, "accessor-bool-output");

            _objectField.RegisterValueChangedCallback(x =>
            {
                SetPopupList(_targetPropertyType);
            });

            SetPopupList(_targetPropertyType);
        }
    }
}
