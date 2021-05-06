using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Test asset", menuName = "Custom Graph Asset/Test obj (remove later)", order = 1)]
public class TestScriptableObj : ScriptableObject
{
    public int _testInt;
    public float _testFloat;
    public bool _testBool;

    public enum TestEnum1
    {
        State1 = 100,
        State2 = 50
    }
    public TestEnum1 _testEnumVar;

    public enum TestEnum2
    {
        State3,
        State4
    }
    public TestEnum2 _testEnumVar2;

    public TestScriptableObj()
    {

    }
}
