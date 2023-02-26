using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GoFish
{
	[DataContract]
	public class Deck
	{
		//------------------------- Private Attributes
		[DataMember]
		public List<Card> CardsInDeck;

		public Deck()
		{
			this.CardsInDeck = GenerateDeck();
		}

		//Function to return the top card from the deck
		public Card DrawTopCard()
		{
			if (CardsInDeck.Count == 0)
				return null;
			else
			{
				Card tmpCard = CardsInDeck[CardsInDeck.Count - 1];
				CardsInDeck.RemoveAt(CardsInDeck.Count - 1);
				return tmpCard;
			}
		}

		//------------------------- Private Helper Functions
		private List<Card> GenerateDeck()
		{
			List<Card> deck = new List<Card>();
			for(int i = 0; i < Enum.GetNames(typeof(Suit)).Length; i++) //Fill deck with inital 52 cards -- This is now built to change automatically based off ENUM sizes
				for (int x = 0; x < Enum.GetNames(typeof(FaceValue)).Length; ++x)
					deck.Add(new Card((Suit)i, (FaceValue)x));

			return deck.OrderBy(a => Guid.NewGuid()).ToList(); //Shuffle list and return
		}

	}
}
