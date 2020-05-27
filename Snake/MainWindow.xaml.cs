using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Snake
{
    public enum Direction
    {
        Up = 0,
        Right = 1,
        Down = 2,
        Left = 3
    }
    public enum Difficulty
    {
        Boring = 1,
        Easy = 2,
        Medium = 3,
        Hard = 4,
        Insane = 5,
    }
    public partial class MainWindow : Window
    {

        
        // Bound to ScoreNumber in the xaml
        public int Score
        {
            get
            {
                return int.Parse(ScoreNumber.Text);
            }
            set
            {
                ScoreNumber.Text = value.ToString();
            }
        }
        public Direction direction = Direction.Right;
        public Difficulty difficulty = Difficulty.Boring;
        public (int y, int x) head = (y: 0, x: 1);
        public (int y, int x) tail = (y: 0, x: 0);
        public (int y, int x) food = (y: 0, x: 0);
        public List<(int y, int x)> snakeBody = new List<(int y, int x)>();
        public (int y, int x) size = (y: 10, x: 10);
        public System.Timers.Timer snakeFrequency = new System.Timers.Timer(1000);
        Image snakeHead = new Image { VerticalAlignment=VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch };

        public MainWindow()
        {
            InitializeComponent();
            snakeFrequency.Elapsed += Timer_Elapsed;
            snakeHead.Source = Application.Current.Resources["snakeHeadDrawingImage"] as DrawingImage;
            ResetGame();
        }

        private void ResetGame()
        {
            snakeFrequency.Stop();
            SnakeGrid.Children.Clear();
            SnakeGrid.ColumnDefinitions.Clear();
            SnakeGrid.RowDefinitions.Clear();
            SnakeGrid.Children.Add(snakeHead);
            for (int i = 0; i < size.x; i++)
            {
                SnakeGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }
            for (int i = 0; i < size.y; i++)
            {
                SnakeGrid.RowDefinitions.Add(new RowDefinition());
            }
            int x = 0;
            int y = 0;
            foreach (ColumnDefinition cd in SnakeGrid.ColumnDefinitions)
            {
                y = 0;
                foreach (RowDefinition rd in SnakeGrid.RowDefinitions)
                {
                    Grid grid = new Grid();
                    grid.Background = Brushes.Transparent;
                    grid.SetValue(Grid.ColumnProperty, x);
                    grid.SetValue(Grid.RowProperty, y);
                    SnakeGrid.Children.Add(grid);
                    y++;
                }
                x++;
            }
            snakeBody.Clear();
            Dispatcher.Invoke(() =>
            {
                SetColor(0, 0, Brushes.Green);
                SetColor(0, 1, Brushes.Green);
                snakeHead.SetValue(Grid.RowProperty, 0);
                snakeHead.SetValue(Grid.ColumnProperty, 1);
            });
            Score = snakeBody.Count-1;
            direction = Direction.Right;
            head = (0, 1);
            AddFood();
            snakeFrequency.Start();
        }
        
        private (int row,int column) GetRandomBlock()
        {
            var rand = new Random();
            int row = rand.Next(0, size.y);
            int column = rand.Next(0, size.x);
            while (snakeBody.Any(t => t.y == row && t.x == column))
            {
                row = rand.Next(0, size.y);
                column = rand.Next(0, size.x);
            }
            return (row, column);
        }
        private void AddFood()
        {
            food = GetRandomBlock();
            SetColor(food.y, food.x, Brushes.Blue);
            IncreaseScore(difficulty);
        }
        private void IncreaseScore(Difficulty difficulty)
        {
            int scalar = 1;
            switch (difficulty)
            {
                case Difficulty.Boring:
                    scalar = 1;
                    break;
                case Difficulty.Easy:
                    scalar = 2;
                    break;
                case Difficulty.Medium:
                    scalar = 3;
                    break;
                case Difficulty.Hard:
                    scalar = 4;
                    break;
                case Difficulty.Insane:
                    scalar = 5;
                    break;
            }
            Score += scalar;
        }
        private void MoveSnake()
        {
            Dispatcher.Invoke(() =>
            {
                if (food.y == tail.y && food.x == tail.x)
                {
                    AddFood();
                }
                else
                {
                    SetColor(tail.y, tail.x, Brushes.Transparent);
                }
                SetColor(head.y, head.x, Brushes.Green);
                snakeHead.SetValue(Grid.RowProperty, head.y);
                snakeHead.SetValue(Grid.ColumnProperty, head.x);
            });
        }

        private void SetColor(int row, int column, SolidColorBrush solidColorBrush)
        {
            int y = snakeBody.Count > 2 ? snakeBody.Last().y : 0;
            int x = snakeBody.Count > 2 ? snakeBody.Last().x : 0;
            if (solidColorBrush == Brushes.Green && (row > size.y - 1 || column > size.x - 1 || row < 0 || column < 0 || (snakeBody.Count>2 && !(snakeBody.Last().y ==  row && snakeBody.Last().x == column) && snakeBody.Any(t => t.y == row && t.x == column))))
            {
                snakeFrequency.Stop();
                GameOverWindow gameOverWindow = new GameOverWindow(this, Score);
                var result = gameOverWindow.ShowDialog();
                ResetGame();
                return;
            }
            Grid grid = SnakeGrid.Children.OfType<Grid>().Single(g => (int?)g.GetValue(Grid.RowProperty) == row && (int?)g.GetValue(Grid.ColumnProperty) == column);
            grid.Background = solidColorBrush;
            if (solidColorBrush == Brushes.Green)
            {
                snakeBody.Add((row, column));
            }
            else if(solidColorBrush == Brushes.Transparent)
            {
                snakeBody.Remove((row, column));
            }
        }
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            snakeFrequency.Stop();
            switch (direction)
            {
                case Direction.Up:
                    head = (snakeBody.Last().y - 1, snakeBody.Last().x);
                    tail = (snakeBody[0].y, snakeBody[0].x);
                    break;
                case Direction.Right:
                    head = (snakeBody.Last().y, snakeBody.Last().x + 1);
                    tail = (snakeBody[0].y, snakeBody[0].x);
                    break;
                case Direction.Down:
                    head = (snakeBody.Last().y + 1, snakeBody.Last().x);
                    tail = (snakeBody[0].y, snakeBody[0].x);
                    break;
                case Direction.Left:
                    head = (snakeBody.Last().y, snakeBody.Last().x - 1);
                    tail = (snakeBody[0].y, snakeBody[0].x);
                    break;
                default:
                    break;
            }
            MoveSnake();
            snakeFrequency.Start();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            Key key = e.Key;
            switch (key)
            {
                case Key.Up:
                    if (direction != Direction.Down)
                    {
                        direction = Direction.Up;
                        Timer_Elapsed(null, null);
                    }
                    break;
                case Key.Right:
                    if (direction != Direction.Left)
                    {
                        direction = Direction.Right;
                        Timer_Elapsed(null, null);
                    }
                    break;
                case Key.Down:
                    if (direction != Direction.Up)
                    {
                        direction = Direction.Down;
                        Timer_Elapsed(null, null);
                    }
                    break;
                case Key.Left:
                    if (direction != Direction.Right)
                    {
                        direction = Direction.Left;
                        Timer_Elapsed(null, null);
                    }
                    break;
                default:
                    break;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            snakeFrequency.Stop();
        }
    }
}
