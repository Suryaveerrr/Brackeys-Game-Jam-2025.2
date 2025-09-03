using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

// Biscuit "Blueprint" definitions
public enum BiscuitType { Add1, Redraw, SwapCard, RestartRound , DoubleDown, BustBlock }

[System.Serializable]
public class Biscuit
{
    public string name;
    public int cost;
    [TextArea] public string description;
    public BiscuitType biscuitType;
    public Sprite icon;
}


public class GameManager : MonoBehaviour
{
    public Deck deck;
    public Sprite cardBackSprite;

    public GameObject jammie;
    public Animator jammieAnim;
    public GameObject textBox;

    private List<Card> playerHand = new List<Card>();
    private List<Card> dealerHand = new List<Card>();
    private bool gameActive = false;
    public int chips = 100;
    public TextMeshProUGUI chipsText;
    private string mainText;
    public TextMeshProUGUI mainTextUI;
    
    public TextMeshProUGUI playerTotalText;
    public TextMeshProUGUI dealerTotalText;
    public int playerTotal = 0;
    public int dealerTotal = 0;

    public Image[] playerCardSlots;
    public Image[] dealerCardSlots;
    
    public GameObject pos1;

    // Betting System UI
    public int currentBet = 10;
    public TextMeshProUGUI betText;
    public Button increaseBetButton;
    public Button decreaseBetButton;
    public Button dealButton;
    public Button hitButton;
    public Button standButton;
    public GameObject shopButton;

    // Round System
    private int currentRound = 0;
    private const int maxRounds = 10;
    public TextMeshProUGUI roundText;
    
    public GameObject cardPrefab;
    public Transform decktransform;
    
    public Transform handArea;
    public Transform dealerHandArea;
    public List<GameObject> playerCardsInHand = new List<GameObject>();
    public List<GameObject> dealerCardsInHand = new List<GameObject>();
    public float cardSpacing = 1.5f;
    public GameObject restartButton;

    [Header("Shop System")]
    public GameObject shopPanel;
    public List<Biscuit> biscuits;
    public List<Image> activeBiscuitSlots;
    private Biscuit[] activeBiscuitData = new Biscuit[6];
    private Coroutine clearTableCoroutine = null;
    public int rerollCost = 60; 
    private bool shopHasBeenGenerated = false;

    [Header("Shop UI Slots")]
    // References for the Left Slot
    public TextMeshProUGUI leftSlotName;
    public TextMeshProUGUI leftSlotDescription;
    public TextMeshProUGUI leftSlotCost;
    public Image leftSlotIcon;
    public Button leftSlotBuyButton;

    // References for the Right Slot
    public TextMeshProUGUI rightSlotName;
    public TextMeshProUGUI rightSlotDescription;
    public TextMeshProUGUI rightSlotCost;
    public Image rightSlotIcon;
    public Button rightSlotBuyButton;

    [Header("Biscuit Power-Ups")]
    public GameObject useBiscuitButton;
    private int selectedBiscuitSlot = -1;
    private bool hasInsurance = false;
    private bool hasBustBlocker = false;
    
    public spawncards spawnCardsScript;

    [FormerlySerializedAs("audio")] public Audio audioManager;

    public Transform[] cardSpawnPoints;
    //public float spawnInterval = 2f; // Time interval between spawns
   // public int cardCount = 10; 
   public ObjectPooling cardPool;
   public GameObject cardFalling;
   private bool hasAdd1;


