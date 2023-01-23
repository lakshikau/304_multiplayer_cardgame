using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using JetBrains.Annotations;
using System.Linq;
using System;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Diagnostics.Tracing;
using System.Timers;

public class game_manager : MonoBehaviour
{
    // Public vars
    public GameObject player_area;
    public GameObject play_table_area;
    public GameObject cardPreFab;
    public Text gameState;
    public Text bid_winner_name;
    public Text bid_winner_val;
    public Text current_bid_display;
    public Text player_warining;
    public Text b_team_score;
    public Text r_team_score;
    public Text trump_status;
    public Text b_team_wins;
    public Text r_team_wins;
    public Text[] player_lables = new Text[6];

    //public buttons
    public Button bid_button;
    public Button pass_button;
    public Button trump_button;
    public Button play_button;
    public Button reset_button;

    // private vars
    private Player[] blue_team = new Player[3];
    private Player[] red_team = new Player[3];
    private bool game_started;
    private string game_state;
    private int red_team_score;
    private int blue_team_score;
    private bool bidding_skipped;
    private int local_player_bid;
    private Player bid_winner_player;
    //private int winning_bid_value;
    private string trump_class;
    private string trump_face;
    //private string play_round_class;
    private int round_start_player;
    private string round_card_class;
    private bool round_trump_open;
    bool round_started;
    private int round_count;


    // master client vars
    private bool bidding_master;
    private bool trump_master;
    private bool playing_master;
    private int rounds_per_game;

    // Photon View
    private PhotonView PV;

    // play_vars
    private int dealer_player;
    //private int current_payer;

    // constants
    private Color32 blue_team_color = new Color32(0, 154, 255, 255);
    private Color32 red_team_color = new Color32(255, 81, 86, 255);
    private Color32 player_warning_color = new Color32(255, 68, 18, 255);

    private string blue_team_name = "Blue";
    private string red_team_name = "Red";

    // When the game starts (this executes before start method)
    private void Awake()
    {
        PV = GetComponent<PhotonView>();
        game_started = false;
        gameState.text = "Waiting";
        game_state = "Waiting";
        bid_button.interactable = false;
        current_bid_display.text = "170";
        bidding_skipped = false;
    }
    // Start is called before the first frame update  
    void Start()
    {
        dealer_player = 0;
        rounds_per_game = 6;
        //round_count = 0;
        bid_button.interactable = false;
        pass_button.interactable = false;
        play_button.interactable = false;
        trump_button.interactable = false;
        reset_button.interactable = false;

        
    }

    // Update is called once per frame
    void Update()
    {
        if ((!game_started) & (PhotonNetwork.InRoom) & (PhotonNetwork.IsMasterClient))
        {
            check_room_for_players();
        }
    }

