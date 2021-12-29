/*
 * Author: Jordan Wootton & Steven Sadl-Kolchetski
 * Date: 4/10/2021
 * Class: Component Based Programming with .Net
 * Professor: Tony Haworth
 * Project: Project 2 (Group) - Personal Game (GoFish)
 * File: Scored.cs
 */


using System.Runtime.Serialization; // WCF data contract types

namespace GoFish
{

	/*Custom class to be built if a user scored a point, with their new score 
		- This is used to show other users which user scored etc... */
	[DataContract]
	public class Scored
	{
		[DataMember]
		public Users UserScored { get; private set; }
		[DataMember]
		public int NewScore { get; set; }

		public Scored(Users scoredUser, int score)
		{
			this.UserScored = scoredUser;
			this.NewScore = score;
		}
	}
}
