using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UIManager : MonoBehaviour
{
    public UnityEvent onOpen;
    public UnityEvent onClose;

    // Start is called before the first frame update
    void Start()
    {
        onOpen.Invoke();
    }

    public void AssignUICamera()
    {
        //Assign Camera
        Canvas _canvas = GetComponent<Canvas>();
        _canvas.worldCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        _canvas.planeDistance = 5;
    }

    /// <summary>
    /// Closes the popup.
    /// </summary>
    public void Close()
    {
        onClose.Invoke();
        Destroy(gameObject, 0.2f);
    }

    public void Close(float _delay)
    {
        onClose.Invoke();
        Destroy(gameObject, _delay);
    }

    /// <summary>
    /// Utility coroutine to automatically destroy the popup after its closing animation has finished.
    /// </summary>
    /// <returns>The coroutine.</returns>
    protected virtual IEnumerator DestroyPopup()
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
}