using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugTool : MonoBehaviour
{
    public Text debugLog;
    // Start is called before the first frame update
    void Start()
    {
        debugLog.text = "init...";
    }

    // Update is called once per frame
    void Update()
    {
        int loaded = 0;
        if (ARLogic.loaded)
            loaded = 1;
        debugLog.text = MagLogic.numBullets + " + " + loaded;
    }
}
