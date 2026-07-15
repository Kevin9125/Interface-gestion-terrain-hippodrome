using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ProjetHippodrome
{
    public partial class IHMs : Window
    {
        // On déclare le client HTTP une seule fois pour toute la classe
        private static readonly HttpClient client = new HttpClient();
        IhmCreationZone creationzone;
        IhmCapteur meteo;
        IhmPenetrometrie pénétrometrie;
        

        // URL de ton API basée sur tes tests
        private readonly string apiUrl = "http://192.168.10.20:7063/api/meteo";

        public IHMs()
        {
            InitializeComponent();

            //compositions
            creationzone = new IhmCreationZone();
            meteo = new IhmCapteur();
            
            
        }

        // --- NOUVEAU : LOGIQUE DE RECHERCHE DATE ET HEURE ---
        private async void BtnRechercher_Click(object sender, RoutedEventArgs e)
        {
            // 1. Récupération et formatage de la date du DatePicker
            string dateSelectionnee = DatePickerMeteo.SelectedDate?.ToString("yyyy-MM-dd");

            if (string.IsNullOrEmpty(dateSelectionnee))
            {
                MessageBox.Show("Veuillez sélectionner une date dans le calendrier avant de rechercher.", "Date manquante", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // CAS 1 : Une heure spécifique est demandée (Index 0 = "Toute la journée", donc Index > 0 = une heure)
                if (ComboHeure.SelectedIndex > 0)
                {
                    // L'index 1 correspond à 00:00, donc heure = Index - 1
                    int heure = ComboHeure.SelectedIndex - 1;

                    string urlUnique = $"{apiUrl}/date/{dateSelectionnee}/heure/{heure}";
                    string resultat = await client.GetStringAsync(urlUnique);

                    MessageBox.Show($"Données de moyenne reçues pour le {dateSelectionnee} à {heure}h :\n\n{resultat}", "Mode Horaire");

                    // TODO : Si tu veux l'envoyer dans ton Dashboard :
                    // MainWindow dashboard = new MainWindow(resultat);
                    // dashboard.Show();
                }
                // CAS 2 : Toute la journée demandée
                else
                {
                    string urlJournee = $"{apiUrl}/date/{dateSelectionnee}";
                    string resultat = await client.GetStringAsync(urlJournee);

                    MessageBox.Show($"288 mesures récupérées avec succès pour la journée du {dateSelectionnee} !", "Mode Journalier");

                    // Ici, le top c'est d'ouvrir ton MainWindow et de lui passer 'resultat' 
                    // pour que son DataGrid affiche les lignes.
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la communication avec l'API : {ex.Message}", "Erreur API", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // --- ENTRAÎNEMENT DES ANCIENS BOUTONS ---

        private async void BtnVitesseVent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string resultat = await client.GetStringAsync($"{apiUrl}/vent/vitesse");
                MessageBox.Show($"Vitesse du vent actuelle : {resultat}", "Donnée reçue");
            }
            catch (Exception ex) { MessageBox.Show($"Erreur : {ex.Message}", "Erreur"); }
        }

        private async void Btn100DernieresValeurs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string resultat = await client.GetStringAsync($"{apiUrl}/all");
                MessageBox.Show("Les 100 dernières valeurs ont bien été chargées !", "Succès");
            }
            catch (Exception ex) { MessageBox.Show($"Erreur : {ex.Message}", "Erreur"); }
        }

        private async void BtnToutesDonnees_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string resultat = await client.GetStringAsync($"{apiUrl}/all");
                MessageBox.Show("L'intégralité des données a été récupérée.", "Succès");
            }
            catch (Exception ex) { MessageBox.Show($"Erreur : {ex.Message}", "Erreur"); }
        }

        private async void BtnDerniereDonnee_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string resultat = await client.GetStringAsync($"{apiUrl}/derniere-insertion");
                MessageBox.Show($"Dernière donnée enregistrée : \n{resultat}", "Base de données");
            }
            catch (Exception ex) { MessageBox.Show($"Erreur : {ex.Message}", "Erreur"); }
        }
        // BOUTON 1 : Route /api/meteo/date/{date}/{champ}
        private async void BtnDateChamp_Click(object sender, RoutedEventArgs e)
        {
            // 1. Vérification et récupération de la date
            if (DpRecherche.SelectedDate == null)
            {
                MessageBox.Show("Veuillez sélectionner une date.", "Erreur");
                return;
            }
            // Formatage de la date (ex: 2026-06-09) selon le format attendu par votre API
            string dateStr = DpRecherche.SelectedDate.Value.ToString("yyyy-MM-dd");

            // 2. Récupération du champ sélectionné
            string champStr = (CboChampRecherche.SelectedItem as ComboBoxItem)?.Content.ToString();

            // 3. Construction de l'URL et appel API
            string url = $"http://192.168.10.20:7063/api/meteo/date/{dateStr}/{champStr}";

            await ExecuterRequeteGenerique(url);
        }

        // BOUTON 2 : Route /api/meteo/date/{date}/heure/{heure}/{champ}
        // Note : J'ai adapté l'URL en /date/{date}/heure/{heure}/{champ} pour inclure dynamiquement la variable heure
        private async void BtnDateHeureChamp_Click(object sender, RoutedEventArgs e)
        {
            // 1. Vérification et récupération de la date
            if (DpRecherche.SelectedDate == null)
            {
                MessageBox.Show("Veuillez sélectionner une date.", "Erreur");
                return;
            }
            string dateStr = DpRecherche.SelectedDate.Value.ToString("yyyy-MM-dd");

            // 2. Récupération de l'heure et du champ
            string heureStr = TxtHeureRecherche.Text.Trim();
            string champStr = (CboChampRecherche.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (string.IsNullOrEmpty(heureStr))
            {
                MessageBox.Show("Veuillez saisir une heure.", "Erreur");
                return;
            }

            // 3. Construction de l'URL (Vérifiez la structure exacte demandée par votre route d'API)
            // Si votre route exacte est exactement "/api/meteo/date/{date}/heure/{champ}" où l'heure est figée ou combinée, adaptez ici :
            string url = $"http://192.168.10.20:7063/api/meteo/date/{dateStr}/heure/{heureStr}/{champStr}";

            await ExecuterRequeteGenerique(url);
        }

        /// <summary>
        /// Méthode utilitaire pour centraliser l'appel HTTP à l'API et l'affichage du résultat
        /// </summary>
        /// <param name="url">url de l'API</param>
        private async Task ExecuterRequeteGenerique(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Add("Accept", "application/json");

                    // Envoi de la requête
                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        // Lecture du résultat (souvent un string brut ou un petit JSON contenant la valeur)
                        string resultat = await response.Content.ReadAsStringAsync();

                        TxtResultatRoute.Text = $"Résultat : {resultat}";
                    }
                    else
                    {
                        TxtResultatRoute.Text = $"Erreur API : Code {response.StatusCode}";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erreur lors de l'appel de la route : " + ex.Message, "Erreur Réseau");
                }
            }
        }
        private void BAffichageMeteo_Click(object sender, RoutedEventArgs e)
        {
            meteo.Show();
            this.Close();
        }

        private void BCreationZone_Click(object sender, RoutedEventArgs e)
        {
            creationzone.Show();
            this.Close();
        }

        private void BPenetrometrie_Click(object sender, RoutedEventArgs e)
        {
            //penetrometrie.Show();
            this.Close();
        }
    }
}