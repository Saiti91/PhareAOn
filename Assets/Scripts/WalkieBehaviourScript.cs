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

    XRGrabInteractable grab;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();

        grab.selectEntered.AddListener(OnGrab);
        grab.selectExited.AddListener(OnRelease);
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        audioSource.PlayOneShot(grabSound);
        StopAllCoroutines();
        transform.SetParent(null);
        GetComponent<Rigidbody>().isKinematic = false;
    }

    void OnRelease(SelectExitEventArgs args)
    {
        audioSource.PlayOneShot(releaseSound);
        GetComponent<Rigidbody>().isKinematic = true;
        StartCoroutine(SnapBack());
    }

    IEnumerator SnapBack()
    {
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

        transform.SetParent(snapAnchor);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }
}