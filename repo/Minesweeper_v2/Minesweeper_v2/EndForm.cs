using System;
using System.Windows.Forms;

namespace Minesweeper
{
	public partial class EndForm : Form
	{
		/// <summary>
		/// Customizable display text.
		/// </summary>
		private readonly string text;
		/// <summary>
		/// Parent Game object.
		/// </summary>
		private readonly Game game;

		/// <summary>
		/// Inits the EndForm with set display text.
		/// </summary>
		/// <param name="game">Game object for logic callback.</param>
		/// <param name="display_text">Text to display on the form.</param>
		public EndForm(Game game, string display_text)
		{
			this.game = game;
			text = display_text;
			InitializeComponent();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			game.Restart();
		}

		private void button2_Click(object sender, EventArgs e)
		{
			game.Close();
		}
	}
}
