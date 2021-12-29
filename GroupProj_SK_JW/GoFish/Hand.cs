/*
 * Author: Jordan Wootton & Steven Sadl-Kolchetski
 * Date: 4/10/2021
 * Class: Component Based Programming with .Net
 * Professor: Tony Haworth
 * Project: Project 2 (Group) - Personal Game (GoFish)
 * File: Hand.cs
 */

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GoFish
{
	[DataContract]
	public class Hand 
	{
		//--------------------------------------- Private Attributes
		[DataMember]
		public int TotalBooks { get; set; }

		[DataMember]
		public List<Card> CurrentCardsList { get; set; }

		[DataMember]
		public Deck CurrentDeck { get; set; }

		
		//---------------------------------- Public constructor to make a new hand passing a context of the deck to store
		public Hand(Deck d)
		{
			this.CurrentDeck = d;
			this.TotalBooks = 0;
			this.CurrentCardsList = new List<Card>();

			int numberOfCardsToDrawFromServer = 5; //Get this from server side

			for (int i = 0; i < numberOfCardsToDrawFromServer; ++i)
			{
				Card drawnCard = CurrentDeck.DrawTopCard();
				if (drawnCard != null)
					CurrentCardsList.Add(drawnCard);
				else
					break; //Desk of cards is empty
			}
		}

		// Add card from deck
		public void AddCard(Card c)
		{
			CurrentCardsList.Add(c);
		}

		public void GoFish()
		{
			if (CurrentDeck.CardsInDeck.Count != 0)
				CurrentCardsList.Add(CurrentDeck.DrawTopCard());
		}

		public bool IsBook(List<Card> book)
		{
			bool trueCase = true;
			FaceValue numberToMatch = book[0].Number;

			foreach(Card c in book)
				if (c.Number != numberToMatch)
					trueCase = false;
			
			return trueCase;
		}

		public string PlayBook(List<Card> book)
		{
			if (!IsBook(book))
				return "";

			string returnString = "";
			foreach (Card c in book)
				returnString += $"{c.CardSuit}, {c.Number} \n";
			TotalBooks++; //User has played a hand/book
			return returnString;
		}

		public string ShowReturnedCard(Card stolenCard)
		{
			return $"Card Stolen: {stolenCard.CardSuit.ToString()}, {stolenCard.Number}";
		}

		public Card CheckForMatch(Card card)
		{
			int count = 0;
			foreach(Card c in CurrentCardsList)
			{
				if (c.Number == card.Number)
				{
					CurrentCardsList.RemoveAt(count); //Remove card from players hand and return it
					return c;
				}
				count++; //Increment count if card not found
			}
			return null; //return null as default / no card found
		}
	}
}
