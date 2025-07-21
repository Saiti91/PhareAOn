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
            StartCoroutine(InitialSnap());
        }
    }

    IEnumerator InitialSnap()
    {
        // Petit délai pour s'assurer que tous les objets sont initialisés
        yield return new WaitForSeconds(startDelay);
        
        // Désactiver temporairement la physique
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;
        
        // Positionner directement à l'ancre
        transform.position = snapAnchor.position;
        transform.rotation = snapAnchor.rotation;
        transform.SetParent(snapAnchor);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        
        isSnapped = true;
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        if (audioSource && grabSound)
            audioSource.PlayOneShot(grabSound);
            
        StopAllCoroutines();
        transform.SetParent(null);
        
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = false;
        
        isSnapped = false;
    }

    void OnRelease(SelectExitEventArgs args)
    {
        if (audioSource && releaseSound)
            audioSource.PlayOneShot(releaseSound);
            
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