using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using ProjetHippodrome;
using GMap.NET;
using System.Windows.Documents;
using GMap.NET.WindowsPresentation;
using System.Collections.Generic;

namespace TestFonctionnel_UC_CreationZones
{
    [TestClass]
    public class TestUnitaire_ValidationZones
    {
        //Objet testé : 
        VerificationDonnees gestion_data = new VerificationDonnees();

        //Liste contenant les vecteurs de test
        GMapPolygon zone;

        //Cas Valide : un polygone lambda
        [TestMethod]
        public void TC_01()
        {

            List<PointLatLng> zpoints = new List<PointLatLng> { new PointLatLng(47.556, -0.567), new PointLatLng(47.556, -0.589), new PointLatLng(47.890, -0.543), new PointLatLng(47.345, -0.5789) };
            zone = new GMapPolygon(zpoints);
            Assert.IsTrue(gestion_data.ValidationZones(zone));
        }

        //Cas au limite : un point avec les coordonnées limite positive
        [TestMethod]
        public void TC_02()
        {
            List<PointLatLng> zpoints = new List<PointLatLng> { new PointLatLng(90, 180), new PointLatLng(47.556, -0.589), new PointLatLng(47.890, -0.543), new PointLatLng(47.345, -0.5789) };
            zone = new GMapPolygon(zpoints);
            Assert.IsTrue(gestion_data.ValidationZones(zone));
        }

        //Cas valide : cas (0;0)
        [TestMethod]
        public void TC_03()
        {

            List<PointLatLng> zpoints = new List<PointLatLng> { new PointLatLng(0, 0), new PointLatLng(47.556, -0.589), new PointLatLng(47.890, -0.543), new PointLatLng(47.345, -0.5789) };
            zone = new GMapPolygon(zpoints);
            Assert.IsTrue(gestion_data.ValidationZones(zone));
        }


        //Cas Invalide : point hors limite
        [TestMethod]
        public void TC_04()
        {

            List<PointLatLng> zpoints = new List<PointLatLng> { new PointLatLng(1000.890, 2000.768), new PointLatLng(47.556, -0.589), new PointLatLng(47.890, -0.543), new PointLatLng(47.345, -0.5789) };
            zone = new GMapPolygon(zpoints);
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => gestion_data.ValidationZones(zone));
        }

        //?//
        //Cas null : un polygone avec une coordonnées vide
        [TestMethod]
        public void TC_05()
        {

            List<PointLatLng> zpoints = new List<PointLatLng> { new PointLatLng(), new PointLatLng(47.556, -0.589), new PointLatLng(47.890, -0.543), new PointLatLng(47.345, -0.5789) };
            zone = new GMapPolygon(zpoints);
            Assert.ThrowsException<ArgumentException>(() => gestion_data.ValidationZones(zone));
        }
    }
}
