using System.Collections;
using UnityEngine;

public class spawncards : MonoBehaviour
{
    public GameObject prefabToSpawn; // Assign the prefab in the Inspector
    public Transform spawnPoint; // Optional: Assign a spawn point in the Inspector
    public Deck deck; // Reference to the Deck script

    void Start()
    {
        StartCoroutine(SpawnPrefabAtRandomPositions());
    }

    public IEnumerator SpawnPrefabAtRandomPositions()
    {
        while (true)
        {
            // Calculate the top of the screen in world coordinates
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;
            Vector3 topScreenPosition = Camera.main.ScreenToWorldPoint(new Vector3(0, screenHeight, Camera.main.nearClipPlane));

            // Generate a random X position within the screen width
            float randomX = Random.Range(Camera.main.ScreenToWorldPoint(new Vector3(0, 0, Camera.main.nearClipPlane)).x,
                Camera.main.ScreenToWorldPoint(new Vector3(screenWidth, 0, Camera.main.nearClipPlane)).x);

            // Spawn the prefab at the random position at the top of the screen
            Vector3 spawnPosition = new Vector3(randomX, topScreenPosition.y, 0f);
            GameObject spawnedPrefab = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);

            // Set a random sprite from the deck
            if (deck != null && deck.cards.Count > 0)
            {
                int randomIndex = Random.Range(0, deck.cards.Count);
                Card randomCard = deck.cards[randomIndex];
                SpriteRenderer spriteRenderer = spawnedPrefab.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.sprite = randomCard.sprite;
                }
            }

            // Wait for 0.2 seconds before spawning the next prefab
            yield return new WaitForSeconds(0.2f);
        }
    }
}
