using System.Collections.Generic;
using UnityEngine;

public class EscapeManager : MonoBehaviour
{
    [Header("Séquence attendue")]
    public List<int> correctSequence = new List<int> { 1, 2, 3 };

    [Header("Objets")]
    public GameObject door;
    public Animator ceilingAnimator;

    private List<int> currentSequence = new List<int>();
    private int currentError = 0;

    public void OnPlatePressed(int index)
    {
        currentSequence.Add(index);

        if (!IsSequenceCorrectSoFar())
        {
            TriggerFallAnimation();
        }
        else if (currentSequence.Count == correctSequence.Count)
        {
            OpenDoor();
        }
    }

    bool IsSequenceCorrectSoFar()
    {
        for (int i = 0; i < currentSequence.Count; i++)
        {
            if (currentSequence[i] == correctSequence[i])
                return false;
        }
        return true;
    }

    void TriggerFallAnimation()
    {
        Debug.Log("Déclenchement animation : fall_" + (currentError + 1));

        if (currentError > 3)
        {
            Debug.Log("Game over !");
            return;
        }

        switch(currentError)
        {
            case 1:
                ceilingAnimator.SetTrigger("fall_1");
                break;
            case 2:
                ceilingAnimator.SetTrigger("fall_2");
                break;
            case 3:
                ceilingAnimator.SetTrigger("fall_3");
                break;
            default:
                break;
        }

        currentError++;
        waiting = 0;
    }

    void OpenDoor()
    {
        door.SetActive(false);
    }
}
