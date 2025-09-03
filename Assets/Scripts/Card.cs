
using UnityEngine;

[System.Serializable]
public class Card
{
    public string suit;  
    public string rank;  
    public int value;     
    public Sprite sprite;
    public Card(string suit, string rank, int value, Sprite sprite)
    {
        this.suit = suit;
        this.rank = rank;
        this.value = value;
        this.sprite = sprite;
    }
}
