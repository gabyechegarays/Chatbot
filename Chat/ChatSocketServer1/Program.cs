using System;
using System.Linq;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using static ChatSocketServer1.ClienteRest;
using System.Globalization;
using System.Xml;
using System.IO;

namespace chatSocketServer1
{
    class Program
    {
        public static Hashtable clientsList = new Hashtable();

        static void Main(string[] args)
        {

            TcpListener serverSocket = new TcpListener(8888);
            TcpClient clientSocket = default(TcpClient);
            int counter = 0;

            //Se inicializa el socket principal
            serverSocket.Start();
            Console.WriteLine("Servidor Chat Iniciado ....");
            counter = 0;
            while ((true))
            {
                counter += 1;

                //Llego una solicitud de conexion nueva de un cliente, se acepta la conexion
                clientSocket = serverSocket.AcceptTcpClient();

                //Se reserva memoria para los mensajes del cliente
                byte[] bytesFrom = new byte[clientSocket.ReceiveBufferSize];
                string dataFromClient = null;

                //Leemos el mensaje recibido del cliente
                NetworkStream networkStream = clientSocket.GetStream();
                networkStream.Read(bytesFrom, 0, clientSocket.ReceiveBufferSize);
                dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
                dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));

                //Si el nombre del cliente ya existe se borra de la lista de clientes
                clientsList.Remove(dataFromClient);
                clientsList.Add(dataFromClient, clientSocket);

                //Se lanza la una nueva conexion para aceptar al cliente nuevo 
                HandleClient client = new HandleClient();
                client.startClient(clientSocket, dataFromClient, clientsList);
                Console.WriteLine(dataFromClient + " se unió a la sala de chat ");

                //Le reenviamos el mensaje al resto de los clientes conectados
                unicast(" Bienvenido: " + dataFromClient, dataFromClient, false);
            }

