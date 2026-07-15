using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using GMap.NET;
using ProjetHippodrome;
using System.Net;
using System.Net.Sockets;

namespace TestFonctionnel_UC_Transmission_coordonnee_robot
{
    [TestClass]
    public class TestUnitaire_EnvoieDonneesCarte
    {
        //Après avoir fait le test enlevé point de messure = null 
        List<PointLatLng> points = new List<PointLatLng>();

        DonnéesAPI donnees = new DonnéesAPI();

        API api = new API("http://192.168.10.20:7063/api/zones");
        //API api = new API("https://postman-echo.com/post");

        /// <summary>
        /// Cas Valide : envoie classique
        /// </summary>
        [TestMethod]
        public void TC_01()
        {
            donnees.id_zone = 11;

            donnees.lat_point = 47.543;
            donnees.lng_point = -0.5143;

            Assert.AreEqual(HttpStatusCode.Created, api.EnvoiDonnéesCarte(new List<DonnéesAPI> { donnees }).Result.StatusCode);
        }

        /// <summary>
        /// Cas Invalide : zone null
        /// </summary>
        [TestMethod]
        public void TC_02()
        {
            donnees.lat_point = 47.543;
            donnees.lng_point = -0.5143;
            Assert.ThrowsExceptionAsync<ArgumentNullException>(() => api.EnvoiDonnéesCarte(new List<DonnéesAPI> { donnees }));
        }


        /// <summary>
        /// Cas invalide : coordonnées null
        /// </summary>
        [TestMethod]
        public void TC_04()
        {
            donnees.id_zone = 11;

            Assert.ThrowsExceptionAsync<ArgumentNullException>(() => api.EnvoiDonnéesCarte(new List<DonnéesAPI> { donnees }));
        }

        /// <summary>
        /// Cas Valide : plusieurs zones et plusieurs point de mesure
        /// </summary>
        [TestMethod]
        public void TC_05()
        {
            points.AddRange(new List<PointLatLng> { new PointLatLng(47.543, -0.5124), new PointLatLng(47.643, -0.5124), new PointLatLng(47.743, -0.5124), new PointLatLng(47.843, -0.5124) });
            donnees.id_zone = 11;
            donnees.lat_point = 47.543;
            donnees.lng_point = -0.5143;

            List<PointLatLng> points2 = new List<PointLatLng> { new PointLatLng(47.543, -0.5124), new PointLatLng(47.643, -0.5124), new PointLatLng(47.743, -0.5124), new PointLatLng(47.843, -0.5124) };

            DonnéesAPI données2 = new DonnéesAPI
            {
                id_zone = 12,
                lat_point = 48.543,
                lng_point = 0.5143
            };

            Assert.AreEqual(HttpStatusCode.Created, api.EnvoiDonnéesCarte(new List<DonnéesAPI> { donnees, données2 }).Result.StatusCode);
        }

        /// <summary>
        /// Cas Invalide : Mauvaise route
        /// </summary>
        [TestMethod]
        public void TC_06()
        {
            API api2 = new API("http://192.168.10.20:7063/api/fausseroute");
            donnees.id_zone = 11;
            donnees.lat_point = 47.543;
            donnees.lng_point = -0.5143;

            Assert.AreEqual(HttpStatusCode.NotFound, api2.EnvoiDonnéesCarte(new List<DonnéesAPI> { donnees }).Result.StatusCode);
        }

        /// <summary>
        /// Cas Invalide : Base de données hors service
        /// </summary>
        [TestMethod]
        public void TC_07()
        {
            donnees.id_zone = 11;
            donnees.lat_point = 47.543;
            donnees.lng_point = -0.5143;

            Assert.AreEqual(HttpStatusCode.ServiceUnavailable, api.EnvoiDonnéesCarte(new List<DonnéesAPI> { }).Result.StatusCode);
        }

        /// <summary>
        /// Cas Invalide : Envoie d'une liste vide
        /// </summary>
        [TestMethod]
        public void TC_08()
        {
            donnees.id_zone = 11;
            donnees.lat_point = 47.543;
            donnees.lng_point = -0.5143;

            Assert.AreEqual(HttpStatusCode.BadRequest, api.EnvoiDonnéesCarte(new List<DonnéesAPI> { donnees }).Result.StatusCode);
        }
    }
}
