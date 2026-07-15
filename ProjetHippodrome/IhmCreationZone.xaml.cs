using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
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
using System.Windows.Shapes;

namespace ProjetHippodrome
{
    /// <summary>
    /// Logique d'interaction pour Window2.xaml
    /// </summary>
    public partial class IhmCreationZone: Window
    {
        //Composition GestionDonnees - Ihm//
        private readonly VerificationDonnees gestion_data = new VerificationDonnees();
        private List<PointLatLng> listeGMapPoints = new List<PointLatLng>();
        private System.Windows.Point position;
        private readonly List<System.Windows.Point> points_px = new List<System.Windows.Point>();
        //IHMs Menu = new IHMs();
        public IhmCreationZone()
        {
            InitializeComponent();

            GMapProvider.UserAgent = "IHMCreationZone/1.0 (contact : kevin.belaire@gmail.com)";

            GMaps.Instance.Mode = AccessMode.ServerAndCache;

            MyGMap.MapProvider = OpenStreetMapProvider.Instance;
        }

        /// <summary>
        /// Creer les zones avec les points que l'on lui fournit
        /// </summary>
        /// <param name="Zpoints">liste des sommets de la zones à créer</param>
        /// <returns>la zone créer</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public GMapPolygon CreationZones(List<System.Windows.Point> Zpoints)
        {
            GMapPolygon zone;
            List<PointLatLng> points = new List<PointLatLng>();

            //verification des points passé en paramètre
            if (Zpoints.Count < 4)
            {
                throw new ArgumentException("4 points doivent être fournit pour créer une zones");
            }

            else
            {
                for (int i = 0; i < Zpoints.Count; i++)
                {
                    //verifie si les points fournit ne sont pas vide
                    if (Zpoints[i] == null)
                    {
                        throw new ArgumentException($"le point numero {i} est null ou vide ");
                    }

                    else if (Zpoints[i].X < 0 || Zpoints[i].Y < 0 || Zpoints[i].X > SystemParameters.PrimaryScreenWidth || Zpoints[i].Y > SystemParameters.PrimaryScreenHeight)
                    {
                        throw new ArgumentOutOfRangeException($"le point numero {i} est dans le mauvais format");
                    }

                    else if (i > 0)
                    {
                        if (Zpoints[i - 1] == Zpoints[i])
                        {
                            throw new ArgumentException($"Les points numéro {i - 1} et {i} sont identique");
                        }
                    }
                }
            }

            //si les points sont valide ont les enregistre temporairement
            for (int i = 0; i < 4; i++)
            {
                points.Add(MyGMap.FromLocalToLatLng((int)Zpoints[i].X, (int)Zpoints[i].Y));
            }

            //on créer la zone avec les points les plus proche des point configurer sur la piste

            //zone = new GMapPolygon(ApproximationZone(points));//version collé a la bordure
            zone = new GMapPolygon(points);//version pas collé a la bordure
            zone.Shape = new Rectangle
            {
                Fill = new SolidColorBrush(Colors.Red),
                Stroke = new SolidColorBrush(Colors.Black)
            };
            MyGMap.Markers.Add(zone);
            points.Clear();
            return zone;
        }

