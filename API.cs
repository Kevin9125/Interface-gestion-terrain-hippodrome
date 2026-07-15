using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ProjetHippodrome
{
    public class API
    {
        private Uri uri;
        private HttpClient client_http = new HttpClient();

        public API(string adresse)
        {
            uri = new Uri(adresse);
        }

        /// <summary>
        /// Methode permettant d'envoyé les points de mesures et les zone a l'API
        /// </summary>
        /// <param name="données_API">objet contenant une zone avec son point de mesure associé</param>
        /// <returns>La reponse Http renvoué par l'API</returns>
        /// <exception cref="ArgumentNullException">se lève si l'un des membre des obket passé en paramètre est null ou si la liste est vide</exception>
        public async Task<HttpResponseMessage> EnvoiDonnéesCarte(List<DonnéesAPI> données_API)
        {
            //Configuration adresse API 
            client_http.BaseAddress = uri;
            client_http.Timeout = TimeSpan.FromSeconds(5); //Timeout de 5 secondes
            string Json = "";

            HttpResponseMessage reponse = new HttpResponseMessage();

            //Serialisation des coordonnées des zones
            for (var i = 0; i < données_API.Count; i++)
            {
                if (données_API.Count == 0 || données_API == null)
                {
                    throw new ArgumentNullException("la liste passé en paramètre est vide");
                }
            }

            Json = Json + JsonConvert.SerializeObject(données_API);

            //Envoie coordonnées serialisé des zones
            var content = new StringContent(Json.ToString());
            content.Headers.ContentType.MediaType = "application/json";
            reponse = await client_http.PostAsync(uri, content);//envoie la requete et attend d'avoir une reponse (la methode est asynchrone pour eviter de bloquer le thread)

            return reponse;
        }
    }

    public struct DonnéesAPI
    {
        public int id_zone { get; set; }
        public double lat_point { get; set; }
        public double lng_point { get; set; }
    }
}
