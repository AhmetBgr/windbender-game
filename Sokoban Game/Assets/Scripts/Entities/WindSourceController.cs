using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WindSourceController : MonoBehaviour
{
    public struct WindRoute {
        public List<Vector3> route;
        public WindSourceController windSource;
        public bool isCompleted;

        public WindRoute(List<Vector3> route, WindSourceController windSource, bool isCompleted = false)
        {
            this.route = route;
            this.windSource = windSource;
            this.isCompleted = isCompleted;
        }
    }
    public List<WindRoute> windRoutes = new List<WindRoute>();

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
    public bool isAlternative = false;
    private bool interactable = true;

    public delegate void OnMouseDownDelegate();
    public static event OnMouseDownDelegate OnMouseDownThis;
    public delegate void OnMouseUpDelegate();
    public static event OnMouseUpDelegate OnMouseUpThis;

    private void OnEnable()
    {
        gameManager = GameManager.instance;
        defWindSP = windSP;
        UpdateWindSPText();
        col = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        

        gameManager.OnStateChange += TryToToggle;
    }

    private void OnDisable() 
    { 
        gameManager.OnStateChange -= TryToToggle;
    }

    private void Start()
    {
        GameManager.instance.windSources.Add(this);
    }

    private void Update()
    {

    }

    private void TryToToggle(GameState from, GameState to)
    {
        if(to == GameState.DrawingRoute)
        {
            if (gameManager.curGame.curWindSource == this)
                MakeInteractable();
            else
                MakeUninteractable();
        }
        else
        {
            if (gameManager.curGame.curWindSource == this)
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
        if (gameManager.curGame.isLooping)
        {
            windSP = defWindSP - routeCount + 1;
        }
        else
        {
            windSP = defWindSP - routeCount;
        }

        windSP = windSP < 0 ? 0 : windSP;
    }

    public void SetWindSP(int value) {
        windSP = value;
    }

    private void UpdateWindSPText()
    {
        windSPText.text = windSP.ToString();

        if(defWindSP == 4 && windSP == 4)
            windSPText.text += "*";

    }
}
