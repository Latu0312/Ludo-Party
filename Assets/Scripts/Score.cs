using TMPro;
using UnityEngine;

public class Score : MonoBehaviour
{
    DiceRoll dice;
    public TextMeshProUGUI scoreText;

    public void Awake()
    {
        dice = FindFirstObjectByType<DiceRoll>();
    }
    void Update()
    {
        if(dice != null)
        {
            if(dice.diceFaceNum != 0)
            {
                scoreText.text = "Score: " + dice.diceFaceNum.ToString();
            }
            else
            {
                scoreText.text = "Score: 0";
            }
        }
    }
}
