/*
 * Author: Jordan Wootton & Steven Sadl-Kolchetski
 * Date: 4/10/2021
 * Class: Component Based Programming with .Net
 * Professor: Tony Haworth
 * Project: Project 2 (Group) - Personal Game (GoFish)
 * File: Users.cs
 */

using System.Runtime.Serialization;

namespace GoFish
{
	[DataContract]
	public class Users
	{
		private string _username = null;
		private Hand _hand = null;
		private int _score = 0;

		public Users() { } // ---------- Used for Seralization

		public Users(string user)
		{
			this._username = user;
		}

		[DataMember]
		public string UserName { get => _username; set => this._username = value; }

		[DataMember]
		public Hand Hand { get => _hand; set => this._hand = value; }

		[DataMember]
		public int Score { get => _score; set => this._score = value; }

		//------------ Assign Functions
		public void SetHand(Hand h)
		{
			this._hand = h;
		}

		public void AddCard(Card c)
		{
			this.Hand.AddCard(c);
		}

	}
}
