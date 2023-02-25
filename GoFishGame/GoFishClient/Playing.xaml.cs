/*
 * Author: Jordan Wootton & Steven Sadl-Kolchetski
 * Date: 4/10/2021
 * Class: Component Based Programming with .Net
 * Professor: Tony Haworth
 * Project: Project 2 (Group) - Personal Game (GoFish)
 * File: Playing.xaml.cs
 */

using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Windows;
using GoFish;

namespace MessageBoardClient
{
	[CallbackBehavior(ConcurrencyMode=ConcurrencyMode.Reentrant, UseSynchronizationContext=false)]
	public partial class Playing : Window
	{
		private Users _currentUser = null;
		IGoFish _serverRef;

		public Playing(IGoFish server, Users cUser)
		{
			_serverRef = server;
			this._currentUser = cUser;
			this._currentUser.Score = 0;
			InitializeComponent();
			OnLoad();
		}

		public void OnLoad()
        {
			foreach (Card c in _currentUser.Hand.CurrentCardsList)
            {
				handList.Items.Add(c.ToString());
            }
			l_playerName.Content = _currentUser.UserName;
			l_deckCardCount.Content = _serverRef.NumDeckCards();

			foreach(Users u in _serverRef.Users())
            {
				if(u.UserName != _currentUser.UserName)
					EnemyPlayers.Items.Add(u.UserName);
            }
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			Environment.Exit(0);
		}

		//get match button
		private void Button_Click(object sender, RoutedEventArgs e)
		{

			if (EnemyPlayers.SelectedItem != null) //check if user has selected enemy
			{
				Card selectedCard = GetSingleSelectedCard();
				if (selectedCard != null)//check if user only has one card selected
				{
					Card temp = _serverRef.SearchPlayerForMatch(EnemyPlayers.SelectedItem.ToString(), selectedCard, _currentUser); //call method to see if there is a match
					string[] tempArr = handList.SelectedItem.ToString().Split(' ');
					if (temp == null)//if there was no match in enemies hand let user know
					{
						Card goFish = _serverRef.DrawCard(); //GO FISH
						if (goFish != null) //Deck is empty and game is done, alert server
						{
							_currentUser.Hand.CurrentCardsList.Add(goFish);
							handList.Items.Add(goFish.ToString());
							l_deckCardCount.Content = _serverRef.NumDeckCards();
						}
					}
					else //otherwise add the Card to users Hand
					{
						_currentUser.Hand.CurrentCardsList.Add(temp);
						handList.Items.Add(temp.ToString());
					}

				}
				else
					msgBlock.Items.Add("To look for match a SINGLE card must be selected!");
			}
			else
			{
				msgBlock.Items.Add("Must select an enemy before searching for match.");
			}
		}

		//For updating callback values ---- PUT HERE AND PASS IN, this is called from MainWindow.xaml.cs callback function
		public void UpdateNextTurn(int deckVal, Scored scoredVal, string msgToAdd, bool refresh, int nextPlayerIndex)
		{
			if(nextPlayerIndex != -1)
			{
				int userIndex = _serverRef.GetUserIndex(_currentUser);
				if (userIndex != -1 && userIndex == nextPlayerIndex) //Next Player
				{
					b_playbook.IsEnabled = true;
					b_askForMatch.IsEnabled = true;
					b_checkBook.IsEnabled = true;
					Console.WriteLine($"Its {_currentUser.UserName}'s Turn!");
				}
				else //Disable buttons while other players wait for turn
				{
					AwaitUserTurn();
				}
			}

			l_deckCardCount.Content = deckVal;

			if (scoredVal != null)
			{
				if (scoredVal.UserScored.UserName == _currentUser.UserName)
				{
					l_score.Content = scoredVal.NewScore;
				}
				else
				{
					msgBlock.Items.Add($"\n{scoredVal.UserScored.UserName} scored a book!");
				}
			}

			if (msgToAdd != null)
			{
				msgBlock.Items.Add(msgToAdd);
			}

			if (refresh)
			{
				handList.Items.Clear();
				foreach (Card c in _currentUser.Hand.CurrentCardsList)
					handList.Items.Add(c.ToString());
			}
		}

		public void AwaitUserTurn()
		{
			b_playbook.IsEnabled = false;
			b_askForMatch.IsEnabled = false;
			b_checkBook.IsEnabled = false;
		}


		//--------------- Check If Selected <Cards> Are book 
		private void Button_Click_1(object sender, RoutedEventArgs e)
		{
			var selectedCards = GetSelectedCards();
			if (selectedCards.Count == 2)
			{
				if (_serverRef.ValidBook(selectedCards, _currentUser)) //Get if book is valid from server
				{
					msgBlock.Items.Add("VALID BOOK!");
				}
				else
				{
					msgBlock.Items.Add("INVALID BOOK!");
				}
			}
			else
			{
				msgBlock.Items.Add("BOOKS ARE 2 CARDS!");
			}
		}

		//---------- Play Book button
		private void Button_Click_2(object sender, RoutedEventArgs e)
		{
			List<Card> _selectedEntries = GetSelectedCards();
			if(_selectedEntries.Count == 2)
			{
				if (_serverRef.ValidBook(_selectedEntries, _currentUser))
				{
					List<Card> handPlayed = _serverRef.PlayBook(_selectedEntries, _currentUser);
					if (handPlayed != null)
					{
						msgBlock.Items.Add("Book Played!");
						if (handPlayed.Count == 0)
							handPlayed.Add(_serverRef.DrawCard());

						//Display players hand after played book
						handList.Items.Clear();
						foreach (Card c in handPlayed)
						{
							handList.Items.Add(c.ToString());
						}
						l_playerName.Content = _currentUser.UserName;
						this._currentUser.Score++;
						l_score.Content = this._currentUser.Score;
					}
					else
					{
						msgBlock.Items.Add("BOOK NOT PLAYED!"); //Error case
					}
				}
				else
				{
					msgBlock.Items.Add("Unable to play book, NON-MATCHES!");
				}
			}
			else //Invalid selection of cards
			{
				msgBlock.Items.Add("BOOKS ARE 2 CARDS!");
			}
		}

		private void Button_Click_3(object sender, RoutedEventArgs e)
		{
			HowToPlay htp = new HowToPlay();
			htp.Show();
		}

		private void b_endGame_Click(object sender, RoutedEventArgs e)
		{
			_serverRef.EndGame();
		}


		//-------------- Helper Functions

		//----- Return a List<Card> of all selected cards from Hand Listbox
		private List<Card> GetSelectedCards()
		{
			var selectedCards = handList.SelectedItems;
			List<Card> _cardsToCheck = new List<Card>();
			if(selectedCards.Count != 0) 
				foreach (string s in selectedCards) //Build list of selected Card from listbox
					_cardsToCheck.Add(new Card((Suit)Enum.Parse(typeof(Suit), s.Split(' ')[2]), (FaceValue)Enum.Parse(typeof(FaceValue), s.Split(' ')[0])));

			return _cardsToCheck;
		}

		private Card GetSingleSelectedCard()
		{
			if (handList.SelectedItems.Count == 1)//check if user only has one card selected
			{
				string selectedCard = handList.SelectedItem.ToString();
				return new Card((Suit)Enum.Parse(typeof(Suit), selectedCard.Split(' ')[2]), (FaceValue)Enum.Parse(typeof(FaceValue), selectedCard.Split(' ')[0]));
			}
			else
				return null;
		}
	}
}
