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
    public partial class MainWindow : Window
    {

        enum Direction
        {
            Up = 0,
            Right = 1,
            Down = 2,
            Left = 3
        }


        Direction direction = Direction.Right;
        Tuple<int, int> head = new Tuple<int, int>(0, 1);
        Tuple<int, int> tail = new Tuple<int, int>(0, 0);
        List<Tuple<int, int>> snakeBody = new List<Tuple<int, int>>();
        int size = 10;
        System.Timers.Timer snakeFrequency = new System.Timers.Timer(500);
        Tuple<int, int> food = new Tuple<int, int>(0, 0);




        public MainWindow()
        {
            InitializeComponent();
            snakeFrequency.Elapsed += Timer_Elapsed;
            ResetGame();
        }

        private void ResetGame()
        {
            snakeFrequency.Stop();

            SnakeGrid.Children.Clear();
            SnakeGrid.ColumnDefinitions.Clear();
            SnakeGrid.RowDefinitions.Clear();

            for (int i = 0; i < size; i++)
            {
                SnakeGrid.ColumnDefinitions.Add(new ColumnDefinition());
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
            });
            direction = Direction.Right;
            head = new Tuple<int, int>(0, 1);
            AddFood();
            snakeFrequency.Start();
        }
        
        private void AddFood()
        {
            var rand = new Random();
            int row = rand.Next(0, size);
            int column = rand.Next(0, size);
            while (snakeBody.Any(t => t.Item1 == row && t.Item2 == column))
            {
                row = rand.Next(0, size);
                column = rand.Next(0, size);
            }
            SetColor(row, column, Brushes.Blue);
            food = new Tuple<int, int>(row, column);
        }

        private void Move()
        {
            Dispatcher.Invoke(() =>
            {
                if (food.Item1 == tail.Item1 && food.Item2 == tail.Item2)
                {
                    AddFood();
                }
                else
                {
                    SetColor(tail.Item1, tail.Item2, Brushes.Transparent);
                }
                SetColor(head.Item1, head.Item2, Brushes.Green);
            });
        }

        private void SetColor(int row, int column, SolidColorBrush solidColorBrush)
        {
            int y = snakeBody.Count > 2 ? snakeBody.Last().Item1 : 0;
            int x = snakeBody.Count > 2 ? snakeBody.Last().Item2 : 0;
            if (solidColorBrush == Brushes.Green && (row > size - 1 || column > size - 1 || row < 0 || column < 0 || (snakeBody.Count>2 && !(snakeBody.Last().Item1 ==  row && snakeBody.Last().Item2 == column) && snakeBody.Any(t => t.Item1 == row && t.Item2 == column))))
            {
                snakeFrequency.Stop();
                GameOverWindow gameOverWindow = new GameOverWindow(this);
                var result = gameOverWindow.ShowDialog();
                ResetGame();
                return;
            }

            Grid grid = SnakeGrid.Children.OfType<Grid>().Single(g => (int?)g.GetValue(Grid.RowProperty) == row && (int?)g.GetValue(Grid.ColumnProperty) == column);
            grid.Background = solidColorBrush;
            if (solidColorBrush == Brushes.Green)
            {
                snakeBody.Add(new Tuple<int, int>(row, column));
            }
            else if(solidColorBrush == Brushes.Transparent)
            {
                snakeBody.Remove(new Tuple<int, int>(row, column));
            }
        }
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            snakeFrequency.Stop();
            switch (direction)
            {
                case Direction.Up:
                    head = new Tuple<int, int>(snakeBody.Last().Item1 - 1, snakeBody.Last().Item2);
                    tail = new Tuple<int, int>(snakeBody[0].Item1, snakeBody[0].Item2);
                    break;
                case Direction.Right:
                    head = new Tuple<int, int>(snakeBody.Last().Item1, snakeBody.Last().Item2 + 1);
                    tail = new Tuple<int, int>(snakeBody[0].Item1, snakeBody[0].Item2);
                    break;
                case Direction.Down:
                    head = new Tuple<int, int>(snakeBody.Last().Item1 + 1, snakeBody.Last().Item2);
                    tail = new Tuple<int, int>(snakeBody[0].Item1, snakeBody[0].Item2);
                    break;
                case Direction.Left:
                    head = new Tuple<int, int>(snakeBody.Last().Item1, snakeBody.Last().Item2 - 1);
                    tail = new Tuple<int, int>(snakeBody[0].Item1, snakeBody[0].Item2);
                    break;
                default:
                    break;
            }
            Move();
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
