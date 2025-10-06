using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives; // <- UniformGrid bor her
using Avalonia.Layout;

namespace ROCKPAPERANDSCISSOR
{
    enum Hand { Rock = 0, Paper = 1, Scissors = 2, Lizard = 3, Spock = 4 }
    enum Outcome { Tie, Player, Agent }

    internal static class Program
    {
        public static void Main(string[] args) =>
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

        public static AppBuilder BuildAvaloniaApp() =>
            AppBuilder.Configure<App>().UsePlatformDetect().LogToTrace();
    }

    public sealed class App : Application
    {
        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                desktop.MainWindow = new MainWindow();
            base.OnFrameworkInitializationCompleted();
        }
    }

    public sealed class MainWindow : Window
    {
        private int _playerScore, _agentScore;
        private const int WinningScore = 5;

        private readonly TextBlock _lblChoices = new() { Text = "You: —   Agent: —" };
        private readonly TextBlock _lblResult  = new() { Text = "Make a move" };
        private readonly TextBlock _lblScore   = new() { Text = "Score 0 : 0" };

        public MainWindow()
        {
            Title = "RPSLS – variant";
            Width = 520;
            Height = 220;

            var buttons = new UniformGrid { Columns = 5, Rows = 1, Margin = new Thickness(0, 0, 0, 6) };
            AddBtn(buttons, "Rock",     Hand.Rock);
            AddBtn(buttons, "Paper",    Hand.Paper);
            AddBtn(buttons, "Scissors", Hand.Scissors);
            AddBtn(buttons, "Lizard",   Hand.Lizard);
            AddBtn(buttons, "Spock",    Hand.Spock);

            var reset = new Button { Content = "New game" };
            reset.Click += (_, __) => Reset();

            var root = new StackPanel { Margin = new Thickness(12), Spacing = 8 };
            root.Children.Add(_lblScore);
            root.Children.Add(buttons);
            root.Children.Add(reset);
            root.Children.Add(_lblChoices);
            root.Children.Add(_lblResult);
            Content = root;
        }

        private void AddBtn(Panel parent, string caption, Hand hand)
        {
            var b = new Button { Content = caption, MinWidth = 90 };
            b.Click += (_, __) => PlayRound(hand);
            parent.Children.Add(b);
        }

        private void PlayRound(Hand player)
        {
            var agent = (Hand)Random.Shared.Next(0, 5);
            _lblChoices.Text = $"You: {player}   Agent: {agent}";

            var outcome = Resolve(player, agent);
            switch (outcome)
            {
                case Outcome.Player: _playerScore++; break;
                case Outcome.Agent:  _agentScore++;  break;
            }

            _lblScore.Text = $"Score {_playerScore} : {_agentScore}";
            _lblResult.Text = outcome switch
            {
                Outcome.Tie    => "Tie",
                Outcome.Player => $"{player} beats {agent} — You win the round",
                Outcome.Agent  => $"{agent} beats {player} — Agent wins the round",
                _              => "-"
            };

            if (_playerScore >= WinningScore || _agentScore >= WinningScore)
            {
                var text = _playerScore >= WinningScore ? "You win the match" : "Agent wins the match";
                new Window
                {
                    Title = "Result",
                    Width = 240,
                    Height = 110,
                    Content = new TextBlock
                    {
                        Text = text,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                }.Show(this);
            }
        }

        private static Outcome Resolve(Hand p, Hand a)
        {
            if (p == a) return Outcome.Tie;

            // eksplicit switch på vinder-cases for spilleren
            return (p, a) switch
            {
                (Hand.Rock,     Hand.Scissors) => Outcome.Player,
                (Hand.Rock,     Hand.Lizard)   => Outcome.Player,
                (Hand.Paper,    Hand.Rock)     => Outcome.Player,
                (Hand.Paper,    Hand.Spock)    => Outcome.Player,
                (Hand.Scissors, Hand.Paper)    => Outcome.Player,
                (Hand.Scissors, Hand.Lizard)   => Outcome.Player,
                (Hand.Lizard,   Hand.Spock)    => Outcome.Player,
                (Hand.Lizard,   Hand.Paper)    => Outcome.Player,
                (Hand.Spock,    Hand.Scissors) => Outcome.Player,
                (Hand.Spock,    Hand.Rock)     => Outcome.Player,
                _ => Outcome.Agent
            };
        }

        private void Reset()
        {
            _playerScore = _agentScore = 0;
            _lblChoices.Text = "You: —   Agent: —";
            _lblResult.Text  = "Make a move";
            _lblScore.Text   = "Score 0 : 0";
        }
    }
}
