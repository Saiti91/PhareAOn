using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class PedestalManager : MonoBehaviour
{
    [SerializeField] private List<Pedestal> pedestals;

    [Header("Action à déclencher")]
    [SerializeField] private GameObject objectToActivate; // Par exemple une porte
    
    [Header("Animator Trigger")]
    [SerializeField] private GameObject ladderFallObject; // L'objet LadderFall
    [SerializeField] private string triggerName = "Fall"; // Nom du trigger dans l'Animator
    [SerializeField] private Animator ladderAnimator; // Référence directe à l'Animator (optionnel)
    
    [Header("Événements")]
    public UnityEvent onPuzzleCompleted; // Pour plus de flexibilité

    private bool puzzleCompleted = false;

    private void Start()
    {
        // Si l'Animator n'est pas assigné, essayer de le trouver
        if (ladderAnimator == null && ladderFallObject != null)
        {
            ladderAnimator = ladderFallObject.GetComponent<Animator>();
        }
    }

    private void Update()
    {
        if (!puzzleCompleted && pedestals.All(p => p.IsCorrect))
        {
            puzzleCompleted = true;
            OnPuzzleCompleted();
        }
    }

    private void OnPuzzleCompleted()
    {
        Debug.Log("🎉 Tous les objets sont correctement placés !");
        
        // Activer l'objet si configuré
        if (objectToActivate != null)
        {
            objectToActivate.SetActive(true); 
        }
        
        // Déclencher l'animation de la chute de l'échelle
        TriggerLadderFall();
        
        // Déclencher l'événement Unity pour d'autres actions
        onPuzzleCompleted?.Invoke();
    }
    
    private void TriggerLadderFall()
    {
        // Méthode 1 : Si l'Animator est directement référencé
        if (ladderAnimator != null)
        {
            ladderAnimator.SetTrigger(triggerName);
            Debug.Log($"Trigger '{triggerName}' activé sur LadderFall");
        }
        // Méthode 2 : Si seulement le GameObject est référencé
        else if (ladderFallObject != null)
        {
            Animator anim = ladderFallObject.GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetTrigger(triggerName);
                Debug.Log($"Trigger '{triggerName}' activé sur {ladderFallObject.name}");
            }
            else
            {
                Debug.LogWarning($"Aucun Animator trouvé sur {ladderFallObject.name}");
            }
        }
        else
        {
            Debug.LogWarning("LadderFall n'est pas configuré!");
        }
    }
    
    // Méthode publique pour tester
    [ContextMenu("Test Puzzle Completion")]
    public void TestPuzzleCompletion()
    {
        OnPuzzleCompleted();
    }
}