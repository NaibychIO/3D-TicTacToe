using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Xml.Linq;

namespace WinFormsApp1
{
    public partial class TicTacToe : Form
    {
        private const int BoardSize = 3;
        private char[,,] board = new char[BoardSize, BoardSize, BoardSize];
        private char currentPlayer = 'X';
        private bool gameWithComputer = false;
        private bool gameEnded = false;
        private Random random = new Random();
        private Button[,,] buttons = new Button[BoardSize, BoardSize, BoardSize];
        private Label statusLabel;
        private Button newGameBtn;
        private Label modeLabel;
        private Button vsComputerBtn;
        private Button vsPlayerBtn;

        public TicTacToe()
        {
            InitializeComponent();
            InitializeBoard();
            SetupUI();
        }

        private System.ComponentModel.IContainer components = null;

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
            SuspendLayout();
            ClientSize = new Size(619, 432);
            Name = "TicTacToe";
            ResumeLayout(false);
        }

        private void InitializeBoard()
        {
            for (int x = 0; x < BoardSize; x++)
                for (int y = 0; y < BoardSize; y++)
                    for (int z = 0; z < BoardSize; z++)
                        board[x, y, z] = ' ';
        }

        private void SetupUI()
        {
            this.Text = "3D Хрестики-Нулики";
            this.ClientSize = new Size(600, 430);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            modeLabel = new Label
            {
                Text = "Оберіть режим гри:",
                Location = new Point(250, 20),
                AutoSize = true
            };
            this.Controls.Add(modeLabel);

            vsComputerBtn = new Button
            {
                Text = "Грати проти комп'ютера",
                Location = new Point(50, 50),
                Size = new Size(200, 30)
            };
            vsComputerBtn.Click += (s, e) => StartNewGame(true);
            this.Controls.Add(vsComputerBtn);

            vsPlayerBtn = new Button
            {
                Text = "Два гравці",
                Location = new Point(350, 50),
                Size = new Size(200, 30)
            };
            vsPlayerBtn.Click += (s, e) => StartNewGame(false);
            this.Controls.Add(vsPlayerBtn);

            int startX = 50;
            int startY = 120;
            int buttonSize = 40;
            int spacing = 50;

            for (int x = 0; x < BoardSize; x++)
            {
                var layerLabel = new Label
                {
                    Text = $"Шар {x + 1}",
                    Location = new Point(startX + 50 + x * (BoardSize * spacing + 30), startY - 20),
                    AutoSize = true
                };
                this.Controls.Add(layerLabel);

                for (int y = 0; y < BoardSize; y++)
                {
                    for (int z = 0; z < BoardSize; z++)
                    {
                        buttons[x, y, z] = new Button
                        {
                            Size = new Size(buttonSize, buttonSize),
                            Location = new Point(
                                startX + x * (BoardSize * spacing + 30) + z * spacing,
                                startY + y * spacing),
                            Tag = new Point3D(x, y, z),
                            Font = new Font("Arial", 12)
                        };
                        buttons[x, y, z].Click += Button_Click;
                        this.Controls.Add(buttons[x, y, z]);
                    }
                }
            }

            statusLabel = new Label
            {
                Location = new Point(230, 300),
                Size = new Size(450, 50),
                Font = new Font("Arial", 12),
                Text = "Оберіть режим гри",
                Visible = false
            };
            this.Controls.Add(statusLabel);

            newGameBtn = new Button
            {
                Text = "Нова гра",
                Location = new Point(225, 350),
                Size = new Size(150, 40),
                Visible = false
            };
            newGameBtn.Click += (s, e) =>
            {
                modeLabel.Visible = true;
                vsComputerBtn.Visible = true;
                vsPlayerBtn.Visible = true;

                newGameBtn.Visible = false;

                statusLabel.Text = "Оберіть режим гри";
            };
            this.Controls.Add(newGameBtn);
        }

        private void StartNewGame(bool withComputer)
        {
            InitializeBoard();
            currentPlayer = 'X';
            gameEnded = false;
            gameWithComputer = withComputer;

            modeLabel.Visible = false;
            vsComputerBtn.Visible = false;
            vsPlayerBtn.Visible = false;

            statusLabel.Visible = true;
            newGameBtn.Visible = true;
            statusLabel.Text = $"Гравець {currentPlayer} ходить";

            foreach (var button in buttons)
            {
                button.Text = "";
                button.Enabled = true;
            }

            if (gameWithComputer && currentPlayer == 'O')
                ComputerMove();
        }

