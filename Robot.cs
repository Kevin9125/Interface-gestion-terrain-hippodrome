using GMap.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ProjetHippodrome
{
    public class Robot
    {
        private string m_adresse = "";
        private TcpClient client = new TcpClient();
        private NetworkStream stream;

        public Robot(string adresse)
        {
            m_adresse = adresse;
        }

        /// <summary>
        /// Methode permettant de ce connecter au robot via une connexion TCP
        /// </summary>
        /// <param name="adresse">adresse IP du robot</param>
        /// <returns>le resultat de la connexion sous forme de string</returns>
        /// <exception cref="SocketException">Se lève quand une erreur ces produit lors de la connexion au robot </exception>
        public string ConnexionRobot()
        {
            string resultat = "Echec de la communication";

            //Se connecte au serveur
            if (client.Connected == false)
            {
                client.Connect(m_adresse, 80);//Connection sur le serveur
                stream = client.GetStream();//Flux ou l'on va envoyé les données
                resultat = "connection au serveur reussit";
            }
            else
            {
                throw new SocketException(1);//???
            }

            return resultat;
        }

        /// <summary>
        /// Methode permettant de fermé le socket reliant le robot à k'application 
        /// </summary>
        public void DeconnexionRobot()
        {
            stream.Flush();
            stream.Close();
            client.Close();
        }

        /// <summary>
        /// Methode permettant l'envoie des points de mesure et des points de trajectoire au robot 
        /// </summary>
        /// <param name="données_robot">objet contenant un point, l'etat du point (si c'est un point de trajectoire ou de mesure) et la distance avec le point precedent dans la liste (calculé au préalable)s</param>
        /// <returns>le resultat de l'envoie des données au robot sous forme de string</returns>
        /// <exception cref="ArgumentNullException">se leve quand un des membres de la structure DonnéesROBOT</exception>
        public string EnvoiePointDeMesure(List<DonnéesRobot> données_robot)
        {
            string resultat = "Echec de l'envoie des données";
            byte[] message = new byte[0];

            if (données_robot.Count > 0)
            {
                //convertissement des données en bit
                for (int i = 0; i < données_robot.Count; i++)
                {
                    if (données_robot[i].point_mesure == null)
                    {
                        throw new ArgumentNullException($"l'un des membres de la structure DonnéesCarte n ° {i}");
                    }

                    else if (données_robot[i].point_mesure.IsEmpty)
                    {
                        throw new ArgumentNullException($"le point numéro {i} est vide");
                    }

                    else
                    {
                        var int_lat = Math.Round(données_robot[i].point_mesure.Lat, 5) * 100000;
                        var int_lng = Math.Round(données_robot[i].point_mesure.Lng, 5) * 100000;

                        //Convertie les int en 4 octets
                        var byte_lat = BitConverter.GetBytes((int)int_lat);
                        var byte_lng = BitConverter.GetBytes((int)int_lng);
                        var byte_distance = BitConverter.GetBytes((short)données_robot[i].distance);

                        //copie des octets dans message
                        var byte_message = byte_lat.Concat(byte_lng).ToArray().Concat(byte_distance).ToArray();
                        message = message.Concat(byte_message).ToArray();
                    }

                }

                //ajout du count a la fin 
                var byte_count = BitConverter.GetBytes((short)données_robot.Count);
                message = message.Concat(byte_count).ToArray();

                //Pour testé
                //message = Encoding.ASCII.GetBytes("forward");

                //Envoie du message
                if (stream.CanWrite == true)
                {
                    //Une fois que l'on aura fait le calcule de traj mettre la distance entre les points avec la latitude et la longitude (comme dit dans le doc)
                    stream.Write(message, 0, message.Length);
                    resultat = "Les points de mesures ont bien été envoyé";
                }

            }

            else
            {
                throw new ArgumentNullException("La liste est vide");
            }

            return resultat;
        }
    }
    public struct DonnéesRobot
    {
        public PointLatLng point_mesure { get; set; }
        public byte etat { get; set; }
        public int distance { get; set; }//distance a partir du point precedent (0 si c'est le point de depart)

    }
}
