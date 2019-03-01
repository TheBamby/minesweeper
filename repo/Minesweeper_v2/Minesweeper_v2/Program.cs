/// Minesweeper
/// Jan Bambousek, I. ročník, I/1-I1X.33`P
/// ZS 2018/19
/// Programování NPRG030

using System;
using System.Windows.Forms;

namespace Minesweeper
{
	class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			NewGame();
		}
		/// <summary>
		/// Utility method for setting the bomb placement probability.
		/// </summary>
		/// <param name="diff">String denoting difficulty.</param>
		/// <returns>Probability for a field to be a bomb.</returns>
		internal static double GetDiff(string diff)
		{
			switch (diff)
			{
				case "Beginner":
					return (double)1 / 12;
				case "Intermediate":
					return (double)1 / 10;
				case "Advanced":
					return (double)1 / 8;
				case "Hardcore":
					return (double)1 / 5;
				default:
					return (double)1 / 12;
			}
		}

		/// <summary>
		/// Initializes the new game prompt.
		/// </summary>
		static void NewGame()
		{
			Application.Run(new StartForm());
		}
		
		/// <summary>
		/// Starts a new game with desired difficulty.
		/// </summary>
		/// <param name="diff_s">Sets the bomb occurence rate.</param>
		public static void StartGame(string diff_s)
		{
			StartGame(20, 20, diff_s);
		}
		/// <summary>
		/// Starts a new game with desired difficulty and dimensions.
		/// </summary>
		/// <param name="x"># of horizontal tiles.</param>
		/// <param name="y"># of vertical tiles.</param>
		/// <param name="diff_s">Sets the bomb occurence rate.</param>
		public static void StartGame(uint x, uint y, string diff_s)
		{
			new Game(x, y, GetDiff(diff_s));
		}
	}

	/// <summary>
	/// Represents a game with logic.
	/// </summary>
	public class Game
	{
		/// <summary>
		/// Defines the pixel size of a tile.
		/// </summary>
		public const uint TILE_SIZE = 16;
		/// <summary>
		/// Defines the border offset around the game.
		/// </summary>
		public const uint BORDER_SIZE = 5;

		/// <summary>
		/// Stores the game Field (tileset) object.
		/// </summary>
		public readonly Field gameField;
		/// <summary>
		/// Stores the game GameForm (display) object.
		/// </summary>
		private readonly GameForm gameForm;
		/// <summary>
		/// Stores the game EndForm object.
		/// </summary>
		private EndForm endForm;

		/// <summary>
		/// True on end condition.
		/// </summary>
		public bool EndState { get; private set; } = false;
		/// <summary>
		/// Stores the last mouseDown event tile position.
		/// </summary>
		public Tuple<uint, uint> MouseDownTile { get; private set; }


		/// <summary>
		/// Creates a new game with desired dimensions and specified bomb probability.
		/// </summary>
		/// <param name="x_tiles"># of horizontal tiles.</param>
		/// <param name="y_tiles"># of vertical tiles.</param>
		/// <param name="bomb_prob">Probability of a tile to be a bomb.</param>
		public Game(uint x_tiles, uint y_tiles, double bomb_prob)
		{
			gameField = new Field(this, x_tiles, y_tiles, bomb_prob);
			gameField.GenBombs();

			gameForm = new GameForm(this);
			gameForm.Show();
		}

		#region MouseHandlers

		/// <summary>
		/// Passes a left|right mouseclick to the field class and rerenders the game.
		/// </summary>
		/// <param name="button">Pressed mouse button.</param>
		/// <param name="x_tile">Clicked tile x coors.</param>
		/// <param name="y_tile">Clicked tile y coors.</param>
		public void MouseClick(MouseButtons button, uint x_tile, uint y_tile)
		{
			switch (button)
			{
				case MouseButtons.Left:
					gameField.LeftClickTile(x_tile, y_tile);
					break;
				case MouseButtons.Right:
					gameField.RightClickTile(x_tile, y_tile);
					break;
			}
			gameForm.Invalidate();
		}

		/// <summary>
		/// Gets the tile coors from mouse coors.
		/// </summary>
		/// <param name="x">Mouse x coors.</param>
		/// <param name="y">Mouse y coors.</param>
		/// <returns>Tuple with x and y tile coors.</returns>
		public Tuple<uint, uint> GetTile(uint x, uint y)
		{
			uint x_tile, y_tile;
			x_tile = (uint)(x - BORDER_SIZE) / TILE_SIZE;
			y_tile = (uint)(y - BORDER_SIZE) / TILE_SIZE;
			return Tuple.Create<uint, uint>(x_tile, y_tile);
		}

		/// <summary>
		/// Registers mouseDown event.
		/// </summary>
		/// <param name="button">Mouse button clicked.</param>
		/// <param name="x">Click X pos.</param>
		/// <param name="y">Click Y pos.</param>
		internal void mouseDown(MouseButtons button, int x, int y)
		{
			MouseDownTile = GetTile((uint)x, (uint)y);
		}

		/// <summary>
		/// Registers mouseUp event.
		/// If coords match to mouseDown, triggers a MouseClick.
		/// </summary>
		/// <param name="button">Mouse button clicked.</param>
		/// <param name="x">Click X pos.</param>
		/// <param name="y">Click Y pos.</param>
		internal void mouseUp(MouseButtons button, int x, int y)
		{
			Tuple<uint, uint> tile = GetTile((uint)x, (uint)y);
			if (tile.Equals(MouseDownTile))
				MouseClick(button, tile.Item1, tile.Item2);
		}

		#endregion

		#region EndState

		/// <summary>
		/// Activated on lose condition, triggers the killscreen.
		/// </summary>
		internal void Lose()
		{
			EndState = true;
			endForm = new EndForm(this, "Sorry you lost.. Want to play again?");
			endForm.Show();
			endForm.Focus();
		}

		/// <summary>
		/// Activated on win condition, triggers the win-screen.
		/// </summary>
		internal void Win()
		{
			EndState = true;
			endForm = new EndForm(this, "Congrats! Care for another round?");
			endForm.Show();
			endForm.Focus();
		}

		/// <summary>
		/// Restarts the game with same settings.
		/// </summary>
		internal void Restart()
		{
			gameField.CleanField();
			gameField.GenBombs();
			endForm.Close();
			EndState = false;
			gameForm.Invalidate();
		}

		/// <summary>
		/// Closes the current game.
		/// </summary>
		internal void Close()
		{
			endForm.Close();
			endForm.Dispose();
			gameForm.Close();
			gameForm.Dispose();
		}

		#endregion

	}

	/// <summary>
	/// Contains the state of the individual tiles as well as supporting methods.
	/// </summary>
	/// Uses reverse-style cartesian for tile addressing.
	public class Field
	{
		/// <summary>
		/// defines the x size of the field (in tiles)
		/// </summary>
		public readonly uint x_tiles;
		/// <summary>
		/// defines the y size of the field
		/// </summary>
		public readonly uint y_tiles;
		/// <summary>
		/// Specifies the probablility of each tile to be a bomb.
		/// </summary>
		public readonly double bomb_prob;

		/// <summary>
		/// Stores 0 on hidden, 1 on flagged, 2 on uncored tile.
		/// </summary>
		public byte[,] TileState { get; private set; }
		/// <summary>
		/// stores numbers hidden beneath the tiles
		/// </summary>
		public byte[,] TileNum { get; private set; }
		/// <summary>
		/// stores true on tiles in bombs
		/// </summary>
		public bool[,] TileBomb { get; private set; }

		/// <summary>
		/// Indicates the total bomb count.
		/// </summary>
		public uint bombCount = 0;
		/// <summary>
		/// Indicates the total placed flag count.
		/// </summary>
		public uint flagCount = 0;
		/// <summary>
		/// Stores number of bombs successfully flagged.
		/// </summary>
		public uint flaggedBombCount = 0;
		/// <summary>
		/// If a bomb is tripped, stores its coordinates.
		/// </summary>
		public Tuple<uint, uint> ClickedBomb { get; private set; }
		/// <summary>
		/// Parent game callback object.
		/// </summary>
		private Game game;

		/// <summary>
		/// Sets up field state variables and cleans the field.
		/// </summary>
		/// <param name="x_tiles"># of horizontal tiles.</param>
		/// <param name="y_tiles"># of vertical tiles.</param>
		public Field(Game game, uint x_tiles, uint y_tiles, double bomb_prob)
		{
			this.x_tiles = x_tiles;
			this.y_tiles = y_tiles;
			this.game = game;
			this.bomb_prob = bomb_prob;

			TileState = new byte[x_tiles, y_tiles];
			TileNum = new byte[x_tiles, y_tiles];
			TileBomb = new bool[x_tiles, y_tiles];

			CleanField();
		}

		#region Setup

		/// <summary>
		/// Sets field state variables to starting values.
		/// </summary>
		internal void CleanField()
		{
			bombCount = 0;
			flagCount = 0;
			flaggedBombCount = 0;
			for (int x = 0; x < x_tiles; x++)
				for (int y = 0; y < y_tiles; y++)
				{
					TileState[x, y] = 0;
					TileNum[x, y] = 0;
					TileBomb[x, y] = false;
				}
		}

		/// <summary>
		/// Generates bomb field based on input probability.
		/// </summary>
		/// <param name="bomb_prob">Probability for any one field containing the bomb.</param>
		internal void GenBombs()
		{
			Random rand = new Random();
			for (uint x = 0; x < x_tiles; x++)
				for (uint y = 0; y < y_tiles; y++)
					if (rand.NextDouble() <= bomb_prob)
						AddBomb(x, y);
			// Fields with 0 bombs are regened.
			if (bombCount == 0)
			{
				CleanField();
				GenBombs();
			}
		}

		/// <summary>
		/// Adds a bomb to tile pos x, y and adjust the surrounding field numbering accordingly.
		/// </summary>
		/// <param name="x">Bomb x tile coors.</param>
		/// <param name="y">Bomb y tile coors.</param>
		private void AddBomb(uint x, uint y)
		{
			Console.Error.WriteLine(String.Concat("Adding a bomb @ ", x.ToString(), ",", y.ToString()));
			TileBomb[x, y] = true;
			bombCount++;
			// Looping the surrounding 8 tiles. Accounts for OutOfBounds errs.
			for (uint x_surround = (x == 0 ? x : x - 1); x_surround <= x + 1 && x_surround < x_tiles; x_surround++)
				for (uint y_surround = (y == 0 ? y : y - 1); y_surround <= y + 1 && y_surround < y_tiles; y_surround++)
					if (!(x_surround == x && y_surround == y))
						TileNum[x_surround, y_surround] += 1;
		}

		#endregion

		#region ClickHandlers

		/// <summary>
		/// Uncovers not flagged covered tiles/explodes the bomb on described coors.
		/// </summary>
		/// <param name="x_tile">Clicked tile x coors.</param>
		/// <param name="y_tile">Clicked tile y coors.</param>
		internal void LeftClickTile(uint x_tile, uint y_tile)
		{
			if (TileState[x_tile, y_tile] == 0)
				// lose condition
				if (TileBomb[x_tile, y_tile])
				{
					ClickedBomb = Tuple.Create<uint, uint>(x_tile, y_tile);
					game.Lose();
				}
				else
					Uncover(x_tile, y_tile);
		}

		/// <summary>
		/// Uncovers the tile and if it is a '0' tile, all surrounding tiles.
		/// </summary>
		/// <param name="x_tile">Tile x coors.</param>
		/// <param name="y_tile">Tile y coors.</param>
		private void Uncover(uint x_tile, uint y_tile)
		{
			if (TileNum[x_tile, y_tile] == 0 && TileState[x_tile, y_tile] == 0)
			{
				TileState[x_tile, y_tile] = 2;
				// uncovering the surrounding 8 tiles around the blanks. accounts for outofbounds errs.
				for (uint x_surround = (x_tile == 0 ? x_tile : x_tile - 1); x_surround <= x_tile + 1 && x_surround < x_tiles; x_surround++)
					for (uint y_surround = (y_tile == 0 ? y_tile : y_tile - 1); y_surround <= y_tile + 1 && y_surround < y_tiles; y_surround++)
						if (!(x_surround == x_tile && y_surround == y_tile))
							Uncover(x_surround, y_surround);
			}
			// skipping uncovered tiles
			else if (TileState[x_tile, y_tile] == 0)
				// sets current tile to uncovered
				TileState[x_tile, y_tile] = 2;

		}

		/// <summary>
		/// Toggles covered tile flag on described coors.
		/// </summary>
		/// <param name="x_tile">Clicked tile x coors.</param>
		/// <param name="y_tile">Clicked tile y coors.</param>
		internal void RightClickTile(uint x_tile, uint y_tile)
		{
			switch (TileState[x_tile, y_tile]) // ignores uncovered tiles
			{
				case 0: // !flagged tile
					TileState[x_tile, y_tile] = 1;
					flagCount++;
					if (TileBomb[x_tile, y_tile])
						flaggedBombCount++;
					break;
				case 1: // flagged tile
					TileState[x_tile, y_tile] = 0;
					flagCount--;
					if (TileBomb[x_tile, y_tile])
						flaggedBombCount--;
					break;
			}
			// win condition
			if (flaggedBombCount == bombCount)
				game.Win();
		}

		#endregion
	}
}

