using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WikiDialog : MonoBehaviour
{
    public TMP_InputField urlField;

    private void Awake()
    {
        Provider.tryOpenWiki += (object _, string s) => Open(s); // TODO: refactor
    }

    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(false); // TODO: refactor
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Open(string s)
    {
        gameObject.SetActive(true);
        urlField.text = s;
    }

    public void OnConfirm()
    {
        Application.OpenURL(urlField.text);
        gameObject.SetActive(false);
    }

    public void OnCancel() => gameObject.SetActive(false);
}
