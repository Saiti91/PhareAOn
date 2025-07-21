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
        
        // Désactiver temporairement le grab
        XRGrabInteractable grab = obj.GetComponent<XRGrabInteractable>();
        if (grab != null)
            grab.enabled = false;
        
        // Geler la physique
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
        
        // Animation de snap
        Vector3 startPos = obj.transform.position;
        Quaternion startRot = obj.transform.rotation;
        float elapsed = 0;
        
        if (audioSource && snapSound)
            audioSource.PlayOneShot(snapSound);
        
        while (elapsed < 1f / snapSpeed)
        {
            float t = elapsed * snapSpeed;
            t = Mathf.SmoothStep(0, 1, t);
            
            obj.transform.position = Vector3.Lerp(startPos, snapPoint.position, t);
            obj.transform.rotation = Quaternion.Slerp(startRot, snapPoint.rotation, t);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Position finale
        obj.transform.position = snapPoint.position;
        obj.transform.rotation = snapPoint.rotation;
        
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
        
        // Réactiver le grab
        if (grab != null)
            grab.enabled = true;
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

// Version encore plus simple du manager
public class SimplePedestalManager : MonoBehaviour
{
    [SerializeField] private Pedestal[] pedestals;
    [SerializeField] private GameObject objectToActivate;
    
    [Header("Pour l'échelle")]
    [SerializeField] private Animator ladderAnimator;
    [SerializeField] private string triggerName = "Fall";
    
    private bool puzzleCompleted = false;
    
    void Update()
    {
        if (!puzzleCompleted)
        {
            bool allCorrect = true;
            foreach (var pedestal in pedestals)
            {
                if (!pedestal.IsCorrect)
                {
                    allCorrect = false;
                    break;
                }
            }
            
            if (allCorrect)
            {
                puzzleCompleted = true;
                OnPuzzleCompleted();
            }
        }
    }
    
    void OnPuzzleCompleted()
    {
        Debug.Log("🎉 Puzzle résolu!");
        
        if (objectToActivate != null)
            objectToActivate.SetActive(true);
            
        if (ladderAnimator != null)
            ladderAnimator.SetTrigger(triggerName);
    }
}