    // --------------------------------------------------
    // start game
    // --------------------------------------------------
    public void check_room_for_players()
    {
        // check if room is full
        if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            Debug.Log("Players connected, starting game");
            // can start the game
            game_started = true;
            reset_button.interactable = true;
            PV.RPC("set_game_state", RpcTarget.All, "Started");
            // send msg to other clients to start the game.
            int player_count = PhotonNetwork.PlayerList.Count();

            if (PhotonNetwork.IsMasterClient)
            {
                //assing players to teams
                for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i += 2)
                {
                    blue_team[i / 2] = PhotonNetwork.PlayerList[i];
                    red_team[i / 2] = PhotonNetwork.PlayerList[i + 1];

                }
                dealer_player = 0;
                //PV.RPC("set_teams", RpcTarget.All, blue_team, red_team);
                PV.RPC("set_dealer_player", RpcTarget.All, 0);
                PV.RPC("start_master_game_loop", RpcTarget.MasterClient, dealer_player,1d);


                
            }
        }

    }

    // --------------------------------------------------
    // Button functions
    // --------------------------------------------------
    public void bid_button_clicked()
    {
        // increase the bid and pass to next player
        int current_bid = int.Parse(current_bid_display.text);
        current_bid += 10;
        local_player_bid = current_bid;
        PV.RPC("player_bid", RpcTarget.All, next_plyer_position(), current_bid);
        bid_button.interactable = false;
        pass_button.interactable = false;
    }

    public void pass_button_clicked()
    {
        // players passed the bidding and turn goes to next
        bidding_skipped = true;
        int current_bid = int.Parse(current_bid_display.text);
        PV.RPC("player_bid", RpcTarget.All, next_plyer_position(), current_bid);
        bid_button.interactable = false;
        pass_button.interactable = false;
    }

    public void trump_button_clicked()
    {
         // for bid player indicate that the selected is trump
        get_selected_card().set_this_trump();
        PV.RPC("set_trump", RpcTarget.All, get_selected_card().card_class, get_selected_card().card_face);
    }

    public void play_button_clickked()
    {
        // let the player play the card
        // the play card will be added in to the play area of all the players

        // record the class of the card
        card playing_card = get_selected_card();
        // move the card from the player area to comman area
        // use a function parameter to show or hide the card face to others.
        bool force_open_trump = false;
        bool card_valid = true;
        // run card validation
        if (!round_started)
        {
            // first_play card
            // bid player cannot stat with trump class on the first round
            if ((playing_card.card_class == trump_class) && (bid_winner_player.NickName == PhotonNetwork.LocalPlayer.NickName) && (round_count == 1))
            {
                card_valid = false;
            }
            if ((bid_winner_player.NickName == PhotonNetwork.LocalPlayer.NickName) && (playing_card.card_class == trump_class) && (playing_card.card_face == trump_face))
            {
                force_open_trump = true;
            }
        }
        else if (round_started)
        {
            // cannot play any other class if the player has the class
            int player_cards_len = player_area.transform.childCount;
            bool player_has_class = false;
            for (int i = 0; i < player_area.transform.childCount; i++)
            {
                var hand_card_class = player_area.transform.GetChild(i).gameObject.GetComponent<card>().card_class;
                if (hand_card_class == round_card_class)
                {
                    player_has_class = true;
                    break;
                }
            }
            if ((player_has_class) && (playing_card.card_class != round_card_class))
            {
                card_valid = false;
            }

            // when cutting only use the pre selected trump card
            if ((bid_winner_player.NickName == PhotonNetwork.LocalPlayer.NickName) && (!round_trump_open))
            {
                if ((round_card_class != trump_class) && (playing_card.card_class == trump_class) && (playing_card.card_face != trump_face))
                {
                    card_valid = false;
                }
            }
        }


        // if validation fails then show msg and retry.
        if (card_valid)
        {
            // need to show whos tun to play to other players
            // if the round is complete start a new round
            // on master decide who won the round score and who is the round winner
            // pass the next turn to the round winner
            show_player_warining(false);
            play_button.interactable = false;
            PV.RPC("play_card_to_table", RpcTarget.All, playing_card.card_class, playing_card.card_face, force_open_trump, local_player_position(), next_plyer_position());
            Destroy(playing_card.gameObject);
            
        }
        else
        {
            // show the warning
            StartCoroutine(show_player_warining(true, "!!! your card selection is not legal. Please try again. !!!", 5f));
        }
    }

    public void reset_button_clicked()
    {
        // clear the card on the player area too
        PV.RPC("clear_player_area", RpcTarget.All);
        PV.RPC("start_master_game_loop", RpcTarget.MasterClient, dealer_player, 1d);
    }

    // --------------------------------------------------
    // Other functions
    // --------------------------------------------------
    private int local_player_position()
    {
        // get the palyer index fron the list of players
        return System.Array.IndexOf(PhotonNetwork.PlayerList, PhotonNetwork.LocalPlayer);
    }

    private int next_plyer_position()
    {
        // index of the next plyer in the list.
        // if current plyer is the last, switch to first
        int nxt_player = local_player_position()+1;
        if (nxt_player < PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            return nxt_player;
        }
        else
        {
            return 0;
        }
    }
    
    private card get_selected_card()
    {
        card selected_card = null;
        var player_cards = player_area.transform.childCount;
        for (var i = 0; i < player_cards; i++)
        {

            if (player_area.transform.GetChild(i).GetComponent<card>().card_selected)
            {
                selected_card = player_area.transform.GetChild(i).GetComponent<card>();
            }
        }
        return selected_card;
    }
       
    IEnumerator show_player_warining(bool show, string warn_text="", float wait_sec=5f)
    {
        if (show)
        {
            // show the player warining when need to show test warning to player
            player_warining.text = warn_text;
            player_warining.color = player_warning_color;
            player_warining.enabled = true;
            yield return new WaitForSeconds(wait_sec);
            player_warining.enabled = false;
        }
        else
        {
            player_warining.text = "";
            player_warining.enabled = false;
        }
    }

    IEnumerator show_winning_team( string team_name, float wait_sec = 5f)
    {
        player_warining.text = team_name + " team won the game ....!";
        if (team_name == blue_team_name)
        {
            player_warining.color = blue_team_color;
        }
        else
        {
            player_warining.color = red_team_color;
        }
        player_warining.enabled = true;
        yield return new WaitForSeconds(wait_sec);
        player_warining.enabled = false;

    }

    async Task run_bidding()
    {
        // this function waits untill the bidding is done
        // used to retun the control to master game loop after bidding
        while(bidding_master)
        {
            await Task.Yield();
        }
        return;
    }

    async Task run_select_trump()
    {
        // wait untill the bid winning player set the trump
        while (trump_master)
        {
            await Task.Yield();
        }
        return;
    }

    private string get_player_team(int player_position)
    {
        string team = blue_team_name;
        if (red_team.Contains(PhotonNetwork.PlayerList[player_position]))
        {
            team = red_team_name;
        }
        return team;
    }

    IEnumerator clear_table_delay(float wait_sec = 10f)
    {

        yield return new WaitForSeconds(wait_sec);
        // clear the table
        int table_card_count = play_table_area.transform.childCount;
        if (table_card_count > 0)
        {
            for (int i = 0; i < table_card_count; i++)
            {
                Destroy(play_table_area.transform.GetChild(i).gameObject);
            }
            PV.RPC("set_round_state", RpcTarget.All, false, "None");
        }
        set_slot_names(round_start_player);

    }

    private void set_slot_names(int player_pos)
    {
        
        // set the card place holder lables for starting from the rund start player
        var player_slot = player_pos + 1;
        for (int i = 0; i <= PhotonNetwork.CurrentRoom.MaxPlayers - 1; i++)
        {
            player_lables[i].text = PhotonNetwork.CurrentRoom.GetPlayer(player_slot).NickName;
            if (player_slot % 2 == 0)
            {
                player_lables[i].color = red_team_color;
            }
            else
            {
                player_lables[i].color = blue_team_color;
            }

            player_slot += 1;
            if (player_slot > PhotonNetwork.CurrentRoom.MaxPlayers)
            {
                player_slot = 1;
            }
        }
    }

      
    // --------------------------------------------------
    //RPC functions
    // --------------------------------------------------
    [PunRPC]
    void set_game_state(string state)
    {
        // update the game state text on the UI
        gameState.text = state;
        game_state = state;
    }

    [PunRPC]
    void send_shffled_deck(string[] shuffled_deck)
    {
        // send the shuffuled deck to the other clients
        if(!PhotonNetwork.IsMasterClient)
        {
            card_manager.card_instance.set_card_deck(shuffled_deck);
        }
    }

    [PunRPC]
    void deal_cards(int deal_count)
    {
        // deals the cards among plyers
        int player_position = local_player_position();
        int start_position;
        // get the start position
        if (deal_count == 4)
        {
            start_position = 0;
        }
        else
        {
            start_position = 4*PhotonNetwork.CurrentRoom.MaxPlayers;
        }
        // get the start card based on the plyer position on the list
        int player_start_card = start_position + (player_position * deal_count);

        for (int i = player_start_card; i < player_start_card+deal_count; i++)
        {
            // add the card to player area and set the card face
            GameObject player_card = Instantiate(cardPreFab, new Vector3(0, 0, 0), Quaternion.identity);
            card p_card = player_card.GetComponent<card>();
            string thisCard = card_manager.card_instance.get_card(i);
            int found = thisCard.IndexOf("_");
            p_card.card_class = thisCard.Substring(0, found);
            p_card.card_face = thisCard.Substring(found + 1);
            p_card.set_face();
            player_card.transform.SetParent(player_area.transform, false);
        }
    }
   
    [PunRPC]
    void player_bid(int player_index, int current_bid)
    {
        // only allow bidding to the current player
        current_bid_display.text = current_bid.ToString();
        int cur_player = local_player_position();
        if (player_index == cur_player)
        {
            if (local_player_bid == current_bid)
            {
                // this means all other players have skipped so this players wins the bid
                PV.RPC("bid_complete", RpcTarget.All, PhotonNetwork.LocalPlayer, current_bid);

            }
            else if (bidding_skipped)
            {
                PV.RPC("player_bid", RpcTarget.All, next_plyer_position(), current_bid);
            }
            else
            {
                bid_button.interactable = true;
                pass_button.interactable = true;
            }
        }
    }

    [PunRPC]
    void bid_complete(Player bid_win_player, int bid_win_value)
    {
        // bidding is complete disable the bid buttons and 
        // set the flag to return to master
        if (PhotonNetwork.IsMasterClient)
        {
            bidding_master = false;
        }
        bid_winner_name.text = bid_win_player.NickName;
        bid_winner_val.text = bid_win_value.ToString();
        bid_winner_player = bid_win_player;
        bid_button.interactable = false;
        pass_button.interactable = false;       
    }

    [PunRPC]
    void start_set_trump()
    {
        play_button.interactable = false;
        trump_button.interactable = false;
        if (PhotonNetwork.LocalPlayer == bid_winner_player)
        {
            // let him set the trumph
            trump_button.interactable = true;

        }
    }

    [PunRPC]
    void set_trump(string card_class, string card_face)
    {
        trump_class = card_class;
        trump_face = card_face;
        trump_button.interactable = false;
        round_trump_open = false;
        if (PhotonNetwork.IsMasterClient)
        {
            trump_master = false;
        }
    }

    [PunRPC]
    void play_card_to_table(string card_class, string card_face, bool force_open_trump, int cur_player, int next_player)
    {
        
        if (cur_player == round_start_player)
        {
            // this player sets the round class
            PV.RPC("set_round_state", RpcTarget.All, true, card_class);
        }
        if (force_open_trump)
        {
            // trump player is exposing the trump
            PV.RPC("round_trump_status", RpcTarget.All, true, card_class, card_face);

        }
        GameObject player_card = Instantiate(cardPreFab, new Vector3(0, 0, 0), Quaternion.identity);
        card p_card = player_card.GetComponent<card>();
        p_card.card_class = card_class;
        p_card.card_face = card_face;
        p_card.card_plyer = cur_player;
        p_card.card_player_team = get_player_team(cur_player);
        //if (((card_class == round_card_class) || (round_trump_open)) && (!((card_class == trump_class) && (card_face == trump_face))))
        //{
        //    p_card.set_face();
        //}
        //else
        //{
        //    p_card.set_face(false);
        //}

        if (round_trump_open)
        {
            p_card.set_face();
        }
        else if ((card_class == round_card_class) && !((card_class == trump_class) && (card_face == trump_face)))
        {
            p_card.set_face();
        }
        else
        {
            p_card.set_face(false);
        }

        player_card.transform.SetParent(play_table_area.transform, false);
        if (( local_player_position() == next_player) && (next_player != round_start_player))
        {
            play_button.interactable = true;
        }

        // check if the round is complete if so close the round and restart the next round.
        if ((PhotonNetwork.IsMasterClient) && (next_player == round_start_player))
        {
            // all the players are done playing for this round..
            // get cards on the table
            int table_card_count = play_table_area.transform.childCount;
            card[] table_cards = new card[table_card_count];

            // get the table cards
            for (int i = 0; i < table_card_count; i++)
            {
                table_cards[i] = play_table_area.transform.GetChild(i).GetComponent<card>();
            }

            bool trump_played_in_this_round = false;
            int win_card = 0;

            if (round_trump_open)
            {
                for (int i = 0; i < table_card_count; i++)
                {
                    if (table_cards[i].card_class == trump_class)
                    {
                        // trump is in the hand on one of the face down card
                        trump_played_in_this_round = true;
                    }
                }
            }
            else
            {
                // check closed card for trump

                for (int i = 0; i < table_card_count; i++)
                {
                    if ((!table_cards[i].show_face_on_table) && (table_cards[i].card_class == trump_class))
                    {
                        // trump is in the hand on one of the face down card
                        PV.RPC("round_trump_status", RpcTarget.All, true, trump_class, trump_face);
                        round_trump_open = true;
                        trump_played_in_this_round = true;
                    }
                }
               
            }

            if (trump_played_in_this_round)
            {
                var highest_trump = -1;
                // trump was found on an earlier round.
                // check if trump is played in the hand
                for (int i = 0; i < table_card_count; i++)
                {
                    if (table_cards[i].card_class == trump_class)
                    {
                        if (highest_trump < card_manager.card_instance.get_card_rank(table_cards[i].card_face))
                        {
                            win_card = i;
                            highest_trump = card_manager.card_instance.get_card_rank(table_cards[i].card_face);
                        }
                    }
                }
            }
            else
            {
                // no trump involment 
                // check the had as usual
                // check only the upside cards
                string round_class = table_cards[0].card_class;
                win_card = 0;

                for (int i = 1; i < table_card_count; i++)
                {
                    if ((table_cards[i].show_face_on_table) && (table_cards[i].card_class == round_class))
                    {
                        if (card_manager.card_instance.get_card_rank(table_cards[win_card].card_face) < card_manager.card_instance.get_card_rank(table_cards[i].card_face))
                        {
                            win_card = i;
                        }
                    }
                }
            }
            // now we know the winning card.

             // calculate the total value in the hand
            int round_score = 0;
            for (int i = 0; i < table_card_count; i++)
            {
                round_score = round_score + card_manager.card_instance.get_card_value(table_cards[i].card_face);
            }

            // detrmine the round winner / team
            string round_win_team = table_cards[win_card].card_player_team;
            if (round_win_team == blue_team_name)
            {
                blue_team_score = blue_team_score + round_score;
            }
            else
            {
                red_team_score = red_team_score + round_score;
            }

            // detarmin the start player.
            var round_start_player_temp = table_cards[win_card].card_plyer;
            


            // show cards on the table to all plyers..
            PV.RPC("show_winning_card_on_table", RpcTarget.All, win_card, round_trump_open, blue_team_score, red_team_score);


            if (round_count == rounds_per_game)
            {
                // all rounds are done for the game
                // determine the if the dealer cahnged to the next player based on the win
                // if the dealr moved more tha max plaers then re set to 0
                var win_team = "None";
                var bidding_team = get_player_team(bid_winner_player.ActorNumber-1);
                var dealer_team = get_player_team(dealer_player);
                if (bidding_team==blue_team_name)
                {
                    if (blue_team_score > int.Parse(bid_winner_val.text))
                    {
                        win_team = blue_team_name;
                    }
                    else
                    {
                        win_team = red_team_name;
                    }
                }
                else if (bidding_team == red_team_name)
                {
                    if (red_team_score > int.Parse(bid_winner_val.text))
                    {
                        win_team = red_team_name;
                    }
                    else
                    {
                        win_team = blue_team_name;
                    }
                }

                PV.RPC("set_win_team_points", RpcTarget.All, win_team);

                if (dealer_team == win_team)
                {
                    dealer_player += 1;
                    if (dealer_player >= PhotonNetwork.CurrentRoom.MaxPlayers)
                    {
                        dealer_player = 0;
                    }
                    PV.RPC("set_dealer_player", RpcTarget.All, dealer_player);

                }
                Debug.Log("----rounds per game ended----. ----- restart a new game------");
                // show the wining team and stay before restarting the new round

                PV.RPC("start_master_game_loop", RpcTarget.MasterClient, dealer_player, 10d);

            }
            else 
            {
                // restat the next round.
                PV.RPC("set_round_start_plyer", RpcTarget.All, round_start_player_temp);
            }

           

        }

    }

    [PunRPC]
    void set_round_start_plyer(int player_pos)
    {
        round_count += 1;
        round_started = false;
        round_start_player = player_pos;
        //StartCoroutine(clear_table_delay(0f));
        set_slot_names(round_start_player);

        if (player_pos == local_player_position())
        {
          play_button.interactable = true;
        }
    }

    [PunRPC]
    void show_winning_card_on_table(int win_card, bool trump_open, int blue_score, int red_score)
    {
        // show the players which card won the round
        b_team_score.text = blue_score.ToString();
        r_team_score.text = red_score.ToString();
        if (!PhotonNetwork.IsMasterClient)
        {
            blue_team_score = blue_score;
            red_team_score = red_score;
        }
        play_table_area.transform.GetChild(win_card).GetComponent<Image>().color = new Color32(255, 180, 255, 100);
        int table_card_count = play_table_area.transform.childCount;
        for (int i = 0; i < table_card_count; i++)
        {
            if (trump_open)
            {
                play_table_area.transform.GetChild(i).GetComponent<card>().set_face();
            }
        }
        // clear the table after showing the card results
        StartCoroutine(clear_table_delay(10f));
    }

    [PunRPC]
    void round_trump_status(bool trump_open, string trum_class, string trump_face="")
    {
        round_trump_open = trump_open;
        trump_status.text = trum_class+" "+ trump_face;
    }

    [PunRPC]
    void set_round_state(bool round_state, string round_class)
    {
        round_card_class = round_class;
        round_started = round_state;
    }

    [PunRPC]
    async void start_master_game_loop(int game_deal_player, double sec=0d)
    {
        // this is only called by paster and will only be executed in master
        // start the master game loop
        await Task.Delay(TimeSpan.FromSeconds(sec));

        Debug.Log("Strating master game loop");
        //reset the round count
        PV.RPC("reset_round_data", RpcTarget.All);

        // --------------------------------------------------
        // Initial Card draw
        // --------------------------------------------------
        // reset the card deck
        card_manager.card_instance.reset_deck();
        PV.RPC("send_shffled_deck", RpcTarget.All, card_manager.card_instance.get_card_deck_string());
        PV.RPC("deal_cards", RpcTarget.All, 4);

        // --------------------------------------------------
        // Bidding functions
        // --------------------------------------------------
        PV.RPC("set_game_state", RpcTarget.All, "Bidding");
        // master client get to bid first
        bidding_master = true;
        var bid_start_player = dealer_player + 1;
        if (bid_start_player>=PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            bid_start_player = 0;
        }

        PV.RPC("player_bid", RpcTarget.All, bid_start_player, 170);
        // wait until bidding round is done
        await run_bidding();
        Debug.Log("Bidding Finished Master");

        // --------------------------------------------------
        // set trump
        // --------------------------------------------------
        PV.RPC("set_game_state", RpcTarget.All, "Set Trump");
        // send signal to set trump
        PV.RPC("start_set_trump", RpcTarget.All);
        trump_master = true;
        await run_select_trump();

        // --------------------------------------------------
        // finish card draw
        // --------------------------------------------------
        PV.RPC("deal_cards", RpcTarget.All, 2);

        // --------------------------------------------------
        // play
        // --------------------------------------------------
        PV.RPC("set_game_state", RpcTarget.All, "Play");
        PV.RPC("round_trump_status", RpcTarget.All, false, "closed", "");
        //PV.RPC("master_play", RpcTarget.All);
        //playing_master = true;
        //await run_main_play();
        // start playing the game
        //play_button.interactable = false;
        PV.RPC("set_round_state", RpcTarget.All, false, "None");
        // this is the player after the deal
        //play_button.interactable = true;

        var new_game_start_plyer = game_deal_player + 1;
        if (new_game_start_plyer>= PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            new_game_start_plyer = 0;
        }
        PV.RPC("set_round_start_plyer", RpcTarget.All, new_game_start_plyer);

    }

    [PunRPC]
    void reset_round_data()
    {
        round_count = 0;
        bid_winner_name.text = "--";
        bid_winner_val.text = "--";
        b_team_score.text = "0";
        r_team_score.text = "0";
        trump_status.text = "--";
        blue_team_score = 0;
        red_team_score = 0;
        bidding_skipped = false;
        local_player_bid = 0;
        StartCoroutine(clear_table_delay(0f));


    }

    [PunRPC]
    void set_dealer_player(int new_dealer_player)
    {
        dealer_player = new_dealer_player;
    }

    [PunRPC]
    void set_win_team_points(string win_team)
    {
        StartCoroutine(show_winning_team(win_team, 10f));
        if (win_team == blue_team_name)
        {
            b_team_wins.text =  (int.Parse(b_team_wins.text) + 1).ToString();
        }
        else
        {
            r_team_wins.text = (int.Parse(r_team_wins.text) + 1).ToString();
        }
        

    }

    [PunRPC]
    void clear_player_area()
    {
        int table_card_count = player_area.transform.childCount;
        if (table_card_count > 0)
        {
            for (int i = 0; i < table_card_count; i++)
            {
                Destroy(player_area.transform.GetChild(i).gameObject);
            }
            
        }
        play_button.interactable = false;
    }

    //[PunRPC]
    //void set_teams(Player[] blue_team_members, Player[] red_team_members)
    //{
    //    blue_team = blue_team_members;
    //    red_team = red_team_members;
    //}

}
