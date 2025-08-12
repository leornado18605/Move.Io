using UnityEngine;

public class VictoryController : MonoBehaviour
{
    public Animator victoryAnimator;
    void Start()
    {
        if (victoryAnimator != null)
        {
            victoryAnimator.SetBool("Victory", true); 
        }
    }
}
