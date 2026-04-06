using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using DG.Tweening;
using static EventData;

public class EventManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public Image artImage;
    public Transform optionsContainer;
    public GameObject optionButtonPrefab;

    [Header("Result Panel")]
    public GameObject resultPanel;
    public TextMeshProUGUI resultText;
    public Button continueButton;
    public CanvasGroup resultCanvasGroup;

    [Header("Animation Settings")]
    public float fadeInDuration = 0.3f;
    public Ease fadeInEase = Ease.OutQuad;
    public float scaleInDuration = 0.4f;
    public Ease scaleInEase = Ease.OutBack;

    public float fadeOutDuration = 0.2f;
    public Ease fadeOutEase = Ease.InQuad;
    public float scaleOutDuration = 0.2f;
    public Ease scaleOutEase = Ease.InBack;

    [Header("Event Database")]
    public List<EventData> allEvents;

    private EventData currentEvent;

    void Start()
    {
        // Configura o painel de resultado
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
            resultCanvasGroup = resultPanel.GetComponent<CanvasGroup>();
            if (resultCanvasGroup == null)
                resultCanvasGroup = resultPanel.AddComponent<CanvasGroup>();
            resultCanvasGroup.alpha = 0f;
            resultPanel.transform.localScale = Vector3.zero;
        }

        // Escolhe um evento aleatório
        if (allEvents.Count > 0)
        {
            currentEvent = allEvents[Random.Range(0, allEvents.Count)];
            DisplayEvent(currentEvent);
        }
        else
        {
            Debug.LogError("Nenhum evento configurado!");
        }

        // Configura o botão continuar
        if (continueButton != null)
            continueButton.onClick.AddListener(FinishEvent);
    }

    void DisplayEvent(EventData eventData)
    {
        titleText.text = eventData.eventName;
        descriptionText.text = eventData.description;
        if (artImage != null && eventData.eventArt != null)
            artImage.sprite = eventData.eventArt;

        // Cria botões para cada opção
        foreach (var opt in eventData.options)
        {
            GameObject btnGO = Instantiate(optionButtonPrefab, optionsContainer);
            TextMeshProUGUI btnText = btnGO.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null) btnText.text = opt.optionText;

            Button btn = btnGO.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => ShowResult(opt.consequence));
            }
        }
    }

    void ShowResult(Consequence cons)
    {
        // Aplica os efeitos
        if (GameManager.Instance != null)
        {
            GameManager.Instance.coins += cons.coinsChange;
            GameManager.Instance.AddHealth(cons.healthChange);
            if (cons.maxHealthChange != 0)
                GameManager.Instance.AddMaxHealth(cons.maxHealthChange, true);
            if (cons.itemReward != null)
                GameManager.Instance.AddItem(cons.itemReward);
            if (cons.removeActiveItem && GameManager.Instance.activeItem != null)
            {
                GameManager.Instance.ownedItems.Remove(GameManager.Instance.activeItem);
                GameManager.Instance.activeItem = null;
                GameManager.Instance.activeItemCooldown = 0;
                GameManager.Instance.UpdateActiveItemUI();
            }
        }

        // Monta a mensagem de resultado
        string message = "";
        if (cons.coinsChange != 0)
            message += (cons.coinsChange > 0 ? $"+{cons.coinsChange} moedas" : $"{cons.coinsChange} moedas") + "\n";
        if (cons.healthChange != 0)
            message += (cons.healthChange > 0 ? $"+{cons.healthChange} vida" : $"{cons.healthChange} vida") + "\n";
        if (cons.maxHealthChange != 0)
            message += (cons.maxHealthChange > 0 ? $"+{cons.maxHealthChange} vida máxima" : $"{cons.maxHealthChange} vida máxima") + "\n";
        if (cons.itemReward != null)
            message += $"Recebeu: {cons.itemReward.itemName}\n";
        if (cons.removeActiveItem)
            message += "Perdeu seu item ativo!\n";

        if (string.IsNullOrEmpty(message))
            message = "Nada aconteceu.";

        resultText.text = message;

        // Mostra o painel com animação de entrada
        resultPanel.SetActive(true);

        // Fade in
        resultCanvasGroup.DOFade(1f, fadeInDuration).SetEase(fadeInEase);

        // Escala in
        resultPanel.transform.DOScale(1f, scaleInDuration).SetEase(scaleInEase);

        // Desativa os botões de opção para não clicar de novo
        foreach (Transform child in optionsContainer)
        {
            Button btn = child.GetComponent<Button>();
            if (btn != null) btn.interactable = false;
        }
    }

    void FinishEvent()
    {
        // Anima a saída
        Sequence fadeOut = DOTween.Sequence();

        fadeOut.Join(resultCanvasGroup.DOFade(0f, fadeOutDuration).SetEase(fadeOutEase));
        fadeOut.Join(resultPanel.transform.DOScale(0.8f, scaleOutDuration).SetEase(scaleOutEase));

        fadeOut.OnComplete(() => {
            // Finaliza o evento
            if (GameManager.Instance != null)
            {
                if (GameManager.Instance.currentNode != null)
                    GameManager.Instance.currentNode.isVisited = true;
                GameManager.Instance.OnEventCompleted();
            }

            // Volta ao mapa
            SceneManager.LoadScene(GameManager.Instance.mapScene);
        });
    }
}