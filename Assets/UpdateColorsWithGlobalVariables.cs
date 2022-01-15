using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateColorsWithGlobalVariables : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var node = transform;
        Logics logics = null;

        while(node.parent != null || logics == null)
        {
            node = node.parent;
            logics = node.GetComponentInChildren<Logics>();
        }
    }
}
