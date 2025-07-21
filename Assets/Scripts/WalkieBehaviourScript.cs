using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class WalkieBehaviour : MonoBehaviour
{
    [Header("Setup")]
    public Transform snapAnchor;
    public AudioSource audioSource;
    public AudioClip grabSound;
    public AudioClip releaseSound;
    public float snapSpeed = 5f;

    private XRGrabInteractable grabInteractable;
    private Rigidbody rb;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();

        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        audioSource.PlayOneShot(grabSound);
        Haptic(args.interactorObject);
        StopAllCoroutines();
        transform.SetParent(null);
        rb.isKinematic = false;
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        audioSource.PlayOneShot(releaseSound);
        Haptic(args.interactorObject);
        rb.isKinematic = true;
        StartCoroutine(SnapBack());
    }

    private IEnumerator SnapBack()
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        Vector3 endPos = snapAnchor.position;
        Quaternion endRot = snapAnchor.rotation;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * snapSpeed;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }

        transform.SetParent(snapAnchor);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    private void Haptic(IXRSelectInteractor interactor)
    {
        var controllerInteractor = interactor as XRBaseControllerInteractor;
        if (controllerInteractor != null && controllerInteractor.xrController != null)
        {
            controllerInteractor.xrController.SendHapticImpulse(0.5f, 0.1f);
        }
    }
}
