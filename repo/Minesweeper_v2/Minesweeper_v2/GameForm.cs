using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Reflection;

namespace Minesweeper
{
	/// <summary>
	/// Handles individual game graphics.
	/// </summary>
	class GameForm : Form
	{
		/// <summary>
		/// Form height in pixels.
		/// </summary>
		private readonly uint height;
		/// <summary>
		/// Form width in pixels.
		/// </summary>
		private readonly uint width;
		/// <summary>
		/// Tile size as defined in Game class.
		/// </summary>
		private readonly uint tileSize;
		/// <summary>
		/// Border size as defined in Game class.
		/// </summary>
		private readonly uint borderSize;
		/// <summary>
		/// Defines the status bar height.
		/// </summary>
		private const uint STATUS_BAR_HEIGHT = 22;

		/// <summary>
		/// Form graphics object.
		/// </summary>
		private readonly Graphics g;
		/// <summary>
		/// Shows flags remaining.
		/// </summary>
		private StatusBar b;


		/// <summary>
		/// Parent Game object.
		/// </summary>
		private readonly Game game;
		/// <summary>
		/// Field object of the parent Game object used for rendering.
		/// </summary>
		private readonly Field gameField;

		/// <summary>
		/// Stores images of uncovered tiles.
		/// </summary>
		private static Image[] tile;
		/// <summary>
		/// Stores images of bomb tiles.
		/// </summary>
		private static Image[] tile_bomb;
		/// <summary>
		/// Stores images of tile covers.
		/// </summary>
		private static Image[] tile_cover;


		/// <summary>
		/// Initializes the game form and binds it to game object.
		/// </summary>
		/// <param name="game">Game object for logic callback.</param>
		public GameForm(Game game)
		{
			this.gameField = game.gameField;
			this.game = game;
			this.tileSize = Minesweeper.Game.TILE_SIZE;
			this.borderSize = Minesweeper.Game.BORDER_SIZE;

			//space for tiles + borders
			width = gameField.x_tiles * tileSize + 2 * borderSize;
			height = gameField.y_tiles * tileSize + 2 * borderSize + STATUS_BAR_HEIGHT;

			if (tile == default(Image[]))
				InitTiles();

			InitializeComponent();
			g = this.CreateGraphics();
		}

		/// <summary>
		/// Redraws the entire form on a Control.Paint event.
		/// </summary>
		private void GameForm_Paint(object sender, PaintEventArgs e)
		{
			// draw the border
			Pen p = new Pen(Color.FromArgb(255, 255, 255));
			g.DrawRectangle(p, borderSize - 1, borderSize - 1, gameField.x_tiles * tileSize + 2, gameField.y_tiles * tileSize + 2);

			// update flag text
			b.Text = string.Concat("Flags: ", gameField.flagCount, "/", gameField.bombCount);

			// Draws an image for every tile at its position.
			for (uint x_tile = 0; x_tile < gameField.x_tiles; x_tile++)
				for (uint y_tile = 0; y_tile < gameField.y_tiles; y_tile++)
					g.DrawImage(GetImage(x_tile, y_tile), 5 + x_tile * tileSize, 5 + y_tile * tileSize, tileSize, tileSize);
		}

		#region MouseHandlers

		/// <summary>
		/// Passes mouseDown events in the game area to game object.
		/// </summary>
		private void GameForm_MouseDown(object sender, MouseEventArgs e)
		{
			if (InGameArea(e.X, e.Y) && !game.EndState)
				game.mouseDown(e.Button, e.X, e.Y);
		}

		/// <summary>
		/// Passes mouseUp events in the game area to game object.
		/// </summary>
		private void GameForm_MouseUp(object sender, MouseEventArgs e)
		{
			if (InGameArea(e.X, e.Y) && !game.EndState)
			{
				game.mouseUp(e.Button, e.X, e.Y);
				this.Invalidate();
			}
		}

		/// <summary>
		/// Checks, if click coordinates are located in the game area.
		/// </summary>
		/// <param name="x">Click x coors.</param>
		/// <param name="y">Click y coors/</param>
		/// <returns>True if click is inside game area.</returns>
		private bool InGameArea(int x, int y)
		{
			return (x <= width - borderSize && x > borderSize && y <= height - borderSize - STATUS_BAR_HEIGHT && y > borderSize);
		}

		#endregion

		#region TileImg

