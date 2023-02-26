using System;
using System.Windows;
using System.ServiceModel;
using GoFish;
using System.Threading;

namespace MessageBoardClient
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = false)]
    public partial class MainWindow : Window, ICallback
    {
        private IGoFish msgBrd = null;
        private Users _currentUser = null;
        private string prefix = "";
        private Playing playing = null;
        public static EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset); //For Turntaking
        private delegate void ClientUpdateDelegate(int numOfCardsInDeck, string[] users, bool startGame, Users enemy, Scored userScored, string updateMsg, bool playerJoined, int nextPlayerIndex, bool gameOver);

        public MainWindow()
        {
            InitializeComponent();
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Environment.Exit(0);
        }

        public void Update(int count, int nextClient, bool gameOver)
        {
            throw new NotImplementedException();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            HowToPlay htp = new HowToPlay();
            htp.Show();

        }

        private void StartGame_Click(object sender, RoutedEventArgs e)
        {
            msgBrd.StartGame();
        }

        private void buttonEnter_Click(object sender, RoutedEventArgs e)
        {
            if (textAlias.Text != "")
            {
                try
                {
                    prefix = "[" + textAlias.Text + "] ";
                    buttonEnter.IsEnabled = textAlias.IsEnabled = false;

                    connectToMessageBoard();
                    b_StartGame.IsEnabled = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        //------------------------ Helper methods
        private void connectToMessageBoard()
        {
            try
            {
                // Configure the ABCs of using the MessageBoard service
                DuplexChannelFactory<IGoFish> channel = new DuplexChannelFactory<IGoFish>(this, "GoFishService");

                // Activate a MessageBoard object
                msgBrd = channel.CreateChannel();
                _currentUser = msgBrd.Join(textAlias.Text);

                if (_currentUser != null)
                {
                    // Alias accepted by the service so update GUI
                    listMessages.ItemsSource = msgBrd.GetAllUsers();
                    textAlias.IsEnabled = buttonEnter.IsEnabled = false;
                }
                else
                {
                    // Alias rejected by the service so nullify service proxies
                    msgBrd = null;
                    MessageBox.Show("ERROR: Alias in use. Please try again.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

		private void Refresh_Click(object sender, RoutedEventArgs e)
		{
            listMessages.ItemsSource = msgBrd.GetAllUsers();
        }

        public void Update(int numOfCardsInDeck, string[] users, bool startGame, Users enemy, Scored userScored, string updateMsg, bool playerJoined, int nextPlayerIndex, bool gameOver)
        {
            if (System.Threading.Thread.CurrentThread == this.Dispatcher.Thread)
            {
                if (gameOver != true)
                {
                    if (playerJoined == true) //refresh lobby screen
                    {
                        listMessages.ItemsSource = msgBrd.GetAllUsers();
                    }
                    else if (startGame == true)
                    {
                        playing = new Playing(msgBrd, _currentUser); //Pass in result if inital user
                        playing.Show();
                        if (msgBrd.GetUserIndex(_currentUser) != nextPlayerIndex) //Set all players except inital to await for their turn
                        {
                            playing.AwaitUserTurn();
                        }
                        this.Hide();
                    }
                    else
                    {
                        //Game started Route calls to playing window
                        if (enemy != null)
                        {
                            if (enemy.UserName == _currentUser.UserName) //Updating current user
                            {
                                _currentUser.Hand = enemy.Hand;
                                //Update playing values
                                playing.UpdateNextTurn(numOfCardsInDeck, userScored, updateMsg, true, nextPlayerIndex);
                            }
                            else
                            {
                                //Update playing values
                                playing.UpdateNextTurn(numOfCardsInDeck, userScored, updateMsg, false, nextPlayerIndex);
                            }
                        }
                        else
                        {
                            //Update normal values such as number of decks in card and if user scored point aswell as message from server
                            playing.UpdateNextTurn(numOfCardsInDeck, userScored, updateMsg, false, nextPlayerIndex);
                        }
                    }
				}
				else
				{
                    Users _gameWinner = msgBrd.GetWinnerOfGame();
                    Winner _winnerScreen = new Winner(_gameWinner);
                    _winnerScreen.Show();
                    playing.Hide();
				}
			}
			else
			{
                this.Dispatcher.BeginInvoke(new ClientUpdateDelegate(Update), numOfCardsInDeck, users, startGame, enemy, userScored, updateMsg, playerJoined, nextPlayerIndex, gameOver);
            }
        }
	}
}
