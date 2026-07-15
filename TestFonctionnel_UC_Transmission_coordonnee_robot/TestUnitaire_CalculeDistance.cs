using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using GMap.NET;
using ProjetHippodrome;

namespace TestFonctionnel_UC_Transmission_coordonnee_robot
{
    [TestClass]
    public class TestUnitaire_CalculeDistance
    {
        VerificationDonnees donnees = new VerificationDonnees();

        //Cas Valide : deux valeur lambda
        [TestMethod]
        public void TC_01()
        {
            PointLatLng depart = new PointLatLng(47.576, -0.5145);
            PointLatLng destination = new PointLatLng(47.678, -0.5234);
            Assert.AreEqual(1136147.638, Math.Round(donnees.CalculeDistance(depart, destination)), 3);
        }

        //Cas valide : deux valeur identique
        [TestMethod]
        public void TC_02()
        {
            PointLatLng depart = new PointLatLng(47.576, -0.5145);
            PointLatLng destination = new PointLatLng(47.576, -0.5145);
            Assert.AreEqual(0, Math.Round(donnees.CalculeDistance(depart, destination)), 3);

        }

        //Cas valide : test traversé de l'anti-meridien
        [TestMethod]
        public void TC_03()
        {
            PointLatLng depart = new PointLatLng(10, 179);
            PointLatLng destination = new PointLatLng(10, -179);
            Assert.AreEqual(21901091.678, Math.Round(donnees.CalculeDistance(depart, destination)), 3);

        }

        //Cas Valide : Inverse TC_01
        [TestMethod]
        public void TC_04()
        {
            PointLatLng depart = new PointLatLng(47.678, -0.5234);
            PointLatLng destination = new PointLatLng(47.576, -0.5145);
            Assert.AreEqual(1136147.638, Math.Round(donnees.CalculeDistance(depart, destination)), 3);
        }
    }
}
