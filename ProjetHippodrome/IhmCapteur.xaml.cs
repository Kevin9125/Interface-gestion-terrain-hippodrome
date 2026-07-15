using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MySql.Data.MySqlClient;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
 
namespace ProjetHippodrome
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class IhmCapteur : Window
    {
        // Attribut conforme au diagramme (+)
        private VerificationDonnees gestion_data = new VerificationDonnees();
        private BaseDeDonnees bdd = new BaseDeDonnees();
        //ssIHMs Menu = new IHMs();
        public ObservableCollection<StationMeteo> ListeMesures { get; set; } = new ObservableCollection<StationMeteo>();
        public IhmCapteur()
        {
            InitializeComponent();
            PluieValues = new ChartValues<double>();
            Labels = new[] { "24h", "7 jours" };

            DataContext = this;
            MettreAJourDateHeure();
        }
        private void MettreAJourDateHeure()
        {
            TxtDate.Text = DateTime.Now.ToString("dddd dd MMMM yyyy");
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) =>
            {
                //TxtHeure.Text = DateTime.Now.ToString("HH:mm:ss");
            };
            timer.Start();
        }
        private void MettreAJourVent(string direction)
        {
            double angle = 0;
            switch (direction.ToUpper())
            {
                case "N": angle = 0; break;
                case "NE": angle = 45; break;
                case "E": angle = 90; break;
                case "SE": angle = 135; break;
                case "S": angle = 180; break;
                case "SO": angle = 225; break;
                case "O": angle = 270; break;
                case "NO": angle = 315; break;
            }

            var animation = new System.Windows.Media.Animation.DoubleAnimation
            {
                To = angle,
                Duration = TimeSpan.FromMilliseconds(400)
            };

            RotationVent.BeginAnimation(System.Windows.Media.RotateTransform.AngleProperty, animation);
        }
        private static double ParseNumber(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return double.MinValue;

            string cleaned = input
                .Replace("°C", "").Replace("°c", "")
                .Replace("km/h", "").Replace("Km/h", "")
                .Replace("mm/j", "").Replace("mm/jour", "")
                .Replace("mm", "").Replace("%", "")
                .Replace(",", ".").Trim();

            if (double.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out double val))
                return val;

            return double.MinValue;
        }
        public ChartValues<double> PluieValues { get; set; }
        public string[] Labels { get; set; }
        private void MettreAJourJaugeETP(double etp)
        {
            double largeurMax = 200;
            double ratio = etp / 6.0;

            if (ratio > 1) ratio = 1;
            if (ratio < 0) ratio = 0;

            DoubleAnimation animWidth = new DoubleAnimation
            {
                To = ratio * largeurMax,
                Duration = TimeSpan.FromMilliseconds(800),
                EasingFunction = new QuarticEase()
            };
            EtpRiskBar.BeginAnimation(Border.WidthProperty, animWidth);

            if (etp < 2)
            {
                TxtEtpStatus.Text = "SÉCHAGE FAIBLE";
                EtpRiskBar.Background = Brushes.SeaGreen;
            }
            else if (etp < 4)
            {
                TxtEtpStatus.Text = "SÉCHAGE MODÉRÉ";
                EtpRiskBar.Background = Brushes.Orange;
            }
            else
            {
                TxtEtpStatus.Text = "SÉCHAGE CRITIQUE";
                EtpRiskBar.Background = Brushes.Crimson;
            }
        }
        private void ActualiserTout()
        {
            bdd.RechercheDonnees();

            gestion_data.SetDonneesMeteo(bdd.getTemperatureSol(), bdd.getTemperatureAir(), bdd.getEnsoleillement(), bdd.getVitesseVent());
            double etp = gestion_data.CalculeEvapo();

            TxtHumiditeSol.Text = bdd.getHumiditeSol().ToString() + " %";
            TxtEvaporationETP.Text = etp.ToString("F2") + " mm";

            double p24 = gestion_data.CalculePluviometrie(1);
            double p7j = gestion_data.CalculePluviometrie(7);
            TxtPluviometrie24h.Text = p24 + " mm";
            TxtPluviometrie7j.Text = p7j + " mm";

            MettreAJourJaugeETP(etp);
            PluieValues.Clear();
            PluieValues.Add(p24);
            PluieValues.Add(p7j);
        }
        private async Task RecupererDonneesMeteo()
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // 1. Autorisation de l'API Sacha
                    client.DefaultRequestHeaders.Add("Accept", "application/json");

                    // 2. Ton adresse IP actuelle .25
                    string jsonStation = await client.GetStringAsync("http://192.168.10.20:7063/api/meteo/last");

                    // 3. Désérialisation de la liste
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var stations = JsonSerializer.Deserialize<List<StationMeteo>>(jsonStation, options);

                    if (stations != null && stations.Count > 0)
                    {
                        var derniereMesure = stations[0];

                        // 4. On remplit les cases du XAML
                        Application.Current.Dispatcher.Invoke(() => {
                            TxtTemperatureAir.Text = derniereMesure.temp_air_deg.ToString("F1") + " °C";
                            TxtHygrometrieAir.Text = derniereMesure.humidite_air_pct.ToString("F0") + " %";
                            TxtPluviometrie24h.Text = derniereMesure.pluviometrie_mm.ToString("F1") + " mm";

                            // --- NOUVELLES DONNÉES ---
                            // Vitesse du vent et ensoleillement
                            TxtVitesseVent.Text = derniereMesure.vent_vitesse_kmh.ToString("F1") + " km/h";
                            TxtEnsoleillement.Text = derniereMesure.ensoleillement_wm2.ToString("F0") + " W/m²";
                            // ÉTAPE 3 : Mettre à jour la liste déroulante avec toutes les mesures reçues
                            ListeMesures.Clear();
                            foreach (var station in stations)
                            {
                                ListeMesures.Add(station);
                            }

                            // Optionnel : Sélectionner automatiquement la première mesure (la plus récente)
                            CboMesures.SelectedIndex = 0;

                            // Animation de la boussole
                            string directionDuVent = ConvertirIdDirectionEnChaine(derniereMesure.vent_direction_id);
                            MettreAJourVent(directionDuVent);
                        });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Impossible de lire l'API de Sacha : " + ex.Message, "Erreur Réseau");
                }

                // API de secours (sol)
                try
                {
                    string jsonSol = await client.GetStringAsync("https://api.meteo.com/sol");
                    var sol = JsonSerializer.Deserialize<CapteursSol>(jsonSol);
                    if (sol != null)
                    {
                        Application.Current.Dispatcher.Invoke(() => {
                            TxtHumiditeSol.Text = sol.HumiditeSol.ToString() + " %";
                        });
                    }
                }
                catch { }
            }
        }
        // ÉTAPE 4 : Événement déclenché quand l'utilisateur choisit une mesure dans la liste
        private void CboMesures_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // On vérifie qu'un élément est bien sélectionné
            if (CboMesures.SelectedItem is StationMeteo mesureSelectionnee)
            {
                // On met à jour les champs de l'interface avec la mesure choisie
                TxtTemperatureAir.Text = mesureSelectionnee.temp_air_deg.ToString("F1") + " °C";
                TxtHygrometrieAir.Text = mesureSelectionnee.humidite_air_pct.ToString("F0") + " %";
                TxtPluviometrie24h.Text = mesureSelectionnee.pluviometrie_mm.ToString("F1") + " mm";
                TxtVitesseVent.Text = mesureSelectionnee.vent_vitesse_kmh.ToString("F1") + " km/h";
                TxtEnsoleillement.Text = mesureSelectionnee.ensoleillement_wm2.ToString("F0") + " W/m²";

                string directionDuVent = ConvertirIdDirectionEnChaine(mesureSelectionnee.vent_direction_id);
                MettreAJourVent(directionDuVent);
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await RecupererDonneesMeteo();
        }
        private string ConvertirIdDirectionEnChaine(int idDirection)
        {
            // On associe l'ID de l'API à un point cardinal
            // (Ajustez ces valeurs selon la doc de votre API si 1 n'est pas le Nord)
            switch (idDirection)
            {
                case 1: return "N";
                case 2: return "NE";
                case 3: return "E";
                case 4: return "SE";
                case 5: return "S";
                case 6: return "SO";
                case 7: return "O";
                case 8: return "NO";
                default: return "N";
            }
        }

        private void BRetourMenu_Click(object sender, RoutedEventArgs e)
        {
            //this.Close();
            //Menu.Show();
        }
    }

    public class StationMeteo
    {
        public int id_mesure { get; set; }
        public int id_capteur { get; set; }
        public string date_releve { get; set; }
        public double humidite_air_pct { get; set; }
        public double pluviometrie_mm { get; set; }
        public double ensoleillement_wm2 { get; set; }
        public double temp_air_deg { get; set; }
        public double vent_vitesse_kmh { get; set; }
        public int vent_direction_id { get; set; }

        // Propriété ajoutée pour afficher joliment la ligne dans le ComboBox
        public string DisplayName => $"Mesure #{id_mesure} - {date_releve}";
    }

    public class CapteursSol
    {
        public double TempSol { get; set; }
        public double HumiditeSol { get; set; }
    }
}