		/// <summary>
		/// Gets tile image based on tile coords.
		/// </summary>
		/// <param name="x_tile">X tile coord.</param>
		/// <param name="y_tile">Y tile coord.</param>
		/// <returns>Image of tile at x, y.</returns>
		private Image GetImage(uint x_tile, uint y_tile)
		{
			Image t_image = tile_cover[0]; // cover fallback
			if (!game.EndState)
				switch (gameField.TileState[x_tile, y_tile])
				{
					case 0: // cover tile
						t_image = tile_cover[0];
						break;
					case 1: // flagged tile
						t_image = tile_cover[1];
						break;
					case 2: // uncovered tile
						t_image = tile[gameField.TileNum[x_tile, y_tile]];
						break;
				}
			else
			{
				switch (gameField.TileState[x_tile, y_tile])
				{
					case 0: // cover tile
						if (gameField.TileBomb[x_tile, y_tile])
							t_image = tile_bomb[1]; // uncovered not found bomb
						else
							t_image = tile_cover[0]; // left covered
						break;
					case 1: // flagged tile
						if (gameField.TileBomb[x_tile, y_tile])
							t_image = tile_cover[1]; // proper flagged bomb
						else
							t_image = tile_bomb[2]; // flag over empty tile
						break;
					case 2: // uncovered tile
						t_image = tile[gameField.TileNum[x_tile, y_tile]];
						break;
				}
				if (gameField.ClickedBomb != (default(Tuple<uint, uint>))) // check whether a bomb was triggered
					if (x_tile == gameField.ClickedBomb.Item1 && y_tile == gameField.ClickedBomb.Item2) // override for clicked bomb
						t_image = tile_bomb[0];
			}
			return t_image;
		}

		/// <summary>
		/// Stores images associated with tiles in arrays.
		/// </summary>
		private void InitTiles()
		{
			tile = new Image[9];
			tile[0] = Bitmap.FromStream(GetEmbeddedResourceStream("Minesweeper.assets.tile_0.png"));
			tile[1] = Bitmap.FromStream(GetEmbeddedResourceStream("Minesweeper.assets.tile_1.png"));
			tile[2] = Bitmap.FromStream(GetEmbeddedResourceStream("Minesweeper.assets.tile_2.png"));
			tile[3] = Bitmap.FromStream(GetEmbeddedResourceStream("Minesweeper.assets.tile_3.png"));
			tile[4] = Bitmap.FromStream(GetEmbeddedResourceStream("Minesweeper.assets.tile_4.png"));
			tile[5] = Bitmap.FromStream(GetEmbeddedResourceStream("Minesweeper.assets.tile_5.png"));
			tile[6] = Bitmap.FromStream(GetEmbeddedResourceStream("Minesweeper.assets.tile_6.png"));
			tile[7] = Bitmap.FromStream(GetEmbeddedResourceStream("Minesweeper.assets.tile_7.png"));
			tile[8] = Bitmap.FromStream(GetEmbeddedResourceStream("Minesweeper.assets.tile_8.png"));

			tile_bomb = new Image[3];
			tile_bomb[0] = Bitmap.FromStream(GetEmbeddedResourceStream("Minesweeper.assets.tile_bomb_triggered.png"));
			tile_bomb[1] = Bitmap.FromStream(GetEmbeddedResourceStream("Minesweeper.assets.tile_bomb_undiscovered.png"));
			tile_bomb[2] = Bitmap.FromStream(GetEmbeddedResourceStream("Minesweeper.assets.tile_bomb_misflagged.png"));

			tile_cover = new Image[2];
			tile_cover[0] = Bitmap.FromStream(GetEmbeddedResourceStream("Minesweeper.assets.tile_cover.png"));
			tile_cover[1] = Bitmap.FromStream(GetEmbeddedResourceStream("Minesweeper.assets.tile_flag.png"));
		}

		/// <summary>
		/// Helper function to address embedded assets.
		/// </summary>
		/// <param name="resourceName">Asset name.</param>
		/// <returns>Stream containing asset file.</returns>
		static Stream GetEmbeddedResourceStream(string resourceName)
		{
			return Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
		}

		#endregion

		#region FormInit

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GameForm));
			this.SuspendLayout();
			// 
			// GameForm
			// 
			// this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.ClientSize = new System.Drawing.Size((int)width, (int)height);
			this.BackColor = Color.FromArgb(189, 189, 189);
			// this.BackColor = Color.FromArgb(255, 255, 255);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "GameForm";
			this.Text = "Minesweeper";
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.GameForm_Paint);
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.GameForm_MouseDown);
			this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.GameForm_MouseUp);

			b = new StatusBar
			{
				Height = (int)STATUS_BAR_HEIGHT,
				ShowPanels = false,
				Text = string.Concat("Flags:\t", gameField.flagCount, "/", gameField.bombCount)
			};

			this.Controls.Add(b);

			this.ResumeLayout();
			this.Refresh();
		}

		#endregion

	}
}
