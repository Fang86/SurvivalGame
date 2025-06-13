using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public bool canPickUp = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public Item PickupItem()
    {
        if (canPickUp)
            return GetComponent<Item>();
        return null;    
    }
}
