using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public int plateIndex;
    public EscapeManager manager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            manager.OnPlatePressed(plateIndex);
        }
    }
}
