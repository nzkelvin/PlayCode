using UnityEngine;
using System.Collections;

public class CardGame : MonoBehaviour {

    public int HandSize = 6;
    public GameObject CardBack;

    private GameObject[] Hand;
    private GameObject[] EnemyHand;

    public GameObject[] FairyDeck = new GameObject[6];
    public GameObject[] WitchDeck = new GameObject[6];

    private int[] MyCards = new int[6];
    private int[] EnemyCards = new int[6];

    public int CardType;
    public string CardName;

	// Use this for initialization.
	void Start () {
        Hand = new GameObject[HandSize];
        for (int x = 0; x < HandSize; x++)
        {
            CardType = Random.Range(0, 5);
            GameObject go =
                GameObject.Instantiate(FairyDeck[CardType]) as GameObject;
            Vector3 positionCard = new Vector3((x * 8) + 1, 1, 0);
            go.transform.position = positionCard;
            Hand[x] = go;
        }

        for (int i = 0; i < HandSize; i++)
        {
            CardType = Random.Range(0, 5);
            CardName = string.Format("Witch{0}", CardType);
            EnemyCards[i] = CardType;

            GameObject go =
                GameObject.Instantiate(CardBack) as GameObject;
            Vector3 positionCard = new Vector3((i * 8) + 1, 17, 0);
            go.transform.position = positionCard;
            Hand[i] = go;
        }
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
