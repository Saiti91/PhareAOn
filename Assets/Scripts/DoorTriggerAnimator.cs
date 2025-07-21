using UnityEngine;

public class DoorTriggerAnimator : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private string openTriggerName = "OpenDoor";
    [SerializeField] private string closeTriggerName = "CloseDoor";
    
    [Header("Audio (Optionnel)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    
    private bool isOpen = false;
    
    void Start()
    {
        // Auto-assigner l'Animator si pas déjà fait
        if (!doorAnimator)
            doorAnimator = GetComponent<Animator>();
            
        if (!doorAnimator && debugMode)
            Debug.LogError("Animator non trouvé sur " + gameObject.name);
    }
    
    public void OpenDoor()
    {
        if (!isOpen && doorAnimator)
        {
            doorAnimator.SetTrigger(openTriggerName);
            isOpen = true;
            
            // Jouer le son d'ouverture
            if (audioSource && openSound)
                audioSource.PlayOneShot(openSound);
                
            if (debugMode)
                Debug.Log("Porte ouverte via Animator trigger");
        }
    }
    
    public void CloseDoor()
    {
        if (isOpen && doorAnimator)
        {
            doorAnimator.SetTrigger(closeTriggerName);
            isOpen = false;
            
            // Jouer le son de fermeture
            if (audioSource && closeSound)
                audioSource.PlayOneShot(closeSound);
                
            if (debugMode)
                Debug.Log("Porte fermée via Animator trigger");
        }
    }
    
    public void ToggleDoor()
    {
        if (isOpen)
            CloseDoor();
        else
            OpenDoor();
    }
    
    // Pour forcer l'état si nécessaire
    public void SetDoorState(bool open)
    {
        if (open)
            OpenDoor();
        else
            CloseDoor();
    }
    
    // Méthodes de test dans l'éditeur
    [ContextMenu("Test Open")]
    private void TestOpen()
    {
        OpenDoor();
    }
    
    [ContextMenu("Test Close")]
    private void TestClose()
    {
        CloseDoor();
    }
}