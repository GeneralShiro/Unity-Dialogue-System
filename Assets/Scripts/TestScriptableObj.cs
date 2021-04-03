using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Test asset", menuName = "Custom Graph Asset/Test obj (remove later)", order = 1)]
public class TestScriptableObj : ScriptableObject
{
    public int _testInt1;

    public TestScriptableObj()
    {
        _testInt1 = 0;
    }
}
