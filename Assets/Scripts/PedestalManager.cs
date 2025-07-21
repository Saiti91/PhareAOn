using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PedestalManager : MonoBehaviour
{
    [SerializeField] private List<Pedestal> pedestals;

    [Header("Action à déclencher")]
    [SerializeField] private GameObject objectToActivate; // Par exemple une porte

    private bool puzzleCompleted = false;

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
        if (objectToActivate != null)
        {
            objectToActivate.SetActive(true); // ou .OpenDoor() selon ton système
        }
    }
}
