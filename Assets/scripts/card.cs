using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;
using UnityEngine.UI;

public class card : MonoBehaviour
{
    public bool card_selected = false;
    public bool show_card_value = true;

    public string card_class;
    public string card_face;
    public int card_value;
    public bool show_face_on_table;
    public int card_plyer;
    public string card_player_team;

    private void Awake()
    {

    }

    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void set_face(bool show_face = true )
    {
        // set the face on the card
        // not sure if this needs to stay
        // this function can go to card manager
        // if so this class is not needed
        string card_name = "card_classes/";
        if (show_face)
        {
            card_name = card_name + card_face + "_of_" + card_class;
        }
        else
        {
            card_name = card_name + "card_back";
        }
        
        Sprite card_face_srptie = Resources.Load<Sprite>(card_name);
        GetComponent<Image>().sprite = card_face_srptie;
        show_face_on_table = show_face;
    }

    public void set_this_trump()
    {
        // this wil be highlighted as the trump card form the trmup setting player
        GetComponent<Image>().color = new Color32(255, 180, 255,100);
    }

    public void on_card_click()
    {
        //set all the cards to unselected
        var foundCanvasObjects = FindObjectsOfType<card>();
        for(int i=0;i< foundCanvasObjects.Length; i++)
        {
            foundCanvasObjects[i].card_selected = false;
        }
        card_selected = true;
    }
}
