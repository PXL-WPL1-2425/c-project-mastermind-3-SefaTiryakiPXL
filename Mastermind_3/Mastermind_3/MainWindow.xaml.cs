using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using Microsoft.VisualBasic;

namespace Mastermind_3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int maxAttempts; // Dynamisch aantal pogingen
        private const int INITIAL_SCORE = 100; // De beginscore
        private readonly string[] COLORS = { "rood", "geel", "groen", "oranje", "wit", "blauw" }; // Beschikbare kleuren.
        private int attempts; // Aantal gemaakte pogingen
        private int score; // Houdt de huidige score van de speler bij.
        private string playerName; // Naam van de speler
        private string[] secretCode;
        private List<List<string>> gameHistory; // Houdt de geschiedenis van pogingen en resultaten bij
        private string[] highscores; // Array voor highscores
        private List<string> playerNames; // Lijst voor meerdere namen
        private int currentPlayerIndex; // Bekijkt de speler die nu speeltv

        public MainWindow()
        {
            InitializeComponent();
            maxAttempts = 10; // Standaard 10 pogingen
            highscores = new string[15]; // Maximaal 15 highscores
            StartGame();
        }

        private void StartGame()
        {
            playerNames = GetPlayerGuess();

            if (playerNames.Count == 0)
            {
                Close();
                return;
            }
            currentPlayerIndex = 0;
            playerName = playerNames[currentPlayerIndex];
            InitializeGame();
        }
        private List<string> GetPlayerGuess()
        {
            var names = new List<string>();
            while (true)
            {
                string naam = Interaction.InputBox(
                    names.Count == 0 ? "Voer de naam van de eerste speler in:" : "Voer de naam van de volgende speler in:",
                    "Spelersnaam",
                    "");

                if (!string.IsNullOrWhiteSpace(naam))
                {
                    names.Add(naam);

                    string extraSpeler = MessageBox.Show(
                        "Wil je een extra speler toevoegen?",
                        "Extra speler",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question
                    ) == MessageBoxResult.Yes ? "Ja" : "Nee";

                    if (extraSpeler.Equals("Nee", StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }
                }
                else
                {
                    MessageBox.Show("Je moet een naam invoeren!", "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            return names;
        }
        private string GetPlayerName()
        {
            while (true)
            {
                string naam = Interaction.InputBox(
                    "Voer je naam in:",
                    "Spelersnaam",
                    "");

                if (!string.IsNullOrWhiteSpace(naam))
                {
                    return naam;
                }
                else
                {
                    MessageBox.Show("Je moet een naam invoeren!", "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
        private void InitializeGame()
        {
            attempts = 0;
            score = INITIAL_SCORE;
            gameHistory = new List<List<string>>();
            GenerateSecretCode();
            KleurComboBoxes();
        }
        private void GenerateSecretCode()
        {
            var random = new Random();
            secretCode = new string[4];
            for (int i = 0; i < 4; i++)
            {
                secretCode[i] = COLORS[random.Next(COLORS.Length)];
            }
            this.Title = $"Mastermind ({string.Join(", ", secretCode)})";
        }
        private void KleurComboBoxes()
        {
            foreach (var comboBox in new[] { ComboBox1, ComboBox2, ComboBox3, ComboBox4 })
            {
                comboBox.Items.Clear();
                foreach (var color in COLORS)
                {
                    comboBox.Items.Add(color);
                }
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Controleer of alle kleuren geselecteerd zijn.
            if (!AreAllColorsSelected())
            {
                MessageBox.Show("Selecteer alstublieft alle kleuren!");
                return;
            }
            string[] playerGuess = GetPlayerGok(); // Geselecteerde code van de speler
            MakeGuess(playerGuess);
        }

        private bool AreAllColorsSelected()
        {
            // Controleer dat geen van de ComboBoxes null is.
            return ComboBox1.SelectedItem != null &&
                   ComboBox2.SelectedItem != null &&
                   ComboBox3.SelectedItem != null &&
                   ComboBox4.SelectedItem != null;
        }

        private string[] GetPlayerGok()
        {
            // geselecteerde kleuren uit de ComboBoxes.
            return new string[]
            {
                ComboBox1.SelectedItem.ToString(),
                ComboBox2.SelectedItem.ToString(),
                ComboBox3.SelectedItem.ToString(),
                ComboBox4.SelectedItem.ToString()
            };
        }
        private void MakeGuess(string[] playerGuess)
        {
            attempts++; // ++ pogingen.
            this.Title = $"Mastermind - Poging {attempts}"; // Titel + pogingen

            if (IsCorrecteGok(playerGuess)) // Controleer of de gok correct is.
            {
                CorrecteInputSpeler();
                return;
            }

            int scorePenalty = BerekenScorePenalty(playerGuess); // Bereken de strafpunten
            UpdateGameScore(playerGuess, scorePenalty);

            if (attempts >= maxAttempts)
            {
                BerichtGameOver(); // einde
            }
        }

        private bool IsCorrecteGok(string[] playerGuess)
        {
            return playerGuess[0] == secretCode[0] &&
                   playerGuess[1] == secretCode[1] &&
                   playerGuess[2] == secretCode[2] &&
                   playerGuess[3] == secretCode[3];
        }
        private void CorrecteInputSpeler()
        {
            currentPlayerIndex++;
            string nextPlayer = currentPlayerIndex < playerNames.Count ? playerNames[currentPlayerIndex] : "niemand";
            // Bepaal de volgende speler
            MessageBox.Show(
                $"Juist! De code is gekraakt in {attempts} pogingen.\nNu is speler {nextPlayer} aan de beurt.",
                $"Mastermind - {playerName}", // titel mastermind + spelerName
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            // als currentPlayerIndex kleiner is dan playerNames dan zijn er nog spelers
            if (currentPlayerIndex < playerNames.Count)
            {
                playerName = playerNames[currentPlayerIndex];
                InitializeGame();
                this.Title = $"Mastermind - Speler: {playerName}";
            }
            else
            {
                MessageBox.Show("Alle spelers hebben gespeeld!", "Spel Klaar", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private int BerekenScorePenalty(string[] playerGuess)
        {
            int scorePenalty = 0;
            string feedback = "";

            for (int i = 0; i < 4; i++)
            {
                if (playerGuess[i] == secretCode[i])
                {
                    SetBorderColor(i, Brushes.DarkRed);
                    feedback += "J ";
                }
                else if (secretCode.Contains(playerGuess[i]))
                {
                    SetBorderColor(i, Brushes.Wheat);
                    scorePenalty += 1;
                    feedback += "FP ";
                }
                else
                {
                    feedback += "F ";
                    scorePenalty += 2;
                }
            }
            return scorePenalty;
        }

        private void UpdateGameScore(string[] playerGuess, int scorePenalty)
        {
            score = Math.Max(0, score - scorePenalty); // strafpunten - score ( 0 staat ervoor dat de score niet 0 kan zijn)

            var currentAttempt = new List<string>(playerGuess) // nu
            {
                $"Score: {score}"
            };  

            gameHistory.Add(currentAttempt);
            UpdateHistoryDisplay();

            Score.Content = $"Score: {score} strafpunten";
        }
        private void UpdateHistoryDisplay()
        {
            ListBoxHistoriek.Items.Clear();
            foreach (var attempt in gameHistory)
            {
                string displayText = $"{attempt[0]}, {attempt[1]}, {attempt[2]}, {attempt[3]} -> {attempt[4]}";
                ListBoxHistoriek.Items.Add(displayText);
            }
        }
        private void BerichtGameOver()
        {
            currentPlayerIndex++;
            string nextPlayer = currentPlayerIndex < playerNames.Count ? playerNames[currentPlayerIndex] : "niemand";

            MessageBox.Show(
                $"Gefaald! De code was: {string.Join(", ", secretCode)}.\nNu is speler {nextPlayer} aan de beurt.",
                $"Mastermind - {playerName}",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            if (currentPlayerIndex < playerNames.Count)
            {
                playerName = playerNames[currentPlayerIndex];
                InitializeGame();
                this.Title = $"Mastermind - Speler: {playerName}";
            }
            else
            {
                MessageBox.Show("Alle spelers hebben gespeeld!", "Spel Klaar", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void RegistreerHighscore()
        {
            // Bereken de score voor de highscore (score/100)
            int highscoreCalculation = score / 10;

            // Maak de highscore entry
            string highscoreEntry = $"{playerName} - {attempts} pogingen - Score: {highscoreCalculation}";

            // Voeg toe aan highscores, schuif andere scores op
            for (int i = highscores.Length - 1; i > 0; i--)
            {
                highscores[i] = highscores[i - 1];
            }
            highscores[0] = highscoreEntry;
        }
        private void GetPlayerName(object sender, RoutedEventArgs e)
        {
            string antwoord = Interaction.InputBox("Geef uw naam", "Invoer", "", 500, 500);

            if (string.IsNullOrEmpty(antwoord))
            {
                MessageBox.Show("Typ alstublieft uw naam", "Foutieve invoer");
                return; 
            }
        }
        private void NieuwSpel_Click(object sender, RoutedEventArgs e)
        {
            StartGame();
        }
        private void Highscores_Click(object sender, RoutedEventArgs e)
        {
            string highscoresTekst = "Highscores:\n\n";
            for (int i = 0; i < highscores.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(highscores[i]))
                {
                    highscoresTekst += $"{i + 1}. {highscores[i]}\n";
                }
            }

            MessageBox.Show(highscoresTekst, "Highscores", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void Afsluiten_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void AantalPogingen_Click(object sender, RoutedEventArgs e)
        {
            string pogingenInput = Interaction.InputBox(
                "Geef het aantal pogingen (3-20):",
                "Aantal Pogingen",
                maxAttempts.ToString());

            if (int.TryParse(pogingenInput, out int aantalPogingen))
            {
                if (aantalPogingen >= 3 && aantalPogingen <= 20)
                {
                    maxAttempts = aantalPogingen;
                    MessageBox.Show($"Aantal pogingen ingesteld op {maxAttempts}", "Bevestiging", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Aantal pogingen moet tussen 3 en 20 liggen.", "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
        private Brush GetColor(string kleur)
        {
            switch (kleur.ToLower())
            {
                case "rood":
                    return Brushes.Red;
                case "geel":
                    return Brushes.Yellow;
                case "groen":
                    return Brushes.Green;
                case "oranje":
                    return Brushes.Orange;
                case "wit":
                    return Brushes.White;
                case "blauw":
                    return Brushes.Blue;
                default:
                    return Brushes.Transparent;
            }
        }
        private void SetBorderColor(int index, Brush color)
        {
            switch (index)
            {
                case 0:
                    kleur1Border.BorderBrush = color;
                    break;
                case 1:
                    kleur2Border.BorderBrush = color;
                    break;
                case 2:
                    kleur3Border.BorderBrush = color;
                    break;
                case 3:
                    kleur4Border.BorderBrush = color;
                    break;
            }
        }
        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is string kleur)
            {
                switch (comboBox.Name)
                {
                    case "ComboBox1":
                        Kleur1.Background = GetColor(kleur);
                        TextBlock1.Text = $"Gekozen kleur: {kleur}";
                        break;
                    case "ComboBox2":
                        Kleur2.Background = GetColor(kleur);
                        TextBlock2.Text = $"Gekozen kleur: {kleur}";
                        break;
                    case "ComboBox3":
                        Kleur3.Background = GetColor(kleur);
                        TextBlock3.Text = $"Gekozen kleur: {kleur}";
                        break;
                    case "ComboBox4":
                        Kleur4.Background = GetColor(kleur);
                        TextBlock4.Text = $"Gekozen kleur: {kleur}";
                        break;
                }
            }
        }
    }
}