using GMap.NET;
using GMap.NET.WindowsPresentation;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows;

namespace ProjetHippodrome
{
    public class VerificationDonnees
    {
        private double temperatureSol;
        private double temperatureAir;
        private double ensoleillement; // 0 à 1
        private double vitesseVent;    // km/h

        public VerificationDonnees() 
        {
            list_donnees_api = new List<DonnéesAPI>();
            list_donnees_robot = new List<DonnéesRobot>();
            //temp_list_zone = new List<GMapPolygon>();
            //temp_list_point_mesure = new List<PointLatLng>();
        }

        /// <summary>
        /// Calcule l'évaporation du sol en mm/jour
        /// Modèle simplifié basé sur température, vent et ensoleillement
        /// Inspiré des principes d'évapotranspiration (type Penman simplifié)
        /// </summary>

        private readonly List<DonnéesAPI> list_donnees_api;
        private readonly List<DonnéesRobot> list_donnees_robot;
        private readonly List<GMapPolygon> temp_list_zone = new List<GMapPolygon>();
        private readonly List<PointLatLng> temp_list_point_mesure = new List<PointLatLng>();
        private readonly API api = new API("http://192.168.10.20:7063/api/zones");
        private readonly Robot robot = new Robot("192.168.10.3");

        /// <summary>
        /// Valide le bon format des points sous format (lat, long) passé en paramètre
        /// </summary>
        /// <param name="points_mesure">les points a verifier</param>
        /// <returns>true si le format des points est conforme</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public bool ValidationPointMesure(PointLatLng points_mesure)
        {
            bool is_valide = true;

            //Verifie que les points de mesure ne sont pas vide
            if (points_mesure.IsEmpty)
            {
                Notifie("le point est vide");
                throw new ArgumentException($"le point est vide");
            }

            //verifie que les points de mesure sont conforme
            else if (points_mesure.Lat < -90 || points_mesure.Lat > 90 || points_mesure.Lng < -180 || points_mesure.Lng > 180)
            {
                Notifie("Le point de mesure n'est pas dans le bon format");
                throw new ArgumentOutOfRangeException($"le point de mesure n'est pas dans le bon format");
            }

            //verifie que les points de mesures sont bien dans leurs zones
            else if (temp_list_zone != null && temp_list_zone.Count > 0)
            {
                if (IsOnTheZone(temp_list_zone.Last(), points_mesure) == false)
                {
                    Notifie("le point n'est pas dans la zone spécifié");
                    throw new ArgumentException($"le point n'est pas dans la zone spécifié");
                }

                else
                {
                    //verifie qu'il n'y aille pas de point de mesure identique
                    if (temp_list_point_mesure.Count > 1)
                    {
                        for (var i = 1; i < temp_list_point_mesure.Count; i++)
                        {
                            if (temp_list_point_mesure[i - 1] == temp_list_point_mesure[i])
                            {
                                Notifie("les points numero {i - 1} et {i} sont identique ");
                                throw new ArgumentException($"les points numero {i - 1} et {i} sont identique");
                            }
                        }
                    }
                    //si tout est bon stocke les points de mesures dans la classe pour les envoyé
                    temp_list_point_mesure.Add(points_mesure);
                }
            }
            return is_valide;
        }

