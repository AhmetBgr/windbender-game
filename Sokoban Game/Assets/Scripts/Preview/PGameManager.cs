using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PGameManager : MonoBehaviour
{
    private GameManager gameManager;

    public delegate void OnSimulateTurnPreviewDelegate();
    public static event OnSimulateTurnPreviewDelegate OnSimulateTurnPreview;

    // Start is called before the first frame update
    void Awake()
    {
        gameManager = GameManager.instance;
    }

    private void OnEnable() {
    
    }

    private void OnDisable() {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SimulateTurnPreview() {

        // get preview move res

        // validate and execute movements


    }
}
