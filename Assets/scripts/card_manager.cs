using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using UnityEngine;
using Photon.Realtime;

public class card_manager : MonoBehaviour
{
    // this part is used to implement a singlton
    // not sure if its needed
    // cards are synced using RPCs
    public static card_manager _card_instance;
    public static card_manager card_instance
    {
        get
        {
            if(_card_instance==null)
            {
                _card_instance = GameObject.FindObjectOfType<card_manager>();
                if (_card_instance==null)
                {
                    GameObject container = new GameObject("CardManager");
                    _card_instance = container.AddComponent<card_manager>();
                }
            }
            return _card_instance;
        }
    }

    // card data
    private static string[] card_classes = new string[] { "clubs", "spades", "hearts", "diamonds" };
    private static string[] card_faces = new string[] { "jack", "9", "ace", "10", "king", "queen", "8", "7", "6" };
    private static int[] card_values = new int[] { 30, 20, 11, 10, 3, 2, 0, 0, 0 };
    private static string[] card_deck = new string[36];

    private List<string> deck;
    public void reset_deck()
    {
        // genarate a new shuffled deck
        deck = gen_deck();
        shuffle_cards(deck);
    }

    public static List<string> gen_deck()
    {
        // genarate a new deck
        List<string> newDeck = new List<string>();
        foreach(string s in card_classes)
        {
            foreach (string v in card_faces)
            {
                newDeck.Add(s +"_"+ v);
            }
        }

        return newDeck;
    }

    void shuffle_cards<T>(List<T> list)
    {
        // shuffle the deck
        System.Random random = new System.Random();
        int n = list.Count;
        while(n>1)
        {
            int k = random.Next(n);
            n--;
            T temp = list[k];
            list[k] = list[n];
            list[n] = temp;
        }
    }

    public int get_card_deck_length()
    {
        // get the deck legth
        return deck.Count;
    }

    public string get_card(int i)
    {
        // gat the card at index
        return deck[i];
    }

    public int get_card_value(string card_face)
    {
        // get the points value for the given card
        int face_index = System.Array.IndexOf(card_faces, card_face);
        return card_values[face_index];
    }

    public int get_card_rank(string card_face)
    {
        // this tank is used to tell the higher card
        return (card_faces.Length - System.Array.IndexOf(card_faces, card_face));
    }

    public void set_card_deck(string[] shuffled_deck)
    {
        // set the local clinets card deck based on the card deck sent by master
        deck = new List<string>();
        for (int i=0; i<shuffled_deck.Length;i++)
        {
            deck.Add(shuffled_deck[i]);
        }
    }

    public string[] get_card_deck_string()
    {
        //send the card deck to other clients - used by master
        for (int i = 0; i < deck.Count; i++)
        {
            card_deck[i] = deck[i];
        }
        return card_deck;
    }
}