    private void Start()
    {
        UpdateBetText();
        roundText.text = $"Round: {currentRound}/{maxRounds}";
        useBiscuitButton.SetActive(false);
        audioManager.Play("Main");
        
        
        textBox.SetActive(true);
       // mainTextUI.text = "Welcome to Crumblejack! Score 5000 to win!";
        
       Debug.Log("GameManager Start called");
   
      
        if (audioManager != null)
        {
       
            audioManager.Play("Main");
        }

        // Example usage: Spawn a card from the pool
        for (int i = 0; i < 100; i++)
        {
            GameObject card = cardPool.GetObject();
            card.transform.position = new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), 0f);
        }
        
        StartCoroutine(SpawnObjectsAtTop());
        
    }

    private void Update()
    {
        chipsText.text = chips.ToString();
        playerTotalText.text = playerTotal.ToString();
        dealerTotalText.text = dealerTotal.ToString();
    }

    // --- SHOP & BISCUIT FUNCTIONS ---

    public void ToggleShopPanel()
    {
        
        if (!shopHasBeenGenerated)
        {
            RandomizeShopDisplay();
            shopHasBeenGenerated = true; // Mark that we've generated it
        }

        shopPanel.SetActive(!shopPanel.activeSelf);
    }
    private void RandomizeShopDisplay()
    {
        // Create a list of all possible biscuit indices
        List<int> availableIndices = new List<int>();
        for (int i = 0; i < biscuits.Count; i++) { availableIndices.Add(i); }

        // --- SETUP LEFT SLOT ---
        int randomIndex1 = Random.Range(0, availableIndices.Count);
        int biscuitIndex1 = availableIndices[randomIndex1];
        Biscuit biscuit1 = biscuits[biscuitIndex1];

        leftSlotName.text = biscuit1.name;
        leftSlotDescription.text = biscuit1.description;
        leftSlotCost.text = "Buy: " + biscuit1.cost;
        leftSlotIcon.sprite = biscuit1.icon;

       
        leftSlotBuyButton.onClick.RemoveAllListeners(); 
        leftSlotBuyButton.onClick.AddListener(() => BuyBiscuit(biscuitIndex1));

        
        availableIndices.RemoveAt(randomIndex1);

        // --- SETUP RIGHT SLOT ---
        int randomIndex2 = Random.Range(0, availableIndices.Count);
        int biscuitIndex2 = availableIndices[randomIndex2];
        Biscuit biscuit2 = biscuits[biscuitIndex2];

        rightSlotName.text = biscuit2.name;
        rightSlotDescription.text = biscuit2.description;
        rightSlotCost.text = "Buy: " + biscuit2.cost;
        rightSlotIcon.sprite = biscuit2.icon;

        
        rightSlotBuyButton.onClick.RemoveAllListeners(); 
        rightSlotBuyButton.onClick.AddListener(() => BuyBiscuit(biscuitIndex2));
        
    }
    public void OnRerollButtonPressed()
    {
        
        if (chips >= rerollCost)
        {
            chips -= rerollCost; 
            RandomizeShopDisplay(); 
        }
        else
        {
            Debug.Log("Not enough chips to reroll!");
            
        }
    }

    private bool IsBiscuitInventoryFull()
    {
        
        foreach (var biscuitData in activeBiscuitData)
        {
            
            if (biscuitData == null)
            {
                return false;
            }
        }
        
        return true;
    }

    private IEnumerator ClearTableAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        textBox.SetActive(false);

        foreach (var card in playerCardsInHand)
        {
            if (card != null) Destroy(card);
        }
        playerCardsInHand.Clear();

        foreach (var card in dealerCardsInHand)
        {
            if (card != null) Destroy(card);
        }
        dealerCardsInHand.Clear();

        // Also clear the win/loss message
        mainTextUI.text = "";
    }
    public void BuyBiscuit(int biscuitIndex)
    {
        
        if (IsBiscuitInventoryFull())
        {
            Debug.Log("Cannot buy biscuit, inventory is full!");
            return; 
        }

        Biscuit biscuitToBuy = biscuits[biscuitIndex];

        if (chips >= biscuitToBuy.cost)
        {
            chips -= biscuitToBuy.cost;
            DisplayPurchasedBiscuit(biscuitToBuy);
        }
        else
        {
            Debug.Log("Not enough chips for " + biscuitToBuy.name);
        }
    }

    private void DisplayPurchasedBiscuit(Biscuit purchasedBiscuit)
    {
        
        for (int i = 0; i < activeBiscuitData.Length; i++)
        {
            
            if (activeBiscuitData[i] == null)
            {
                
                activeBiscuitData[i] = purchasedBiscuit;

                Image slotImage = activeBiscuitSlots[i];
                slotImage.sprite = purchasedBiscuit.icon;
                slotImage.color = Color.white;

                return; 
            }
        }

        // If the loop finishes, all slots are full
        Debug.Log("All biscuit slots are full!");
    }


    public void OnBiscuitIconClicked(int slotIndex)
    {
        if (activeBiscuitData[slotIndex] != null)
        {
            selectedBiscuitSlot = slotIndex;
            useBiscuitButton.transform.position = activeBiscuitSlots[slotIndex].transform.position;
            useBiscuitButton.SetActive(true);
        }
    }

    public void UseSelectedBiscuit()
    {
        if (selectedBiscuitSlot != -1)
        {
            Biscuit biscuitToUse = activeBiscuitData[selectedBiscuitSlot];

            // --- TIMING RULES ---
            
            if (biscuitToUse.biscuitType == BiscuitType.RestartRound && gameActive) return;

            
            if (biscuitToUse.biscuitType != BiscuitType.RestartRound && !gameActive) return;
            

            ActivateBiscuit(biscuitToUse.biscuitType);

            activeBiscuitData[selectedBiscuitSlot] = null;
            activeBiscuitSlots[selectedBiscuitSlot].sprite = null;
            activeBiscuitSlots[selectedBiscuitSlot].color = new Color(1, 1, 1, 0);

            useBiscuitButton.SetActive(false);
            selectedBiscuitSlot = -1;
        }
    }

    private void ActivateBiscuit(BiscuitType type)
    {
        switch (type)
        {
            case BiscuitType.Add1:
                hasAdd1 = true;
                playerTotal++; 
                break;

            case BiscuitType.Redraw:
                if (playerHand.Count > 0)
                {
                    // Destroy the last card GameObject
                    Destroy(playerCardsInHand[playerCardsInHand.Count - 1]);
                    playerCardsInHand.RemoveAt(playerCardsInHand.Count - 1);

                    // Replace the data
                    playerHand.RemoveAt(playerHand.Count - 1);
                    playerHand.Add(deck.DrawCard());

                    // Draw the new card
                    ShowHands();
                    playerTotal = CalculateHand(playerHand);
                }
                break;

            case BiscuitType.SwapCard:
                if (playerHand.Count > 0 && dealerHand.Count > 0)
                {
                    // Swap the data
                    Card playerCard = playerHand[playerHand.Count - 1];
                    Card dealerCard = dealerHand[0];
                    playerHand[playerHand.Count - 1] = dealerCard;
                    dealerHand[0] = playerCard;

                    // Update the visuals by swapping sprites
                    SpriteRenderer playerSR = playerCardsInHand[playerCardsInHand.Count - 1].GetComponent<SpriteRenderer>();
                    SpriteRenderer dealerSR = dealerCardsInHand[0].GetComponent<SpriteRenderer>();
                    playerSR.sprite = dealerCard.sprite;
                    dealerSR.sprite = playerCard.sprite;

                    // Recalculate totals
                    playerTotal = CalculateHand(playerHand);
                    dealerTotal = CalculateHand(dealerHand);
                }
                break;

            case BiscuitType.RestartRound:
                chips += currentBet; // Return the bet
                mainTextUI.text = "Round Restarted!";

                dealButton.interactable = false;
                increaseBetButton.interactable = false;
                decreaseBetButton.interactable = false;

                StartGame();
                break;

            case BiscuitType.DoubleDown:
                if (chips >= currentBet)
                {
                    
                    currentBet *= 2;
                    UpdateBetText();
                }
                break;

            case BiscuitType.BustBlock:
                hasBustBlocker = true;
                break;

        }
    }


    // --- CORE GAME LOGIC ---

    public void IncreaseBet()
    {
        int newBet = currentBet + 5;
        if (newBet <= chips)
        {
            currentBet = newBet;
            UpdateBetText();
        }
    }

    public void DecreaseBet()
    {
        int newBet = currentBet - 5;
        if (newBet >= 5)
        {
            currentBet = newBet;
            UpdateBetText();
        }
    }

    private void UpdateBetText()
    {
        betText.text = "Bet: " + currentBet.ToString();
    }

    private void ClearTable()
    {
        mainTextUI.text = "";
        for (int i = 0; i < playerCardSlots.Length; i++)
        {
            playerCardSlots[i].sprite = null;
            playerCardSlots[i].color = new Color(1, 1, 1, 0f);
        }
        for (int i = 0; i < dealerCardSlots.Length; i++)
        {
            dealerCardSlots[i].sprite = null;
            dealerCardSlots[i].color = new Color(1, 1, 1, 0f);
        }
    }

    public void OnDeal()
    {
        shopHasBeenGenerated = false;

        if (chips >= 5000)
        {
            cardFalling.SetActive(true);
            mainTextUI.text = "Congratulations! You win!";
            textBox.SetActive(true);
            gameActive = false;
            restartButton.SetActive(true);
            return;
        }

        if (currentRound >= maxRounds)
        {
            if (chips < 5000)
            {
                cardFalling.SetActive(true);
                mainTextUI.text = "Game Over! You lose!";
                textBox.SetActive(true);
                restartButton.SetActive(true);
                cardFalling.SetActive(true);
              // TriggerGameOverAnimation();
            }
            else
            {
                mainTextUI.text = "Congratulations! You win you Jammie Buggar!";
                textBox.SetActive(true);
                restartButton.SetActive(true);
            }
            gameActive = false;
            return;
        }

        if (chips >= currentBet)
        {
            currentRound++;
            roundText.text = $"Round: {currentRound}/{maxRounds}";
            ClearTable();

            chips -= currentBet;
            textBox.SetActive(false);
            StartGame();
            CheckPlayerBust();

            dealButton.interactable = false;
            increaseBetButton.interactable = false;
            decreaseBetButton.interactable = false;
        }
        else
        {
            mainTextUI.text = "Not enough chips to bet " + currentBet + "!";
            // Check if the player has 0 chips and end the game
            if (chips <= 0)
            {
               
                mainTextUI.text = "Game Over! Thanks for playing CrumbleJack!";
                textBox.SetActive(true);
                restartButton.SetActive(true);
                gameActive = false;
                cardFalling.SetActive(true);
               // TriggerGameOverAnimation();
            }
        }
    }

    public void OnHit()
    {
        if (!gameActive) return;
        Card newCard = deck.DrawCard();
        playerHand.Add(newCard);
        ShowHands();
        playerTotal = CalculateHand(playerHand);
        
        if (playerTotal == 21)
        {
            StartCoroutine(AnimateJammie());
            textBox.SetActive(true);
            mainTextUI.text = "Jammie buggar! You win!";
            chips += currentBet * 2; // Payout for a win
            gameActive = false;

            
            dealButton.interactable = true;
            increaseBetButton.interactable = true;
            decreaseBetButton.interactable = true;
            return;
        }

        // Check for bust or apply BustBlocker power-up
        if (playerTotal > 21 && hasBustBlocker)
        {
            StartCoroutine(AnimateJammie());
            textBox.SetActive(true);
            mainTextUI.text = "Bust Prevented! Bet returned.";
            chips += currentBet; // Return the bet

            gameActive = false;
            hasBustBlocker = false; // Consume the power-up
            dealButton.interactable = true;
            increaseBetButton.interactable = true;
            decreaseBetButton.interactable = true;
        }
        else
        {
            // Check for a normal bust
            CheckPlayerBust();
        }
    }
    public void OnStand()
    {
        if (!gameActive) return;
        dealerCardSlots[1].sprite = dealerHand[1].sprite; // Reveal dealer's card
        DealerTurn();
    }

    private void StartGame()
    {
        playerHand.Clear();
        dealerHand.Clear();

        if (clearTableCoroutine != null)
        {
            StopCoroutine(clearTableCoroutine);
        }
        
        foreach (var card in playerCardsInHand)
        {
            Destroy(card);
        }
        playerCardsInHand.Clear();

        foreach (var card in dealerCardsInHand)
        {
            Destroy(card);
        }
        dealerCardsInHand.Clear();
        
        deck.Awake();
        StartCoroutine(DealInitialCards());
    }


    private void ShowHands()
    {
        if (playerCardsInHand.Count > playerHand.Count)
        {
            foreach (var card in playerCardsInHand)
            {
                Destroy(card);
            }
            playerCardsInHand.Clear();
        }


        for (int i = playerCardsInHand.Count; i < playerHand.Count; i++)
        {
            DrawCard(playerHand[i], handArea, playerCardsInHand);
        }

        // Clear dealer cards if the hand size has changed
        if (dealerCardsInHand.Count > dealerHand.Count)
        {
            foreach (var card in dealerCardsInHand)
            {
                Destroy(card);
            }
            dealerCardsInHand.Clear();
        }

        // Add new dealer cards
        for (int i = dealerCardsInHand.Count; i < dealerHand.Count; i++)
        {
            DrawCard(dealerHand[i], dealerHandArea, dealerCardsInHand);
        }
    }

    
    private void CheckPlayerBust()
    {
        playerTotal = CalculateHand(playerHand);
        Debug.Log("Checking for bust. Player Total is: " + playerTotal);
        if (playerTotal > 21)
        {
            StartCoroutine(AnimateJammie());
            mainText = "Looks like you bust! Jammie wins!";
            textBox.SetActive(true);
            mainTextUI.text = mainText;
            gameActive = false;

           
            
            dealButton.interactable = true;
            increaseBetButton.interactable = true;
            decreaseBetButton.interactable = true;
            clearTableCoroutine = StartCoroutine(ClearTableAfterDelay(5f));
        }
        
    }

   private void DealerTurn()
{
    StartCoroutine(DealerTurnCoroutine());
}

