using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    [System.Serializable]
    public class State
    {
        public Color fillColor;
        public Color outlineColor;
    }

    public State state { get; private set; }
    public char letter { get; private set; }

    public int rowIndex;
    public int colIndex;
    public Color outlineColor = Color.yellow;

    private Image fill;
    private Outline outline;
    private TextMeshProUGUI text;
    private Button button;

    private void Awake()
    {
        fill = GetComponent<Image>();
        outline = GetComponent<Outline>();
        text = GetComponentInChildren<TextMeshProUGUI>();
        button = GetComponent<Button>();
        if (button == null)
            button = gameObject.AddComponent<Button>();
        button.onClick.AddListener(OnTileClicked);
    }

    public void SetLetter(char letter)
    {
        this.letter = letter;
        text.text = letter.ToString();
    }

    public void SetState(State state)
    {
        this.state = state;
        fill.color = state.fillColor;
        outline.effectColor = state.outlineColor;
    }

    private void OnTileClicked()
    {
        MultiBoardManager manager = FindObjectOfType<MultiBoardManager>();
        if (manager != null)
            manager.MoveCursorToTile(this);
    }

    public void SetHighlight(bool highlight)
    {
        if (outline == null) return;
        if (highlight)
        {
            outline.effectColor = outlineColor;
            outline.effectDistance = new Vector2(3, 3);
        }
        else
        {
            outline.effectColor = Color.gray;
            outline.effectDistance = new Vector2(1, 1);
        }
    }
}