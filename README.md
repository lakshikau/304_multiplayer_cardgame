# 304_multiplayer_cardgame
Online multiplayer version of the card game: 304

304 is a card game. There are few versions of the card game
More info on the game: 
https://www.pagat.com/jass/304.html
https://www.catsatcards.com/Games/304.html

The version in this online game uses 6 cards per player and 6 players are needed.

![game screen](https://github.com/lakshikau/304_multiplayer_cardgame/blob/main/304_game_screen_shot.JPG?raw=true)

Players can join from the same network or over the internet. 
A windows build is already included. A Linux and MacOS versions were also tested and worked.

Steps to play.
1) Once the game is loaded every play first need to enter the name and connect. Game will not start untill all 6 players are connected. Players are assinged to the teams alternatingly in the order they connect.

2) Once all players are in 4 cards will be dealt and bidding can start. Only once player can bid at any given time. Use the dropdown at the right to select a bid. Players can either bid or pass. Then the next player will get the opportunity to bid/pass. This can go few rounds until only one bid remains

3) The winner of the bid will get the opportunity to select the trump. Select the card and click on 'Set Trump'

4) Two remaining cards will be dealt and one player will be given the opportunity to start the play. Select the card and click on 'Play Card'. Then the card will move to the center table and the next player will get the opportunity to play

5) The game play will follow the regular playing rules. It will not allow to play other suits when you already have cards from the suit of the current round.

6) When the trump is closed any card of another suit will be played covered. If that card is from the suit of the trump, the trump will be revealed.

7) Once the round is done the winning card will be highlighted and points will be added to the team. The winning player will then get to draw the starting card of the next round

8) Once the game is done the point sum will get compared to the bids and the winning team will be determined and shown. It will auto start another game.

Networking
Photon (free) is used as the network interface. If you want to try use the project it will require the photon plugin to be installed.

https://www.photonengine.com/en-us/photon
https://assetstore.unity.com/packages/tools/network/photon-unity-networking-classic-free-1786
