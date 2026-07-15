using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using ProjetHippodrome;
using GMap.NET;
using GMap.NET.WindowsPresentation;
using System.Collections.Generic;

namespace TestFonctionnel_UC_CreationZones
{
    [TestClass]
    public class TestUnitaire_ValidationPointDeMesure
    {
        //Objet testé :
        VerificationDonnees gestion_data = new VerificationDonnees();

        //Cas Valide : un point lambda
        [TestMethod]
        public void TC_01()
        {
            Assert.IsTrue(gestion_data.ValidationPointMesure(new PointLatLng(47.567, -0.5145)));
        }

        //Cas Limite : un point au limite positive
        [TestMethod]
        public void TC_02()
        {
            Assert.IsTrue(gestion_data.ValidationPointMesure(new PointLatLng(90, 180)));
        }

        //Cas Limite : un point au limite negative
        [TestMethod]
        public void TC_03()
        {
            Assert.IsTrue(gestion_data.ValidationPointMesure(new PointLatLng(-90, -180)));
        }

        //Cas Valide : un point dans la zone
        [TestMethod]
        public void TC_04()
        {
            //Initialisation de la zone lié au point de mesure
            List<PointLatLng> Zpoints = new List<PointLatLng> { new PointLatLng(47.515, -0.4569), new PointLatLng(48.443, -0.4569), new PointLatLng(48.443, -0.765), new PointLatLng(47.515, -0.765) };
            GMapPolygon zone = new GMapPolygon(Zpoints);
            gestion_data.SetZone(zone);

            //on ajoute la coordonnées au millieu de la zone comme point de mesure
            Assert.IsTrue(gestion_data.ValidationPointMesure(new PointLatLng((gestion_data.GetZone(0).Points[0].Lat + gestion_data.GetZone(0).Points[2].Lat) / 2, (gestion_data.GetZone(0).Points[0].Lng + gestion_data.GetZone(0).Points[2].Lng) / 2)));
            gestion_data.ClearZone();
        }

        //Cas vide : un point vide
        [TestMethod]
        public void TC_05()
        {
            Assert.ThrowsException<ArgumentException>(() => gestion_data.ValidationPointMesure(new PointLatLng()));
        }

        //Cas Invalide : deux point identique
        [TestMethod]
        public void TC_06()
        {
            gestion_data.SetPointMesure(new PointLatLng(47.567, -0.5145));
            Assert.ThrowsException<ArgumentException>(() => gestion_data.ValidationPointMesure(new PointLatLng(47.567, -0.5145)));
        }

        //Cas Invalide : un point pas dans la zone 
        [TestMethod]
        public void TC_07()
        {
            //Initialisation de la zone lié au point de mesure
            List<PointLatLng> Zpoints = new List<PointLatLng> { new PointLatLng(47.515, -0.4569), new PointLatLng(48.443, -0.4569), new PointLatLng(48.443, -0.765), new PointLatLng(47.515, -0.765) };
            GMapPolygon zone = new GMapPolygon(Zpoints);
            gestion_data.SetZone(zone);

            //on ajoute la coordonnées au millieu de la zone comme point de mesure
            Assert.ThrowsException<ArgumentException>(() => gestion_data.ValidationPointMesure(new PointLatLng((gestion_data.GetZone(0).Points[0].Lat - gestion_data.GetZone(0).Points[2].Lat), (gestion_data.GetZone(0).Points[0].Lng - gestion_data.GetZone(0).Points[2].Lng) / 2)));
            gestion_data.ClearZone();
        }
    }
}
