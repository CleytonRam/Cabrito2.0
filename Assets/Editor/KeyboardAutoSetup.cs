using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class KeyboardAutoSetup : EditorWindow
{
    private Transform line1;
    private Transform line2;
    private Transform line3;

    [MenuItem("Tools/Auto Setup Keyboard")]
    public static void ShowWindow()
    {
        GetWindow<KeyboardAutoSetup>("Auto Setup Keyboard");
    }

    void OnGUI()
    {
        GUILayout.Label("Auto Setup do Teclado Virtual", EditorStyles.boldLabel);
        GUILayout.Space(10);

        GUILayout.Label("Arraste as linhas do teclado:", EditorStyles.label);
        line1 = (Transform)EditorGUILayout.ObjectField("Line 1 (Q W E R T Y U I O P)", line1, typeof(Transform), true);
        line2 = (Transform)EditorGUILayout.ObjectField("Line 2 (A S D F G H J K L)", line2, typeof(Transform), true);
        line3 = (Transform)EditorGUILayout.ObjectField("Line 3 (Z X C V B N M)", line3, typeof(Transform), true);

        GUILayout.Space(10);

        if (GUILayout.Button("Configurar Teclado Automaticamente", GUILayout.Height(40)))
        {
            SetupKeyboard();
        }
    }

    void SetupKeyboard()
    {
        VirtualKeyboardManager manager = FindObjectOfType<VirtualKeyboardManager>();
        if (manager == null)
        {
            Debug.LogError("VirtualKeyboardManager não encontrado na cena!");
            return;
        }

        string[] lettersLine1 = { "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P" };
        string[] lettersLine2 = { "A", "S", "D", "F", "G", "H", "J", "K", "L" };
        string[] lettersLine3 = { "Z", "X", "C", "V", "B", "N", "M" };

        manager.keys.Clear();
        int keysFound = 0;

        if (line1 != null)
        {
            foreach (string letter in lettersLine1)
            {
                Transform keyTransform = line1.Find("Key_" + letter);
                if (keyTransform != null)
                {
                    AddKeyToManager(manager, keyTransform, letter);
                    keysFound++;
                }
                else
                {
                    Debug.LogWarning($"Tecla não encontrada: Key_{letter} em {line1.name}");
                }
            }
        }

        if (line2 != null)
        {
            foreach (string letter in lettersLine2)
            {
                Transform keyTransform = line2.Find("Key_" + letter);
                if (keyTransform != null)
                {
                    AddKeyToManager(manager, keyTransform, letter);
                    keysFound++;
                }
                else
                {
                    Debug.LogWarning($"Tecla não encontrada: Key_{letter} em {line2.name}");
                }
            }
        }

        if (line3 != null)
        {
            foreach (string letter in lettersLine3)
            {
                Transform keyTransform = line3.Find("Key_" + letter);
                if (keyTransform != null)
                {
                    AddKeyToManager(manager, keyTransform, letter);
                    keysFound++;
                }
                else
                {
                    Debug.LogWarning($"Tecla não encontrada: Key_{letter} em {line3.name}");
                }
            }
        }

        EditorUtility.SetDirty(manager);
        Debug.Log($"Setup concluído! {keysFound} teclas configuradas.");
    }

    void AddKeyToManager(VirtualKeyboardManager manager, Transform keyTransform, string letter)
    {
        GameObject keyGO = keyTransform.gameObject;

        // Procura o Button dentro dos filhos (funcionou antes)
        Button button = keyGO.GetComponentInChildren<Button>();
        if (button == null)
        {
            Debug.LogError($"Botão não encontrado em Key_{letter} (nem nos filhos)");
            return;
        }

        TextMeshProUGUI letterText = keyGO.GetComponentInChildren<TextMeshProUGUI>();
        if (letterText == null)
        {
            Debug.LogError($"Texto não encontrado em Key_{letter}");
            return;
        }

        letterText.text = letter;
        Image background = keyGO.GetComponent<Image>();
        if (background == null) background = button.GetComponent<Image>();

        // Procura TODOS os quadrantes (Quad_1, Quad_2, Quad_3, Quad_4) que existirem
        List<Image> quadrants = new List<Image>();
        for (int i = 1; i <= 4; i++)
        {
            Transform quad = keyGO.transform.Find($"Quad_{i}");
            if (quad != null)
            {
                Image quadImage = quad.GetComponent<Image>();
                if (quadImage != null)
                    quadrants.Add(quadImage);
            }
        }

        VirtualKeyboardManager.VirtualKey newKey = new VirtualKeyboardManager.VirtualKey();
        newKey.letter = letter;
        newKey.button = button;
        newKey.letterText = letterText;
        newKey.quadrants = quadrants;

        manager.keys.Add(newKey);
        Debug.Log($"Tecla {letter}: {quadrants.Count} quadrante(s)");
    }
}