        private void MyGMap_Loaded(object sender, RoutedEventArgs e)
        {
            string path = "..\\..\\..\\pistes_hippodrome\\petite_piste_hippodrome.geojson";

            //recuperer les infos dans le fichier GeoJSON
            string data = System.IO.File.ReadAllText(path);

            //declaration d'un GeoJsonReader permettant de transformer les données en objet geometrique
            NetTopologySuite.IO.GeoJsonReader reader = new NetTopologySuite.IO.GeoJsonReader();

            //Lecture, convertissement des données en objet geometrique (GeoJsonReader) et rangement de ces figure dans un FeatureCollection
            NetTopologySuite.Features.FeatureCollection collection = reader.Read<NetTopologySuite.Features.FeatureCollection>(data);

            //Une feature Collection va stocker les Coordonnées dans une Feature ce qui va permettre de pourra par exemple les nommé (par rapport a leur position ou pour les identifier) et
            //une feature collection est une collection de feature

            //On boucle sur chaque Feature (chaque objet du GeoJSON)(boucle au cas ou on rajoute des features)
            foreach (var feature in collection)
            {
                // On récupère la géométrie (la ligne)
                var geometrie = feature.Geometry;

                //boucle adaptant toutes les coordonnée de la feature au format de GMap.net
                foreach (var coord in geometrie.Coordinates)
                {
                    // GeoJSON utilise [Longitude (X), Latitude (Y)]
                    // GMap.NET utilise [Latitude, Longitude]

                    PointLatLng point = new PointLatLng(coord.Y, coord.X);

                    //On ajoute le point converti à notre liste
                    listeGMapPoints.Add(point);
                }
            }

            // Créer la route à partir de la liste de points
            GMapRoute laPiste = new GMapRoute(listeGMapPoints);

            // Personnaliser l'apparence en WPF
            laPiste.Shape = new System.Windows.Shapes.Path
            {
                Stroke = Brushes.Blue,
                StrokeThickness = 3,
            };

            //Ajoute la piste sur la carte
            MyGMap.Markers.Add(laPiste);

            //placement de la caméra au centre de l'hippodrome
            double centre_y = 47.497887;
            double centre_x = -0.507892;

            MyGMap.Position = new PointLatLng(centre_y, centre_x);

            //zoome sur le point selectionner au dessus
            MyGMap.Zoom = 16;

        }

        //methode appellé lorsque l'on enclenche le clic gauche sur la carte
        private void MyGMap_MouseDown(object sender, MouseButtonEventArgs e)
        {
            position = e.GetPosition(MyGMap);
        }

        //methode appelé lorsque l'on relache le clic gauche sur la carte
        private void MyGMap_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //si la souris n'as pas trop bouger entre la position ou on a appuyé sur le clic gauche et la position ou on la laché : créer un point et l'affiche sur la carte
            if (Math.Abs(position.X - e.GetPosition(MyGMap).X) < SystemParameters.MinimumHorizontalDragDistance && Math.Abs(position.Y - e.GetPosition(MyGMap).Y) < SystemParameters.MinimumVerticalDragDistance)
            {
                points_px.Add(position);

                GMapMarker marker = new GMapMarker(MyGMap.FromLocalToLatLng((int)position.X, (int)position.Y));

                if (points_px.Count < 5)
                {
                    marker.Shape = new Ellipse
                    {
                        Width = 5,
                        Height = 5,
                        Stroke = Brushes.Red,
                        Fill = new SolidColorBrush(Colors.Red)
                    };

                    //si 4 points ont été configurer on créer une zone
                    if (points_px.Count == 4)
                    {
                        var zone = CreationZones(points_px);
                        if (gestion_data.ValidationZones(zone) == false)
                        {
                            gestion_data.Notifie("format zone invalide");
                        }
                    }
                }
                //le 5ème point sera le point de mesure
                else if (points_px.Count == 5)
                {
                    marker.Shape = new Ellipse
                    {
                        Width = 5,
                        Height = 5,
                        Stroke = Brushes.Blue,
                        Fill = new SolidColorBrush(Colors.Blue)
                    };

                    if (gestion_data.ValidationPointMesure(MyGMap.FromLocalToLatLng((int)points_px[4].X, (int)points_px[4].Y)) == false)
                    {
                        gestion_data.Notifie("format point de mesure invalide");
                    }

                    else
                    {
                        points_px.Clear();
                    }

                }

                //ajout du point sur la carte
                MyGMap.Markers.Add(marker);
            }
        }

        private void BEnregistrer_Click(object sender, RoutedEventArgs e)
        {
            gestion_data.EnregistrementConfiguration();
        }

        private void BSupprimer_Click(object sender, RoutedEventArgs e)
        {
            //On passe 5 fois dans MyGMap.Markers car il ne peux enregistrer que 5 markers a la fois donc je ne peux en supprimer que 5 a la fois
            for (int j = 0; j < 5; j++)
            {
                for (int i = 1; i < MyGMap.Markers.Count; i++)
                {
                    if (MyGMap.Markers[i] != null)
                    {
                        MyGMap.Markers.RemoveAt(i);
                    }
                }
            }
        }
        private void BRetourMenu_Click(object sender, RoutedEventArgs e)
        {
            //this.Close();
            //Menu.Show();
        }
    }
}
