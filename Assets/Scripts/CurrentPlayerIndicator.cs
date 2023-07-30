using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CurrentPlayerIndicator : MonoBehaviour
{
    public GameManager gameManager;
    TMP_Text text;

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<TMP_Text>();
        Sync();
    }

    public void Sync()
    {
        text.text = $"Current Side: {gameManager.state.CurrentSide.Name}";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
