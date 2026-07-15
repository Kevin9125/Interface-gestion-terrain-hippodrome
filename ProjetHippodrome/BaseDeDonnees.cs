using System;
using MySql.Data.MySqlClient;

namespace ProjetHippodrome
{
    public class BaseDeDonnees
    {
        // Attributs privés conformes au diagramme (-)
        private double pluviometrie;
        private double temperatureSol;
        private double temperatureAir;
        private double ensoleillement;
        private double vitesseVent;
        private string directionVent;
        private string typePiste;
        private int humiditeSol;
        private int hygrometrie;
        private string connectionString = "server=localhost;database=hippodrome_angers_db;user=root;password=";

        public BaseDeDonnees() { }
        public BaseDeDonnees(double pluvio, double tempS, double tempA, double soleil, double vent)
        {
            this.pluviometrie = pluvio;
            this.temperatureSol = tempS;
            this.temperatureAir = tempA;
            this.ensoleillement = soleil;
            this.vitesseVent = vent;
        }

        // Méthode principale du diagramme pour charger les infos
        public void RechercheDonnees()
        {
            using (MySqlConnection cnn = new MySqlConnection(connectionString))
            {
                cnn.Open();
                string sql = "SELECT * FROM mesure_meteo ORDER BY date_releve DESC LIMIT 1";
                MySqlCommand cmd = new MySqlCommand(sql, cnn);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // On remplit les attributs de la classe
                        this.temperatureSol = Convert.ToDouble(reader["temp_sol_deg"]);
                        this.temperatureAir = Convert.ToDouble(reader["temp_air_deg"]);
                        this.ensoleillement = Convert.ToDouble(reader["ensoleillement_wm2"]);
                        this.vitesseVent = Convert.ToDouble(reader["vent_vitesse_kmh"]);
                        this.pluviometrie = Convert.ToDouble(reader["pluviometrie_mm"]);
                        this.humiditeSol = Convert.ToInt32(reader["humidite_sol_pct"]);
                        //this.hygrometrie = Convert.ToInt32(reader["hygrometrie_air_pct"]);
                        //this.directionVent = reader["vent_direction"].ToString();
                    }
                }
            }
        }
        public double getSommePluie(int nbJours)
        {
            double cumul = 0;
            using (MySqlConnection cnn = new MySqlConnection(connectionString))
            {
                cnn.Open();
                // Requête SQL pour sommer la pluie sur X jours
                string sql = $"SELECT SUM(pluviometrie_mm) FROM mesure_meteo WHERE date_releve >= NOW() - INTERVAL {nbJours} DAY";
                MySqlCommand cmd = new MySqlCommand(sql, cnn);

                object result = cmd.ExecuteScalar();
                if (result != DBNull.Value && result != null)
                {
                    cumul = Convert.ToDouble(result);
                }
            }
            return cumul;
        }

        // Getters conformes au diagramme (+)
        public double getTemperatureSol() => this.temperatureSol;
        public double getTemperatureAir() => this.temperatureAir;
        public double getEnsoleillement() => this.ensoleillement;
        public double getVitesseVent() => this.vitesseVent;
        public double getPluviometrie() => this.pluviometrie;
        // N'oublie pas d'ajouter les "Getters" pour ces nouvelles données
        public int getHumiditeSol() => this.humiditeSol;
        public int getHygrometrie() => this.hygrometrie;
        public string getDirectionVent() => this.directionVent;
    }
}