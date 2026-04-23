using System.Collections.Generic;
using UnityEngine;

public class LetterStateManager : MonoBehaviour
{
    private Dictionary<char, char> letterStates = new Dictionary<char, char>();

    public void ResetLetterStates() 
    {
        letterStates.Clear();
    }

    public void UpdateLetterStates(char[] letters, char[] statuses)
    {
        for (int i = 0; i < letters.Length; i++) 
        {
            char letter = letters[i];
            char newStatus = statuses[i];

            if(!letterStates.ContainsKey(letter))
                letterStates[letter] = newStatus;
            else 
            {
                char current = letterStates[letter];
                if (newStatus == 'G')
                    letterStates[letter] = 'G';
                else if (newStatus == 'Y' && current != 'G')
                    letterStates[letter] = 'Y';
                else if (newStatus == 'X' && current != 'G' && current != 'Y')
                    letterStates[letter] = 'X';
            }
        }
    }

    public char GetLetterState(char letter) 
    {
        if(letterStates.TryGetValue(letter, out char state))
            return state;
        return 'N';
    }
}
