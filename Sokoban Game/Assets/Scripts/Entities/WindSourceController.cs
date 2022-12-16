using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WindSourceController : MonoBehaviour
{
    public TextMeshProUGUI windSPText;

    [SerializeField]
    private int _windSP;
    public int windSP{
        get { return _windSP; }
        set{
            _windSP = value;
            UpdateWindSPText();
        }
    }

    [HideInInspector] public int defWindSP;

    public List<Command> oldCommands = new List<Command>();

    private Collider2D col;
    private SpriteRenderer spriteRenderer;
    private GameManager gameManager;

    public bool isDrawing = false;
    public bool isUsed = false;
    private bool interactable = true;

    public delegate void OnMouseDownDelegate();
    public static event OnMouseDownDelegate OnMouseDownThis;
    public delegate void OnMouseUpDelegate();
    public static event OnMouseUpDelegate OnMouseUpThis;

    private void OnEnable()
    {
        defWindSP = windSP;
        UpdateWindSPText();
        col = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        gameManager = GameManager.instance;

        gameManager.OnStateChange += TryToToggle;
    }

    private void OnDisable() 
    { 
        gameManager.OnStateChange -= TryToToggle;
    }

    private void TryToToggle(GameState from, GameState to)
    {
        if(to == GameState.DrawingRoute)
        {
            if (gameManager.curWindSource == this)
                MakeInteractable();
            else
                MakeUninteractable();
        }
        else
        {
            if (gameManager.curWindSource == this)
                MakeUninteractable();
            else
            {
                if (isUsed)
                    MakeUninteractable();
                else
                    MakeInteractable();
            }
        }
    }

    public void MakeUninteractable()
    {
        if (!interactable) return;

        interactable = false;
        col.enabled = false;
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0.3f);
        windSPText.color = new Color(windSPText.color.r, windSPText.color.g, windSPText.color.b, 0.3f);
    }
    public void MakeInteractable()
    {
        if (interactable) return; 

        interactable = true;
        col.enabled = true;
        spriteRenderer.color = Color.white;
        windSPText.color = Color.white;
    }

    public void UpdateWindSP(int routeCount)
    {
        if (gameManager.isLooping)
        {
            windSP = defWindSP - routeCount + 1;
        }
        else
        {
            windSP = defWindSP - routeCount;
        }
    }

    private void UpdateWindSPText()
    {
        windSPText.text = windSP.ToString();
    }
}
