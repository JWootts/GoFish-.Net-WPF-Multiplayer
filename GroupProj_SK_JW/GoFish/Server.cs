/*
 * Author: Jordan Wootton & Steven Sadl-Kolchetski
 * Date: 4/10/2021
 * Class: Component Based Programming with .Net
 * Professor: Tony Haworth
 * Project: Project 2 (Group) - Personal Game (GoFish)
 * File: Server.cs
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace GoFish
{
    public interface ICallback
    {
        [OperationContract(IsOneWay = true)]
        void Update(int numOfCardsInDeck, string[] users, bool startGame, Users enemy, Scored userScored, string updateMsg, bool playerJoined, int nextPlayerIndex, bool gameOver);
    }

    [ServiceContract(CallbackContract = typeof(ICallback))]
    public interface IGoFish
    {
        [OperationContract]
        Users Join(string name);
        [OperationContract(IsOneWay = true)]
        void Leave(string name);
        [OperationContract]
        string[] GetAllUsers();
        [OperationContract]
        int NumDeckCards();
        [OperationContract]
        Card DrawCard();
        [OperationContract]
        List<Users> Users();
        [OperationContract]
        int GetUserIndex(Users u);
        [OperationContract]
        bool ValidBook(List<Card> selectCards, Users currentUser);
        [OperationContract(IsOneWay =true)]
        void StartGame();
        [OperationContract]
        Users GetWinnerOfGame();
        [OperationContract(IsOneWay = true)]
        void EndGame();
        [OperationContract]
        List<Card> PlayBook(List<Card> book, Users userPlaying);
        [OperationContract]
        Card SearchPlayerForMatch(string enemy, Card card, Users currentUsers);
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class Server : IGoFish
    {
        private List<Users> _users = new List<Users>();
        private Deck _deck = new Deck();
        private Dictionary<Users, ICallback> callbacks = new Dictionary<Users, ICallback>();    // Stores id and callback for each client
        private int _currentPlayerIndex;
        private Users _winner = null; //winner of game - initally null

        //------- Public constructor to build type Server
        public Server()
		{
            _currentPlayerIndex = 0;
        }

        //When user joins, return refrence to user to store within gui context
        public Users Join(string name)
        {
            ICallback cb = OperationContext.Current.GetCallbackChannel<ICallback>();

            if (callbacks.ContainsValue(cb))
			{
                int i = callbacks.Values.ToList().IndexOf(cb);
                return callbacks.Keys.ElementAt(i);
			}
                
            else
            {
                Users u = new Users(name.ToUpper());
                u.SetHand(new Hand(this._deck));
                _users.Add(u);
                callbacks.Add(u, cb);
                RefreshLobbyWindow();
                return u;
            }
        }

        //When a user 'leaves' the game, flush them from users list and callbacks
        public void Leave(string name)
        {
            RemoveUser(name); //Removed user from local list
            ICallback cb = OperationContext.Current.GetCallbackChannel<ICallback>();
            if (callbacks.ContainsValue(cb))
            {
                int i = callbacks.Values.ToList().IndexOf(cb);
                Users id = callbacks.ElementAt(i).Key;
                callbacks.Remove(id);
            }
        }

        //Return an string array of all users - to be used for displaying in listbox
        public string[] GetAllUsers()
        {
            Console.WriteLine(_deck.CardsInDeck.Count);
            List<string> _usernames = new List<string>();
            foreach (Users u in _users)
                _usernames.Add(u.UserName);
            return _usernames.ToArray<string>();
        }

        //Return Current 'single' deck count
        public int NumDeckCards()
        {
           return _deck.CardsInDeck.Count; 
        }

        //To start event of game for all players
		public void StartGame()
		{
            startGameForAllClient();
        }

        //----------------------------------------------------------------------------------------- Playing Game Functions
        public Card DrawCard()
        {
            Card returnCard = _deck.DrawTopCard();
            if (returnCard == null) //Game is over no more cards in deck
            {
                EndGame();
            }
            else
            {
                UpdateClients();
            }
            return returnCard;
        }

        //Will return hand after book has been played
        public List<Card> PlayBook(List<Card> book, Users userPlaying)
		{
            foreach (Users u in _users)
            {
                if (u.UserName == userPlaying.UserName)
                {
                    List<Card> tmpHand = u.Hand.CurrentCardsList;
                    foreach (Card c in book)
                        foreach (Card a in tmpHand)
                            if (c.CardSuit == a.CardSuit && c.Number == a.Number)
                            {
                                tmpHand.Remove(a);
                                break;
                            }

                    u.Hand.CurrentCardsList = tmpHand; //Reassign players hand after cards removed
                    u.Score++; //Add to players score
                    _currentPlayerIndex = (_users.Count-1 > _currentPlayerIndex) ? (_currentPlayerIndex + 1) : 0; //increment current player count after hand has been player if does not fall out of play list scope
                    UpdateBookPlayed(new Scored(u, u.Score), _currentPlayerIndex);
                    return u.Hand.CurrentCardsList; //Return that book was played
                }
            }
            return null; //Username not found as player
        }

        //Return all current users in the game
        public List<Users> Users()
        {
            return _users;
        }

        //Pass in a user to retrieve their 'index' value, this is the order that the game is played 
        //   - ie (player 1, player 2, player 3 etc..)
        public int GetUserIndex(Users u)
		{
            for(int i = 0; i < _users.Count; ++i)
                if (_users.ElementAt(i).UserName == u.UserName)
                    return i;

            return -1;
		}

        //Pass in selected cards from the listbox and the current user to determine if their book is valid
        public bool ValidBook(List<Card> selectCards, Users currentUser)
		{
            if (selectCards.Count != 2) //Return if cards selected are not 2
                return false;

                Card _cardToCheck = selectCards.ElementAt(0);
                foreach (Card c in selectCards)
                    if (_cardToCheck.Number != c.Number)
                        return false; //Not all card numbers match

                return true; //Made through all check cases, is a valid book
        }

        //Pass in an enemy name and the current user to determine if the 'enemy' has a matched card. If so remove from
        //the enemy list and return it to be added to the current player
        public Card SearchPlayerForMatch(string enemy, Card card, Users currentUsers)
        {
            foreach(Users u in _users) // look through the list of players
            {
                if(enemy == u.UserName) //if the selected enemy matches the one in the list
                {
                    foreach(Card c in u.Hand.CurrentCardsList) //look at every card in the enemies list
                    {
                        if(c.Number == card.Number) //if enemy player has a matching VALUE
                        {
                            Card cTemp = c;
                            u.Hand.CurrentCardsList.Remove(c); //delete Enemies instance of Card.
                            if (u.Hand.CurrentCardsList.Count == 0)
                                u.Hand.CurrentCardsList.Add(DrawCard()); //Add card if hand is empty
                            UpdateClientsCardRequested(u, $"{currentUsers.UserName} STOLE {cTemp} from {enemy}!", _currentPlayerIndex);
                            return cTemp; //return that back to the plaer who asked
                        }
                    }
                }
            }
            _currentPlayerIndex = (_users.Count - 1 > _currentPlayerIndex) ? (_currentPlayerIndex + 1) : 0; //increment current player count after hand has been player if does not fall out of play list scope
            UpdateClientsCardRequested(null, $"{enemy} MADE {currentUsers.UserName} GOFISH!", _currentPlayerIndex);
            return null; //return null if card is not found
        }

        //End Game and Return the User that 'Won' to display
        public void EndGame()
		{
            Users highestScoreUser = null; //Assing first user as starting point
            int highestScore = int.MinValue;
            foreach (Users u in _users)
                if (highestScore < u.Score)
                {
                    highestScoreUser = u;
                    highestScore = u.Score;
                }
            _winner = highestScoreUser;
            UpdateGameOver();//Call update on clients 
        }

        //------------- Function to return 'Winner' of game to clients
        public Users GetWinnerOfGame()
		{
            return _winner;
		}

        //------------------------- Helper Functions For Users List
        private bool hasUser(string name)
        {
            bool flag = false;
            foreach (Users u in _users)
                if (u.UserName == name.ToUpper())
                    return true;
            return flag;
        }

        private void RemoveUser(string name)
        {
            int index = 0;

            if (hasUser(name))
            {
                foreach (Users u in _users)
                    if (!u.UserName.Equals(name.ToUpper()))
                        index++;
                _users.RemoveAt(index);
            }
        }

        // ----------------------- Helper methods to invoke an update on clients
        private void RefreshLobbyWindow()
        {
            foreach (ICallback cb in callbacks.Values)
                cb.Update(NumDeckCards(), GetAllUsers(), false, null, null, null, true, -1, false); //Set playJoined field to true
        }

        private void startGameForAllClient()
        {
            foreach (ICallback cb in callbacks.Values)
                cb.Update(NumDeckCards(), GetAllUsers(), true, null, null, null, false, 0, false);
        }

        private void UpdateClientsCardRequested(Users enemyToUpdatem, string msgForUsers, int nextUserID)
        {
            foreach (ICallback cb in callbacks.Values)
                cb.Update(NumDeckCards(), GetAllUsers(), false, enemyToUpdatem, null, msgForUsers, false, nextUserID, false); //Set playJoined field to true
        }

        private void UpdateBookPlayed(Scored s, int nextPlayerIndex)
        {
            foreach (ICallback cb in callbacks.Values)
                cb.Update(NumDeckCards(), GetAllUsers(), false, null, s, null, false, nextPlayerIndex, false); //Set playJoined field to true
        }

        private void UpdateClients()
        {
            foreach (ICallback cb in callbacks.Values)
                cb.Update(NumDeckCards(), GetAllUsers(), false, null, null, null, false, -1, false); //Set playJoined field to true
        }

        private void UpdateGameOver()
        {
            //run final callbacks
            foreach (ICallback cb in callbacks.Values)
                cb.Update(NumDeckCards(), GetAllUsers(), false, null, null, null, false, -1, true); //Set playJoined field to true

            //flush server (Game Done)
            //_deck = null;
            //_users = null;
        }
    }
}

