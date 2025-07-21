using UnityEngine;

using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Collections;

public class Pedestal : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private GameObject correctObject; // L'objet spécifique attendu
    [SerializeField] private Transform snapPoint;
    [SerializeField] private float snapSpeed = 5f;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip snapSound;
    [SerializeField] private AudioClip correctSound;
    [SerializeField] private AudioClip wrongSound;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    
    public bool IsCorrect { get; private set; } = false;
    public bool IsOccupied => currentObject != null;
    
    private GameObject currentObject;
    private Coroutine snapCoroutine;
    
    void Start()
    {
        // Créer le snap point si nécessaire
        if (snapPoint == null)
        {
            GameObject snapObj = new GameObject("SnapPoint");
            snapObj.transform.SetParent(transform);
            snapObj.transform.localPosition = new Vector3(0, 0.5f, 0);
            snapPoint = snapObj.transform;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Si c'est un objet grabable
        XRGrabInteractable grab = other.GetComponent<XRGrabInteractable>();
        if (grab == null) return;
        
        // Si l'objet est tenu, ne pas le snap
        if (grab.isSelected)
        {
            if (debugMode)
                Debug.Log($"Objet tenu détecté : {other.name}");
            return;
        }
        
        // Si déjà occupé, ne rien faire
        if (currentObject != null) return;
        
        // Snap l'objet
        AttemptSnap(other.gameObject);
    }
    
    void OnTriggerStay(Collider other)
    {
        // Si l'objet est relâché dans la zone
        if (currentObject == null)
        {
            XRGrabInteractable grab = other.GetComponent<XRGrabInteractable>();
            if (grab != null && !grab.isSelected)
            {
                AttemptSnap(other.gameObject);
            }
        }
        else if (currentObject == other.gameObject)
        {
            // Maintenir la position si l'objet est toujours dans la zone et pas tenu
            XRGrabInteractable grab = other.GetComponent<XRGrabInteractable>();
            if (grab != null && !grab.isSelected)
            {
                // Forcer la position pour éviter qu'il glisse
                other.transform.position = snapPoint.position;
                other.transform.rotation = snapPoint.rotation;
            }
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        // Si l'objet placé est retiré
        if (currentObject == other.gameObject)
        {
            RemoveObject();
        }
    }
    
    private void AttemptSnap(GameObject obj)
    {
        if (currentObject != null) return;
        
        if (snapCoroutine != null)
            StopCoroutine(snapCoroutine);
            
        snapCoroutine = StartCoroutine(SnapObject(obj));
    }
    
    private IEnumerator SnapObject(GameObject obj)
    {
        currentObject = obj;
        
        // Désactiver temporairement le grab (très court)
        // XRGrabInteractable grab = obj.GetComponent<XRGrabInteractable>();
        // if (grab != null)
        //     grab.enabled = false;
        
        // Geler la physique
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
        
        // Snap instantané
        obj.transform.position = snapPoint.position;
        obj.transform.rotation = snapPoint.rotation;
        
        if (audioSource && snapSound)
            audioSource.PlayOneShot(snapSound);
        
        // Vérifier si c'est le bon objet
        IsCorrect = (obj == correctObject);
        
        // Feedback audio
        if (IsCorrect)
        {
            if (audioSource && correctSound)
                audioSource.PlayOneShot(correctSound);
            Debug.Log("✅ BON objet placé!");
        }
        else
        {
            if (audioSource && wrongSound)
                audioSource.PlayOneShot(wrongSound);
            Debug.Log("❌ MAUVAIS objet placé!");
        }
        
        // Attendre un frame pour éviter les conflits
        yield return null;
        
        // Réactiver le grab ET la physique pour pouvoir reprendre l'objet
        // if (grab != null)
        //     grab.enabled = true;
            
        if (rb != null)
            rb.isKinematic = false;
    }
    
    private void RemoveObject()
    {
        if (currentObject == null) return;
        
        // Réactiver la physique
        Rigidbody rb = currentObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.AddForce(Vector3.up * 2f, ForceMode.Impulse);
        }
        
        currentObject = null;
        IsCorrect = false;
        
        if (debugMode)
            Debug.Log("Objet retiré du piédestal");
    }
    
    void OnDrawGizmos()
    {
        if (snapPoint != null)
        {
            Gizmos.color = IsCorrect ? Color.green : (currentObject ? Color.red : Color.yellow);
            Gizmos.DrawWireSphere(snapPoint.position, 0.1f);
        }
    }
}