using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using CustomSystem;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

namespace CustomEditors
{
    public class EnumComparisonNode : AccessorNode
    {
        public List<string> _enumValList;
        public PopupField<string> _enumValField;

        public EnumComparisonNode()
        {
            title = "Enum Compare";

            _targetPropertyType = SerializedPropertyType.Enum;

            _enumValList = new List<string>();
            _enumValList.Add("None");

            _enumValField = new PopupField<string>("Is Equal To:", _enumValList, "None");
            inputContainer.Add(_enumValField);

            // add output port
            AddPort("", typeof(bool), false, Port.Capacity.Multi, "enum-compare-output");

            _objectField.RegisterValueChangedCallback(x =>
            {
                SetPopupList(_targetPropertyType);
            });

            SetPopupList(_targetPropertyType);

            _popupField.RegisterValueChangedCallback(x =>
            {
                SetEnumValList();
            });
        }

        public void SetEnumValList()
        {
            _enumValList.Clear();

            if (_objectField.value == null)
            {
                _enumValList.Add("None");
                _enumValField.value = "None";
                return;
            }
            else
            {
                FieldInfo info = _objectField.value.GetType().GetField(_popupField.value);

                if (info == null)
                {
                    _enumValList.Add("None");
                    _enumValField.value = "None";
                }
                else
                {
                    string[] names = info.FieldType.GetEnumNames();
                    foreach (string s in names)
                    {
                        _enumValList.Add(s);
                    }
                    _enumValField.value = names[0];
                }
            }
        }

        public void InitializeFromData(EnumComparisonNodeData data)
        {
            AccessorNodeData baseData = new AccessorNodeData()
            {
                _nodeGuid = data._nodeGuid,
                _nodePosition = data._nodePosition,
                _nodeType = data._nodeType,
                _scriptableObj = data._scriptableObj,
                _chosenPropertyString = data._chosenPropertyString,
                _typeEnumVal = data._typeEnumVal
            };
            InitializeFromData(baseData);

            SetEnumValList();
            _enumValField.value = data._chosenEnumValue;
        }
    }
}