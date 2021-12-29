/*
 * Author: Jordan Wootton & Steven Sadl-Kolchetski
 * Date: 4/10/2021
 * Class: Component Based Programming with .Net
 * Professor: Tony Haworth
 * Project: Project 2 (Group) - Personal Game (GoFish)
 * File: CardValues.cs
 */

//-- Contains ENUM types for creating card types within current 'bag' context
using System.Runtime.Serialization;
namespace GoFish
{
	[DataContract]
	public enum Suit
	{
		[EnumMember]
		DIAMONDS,
		[EnumMember]
		CLUBS,
		[EnumMember]
		HEARTS,
		[EnumMember]
		SPADES
	}

	[DataContract]
	public enum FaceValue
	{
		[EnumMember]
		ACE,
		[EnumMember]
		TWO,
		[EnumMember]
		THREE,
		[EnumMember]
		FOUR,
		[EnumMember]
		FIVE,
		[EnumMember]
		SIX,
		[EnumMember]
		SEVEN,
		[EnumMember]
		EIGHT,
		[EnumMember]
		NINE,
		[EnumMember]
		TEN,
		[EnumMember]
		JACK,
		[EnumMember]
		QUEEN,
		[EnumMember]
		KING
	}
}
