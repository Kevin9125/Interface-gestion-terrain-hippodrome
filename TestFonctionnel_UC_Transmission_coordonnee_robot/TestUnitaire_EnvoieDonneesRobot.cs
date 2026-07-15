using GMap.NET;
using ProjetHippodrome;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System;

namespace TestFonctionnel_UC_Transmission_coordonnee_robot
{
    [TestClass]
    public class TestUnitaire_EnvoieDonneesRobot
    {
        Robot robot = new Robot("192.168.10.3");
        List<DonnéesRobot> list_données_robot = new List<DonnéesRobot>();

        DonnéesRobot données_robot = new DonnéesRobot();
        DonnéesRobot donnees_robot2 = new DonnéesRobot();

        /// <summary>
        /// Cas valide : point lambda
        /// </summary>
        [TestMethod]
        public void TC_01()
        {
            list_données_robot.Clear();
            données_robot.etat = 0;
            données_robot.point_mesure = new PointLatLng(47.543, -0.5124);
            données_robot.distance = 123;

            list_données_robot.Add(données_robot);
            robot.ConnexionRobot();
            Assert.AreEqual("Les points de mesures ont bien été envoyé", robot.EnvoiePointDeMesure(list_données_robot));
            robot.DeconnexionRobot();
        }

        
        /// <summary>
        /// Cas Invalide : point de mesure vide
        /// </summary>
        [TestMethod]
        public void TC_02()
        {
            list_données_robot.Clear();

            données_robot.etat = 0;
            données_robot.point_mesure = new PointLatLng();
            données_robot.distance = 123;

            list_données_robot.Add(données_robot);
            robot.ConnexionRobot();
            Assert.ThrowsException<ArgumentNullException>(() => robot.EnvoiePointDeMesure(list_données_robot));
            robot.DeconnexionRobot();
        }

        /// <summary>
        /// Cas Invalide : Liste Vide
        /// </summary>
        [TestMethod]
        public void TC_03()
        {
            list_données_robot.Clear();
            
            robot.ConnexionRobot();
            Assert.ThrowsException<ArgumentNullException>(() => robot.EnvoiePointDeMesure(list_données_robot));
            robot.DeconnexionRobot();
        }

        /// <summary>
        /// Cas valide : deux point lambda
        /// </summary>
        [TestMethod]
        public void TC_04()
        {
            list_données_robot.Clear();

            données_robot.etat = 0;
            données_robot.point_mesure = new PointLatLng(47.543, -0.5124);
            données_robot.distance = 123;

            donnees_robot2 = new DonnéesRobot
            {
                point_mesure = new PointLatLng(48.543, -0.4951),
                etat = 0,
                distance = 234
            };

            list_données_robot.Add(données_robot);
            list_données_robot.Add(donnees_robot2);
            robot.ConnexionRobot();
            Assert.AreEqual("Les points de mesures ont bien été envoyé", robot.EnvoiePointDeMesure(list_données_robot));
            robot.DeconnexionRobot();
        }
    }
}
