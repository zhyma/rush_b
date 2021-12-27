using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoltLogic : MonoBehaviour
{
    private static GameObject boltCarrier;
    private static GameObject boltExtractor;
    private static GameObject chargingHandle;
    private static GameObject bullet;

    // bolt and charging handle position data
    public const float fullyOpen = 0.08f;
    public const int TOFLowThreshold = 22;
    public const int TOFHighThreshold = 63;

    private static Vector3 chargingHandleOriginPos;
    private static Vector3 boltCarrierOriginPos;
    private static Vector3 boltExtractorOriginPos;
    private static Vector3 bulletOriginPos;

    // Start is called before the first frame update
    void Start()
    {
        boltCarrier = ARLogic.ar15_main.transform.GetChild(0).gameObject;
        boltExtractor = ARLogic.ar15_main.transform.GetChild(1).gameObject;
        chargingHandle = ARLogic.ar15_main.transform.GetChild(2).gameObject;
        bullet = this.transform.GetChild(2).gameObject;

        chargingHandleOriginPos = chargingHandle.transform.localPosition;
        boltCarrierOriginPos = boltCarrier.transform.localPosition;
        boltExtractorOriginPos = boltExtractor.transform.localPosition;
        bulletOriginPos = bullet.transform.localPosition;
    }

    public static Vector3 chargingHandlePos(int TOFDistance)
    {
        float distance = fullyOpen;
        if (TOFDistance <= TOFLowThreshold)
            return new Vector3(0, 0, 0);
        else if (TOFDistance >= TOFHighThreshold)
            return new Vector3(-distance, 0, 0);
        else
        {
            distance = ((float)(TOFDistance - TOFLowThreshold)) / ((float)TOFHighThreshold - TOFLowThreshold) * fullyOpen;
            return new Vector3(-distance, 0, 0);
        }
    }

    public static Vector3 boltOpenOffset()
    {
        return new Vector3(-fullyOpen*0.875f, 0, 0);
    }

    public static void updateHandlePos(Vector3 vec)
    {
        // closest deadzone
        chargingHandle.transform.localPosition = chargingHandleOriginPos + vec;
    }

    public static void updateBoltPos(Vector3 vec)
    {
        boltCarrier.transform.localPosition = boltCarrierOriginPos + vec;
        boltExtractor.transform.localPosition = boltExtractorOriginPos + vec;
    }

    public static void showBullet()
    {
        bullet.SetActive(true);
    }

    public static void hideBullet()
    {
        bullet.SetActive(false);
    }

    public static void updateBulletPos(Vector3 vec)
    {
        bullet.transform.localPosition = bulletOriginPos + vec;
    }
}