        private void Button_Click(object sender, EventArgs e)
        {
            if (!statusLabel.Visible || gameEnded) return;

            var button = (Button)sender;
            var position = (Point3D)button.Tag;

            if (board[position.X, position.Y, position.Z] != ' ') return;

            button.Text = currentPlayer.ToString();
            board[position.X, position.Y, position.Z] = currentPlayer;
            button.Enabled = false;

            if (CheckWin(currentPlayer))
            {
                EndGame($"Гравець {currentPlayer} переміг!");
                return;
            }

            if (IsBoardFull())
            {
                EndGame("Нічия!");
                return;
            }

            currentPlayer = currentPlayer == 'X' ? 'O' : 'X';
            statusLabel.Text = $"Гравець {currentPlayer} ходить";

            if (gameWithComputer && currentPlayer == 'O' && !gameEnded)
                ComputerMove();
        }

        private void EndGame(string message)
        {
            statusLabel.Text = message;
            gameEnded = true;

            modeLabel.Visible = true;
            vsComputerBtn.Visible = true;
            vsPlayerBtn.Visible = true;
        }

        private void ComputerMove()
        {
            Point3D? bestMove = FindWinningMove('O');
            if (bestMove == null) bestMove = FindWinningMove('X');
            if (bestMove == null) bestMove = FindRandomMove();

            if (bestMove.HasValue)
            {
                var pos = bestMove.Value;
                buttons[pos.X, pos.Y, pos.Z].PerformClick();
            }
        }

        private Point3D? FindWinningMove(char player)
        {
            for (int x = 0; x < BoardSize; x++)
                for (int y = 0; y < BoardSize; y++)
                    for (int z = 0; z < BoardSize; z++)
                        if (board[x, y, z] == ' ')
                        {
                            board[x, y, z] = player;
                            bool wins = CheckWin(player);
                            board[x, y, z] = ' ';
                            if (wins) return new Point3D(x, y, z);
                        }
            return null;
        }

        private Point3D? FindRandomMove()
        {
            var empty = new List<Point3D>();
            for (int x = 0; x < BoardSize; x++)
                for (int y = 0; y < BoardSize; y++)
                    for (int z = 0; z < BoardSize; z++)
                        if (board[x, y, z] == ' ')
                            empty.Add(new Point3D(x, y, z));

            return empty.Count > 0 ? empty[random.Next(empty.Count)] : null;
        }

        private bool CheckWin(char player)
        {
            for (int x = 0; x < BoardSize; x++)
                for (int y = 0; y < BoardSize; y++)
                    if (board[x, y, 0] == player && board[x, y, 1] == player && board[x, y, 2] == player)
                        return true;

            for (int x = 0; x < BoardSize; x++)
                for (int z = 0; z < BoardSize; z++)
                    if (board[x, 0, z] == player && board[x, 1, z] == player && board[x, 2, z] == player)
                        return true;

            for (int y = 0; y < BoardSize; y++)
                for (int z = 0; z < BoardSize; z++)
                    if (board[0, y, z] == player && board[1, y, z] == player && board[2, y, z] == player)
                        return true;

            if (board[0, 0, 0] == player && board[1, 1, 1] == player && board[2, 2, 2] == player)
                return true;
            if (board[0, 0, 2] == player && board[1, 1, 1] == player && board[2, 2, 0] == player)
                return true;
            if (board[2, 0, 0] == player && board[1, 1, 1] == player && board[0, 2, 2] == player)
                return true;
            if (board[2, 0, 2] == player && board[1, 1, 1] == player && board[0, 2, 0] == player)
                return true;

            return false;
        }

        private bool IsBoardFull()
        {
            for (int x = 0; x < BoardSize; x++)
                for (int y = 0; y < BoardSize; y++)
                    for (int z = 0; z < BoardSize; z++)
                        if (board[x, y, z] == ' ')
                            return false;
            return true;
        }

        private struct Point3D
        {
            public int X, Y, Z;
            public Point3D(int x, int y, int z)
            {
                X = x;
                Y = y;
                Z = z;
            }
        }
    }
}
