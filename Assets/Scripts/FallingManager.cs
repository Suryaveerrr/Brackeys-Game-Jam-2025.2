using UnityEngine;

public class FallingManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void DropBiscuits()
    {
        foreach (Transform child in transform)
        {
          //set the child to active
            child.gameObject.SetActive(true);
        }
    }
}
