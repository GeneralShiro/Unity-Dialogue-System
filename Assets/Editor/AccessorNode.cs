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
    public class AccessorNode : GraphNode
    {
        public ObjectField _objectField;
        public PopupField<string> _popupField;

        private List<string> _testList;


        public AccessorNode()
        {
            _objectField = new ObjectField()
            {
                objectType = typeof(ScriptableObject),
                allowSceneObjects = false
            };
            _objectField.RegisterValueChangedCallback(x =>
            {
                BindObjectToPopup();
            });
            mainContainer.Add(_objectField);

            _testList = new List<string>();
            SetPopupList();
            _popupField = new PopupField<string>(_testList, 0);
            mainContainer.Add(_popupField);
        }

        public void BindObjectToPopup()
        {
            Debug.Log("reached");
            SetPopupList();
        }

        private void SetPopupList()
        {
            _testList.Clear();

            if (_objectField.value == null)
            {
                _testList.Add("None");
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
                        if (property.propertyType == SerializedPropertyType.Integer)
                        {
                            _testList.Add(property.name);
                        }
                    }
                }

                if (_testList.Count == 0)
                {
                    _testList.Add("None");
                }

            }
        }
    }
}