private IEnumerator DealerTurnCoroutine()
{
    for (int i = 0; i < dealerCardSlots.Length; i++)
    {
        if (i < dealerHand.Count)
        {
            dealerCardSlots[i].sprite = dealerHand[i].sprite;
            dealerCardSlots[i].color = Color.white;
        }
        else
        {
            dealerCardSlots[i].sprite = null;
            dealerCardSlots[i].color = new Color(1, 1, 1, 0f);
        }
    }
    
    if (dealerHand.Count > 0)
    {
        DrawCard(dealerHand[0], dealerHandArea, dealerCardsInHand);
        yield return new WaitForSeconds(0.1f); 
    }

    // Dealer draws additional cards with animation
    while (CalculateHand(dealerHand) <= 17)
    {
        Card newCard = deck.DrawCard();
        dealerHand.Add(newCard);
        DrawCard(newCard, dealerHandArea, dealerCardsInHand);
        yield return new WaitForSeconds(0.34f); // Delay between card draws
    }

    playerTotal = CalculateHand(playerHand);
    if (hasAdd1)
    {
        playerTotal++; 
    }
    dealerTotal = CalculateHand(dealerHand);

     if (dealerTotal > 21 || playerTotal > dealerTotal)
    {
        StartCoroutine(AnimateJammie());
        textBox.SetActive(true);
        mainText = "Player Wins!";
        mainTextUI.text = mainText;
        chips += currentBet * 2;

        // Call the spawncards coroutine on win
        if (spawnCardsScript != null)
        {
            StartCoroutine(spawnCardsScript.SpawnPrefabAtRandomPositions());
        }
    }
    else if (playerTotal == dealerTotal)
    {
        textBox.SetActive(true);
        mainText = "Looks like we draw.";
        mainTextUI.text = mainText;
        chips += currentBet;
    }
    else
    {
        StartCoroutine(AnimateJammie());
        textBox.SetActive(true);
        mainText = "Dealer Wins!";
        mainTextUI.text = mainText;

        // Call the spawncards coroutine on loss
        if (spawnCardsScript != null)
        {
            StartCoroutine(spawnCardsScript.SpawnPrefabAtRandomPositions());
        }
        
        
    }

    gameActive = false;
    hasInsurance = false;
    hasBustBlocker = false;

        hasAdd1 = false;

        
        dealButton.interactable = true;
        increaseBetButton.interactable = true;
        decreaseBetButton.interactable = true;
        clearTableCoroutine = StartCoroutine(ClearTableAfterDelay(5f));
    }

    private int CalculateHand(List<Card> hand)
    {
        int total = 0;
        int aceCount = 0;

        foreach (var c in hand)
        {
   
            total += c.value;
            if (c.rank == "A")
            {
                aceCount++;
            }
        }

        while (total > 21 && aceCount > 0)
        {
            total -= 10;
            aceCount--;
        }

        return total;
    }
    
    private void DrawCard(Card card, Transform handArea, List<GameObject> cards)
    {
        GameObject newCard = Instantiate(cardPrefab, decktransform.position, Quaternion.identity);
      
        SpriteRenderer spriteRenderer = newCard.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = card.sprite;
        }
        
        cards.Add(newCard);
        // Play the CardDeal sound
        if (audioManager != null)
        {
            audioManager.Play("CardDeal");
        }
        else
        {
            Debug.LogWarning("AudioManager is not assigned!");
        }
        
        StartCoroutine(AnimateCardToPosition(newCard, handArea, cards));
    }

    private void UpdateHandLayout(Transform handArea, List<GameObject> cards)
    {
        StartCoroutine(LerpAllCardsToPosition(handArea, cards));
    }
 


    private IEnumerator AnimateCardToPosition(GameObject card, Transform handArea, List<GameObject> cards)
    {
        yield return null;
        
        int cardIndex = cards.IndexOf(card);
        int cardCount = cards.Count;
        float totalWidth = (cardCount - 1) * cardSpacing;
        float xPos = (cardIndex * cardSpacing) - (totalWidth / 2f);
        Vector3 targetPos = handArea.TransformPoint(new Vector3(xPos, 0, 0));

       
        float speed = 5f; 
        float elapsedTime = 0f;
        Vector3 startPos = card.transform.position;

        
        card.transform.SetParent(handArea, true);

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * speed;
            card.transform.position = Vector3.Lerp(startPos, targetPos, elapsedTime);
            yield return null;
        }
        
        card.transform.position = targetPos;
        
        UpdateHandLayout(handArea, cards);
    }
    private IEnumerator LerpCardToPosition(GameObject card, Transform handArea, Vector3 targetLocalPos)
    {
        float speed = 5f; 
        float elapsedTime = 0f;
        Vector3 startPos = card.transform.localPosition;

     
        card.transform.SetParent(handArea, false);

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * speed;
            card.transform.localPosition = Vector3.Lerp(startPos, targetLocalPos, elapsedTime);
            yield return null;
        }
        
        card.transform.localPosition = targetLocalPos;
    }
    
    private IEnumerator DealInitialCards()
    {
        float dealDelay = 0.34f;

        for (int i = 0; i < 2; i++)
        {
            playerHand.Add(deck.DrawCard());
            ShowHands();
            yield return new WaitForSeconds(dealDelay);

            dealerHand.Add(deck.DrawCard());
            ShowHands();
            yield return new WaitForSeconds(dealDelay);
        }

     
        gameActive = true;
        playerTotal = CalculateHand(playerHand);
        dealerTotal = CalculateHand(dealerHand);

        if (dealerTotal == 21)
        {
           
            dealerHand.Clear();
            foreach (var card in dealerCardsInHand)
            {
                Destroy(card);
            }
            dealerCardsInHand.Clear();
            deck.Shuffle();
            
            for (int i = 0; i < 2; i++)
            {
                dealerHand.Add(deck.DrawCard());
                ShowHands();
                yield return new WaitForSeconds(dealDelay);
            }
            
            dealerTotal = CalculateHand(dealerHand);
        }

        if (playerTotal == 21)
        {
            if (dealerTotal == 21)
            {
                StartCoroutine(AnimateJammie());
                textBox.SetActive(true);
                mainText = "Push! Both have Blackjack.";
                mainTextUI.text = mainText;
                chips += currentBet; // Return the bet
            }
            else
            {
                StartCoroutine(AnimateJammie());
                textBox.SetActive(true);
                mainText = "Jammy buggar! You Win!";
                mainTextUI.text = mainText;
                chips += Mathf.RoundToInt(currentBet * 2.5f);
            }
            gameActive = false;
            shopButton.SetActive(true);
            dealButton.interactable = true;
            increaseBetButton.interactable = true;
            decreaseBetButton.interactable = true;
            yield break; // End the coroutine early
        }
    }
    
    
    
    private IEnumerator LerpAllCardsToPosition(Transform handArea, List<GameObject> cards)
    {
        float speed = 5f; 
        float elapsedTime = 0f;

       
        foreach (var card in cards)
        {
            card.transform.SetParent(handArea, false);
        }

    
        List<Vector3> startPositions = new List<Vector3>();
        foreach (var card in cards)
        {
            startPositions.Add(card.transform.localPosition);
        }


        int cardCount = cards.Count;
        float totalWidth = (cardCount - 1) * cardSpacing;
        List<Vector3> targetPositions = new List<Vector3>();

        for (int i = 0; i < cardCount; i++)
        {
            float xPos = (i * cardSpacing) - (totalWidth / 2f);
            targetPositions.Add(new Vector3(xPos, 0, 0));
        }

    
        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * speed;
            for (int i = 0; i < cards.Count; i++)
            {
                cards[i].transform.localPosition = Vector3.Lerp(startPositions[i], targetPositions[i], elapsedTime);
            }
            yield return null;
        }

 
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].transform.localPosition = targetPositions[i];
        }
    }

    private IEnumerator AnimateJammie()
    {
        float animationDuration = 0.7f;
        
        jammieAnim.SetBool("isTalking", true);
        
        yield return new WaitForSeconds(animationDuration);
        jammieAnim.SetBool("isTalking", false);
    }

    public void OnRestart()
    {
        //relaod the scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
  
    
    public ObjectPooling objectPool; // Reference to the object pool
    public float spawnInterval = 1f; // Time interval between spawns

    
    private IEnumerator SpawnObjectsAtTop()
    {
        while (true)
        {
            // Calculate the top of the screen in world coordinates
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;
           // Vector3 topScreenPosition = Camera.main.ScreenToWorldPoint(new Vector3(Random.Range(0, screenWidth), screenHeight, Camera.main.nearClipPlane + 1f));

            // Get an object from the pool
            GameObject pooledObject = objectPool.GetObject();
            if (pooledObject != null)
            {
                // Set the object's position to the top of the screen
                pooledObject.transform.position = pos1.transform.position;
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }
    

}