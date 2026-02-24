using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class ScrollViewFix : MonoBehaviour
{
    void Start()
    {
        ScrollRect scroll = GetComponent<ScrollRect>();     
        scroll.horizontal = false;  
        scroll.vertical = true;  
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.inertia = true;
        scroll.decelerationRate = 0.135f; 
    }
}
