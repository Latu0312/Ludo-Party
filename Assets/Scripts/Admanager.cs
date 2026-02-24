using GoogleMobileAds;
using GoogleMobileAds.Api;
using UnityEngine;

public class GoogleMobileAdsDemoScript : MonoBehaviour
{
    public void Start()
    {
        
        MobileAds.Initialize((InitializationStatus initStatus) =>
        {
            
        });
    }
}