using System.Collections;
using System.Collections.Generic;

using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

namespace CustomEditors
{
    /// <summary>
    /// Base class for getting/setting a variable from a serialized ScriptableObject asset.
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
            
            // get rid of the collapse button
            titleButtonContainer.RemoveFromHierarchy();
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
    }

    public class IntGetterNode : AccessorNode
    {
        public IntGetterNode()
        {
            title = "Get (Int)";
            _targetPropertyType = SerializedPropertyType.Integer;
            
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
            
            _objectField.RegisterValueChangedCallback(x =>
            {
                SetPopupList(_targetPropertyType);
            });

            SetPopupList(_targetPropertyType);
        }
    }
}
