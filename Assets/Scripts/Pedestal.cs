using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Pedestal : MonoBehaviour
{
    [SerializeField] private ItemTypeSO expectedType;
    [SerializeField] private Transform snapPoint;

    public bool IsCorrect { get; private set; } = false;

    private GameObject currentObject;

    private void OnTriggerEnter(Collider other)
    {
        XRGrabInteractable grab = other.GetComponent<XRGrabInteractable>();
        if (grab != null && grab.isSelected)
            return;

        if (currentObject != null)
        {
            RemoveCurrentObject();
        }

        PlaceObject(other.gameObject);
    }

    private void PlaceObject(GameObject obj)
    {
        obj.transform.position = snapPoint.position;
        obj.transform.rotation = snapPoint.rotation;

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        currentObject = obj;

        PedestalItem item = obj.GetComponent<PedestalItem>();
        IsCorrect = (item != null && item.itemType == expectedType);

        Debug.Log(IsCorrect ? "✅ Objet correct posé." : "❌ Mauvais objet.");
    }

    private void RemoveCurrentObject()
    {
        if (currentObject != null)
        {
            Rigidbody rb = currentObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.AddForce(Vector3.up * 2f, ForceMode.Impulse);
            }

            currentObject = null;
        }
        IsCorrect = false;
    }

    private void OnTriggerExit(Collider other)
    {
        if (currentObject == other.gameObject)
        {
            RemoveCurrentObject();
        }
    }
}