            clientSocket.Close();
            serverSocket.Stop();
            Console.WriteLine("exit");
            Console.ReadLine();
        }


        public static void unicast(string msg, string uName, bool flag)
        {
            var Item = clientsList.OfType<DictionaryEntry>().Where(x => (x.Key as string).Equals(uName));

            TcpClient unicastSocket;
            unicastSocket = (TcpClient)Item.FirstOrDefault().Value;
            NetworkStream unicastStream = unicastSocket.GetStream();
            Byte[] unicastBytes = null;


            if (flag == true)
            {
                //message sending

                unicastBytes = Encoding.ASCII.GetBytes("Bot: " + msg);
            }
            else
            {
                unicastBytes = Encoding.ASCII.GetBytes(msg);
            }

            unicastStream.Write(unicastBytes, 0, unicastBytes.Length);
            unicastStream.Flush();
        }
    }

    public class HandleClient
    {
        TcpClient clientSocket;
        string clNo;
        Hashtable clientsList;

        public void startClient(TcpClient inClientSocket, string clineNo, Hashtable cList)
        {
            //Se inicializa la clase con los datos del cliente nuevo
            this.clientSocket = inClientSocket;
            this.clNo = clineNo;
            this.clientsList = cList;

            //Se lanza un hilo para poder recibir asincronamente mensajes
            Thread ctThread = new Thread(doChat);
            ctThread.Start();

        } //end StarCLiente


        private void doChat()
        {
            int requestCount = 0;
            byte[] bytesFrom = new byte[clientSocket.ReceiveBufferSize];
            string dataFromClient = null;
            //Byte[] sendBytes = null;
            //string serverResponse = null;
            string rCount = null;
            requestCount = 0;

            while ((true))
            {
                try
                {
                    requestCount = requestCount + 1;
                    NetworkStream networkStream = clientSocket.GetStream();
                    //byte[] bytesFrom = new byte[clientSocket.ReceiveBufferSize];
                    networkStream.Read(bytesFrom, 0, (int)clientSocket.ReceiveBufferSize);
                    dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
                    dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));
                    Console.WriteLine("Del cliente - " + clNo + " : " + dataFromClient);
                    rCount = Convert.ToString(requestCount);

                    botLogic(dataFromClient, clNo);
                }
                catch (Exception ex)
                {
                    //clientSocket.Close();
                    Console.WriteLine(ex.ToString());
                    return;
                }
            }//end while
        }//end doChat
        public static void botLogic(string aws, string nameCL)
        {
            string lat;
            string lon;
            string fororcast;

            switch (aws)
            {

                case string a when aws.Contains("Hola" + "holis" + "hi" + "oi" + "hey" + "hola"):
                    aws = "Hola " + "holis" + "hi" + "oi" + "hey" + "hola" + nameCL; break;
                case string a when aws.Contains("chiste"):
                    aws = "Que le dijo una iguana a otra iguana? " +
                        "Somos iguanitas"; break;
                case string a when aws.Contains("otro"):
                    aws = "Que dice Kung-fu cuando tiene una duda?" + "Estoy Kung-fundido"; break;
                case string a when aws.Contains("voto"):
                    aws = "El 3 de julio de 1955"; break;
                case string a when aws.Contains("dragon ball"):
                    aws = "El primer tomo fue publicado el 15 de mayo de 1995"; break;
                case string a when aws.Contains("trata"):
                    aws = " Su trama describe las aventuras de Goku, un guerrero saiyajin, " +
                        "cuyo fin es proteger a la Tierra de otros seres que quieren " +
                        "conquistarla y exterminar a la humanidad."; break;
                case string a when aws.Contains("Torre Eiffel"):
                    aws = "Francia"; break;
                case string a when aws.Contains("mundo"):
                    aws = "China"; break;
                case string a when aws.Contains("Mona Lisa"):
                    aws = "Leonardo da Vinci"; break;
                case string a when aws.Contains("algo"):
                    aws = "Necesito dormir mínimo 4395815669 horas!!!"; break;
                case string a when aws.Contains("Arizona"):
                    lat = aws.Substring(aws.IndexOf(':') + 1, 5);
                    lon = aws.Substring(aws.IndexOf(':', aws.IndexOf(':') + 1) + 1, 5);
                    fororcast = aws.Substring(aws.IndexOf('&') + 1, 8);
                    aws = wheater(lat, lon, fororcast);
                    break;
                case string a when aws.Contains("tasa de cambio"):
                    ChatSocketServer1.ClienteRest cliente = new ChatSocketServer1.ClienteRest();
                    aws = cliente.ReadSeries();

                    break;

                default:
                    aws = "No entiendo eso, puedes escribirlo de nuevo?"; //mensaje de error
                    break;

            }
            Program.unicast(aws, nameCL, true);
        }


        //uso de la API del Tiempo
        public static string wheater(string cd, string lon, string forOrCast)
        {
            string CurrentUrl = "http://api.openweathermap.org/data/2.5/weather?" + "q=" + cd + "&mode=xml&units=metric&APPID=" + "d101315d5c5642cdc7efd1f29ea44b07";


            // Create a web client.
            using (WebClient client = new WebClient())
            {
                // Get the response string from the URL.
                try
                {
                    return DisplayWeather(client.DownloadString(CurrentUrl));
                }

                catch (WebException ex)
                {
                    return ex.ToString();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unknown error\n" + ex.Message);
                    return ex.ToString();
                }
            }
        }
        private static string DisplayWeather(string xml)
        {
            // Load the response into an XML document.
            XmlDocument xml_doc = new XmlDocument();
            xml_doc.LoadXml(xml);

            string msg = "";

            // Get the city, country, latitude, and longitude.
            XmlNode loc_node = xml_doc.SelectSingleNode("current/city");
            msg += loc_node.Attributes["name"].Value + " ";
            msg += loc_node.SelectSingleNode("country").InnerText + " ";
            XmlNode geo_node = loc_node.SelectSingleNode("coord");
            msg += geo_node.Attributes["lat"].Value;
            msg += geo_node.Attributes["lon"].Value;

            char degrees = (char)176;
            // Get the temperature.
            XmlNode temp_node = xml_doc.SelectSingleNode("current/temperature");
            string temp = " Temperatura actual: " + temp_node.Attributes["value"].Value;
            msg += temp + " grados centigrados";
            return msg;
        }


    }//end class handleClinet
}//end namespace








