/*
 * Author: Jordan Wootton & Steven Sadl-Kolchetski
 * Date: 4/10/2021
 * Class: Component Based Programming with .Net
 * Professor: Tony Haworth
 * Project: Project 2 (Group) - Personal Game (GoFish)
 * File: Winner.xmal.cs
 */

using System;
using System.Windows;
using GoFish;

namespace MessageBoardClient
{

	public partial class Winner : Window
	{
		private Users _winner;

		public Winner(Users winner)
		{
			_winner = winner;
			InitializeComponent();
			l_winnerUserName.Content = _winner.UserName;
			l_score.Content = _winner.Score;
			Console.WriteLine($"WINNER: {_winner.UserName} SCORE: {_winner.Score}");
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			Environment.Exit(0);
		}
	}
}
