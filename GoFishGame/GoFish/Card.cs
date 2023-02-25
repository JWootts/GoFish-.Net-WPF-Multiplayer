/*
 * Author: Jordan Wootton & Steven Sadl-Kolchetski
 * Date: 4/10/2021
 * Class: Component Based Programming with .Net
 * Professor: Tony Haworth
 * Project: Project 2 (Group) - Personal Game (GoFish)
 * File: Card.cs
 */

using System.Runtime.Serialization;
namespace GoFish
{
	[DataContract]
	public class Card
	{
		//---------------------- Private Attributes
		private Suit cardSuit;
		private FaceValue number;

		//-------------------------- Public Card(suit, card_number) 
		public Card(Suit suit, FaceValue num)
		{
			this.cardSuit = suit;
			this.number = num;
		}

		//--------------------- Public accessors for returning Card Contents
		[DataMember]
		public Suit CardSuit { get => this.cardSuit; set => this.cardSuit = value; }
		[DataMember]
		public FaceValue Number { get => this.number; set => this.number = value; }

        public override string ToString()
        {
            return $"{this.number} of {this.cardSuit}";
        }
    }
}
