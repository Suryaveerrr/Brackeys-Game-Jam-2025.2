using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    public List<Card> cards;
    private System.Random rng = new System.Random();
    private Dictionary<string, Sprite> spriteMap;

    [SerializeField] private Sprite[] cardSprites; 

    public void Awake()
    {
        LoadSprites();
        GenerateDeck();
        Shuffle();
    }

    private void LoadSprites()
    {
        spriteMap = new Dictionary<string, Sprite>();

        foreach (var sprite in cardSprites)
        {
            spriteMap[sprite.name] = sprite; 
        }
    }

    private void GenerateDeck()
    {
        cards = new List<Card>();
        string[] suits = { "Hearts", "Diamonds", "Clubs", "Spades" };
        string[] ranks = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };

        Dictionary<string, int> values = new Dictionary<string, int>()
        {
            { "2", 2 }, { "3", 3 }, { "4", 4 }, { "5", 5 },
            { "6", 6 }, { "7", 7 }, { "8", 8 }, { "9", 9 },
            { "10", 10 }, { "J", 10 }, { "Q", 10 }, { "K", 10 },
            { "A", 11 }
        };

        foreach (var suit in suits)
        {
            foreach (var rank in ranks)
            {
                string spriteName = rank + suit[0]; 
                if (spriteMap.TryGetValue(spriteName, out Sprite sprite))
                {
                    Debug.Log($"Found sprite for {spriteName}");
                    cards.Add(new Card(suit, rank, values[rank], sprite));
                }
                else
                {
                    Debug.LogWarning($"Sprite not found for {spriteName}");
                }
            }
        }
        
        
    }

    public void Shuffle()
    {
        int n = cards.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (cards[k], cards[n]) = (cards[n], cards[k]);
        }
    }

    public Card DrawCard()
    {
        if (cards.Count == 0)
        {
            Debug.LogWarning("Deck is empty!");
            return null;
        }
        

        Card drawn = cards[0];
        cards.RemoveAt(0);
        return drawn;
    }
}
