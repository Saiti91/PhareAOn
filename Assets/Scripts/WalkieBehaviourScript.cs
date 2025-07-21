using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class WalkieBehaviour : MonoBehaviour
{
    [Header("Setup")]
    public Transform snapAnchor;
    public AudioSource audioSource;
    public AudioClip grabSound, releaseSound;
    public float snapSpeed = 5f;
    
    [Header("Initial Position")]
    [SerializeField] private bool snapOnStart = true;
    [SerializeField] private float startDelay = 0.1f; // Petit délai pour s'assurer que tout est initialisé

    XRGrabInteractable grab;
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    [SerializeField] private Vector3 localPositionOffset = Vector3.zero;
    [SerializeField] private Vector3 localRotationOffset = Vector3.zero;
    
    private bool isSnapped = false;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();

        grab.selectEntered.AddListener(OnGrab);
        grab.selectExited.AddListener(OnRelease);
    }

    void Start()
    {
        // Positionner le talkie à sa position de snap au démarrage
        if (snapOnStart && snapAnchor != null)
        {
            audioSource.Stop();
            StartCoroutine(InitialSnap());
        }
    }

    IEnumerator InitialSnap()
    {
        yield return new WaitForSeconds(startDelay);
        
        if (debugMode)
        {
            Debug.Log($"Initial Snap - Walkie position: {transform.position}");
            Debug.Log($"Initial Snap - Anchor position: {snapAnchor.position}");
            Debug.Log($"Initial Snap - Anchor scale: {snapAnchor.lossyScale}");
        }
        
        // Désactiver temporairement la physique
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;
        
        // IMPORTANT : Ne pas devenir enfant du socket s'il est dans le XR Origin
        // Sinon le walkie suivra les mouvements du joueur
        bool shouldParent = !IsInXRRig(snapAnchor);
        
        if (shouldParent)
        {
            // Méthode normale : devenir enfant
            transform.position = snapAnchor.position;
            transform.rotation = snapAnchor.rotation;
            transform.SetParent(snapAnchor);
            transform.localPosition = localPositionOffset;
            transform.localRotation = Quaternion.Euler(localRotationOffset);
            transform.localScale = Vector3.one;
        }
        else
        {
            // Méthode alternative : juste se positionner sans devenir enfant
            transform.position = snapAnchor.position + snapAnchor.TransformDirection(localPositionOffset);
            transform.rotation = snapAnchor.rotation * Quaternion.Euler(localRotationOffset);
            
            if (debugMode)
                Debug.Log("Socket est dans XR Rig - pas de parenting");
        }
        
        isSnapped = true;
        
        if (debugMode)
        {
            Debug.Log($"After Snap - Local position: {transform.localPosition}");
            Debug.Log($"After Snap - World position: {transform.position}");
        }
    }
    
    private bool IsInXRRig(Transform t)
    {
        Transform current = t;
        while (current != null)
        {
            if (current.name.Contains("XR") && current.name.Contains("Rig"))
                return true;
            current = current.parent;
        }
        return false;
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        if (audioSource && grabSound)
        {
            audioSource.Stop();
            audioSource.PlayOneShot(grabSound);
        }

        StopAllCoroutines();
        transform.SetParent(null);
        
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = false;
        
        isSnapped = false;
    }

    void OnRelease(SelectExitEventArgs args)
    {
        if (audioSource && releaseSound)
        {
            audioSource.Stop();
            audioSource.PlayOneShot(releaseSound);
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;
        
        StartCoroutine(SnapBack());
    }

    IEnumerator SnapBack()
    {
        if (snapAnchor == null)
        {
            Debug.LogWarning("Snap Anchor is not assigned!");
            yield break;
        }
        
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        Vector3 endPos = snapAnchor.position;
        Quaternion endRot = snapAnchor.rotation;

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * snapSpeed;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }

        // S'assurer que la position finale est exacte
        transform.position = snapAnchor.position;
        transform.rotation = snapAnchor.rotation;
        transform.SetParent(snapAnchor);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        
        isSnapped = true;
    }
    
    // Méthode publique pour forcer le snap si nécessaire
    public void ForceSnapToAnchor()
    {
        if (snapAnchor != null)
        {
            StopAllCoroutines();
            transform.position = snapAnchor.position;
            transform.rotation = snapAnchor.rotation;
            transform.SetParent(snapAnchor);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb) rb.isKinematic = true;
            
            isSnapped = true;
        }
    }
    
    // Visualisation dans l'éditeur
    void OnDrawGizmos()
    {
        if (snapAnchor != null)
        {
            Gizmos.color = isSnapped ? Color.green : Color.yellow;
            Gizmos.DrawWireCube(snapAnchor.position, Vector3.one * 0.1f);
            
            if (!isSnapped)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, snapAnchor.position);
            }
        }
    }
}