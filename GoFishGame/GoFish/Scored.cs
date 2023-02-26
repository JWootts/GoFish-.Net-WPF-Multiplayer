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
