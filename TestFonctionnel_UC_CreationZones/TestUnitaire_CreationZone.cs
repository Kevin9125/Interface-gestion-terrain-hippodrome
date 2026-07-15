using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows;
using ProjetHippodrome;
using GMap.NET;
using System.Collections.Generic;
using GMap.NET.WindowsPresentation;

namespace TestFonctionnel_UC_CreationZones
{
    [TestClass]
    public class TestUnitaire_CreationZone
    {
        //Constante
        IhmCreationZone application = new IhmCreationZone();
        List<System.Windows.Point> points = new List<System.Windows.Point>();

        //Cas valide : zone lambda
        [TestMethod]
        public void TC_01()
        {
            points.Clear();
            points.AddRange(new List<Point> { new Point(400, 500), new Point(600, 800), new Point(200, 300), new Point(0, 100) });
            Assert.AreEqual(typeof(GMapPolygon), application.CreationZones(points).GetType());
        }

        //Cas limite : zone qui fait la taille de l'écran
        [TestMethod]
        public void TC_02()
        {
            points.Clear();
            points.AddRange(new List<Point> { new Point(SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight), new Point(0, SystemParameters.PrimaryScreenHeight), new Point(0, 0), new Point(SystemParameters.PrimaryScreenWidth, 0) });
            Assert.AreEqual(typeof(GMapPolygon), application.CreationZones(points).GetType());
        }

        //Cas limite 2 : une zone qui fait la moitié de l'ecran 
        [TestMethod]
        public void TC_03()
        {
            points.Clear();
            points.AddRange(new List<Point> { new Point(SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight), new Point(0, SystemParameters.PrimaryScreenHeight), new Point(0, 544), new Point(SystemParameters.PrimaryScreenWidth, 544) });
            Assert.AreEqual(typeof(GMapPolygon), application.CreationZones(points).GetType());
        }

        //Cas Invalide : la zone contient une coordonnées négative
        [TestMethod]
        public void TC_04()
        {
            points.Clear();
            points.AddRange(new List<Point> { new Point(-400, 500), new Point(600, 800), new Point(200, 300), new Point(0, 100) });
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => application.CreationZones(points));
        }

        //Cas Invalide : une zone hors limite
        [TestMethod]
        public void TC_05()
        {
            points.Clear();
            points.AddRange(new List<Point> { new Point(3000, 2800), new Point(3000, 2500), new Point(2500, 2500), new Point(2500, 2800) });
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => application.CreationZones(points));
        }

        //Cas Invalide : une zone dont la moitier est hors limite
        [TestMethod]
        public void TC_06()
        {
            points.Clear();
            points.AddRange(new List<Point> { new Point(1920, 2800), new Point(1920, 550), new Point(0, 550), new Point(0, 2800) });
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => application.CreationZones(points));
        }

        //Cas Invalide : 3 point lambda
        [TestMethod]
        public void TC_07()
        {
            points.Clear();
            points.AddRange(new List<Point> { new Point(400, 500), new Point(600, 800), new Point(200, 300) });
            Assert.ThrowsException<ArgumentException>(() => application.CreationZones(points));
        }

        //Cas valide : 5 points lambda
        [TestMethod]
        public void TC_08()
        {
            points.Clear();
            points.AddRange(new List<Point> { new Point(400, 500), new Point(600, 800), new Point(200, 300), new Point(0, 100), new Point(850, 100) });
            Assert.AreEqual(typeof(GMapPolygon), application.CreationZones(points).GetType());
        }

        [TestMethod]
        //Cas Invalide : deux point superposé
        public void TC_09()
        {
            points.Clear();
            points.AddRange(new List<Point> { new Point(400, 500), new Point(400, 500), new Point(200, 300), new Point(0, 100) });
            Assert.ThrowsException<ArgumentException>(() => application.CreationZones(points));
        }

        //Cas vide : Une liste vide
        [TestMethod]
        public void TC_10()
        {
            points.Clear();
            Assert.ThrowsException<ArgumentException>(() => application.CreationZones(points));
        }
    }
}