        /// <summary>
        /// Valide la bonne conformité des zones passée en paramètre
        /// </summary>
        /// <param name="zones">liste des zones a verifier</param>
        /// <returns>true si les zones sont conforme</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public bool ValidationZones(GMapPolygon zone)
        {
            //la zone n'est pas vide
            if (zone.Points.Count == 0)
            {
                Notifie("La zone est vide");
                throw new ArgumentException("la zone est vide");
            }

            //les points ne sont pas null
            for (int j = 0; j < zone.Points.Count; j++)
            {
                {
                    if (zone == null || zone.Points == null || zone.Points[j] == null)
                    {
                        Notifie("la zone est null ou contient un point qui est null");
                        throw new ArgumentNullException("la zone est null ou contient un point qui est null");
                    }
                }

                //les points sont correcte
                foreach (PointLatLng point in zone.Points)
                {
                    //verifie que la zone ne contienne pas de coordonnées vide
                    if (point.IsEmpty)
                    {
                        Notifie("un des points dans la zone est vide");
                        throw new ArgumentException("un des points dans la zone est vide");
                    }

                    //verifie qu'elle n'aille pas de coordonnées avec le mauvais format
                    else if (point.Lat < -90 || point.Lat > 90 || point.Lng < -180 || point.Lng > 180)
                    {
                        Notifie("un des points dans la zone n'est pas dans le bon format");
                        throw new ArgumentOutOfRangeException("un des points dans la zone n'est pas dans le bon format");
                    }
                }
            }

            temp_list_zone.Add(zone);

            //verification de la superposition des zones
            if (temp_list_zone.Count >= 2)
            {
                for (var i = 0; i < temp_list_zone.Count - 1; i++)
                {
                    if (VerificationSuperpositionZone(temp_list_zone[i], temp_list_zone[temp_list_zone.Count - 1]))
                    {
                        Notifie("deux zones se superposent");
                        throw new ArgumentException("deux zones se superposent");
                    }
                }

            }

            return true;
        }

        public double CalculeEvapo()
        {
            // Sécurité : on utilise l'attribut de la classe
            if (this.temperatureAir <= 0) return 0;

            // Calcul du coefficient basé sur l'attribut temperatureSol
            double coefSol = 0.40 + (this.temperatureSol * 0.01);

            // Calcul de la part Soleil
            double partSoleil = (this.ensoleillement / 1000) * 5 * coefSol;

            // Calcul de la part Vent (vitesseVent est un attribut)
            double partVent = (this.vitesseVent / 100) * (1 + (this.temperatureAir / 100));

            double etp = (partSoleil + partVent) * 1.2;

            return Math.Round(etp, 2);
        }

        /// <summary>
        /// Mise à jour des données météo
        /// </summary>
        public void SetDonneesMeteo(double tempAir, double tempSol, double soleil, double vent)
        {
            this.temperatureAir = tempAir;
            this.temperatureSol = tempSol;
            this.ensoleillement = soleil;
            this.vitesseVent = vent;
        }

