using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultDisplay : MonoBehaviour
{
    [SerializeField] Text _text;
    [SerializeField] string _clearText = "Clear";
    [SerializeField] string _failedText = "Failed";

    Action _onDisplayEnd;
    Action _onDisplayEndSingle;

    private void Start()
    {
        if (_text)
        {
            _text.enabled = false;
        }
    }

    public void OnDisplayEnd(Action onDisplayEnd, bool singleUse)
    {
        if (singleUse)
        {
            _onDisplayEndSingle = onDisplayEnd;
        }
        else
        {
            _onDisplayEnd = onDisplayEnd;
        }
    }


    public void ClearDisplay()
    {
        if (_text)
        {
            _text.enabled = true;
            _text.text = _clearText;
        }
        StartCoroutine(DisplayStay());
    }

    public void FailedDisplay()
    {
        if (_text)
        {
            _text.enabled = true;
            _text.text = _failedText;
        }

        StartCoroutine(DisplayStay());
    }

    IEnumerator DisplayStay()
    {
        yield return null;
        while (!Input.anyKeyDown)
        {
            yield return null;
        }
        if (_text)
        {
            _text.enabled = false;
        }
        if (_onDisplayEnd != null)
        {
            _onDisplayEnd.Invoke();
        }
        if (_onDisplayEndSingle != null)
        {
            _onDisplayEndSingle.Invoke();
            _onDisplayEndSingle = null;
        }
    }
}
