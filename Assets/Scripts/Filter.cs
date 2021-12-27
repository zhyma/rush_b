using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Filter : MonoBehaviour
{
    public static int len = 15;
    private static int[] buffer;
    private static int index = 0;
    private static bool flatFlag = false;
    private static int peakValue = BoltLogic.TOFLowThreshold;
    // Start is called before the first frame update
    void Start()
    {
        buffer = new int[len];
        for (int i = 0; i < len; i++)
            buffer[i] = BoltLogic.TOFLowThreshold;

    }

    public static int Smooth(int rawData)
    {
        int filteredData = BoltLogic.TOFHighThreshold;
        //if (rawData >= BoltLogic.TOFHighThreshold)
        //    buffer[index] = BoltLogic.TOFHighThreshold;
        //else if (rawData <= BoltLogic.TOFLowThreshold)
        //    buffer[index] = BoltLogic.TOFLowThreshold;
        //else
        //    buffer[index] = rawData;
        buffer[index] = rawData;

        if (rawData <= BoltLogic.TOFLowThreshold)
        {
            filteredData = BoltLogic.TOFLowThreshold;
        }
        else if (rawData >= BoltLogic.TOFHighThreshold)
        {
            filteredData = BoltLogic.TOFHighThreshold;
        }
        else
        {
            int increase = 0, decrease = 0, equal = 0;
            for (int i = 1; i < len; i++)
            {
                int delta = buffer[(index + i) % len] - buffer[(index + i + 1) % len];
                if (delta < 0)
                    increase++;
                else if (delta == 0)
                    equal++;
                else
                    decrease++;
            }

            if (increase + equal == len - 1)
            {
                filteredData = buffer[(index + len - len / 2) % len];
                flatFlag = false;
            }
            else if (decrease + equal == len - 1)
            {
                filteredData = buffer[(index + len - len / 2) % len];
                flatFlag = false;
            }
            else
            {
                if (flatFlag == false)
                {
                    flatFlag = true;
                    peakValue = buffer[(index + len - len / 2) % len];
                }
                filteredData = peakValue;
            }
        }
        index = (index + 1) % len;

        return filteredData;
    }

    //// Update is called once per frame
    //void Update()
    //{
        
    //}
}