        public double CalculePluviometrie(int nbJours)
        {
            BaseDeDonnees bddLocal = new BaseDeDonnees();
            double cumulBrut = bddLocal.getSommePluie(nbJours);

            return Math.Round(cumulBrut, 1);
        }
        /// <summary>
        /// Methode calculant la distance entre deux coordonnées GPS a l'aide de la formule haversine
        /// </summary>
        /// <param name="depart">point de depart du trajet </param>
        /// <param name="destination">point d'arrivé du trajet</param>
        /// <returns>la distance en cm entre <paramref name="depart"/> et <paramref name="destination"/></returns>
        public double CalculeDistance(PointLatLng depart, PointLatLng destination)
        {
            double distance = 0;
            double lat1 = depart.Lat * Math.PI / 180;//passer en rad//
            double lat2 = destination.Lat * Math.PI / 180;
            double lng1 = depart.Lng * Math.PI / 180;
            double lng2 = destination.Lng * Math.PI / 180;
            int rayon = 637100000;

            distance = 2 * rayon * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin((lat2 - lat1) / 2), 2) + Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin((lng2 - lng1) / 2), 2)));

            return distance;
        }

        public void EnregistrementConfiguration()
        {
            double a_distance = 0;

            // Partie Enregistrement des données
            if (temp_list_point_mesure.Count != temp_list_zone.Count)
            {
                Notifie("ajouté un point de mesure a la dernière zone que vous avez créer");
            }
            else
            {
                //ici temps_list_zone.Count == temp_list_point_mesure car il y a 1 point de mesure par zone
                for (var i = 0; i < temp_list_zone.Count; i++)
                {
                    if (i > 0)
                    {
                        a_distance = CalculeDistance(temp_list_point_mesure[i - 1], temp_list_point_mesure[i]);
                    }

                    DonnéesAPI api = new DonnéesAPI()
                    {
                        lat_point = temp_list_point_mesure[i].Lat,
                        lng_point = temp_list_point_mesure[i].Lng,
                        id_zone = i + 1
                    };

                    DonnéesRobot robot = new DonnéesRobot()
                    {
                        point_mesure = temp_list_point_mesure[i],
                        distance = (int)a_distance,
                        etat = 0
                    };

                    list_donnees_api.Add(api);
                    list_donnees_robot.Add(robot);
                }

                temp_list_zone.Clear();
                temp_list_point_mesure.Clear();

                // Partie transmission des données au robot (a enlevé si le robot ou l'API n'est pas prête//
                var result_connexion = robot.ConnexionRobot();

                if (result_connexion == "connection au serveur reussit")
                {
                    var result_envoie = robot.EnvoiePointDeMesure(list_donnees_robot);
                    Notifie(result_envoie);

                    if (result_envoie == "Les points de mesures ont bien été envoyé")
                    {
                        robot.DeconnexionRobot();
                    }
                }
                else
                {
                    Notifie(result_connexion);
                }

                //Partie Transmission API
                
                var result_api = api.EnvoiDonnéesCarte(list_donnees_api);
                Notifie(result_api.Result.ToString());
                
            }
        }

        /// <summary>
        /// Affiche une fenetre contenant le message passé en paramètre
        /// </summary>
        /// <param name="message">le message a affiché</param>
        public void Notifie(string message)
        {
            MessageBox.Show(message);
        }

        /// <summary>
        /// Stocke une zone dans GestionDonnées
        /// </summary>
        /// <param name="zone">la zone à stocké</param>
        public void SetZone(GMapPolygon zone)
        {
            temp_list_zone.Add(zone);
        }

        /// <summary>
        /// Recupere toutes les zones stocké dans GestionDonnées
        /// </summary>
        /// <returns>une liste des zones stocké dans GestionDonnées</returns>
        public List<GMapPolygon> GetZone()
        {
            return temp_list_zone;
        }

        /// <summary>
        /// Recupere la zone stocké a l'index passé en paramètre dans GestionDonnées
        /// </summary>
        /// <param name="index">index de la zone stocké</param>
        /// <returns>la zone stocké a l'emplacement <paramref name="index"/></returns>
        public GMapPolygon GetZone(int index)
        {
            return temp_list_zone[index];
        }

        public void SetPointMesure(PointLatLng new_point)
        {
            temp_list_point_mesure.Add(new_point);
        }

        /// <summary>
        /// Efface toutes les zones stocker dans GestionDonnées
        /// </summary>
        public void ClearZone()
        {
            temp_list_zone.Clear();

        }

        /// <summary>
        /// Verifie si le point passé en paramètre est dans la zone passé en paramètre
        /// </summary>
        /// <param name="zone">zone a testé</param>
        /// <param name="point">point a testé</param>
        /// <returns>true si <paramref name="point"/> est dans <paramref name="zone"/></returns>
        private bool IsOnTheZone(GMapPolygon zone, PointLatLng point)
        {
            bool is_valide = false;
            List<System.Windows.Point> list_XY = new List<System.Windows.Point>();
            System.Windows.Point point_mesure = new System.Windows.Point();

            //Conversion des sommets des zones en point de coordonnées (x, y) 
            foreach (PointLatLng z_point in zone.Points)
            {
                list_XY.Add(LatLngToPoint(z_point));
            }

            //Conversion du point de mesure de coordonnées (lat, long) en point de coordonnées (x, y)
            point_mesure = LatLngToPoint(point);

            //Creation des vecteurs
            System.Windows.Point AB = new System.Windows.Point(list_XY[0].X - list_XY[1].X, list_XY[0].Y - list_XY[1].Y);
            System.Windows.Point AD = new System.Windows.Point(list_XY[2].X - list_XY[1].X, list_XY[2].Y - list_XY[1].Y);
            System.Windows.Point AP = new System.Windows.Point(point_mesure.X - list_XY[1].X, point_mesure.Y - list_XY[1].Y);

            //Calcule des produit scalaire
            double dotABAP = AP.X * AB.X + AP.Y * AB.Y;
            double dotADAP = AP.X * AD.X + AP.Y * AD.Y;

            double dotABAB = AB.X * AB.X + AB.Y * AB.Y;
            double dotADAD = AD.X * AD.X + AD.Y * AD.Y;

            //verification de la validité du point (Par produit scalaire)
            if (dotABAP >= 0 && dotABAP <= dotABAB && dotADAP >= 0 && dotADAP <= dotADAD)
            {
                is_valide = true;
            }

            return is_valide;
        }

        /// <summary>
        /// projection d'un point de coordonnées (lat, long) sur un plan 2D en utilisant la projection Mercator
        /// </summary>
        /// <param name="point">point à convertir</param>
        /// <returns>les coordonnées du point projeté</returns>
        public System.Windows.Point LatLngToPoint(PointLatLng point)
        {
            System.Windows.Point local_point = new System.Windows.Point();
            double map_height = 800;
            double map_width = 450;

            //Conversion de la latitude et de la longitude en radiant
            double lat_rad = point.Lat * Math.PI / 180;
            double long_rad = point.Lng * Math.PI / 180;

            //Projection Mercator//
            local_point.X = map_width * ((long_rad + Math.PI) / (2 * Math.PI));
            local_point.Y = map_height / 2 - map_width / (2 * Math.PI) * Math.Log(Math.Tan(Math.PI / 4 + lat_rad / 2));

            return local_point;
        }

        /// <summary>
        /// projection d'une liste de point de coordonnées (lat, long) sur un plan 2D en utilisant la projection Mercator
        /// </summary>
        /// <param name="points">liste de point à convertir</param>
        /// <returns>les coordonnées des points projetées</returns>
        public List<System.Windows.Point> LatLngToPoint(List<PointLatLng> points)
        {
            List<System.Windows.Point> list_point = new List<System.Windows.Point>();
            double map_height = 800;
            double map_width = 450;

            foreach (PointLatLng point in points)
            {
                var local_point = new System.Windows.Point();
                //Conversion de la latitude et de la longitude en radiant
                double lat_rad = point.Lat * Math.PI / 180;
                double long_rad = point.Lng * Math.PI / 180;

                //Projection Mercator//
                local_point.X = map_width * ((long_rad + Math.PI) / (2 * Math.PI));
                local_point.Y = map_height / 2 - map_width / (2 * Math.PI) * Math.Log(Math.Tan(Math.PI / 4 + lat_rad / 2));
                list_point.Add(local_point);
            }
            return list_point;
        }

        private List<Vector2> ConvertSommetsToVector2(List<PointLatLng> points)
        {
            List<Vector2> sommets = new List<Vector2>();
            foreach (PointLatLng point in points)
            {
                var p = LatLngToPoint(point);
                sommets.Add(new Vector2(Convert.ToSingle(p.X), Convert.ToSingle(p.Y)));
            }
            return sommets;
        }

        /// <summary>
        /// Methode convertissant chaque arete de la zone passé en paramètre en Vector2        
        ///</summary>
        /// <param name="zone">zone pour laquelle on veut récuperer les aretes</param>
        /// <returns>une liste contenant toutes les aretes</returns>
        private List<Vector2> RecuperationAreteZone(GMapPolygon zone)
        {
            List<Vector2> aretes = new List<Vector2>();

            //Creation aretes
            for (int i = 0; i < zone.Points.Count; i++)
            {
                var P1 = LatLngToPoint(zone.Points[i]);
                var P2 = LatLngToPoint(zone.Points[(i + 1) % zone.Points.Count]); // prend le reste de la division euclidienne (i+1)/count : permet d P4 - P1

                Vector2 arete = new Vector2(Convert.ToSingle(P2.X - P1.X), Convert.ToSingle(P2.Y - P1.Y));
                aretes.Add(arete);
            }

            return aretes;
        }

        /// <summary>
        /// Projete les sommets passé en paramètre sur l'axe passé en paramètre en faisant un produit scalaire
        /// </summary>
        /// <param name="sommets">liste des sommets que l'on veut projeté sur l'axe</param>
        /// <param name="axe">l'axe de projection</param>
        /// <returns>une listes contenant les resultats de chaque produit scalaire</returns>
        private List<float> ProjectionSommetsAxe(List<Vector2> sommets, Vector2 axe)
        {
            List<float> resultat = new List<float>();
            foreach (Vector2 sommet in sommets)
            {
                resultat.Add(Vector2.Dot(sommet, axe));

            }
            return resultat;
        }

        //Faire les normales par rapport aux aretes de la zone//
        /// <summary>
        /// Créer des normales par rapport au arete passé en paramètre
        /// </summary>
        /// <param name="aretes">liste des arete pour lesquelles on veut créer des normales</param>
        /// <returns>une listes contenant les normales de chaque aretes</returns>
        private List<Vector2> CreationNormales(List<Vector2> aretes)
        {
            List<Vector2> normales = new List<Vector2>();

            foreach (Vector2 arete in aretes)
            {
                Vector2 normale = new Vector2(-arete.Y, arete.X);
                normales.Add(normale);
            }
            return normales;
        }

        /// <summary>
        /// verifie si deux zone se superpose en utilisant le theorême de l'axe separé (SAT) 
        /// </summary>
        /// <param name="previous_zone">premiere zone a testé</param>
        /// <param name="zone">deuxième zone a testé</param>
        /// <returns>true si il existe une superposition entre <paramref name="previous_zone"/> et <paramref name="zone"/></returns>
        private bool VerificationSuperpositionZone(GMapPolygon previous_zone, GMapPolygon zone)
        {
            bool is_valide = true;
            List<Vector2> sommets_previous_zone = new List<Vector2>();
            List<Vector2> sommets_zone = new List<Vector2>();
            List<Vector2> aretes_previous_zone = new List<Vector2>();
            List<Vector2> aretes_zone = new List<Vector2>();
            List<float> resultat_previous_zone = new List<float>();
            List<float> resultat_zone = new List<float>();
            List<Vector2> normale_previous_zone = new List<Vector2>();
            List<Vector2> normale_zone = new List<Vector2>();

            //Récuperation des sommets des zones
            sommets_previous_zone = ConvertSommetsToVector2(previous_zone.Points);
            sommets_zone = ConvertSommetsToVector2(zone.Points);

            //Convertissement des aretes des deux polygon en vecteurs
            aretes_previous_zone = RecuperationAreteZone(previous_zone);
            aretes_zone = RecuperationAreteZone(zone);

            //Calcule des normales de chaque aretes
            normale_previous_zone = CreationNormales(aretes_previous_zone);
            normale_zone = CreationNormales(aretes_zone);

            //Projection des sommets sur les normales de chaque aretes
            foreach (Vector2 normale in normale_previous_zone)
            {
                resultat_previous_zone = ProjectionSommetsAxe(sommets_previous_zone, normale);
                resultat_zone = ProjectionSommetsAxe(sommets_zone, normale);

                //aucune collision si le max de la première zone est inferieur au min de la deuxième et inversement
                if (resultat_previous_zone.Max() < resultat_zone.Min() || resultat_zone.Max() < resultat_previous_zone.Min())
                {
                    is_valide = false;
                    break;
                }

            }

            if (is_valide == true)
            {
                //même chose qu'avant mais pour l'autre zone
                foreach (Vector2 normale in normale_zone)
                {
                    resultat_previous_zone = ProjectionSommetsAxe(sommets_previous_zone, normale);
                    resultat_zone = ProjectionSommetsAxe(sommets_zone, normale);

                    if (resultat_previous_zone.Max() < resultat_zone.Min() || resultat_zone.Max() < resultat_previous_zone.Min())
                    {
                        is_valide = false;
                        break;
                    }
                }
            }
            return is_valide;
        }
    }
}