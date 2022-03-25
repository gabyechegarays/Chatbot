
using System;
using System.Net;
using System.Runtime.Serialization;

using System.Runtime.Serialization.Json;

namespace ChatSocketServer1
{
   public class ClienteRest
    {
        [DataContract]
        public class Serie
        {
            [DataMember(Name = "titulo")]
            public string Title { get; set; }

            [DataMember(Name = "idSerie")]
            public string IdSerie { get; set; }

            [DataMember(Name = "datos")]
            public DataSerie[] Data { get; set; }
        }
        [DataContract]
       public class DataSerie
        {
            [DataMember(Name = "fecha")]
            public string Date { get; set; }

            [DataMember(Name = "dato")]
            public string Data { get; set; }
        }

        [DataContract]
        public class SeriesResponse
        {
            [DataMember(Name = "series")]
            public Serie[] series { get; set; }
        }

        [DataContract]
       public class Response
        {
            [DataMember(Name = "bmx")]
            public SeriesResponse seriesResponse { get; set; }
        }
        public  Response CambioDolar()
        {
            try
            {
                /* se utilizan la fecha Inicial y feha final*/
                //SF43718 --> Tipo de Cambio - pesos a dollar
                //SR465 --> cambio de moneda de 111 paises
                string fecha = DateTime.Now.ToString("yyyy-MM-dd");
                string url = "https://www.banxico.org.mx/SieAPIRest/service/v1/series/SF43718/datos/" + fecha+"/"+fecha+"";
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Accept = "application/json";
                request.Headers["Bmx-Token"] = "fe05bb220f537d2b9395f03888093e72c8ca375813bb71b1bf2d3797c5acdf5d";
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                if (response.StatusCode != HttpStatusCode.OK)
                    throw new Exception(String.Format(
                    "Server error (HTTP {0}: {1}).",
                    response.StatusCode,
                    response.StatusDescription));
                DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(Response));
                object objResponse = jsonSerializer.ReadObject(response.GetResponseStream());
                Response jsonResponse = objResponse as Response;
                return jsonResponse;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return null;
        }
        public  string ReadSeries()
        {
            
            Response response = CambioDolar();
            Serie serie = response.seriesResponse.series[0];
            string tipoCambio = "";
            Console.WriteLine("Serie: {0}", serie.Title);
            foreach (DataSerie dataSerie in serie.Data)
            {
                if (dataSerie.Data.Equals("N/E")) continue;
                tipoCambio = dataSerie.Data;
            }
            return tipoCambio;
        }


    }
}


