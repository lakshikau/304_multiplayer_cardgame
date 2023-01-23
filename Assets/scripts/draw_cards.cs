using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class draw_cards : MonoBehaviour
{
    // game objects import
    public GameObject card1;
    public GameObject card2;
    public GameObject player_area;
    public GameObject opponent_ara;
    public static string[] card_classes = new string[] { "clubs", "spades", "hearts", "diamonds" };
    public static string[] card_faces = new string[] { "jack", "9", "ace", "10", "king", "queen", "8", "7", "6" };
    //private var firstObject : card_values;
    public List<string> deck;
    public string card_class;
    // Start is called before the first frame update
    void Start()
    {
        set_cards();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void On_click()
    {
        
        for (var i=0; i<6; i++)
        {
           
            // add cards to the player area
            GameObject player_card = Instantiate(card1, new Vector3(0, 0, 0), Quaternion.identity);
            card myCard = player_card.GetComponent <card>();
            string thisCard = deck[i];
            int found = thisCard.IndexOf("_");
            print(thisCard);
            myCard.card_class = thisCard.Substring(0, found);
            myCard.card_face = thisCard.Substring(found + 1);
            myCard.set_face();
            player_card.transform.SetParent(player_area.transform, false);
        }
        
    }
    public void set_cards()
    {
        deck = gen_deck();
        shuffle_cards(deck);

    }

    public static List<string> gen_deck()
    {
        List<string> newDeck = new List<string>();
        foreach (string s in card_classes)
        {
            foreach (string v in card_faces)
            {
                newDeck.Add(s + "_" + v);
            }
        }

        return newDeck;
    }

    void shuffle_cards<T>(List<T> list)
    {
        System.Random random = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            int k = random.Next(n);
            n--;
            T temp = list[k];
            list[k] = list[n];
            list[n] = temp;
        }
    }
}

