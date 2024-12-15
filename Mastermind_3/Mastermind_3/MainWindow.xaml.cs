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
        private int highscoreCount = 0; // Houdt het aantal gespeelde highscores bij
        public MainWindow()
        {
            InitializeComponent();
            maxAttempts = 10; // Standaard 10 pogingen
            highscores = new string[15]; // Maximaal 15 highscores
            StartGames();
        }
        private void StartGames()
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
            UpdateStatusLabel();

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
        private void HintButton_Click(object sender, RoutedEventArgs e)
        {
            if (score < 15)
            {
                MessageBox.Show("Je hebt niet genoeg punten voor een hint!", "Hint", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string hintType = MessageBox.Show(
                "JA = Wil je een juiste kleur (15 strafpunten) of NEE = een juiste kleur op de juiste plaats (25 strafpunten)?",
                "Hint kopen",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes ? "kleur" : "positie";

            if (hintType == "kleur")
            {
                GeefHintKleur();
                score -= 15;
            }
            else
            {
                if (score < 25)
                {
                    MessageBox.Show("Je hebt niet genoeg punten voor deze hint!", "Hint", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                GeefHintPositie();
                score -= 25;
            }

            UpdateStatusLabel(); // Update de status met de nieuwe score
        }
        private void GeefHintKleur()
        {
            List<string> ongebruikteKleuren = new List<string>();
            foreach (var kleur in secretCode)
            {
                bool kleurGevonden = false;
                foreach (var gok in gameHistory)
                {
                    // Als de kleur in de huidige gok voorkomt
                    if (gok.Contains(kleur))
                    {
                        // kleur is gevonden in een gok
                        kleurGevonden = true;
                        break;
                    }
                }
                // Als de kleur niet is gevonden in de goklist
                if (!kleurGevonden)
                {
                    // Voeg de kleur toe aan de lijst van ongebruikte kleuren
                    ongebruikteKleuren.Add(kleur);
                }
            }
            if (ongebruikteKleuren.Count > 0)
            {
                string hintKleur = ongebruikteKleuren[0];
                // Toon een hint met deze kleur
                MessageBox.Show($"Een juiste kleur in de code is: {hintKleur}", "Hint", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Als er geen ongebruikte kleuren zijn, geef dan een bericht dat alle kleuren al geprobeerd zijn
                MessageBox.Show("Alle kleuren zijn al geprobeerd!", "Hint", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void GeefHintPositie()
        {
            for (int i = 0; i < secretCode.Length; i++)
            {
                if (gameHistory.All(gok => gok[i] != secretCode[i]))
                {
                    MessageBox.Show($"Een kleur op de juiste plaats is: {secretCode[i]} op positie {i + 1}", "Hint", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }
            MessageBox.Show("Geen nieuwe hints beschikbaar voor posities!", "Hint", MessageBoxButton.OK, MessageBoxImage.Information);
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
        private void UpdateStatusLabel()
        {
            Score.Content = $"Speler: {playerName} | Pogingen: {attempts}/{maxAttempts} | Score: {score}";
            // deze werkt met de speler/poging/score van de speler die nu bezig is
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

            RegistreerHighscore(playerName, score, attempts);

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
                    SetBorderColor(i, Brushes.DarkRed, "Juiste kleur, juiste positie"); // Rode rand + tooltip
                    feedback += "J ";
                }
                else if (secretCode.Contains(playerGuess[i]))
                {
                    SetBorderColor(i, Brushes.Wheat, "Juiste kleur, foute positie"); // Witte rand + tooltip
                    scorePenalty += 1;
                    feedback += "FP ";
                }
                else 
                {
                    SetBorderColor(i, Brushes.Transparent, "Foute kleur"); // Geen rand + tooltip
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
            RegistreerHighscore(playerName, score, attempts);
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
        private void RegistreerHighscore(string playerName, int score, int attempts)
        {
            if (highscoreCount < highscores.Length)
            {
                highscores[highscoreCount++] = $"{playerName} - Pogingen: {attempts} - Score: {score}";
            }
        }
        private void ShowHighscores()
        {
            if (highscoreCount == 0)
            {
                MessageBox.Show("Er zijn nog geen highscores.", "Mastermind highscores");
            }
            else
            {
                // Voeg niet-lege entries samen om ze in een lijst weer te geven
                string highscoreList = string.Join("\n", highscores.Where(h => !string.IsNullOrEmpty(h)));
                MessageBox.Show(highscoreList, "Mastermind highscores");
            }
        }
        private void NieuwSpel_Click(object sender, RoutedEventArgs e)
        {
            StartGames();
        }
        private void Highscores_Click(object sender, RoutedEventArgs e)
        {
            ShowHighscores();
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
        private void SetBorderColor(int index, Brush color, string tooltip)
        {
            switch (index)
            {
                case 0:
                    kleur1Border.BorderBrush = color;
                    kleur1Border.ToolTip = tooltip; // Voeg de tooltip toe
                    break;
                case 1:
                    kleur2Border.BorderBrush = color;
                    kleur2Border.ToolTip = tooltip; // Voeg de tooltip toe
                    break;
                case 2:
                    kleur3Border.BorderBrush = color;
                    kleur3Border.ToolTip = tooltip; // Voeg de tooltip toe
                    break;
                case 3:
                    kleur4Border.BorderBrush = color;
                    kleur4Border.ToolTip = tooltip; // Voeg de tooltip toe
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