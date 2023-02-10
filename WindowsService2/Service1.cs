using Npgsql;
using System;
using System.Configuration;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Collections.Generic;

namespace WindowsService2
{
    public partial class Service1 : ServiceBase
    {
        private bool blBandera = false;
        private NpgsqlConnection conn;
        private NpgsqlConnection conn2;
        private NpgsqlConnection conn3;
        private NpgsqlCommand comm;
        private NpgsqlCommand comm2;
        private NpgsqlCommand comm3;
        private NpgsqlCommand comm4;
        private NpgsqlCommand comm5;
        private HttpWebRequest request;
        private static string server;
        private static string server2 = "localhost";
        private static string prt;
        private static string port = "5432";
        private static string db;
        private static string db2 = "JOIN";
        private static string userId;
        private static string userI = "postgres";
        private static string passwd;
        private static string passwd2 = "12345678";
        private static string domain;
        private static string tokenn;
        private static string time;
        private static string msg;

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            readAllSettings();
            string ConnectionString = "server=" + server + ";" + "port=" + prt + ";" + "user id=" + userId + ";" + "password=" + passwd + ";" + "database=" + db + ";";
            string ConnectionString2 = "server=" + server2 + ";" + "port=" + port + ";" + "user id=" + userI + ";" + "password=" + passwd2 + ";" + "database=" + db2 + ";";

            conn = new NpgsqlConnection(ConnectionString);
            conn2 = new NpgsqlConnection(ConnectionString2);
            conn3 = new NpgsqlConnection(ConnectionString);

            string query = "insert into \"result_api\" (\"Company\", \"Result\", \"JsonError\", \"EstructuraError\", \"DetalleError\", \"DocGenerado\", \"secuencial\", \"empresa\") values (@COMPANY, @RESULT, @JSONERROR, @ESTRUCTURAERROR, @DETALLEERROR, @DOCGENERADO, @SECUENCIAL, @EMPRESA) on conflict (id) do nothing";
            string queryS1 = "select tc.fecha, case when mc.nombre is null or mc.nombre = '' then 'CONSUMIDOR FINAL' else mc.nombre end as cliente, case when mc.direccion is null or mc.direccion = '' then 'Dirección' else mc.direccion end as direccion, case when mc.telefono is null or mc.telefono = '' then '9999999999' else mc.telefono end as telefono, case when mc.cedula is null or mc.cedula = '' then '9999999999999' else mc.cedula end as ruc, '01' as tipoComprobante, '01' as tipoIdentificador, case when mc.email is null or mc.email = '' then 'correo@correo.com' else mc.email end as correo, substring(cc.prefijo,1,3) as establecimiento, substring(cc.prefijo,5,3) as ptoEmision, '0201074093001' as rucEmpresa, to_char (tc.mov,'000000000') as secuencial, 1 as ambiente, 'CARMEN RECALDE' as razonSocial, 'FARMACIA AJD' as nombreComercial, 'GUALBERTO PEREZ E261 Y ANDRES PEREZ' as direccionMatriz, 'NO' as obligadoContabilidad, '' as numeroCE, '' as claveAcceso, tc.subtotal as importeSinImpuestos, vlr_dscto as descuento, tc.subtotal+tc.vlr_impto as importeTotal, 0.0 as baseIva12, vlr_impto as valorIva12, 0 as baseIva0, '' as adicionales, replace(replace(cc.prefijo,'-','') || to_char (tc.mov,'000000000'),' ' ,'') as secuencialunico from tcfactura as tc left join mclientes as mc on tc.numero = mc.numero inner join ccajas as cc on tc.caja = cc.caja where not exists ( select empresa, secuencial from result_api as ra where  replace(replace(cc.prefijo,'-','') || to_char (tc.mov,'000000000'),' ' ,'') = ra.secuencial) and tc.fecha >= '2023-07-02'";
            string queryS2 = "select item as idlinea, unid as cantidad, mp.descripcion , td.refe as coditem, precio as precioUnitario, vlr_venta as total, 12 as iva, 0 as ice, 0 as irbpnr, 0 as codigoIce, 0 as codigoPorcentajeIce, 0 as baseImponibleIce, 0 as tarifaIce, 0 as ValorIce, 0 as codigoIrbpnr, 0 as codigoPorcentajeIrbpnr, 0 as baseImponibleIrbpnr, 0 as tarifaIrbpnr, 0 as valorIrbpnr from tdfactura as td inner join tcfactura as tc on td.caja = tc.caja and td.mov = tc.mov inner join ccajas as cc on tc.caja = cc.caja left join mproductos as mp on td.refe = mp.refe where replace(cc.prefijo,'-','') || REPLACE(to_char (tc.mov,'000000000'),' ','') = @SEQUENTIAL";
            string queryS3 = "select sec as idlinea, '01' as fp, valor as total, 0 as plazo, 'DIAS' as unidadtiempo from tpfactura as tp inner join tcfactura as tc on tp.caja = tc.caja and tp.mov = tc.mov inner join ccajas as cc on tc.caja = cc.caja where replace(cc.prefijo,'-','') || REPLACE(to_char (tc.mov,'000000000'),' ','') = @SEQUENTIAL";
            
            comm = new NpgsqlCommand(queryS1, conn);
            comm2 = new NpgsqlCommand(query, conn2);
            comm3 = new NpgsqlCommand("select * from result_api", conn2);
            comm4 = new NpgsqlCommand(queryS2, conn3);
            comm5 = new NpgsqlCommand(queryS3, conn3);
            comm4.Parameters.Add("@SEQUENTIAL", NpgsqlTypes.NpgsqlDbType.Varchar);
            comm5.Parameters.Add("@SEQUENTIAL", NpgsqlTypes.NpgsqlDbType.Varchar);

            try
            {
                conn.Open();
                conn2.Open();
                conn3.Open();
                EventLog.WriteEntry("Established Connection!", EventLogEntryType.Information);
                timerSc.Start();
            }
            catch (NpgsqlException ex)
            {
                msg = ex.Message + "\n" + ex.HelpLink + "\n" + ex.Source + "\n" + ex.ErrorCode + "\n" + ex.Data;
                EventLog.WriteEntry("Connection Error: " + msg, EventLogEntryType.Error);
            }
            catch (Exception ex)
            {
                msg = ex.Message + "\n" + ex.Source + "\n" + ex.StackTrace + "\n" + ex.Data;
                EventLog.WriteEntry("Error ocurrido en el transcurso del servicio: " + msg, EventLogEntryType.Error);
            }

        }

        protected override void OnStop()
        {
            conn.Close();
            conn2.Close();
            conn3.Close();
            timerSc.Stop();
        }

        public void connectionNpgsql()
        {
            NpgsqlConnection.ClearPool(conn);
            NpgsqlConnection.ClearPool(conn2);
            NpgsqlConnection.ClearPool(conn3);
            NpgsqlDataReader rd = comm3.ExecuteReader();
            if (rd.HasRows)
            {
                formatJson(rd);
            }
            else
            {
                formatJson(rd);
            }
            rd.Close();
        }

        public void formatJson(NpgsqlDataReader rdr)
        {
            NpgsqlDataReader rd = comm.ExecuteReader();
            if (rd.HasRows)
            {
                int aff = 0;

                UserData userData = new UserData();
                while (rd.Read())
                {
                    if (rdr.IsClosed)
                    {
                        rdr = comm3.ExecuteReader();
                    }

                    userData.fecha = Convert.ToDateTime(rd["fecha"]);
                    userData.cliente = rd["cliente"].ToString().Trim();
                    userData.direccion = rd["direccion"].ToString().Trim();
                    userData.telefono = rd["telefono"].ToString().Trim();
                    userData.ruc = rd["ruc"].ToString().Trim();
                    userData.tipoComprobante = rd["tipocomprobante"].ToString().Trim();
                    userData.tipoIdentificador = rd["tipoidentificador"].ToString().Trim();
                    userData.correo = rd["correo"].ToString().Trim();
                    userData.establecimiento = rd["establecimiento"].ToString().Trim();
                    userData.ptoEmision = rd["ptoemision"].ToString().Trim();
                    userData.rucEmpresa = rd["rucempresa"].ToString().Trim();
                    userData.secuencial = rd["secuencial"].ToString().Trim();
                    userData.ambiente = Convert.ToInt32(rd["ambiente"]);
                    userData.razonSocial = rd["razonsocial"].ToString().Trim();
                    userData.nombreComercial = rd["nombrecomercial"].ToString().Trim();
                    userData.direccionMatriz = rd["direccionmatriz"].ToString().Trim();
                    userData.obligadoContabilidad = rd["obligadocontabilidad"].ToString().Trim();
                    userData.numeroCE = rd["numeroce"].ToString().Trim();
                    userData.claveAcceso = rd["claveacceso"].ToString().Trim();
                    userData.importeSinImpuestos = Convert.ToDouble(rd["importesinimpuestos"]);
                    userData.descuento = Convert.ToDouble(rd["descuento"]);
                    userData.importeTotal = Convert.ToDouble(rd["importetotal"]);
                    userData.baseIva12 = Convert.ToDouble(rd["baseiva12"]);
                    userData.valorIva12 = Convert.ToDouble(rd["valoriva12"]);
                    userData.baseIva0 = Convert.ToDouble(rd["baseiva0"]);
                    userData.adicionales = rd["adicionales"].ToString().Trim();

                    string sequential = rd["secuencialunico"].ToString().Trim();

                    comm4.Parameters["@SEQUENTIAL"].Value = sequential;
                    NpgsqlDataReader rd2 = comm4.ExecuteReader();
                    if (rd2.HasRows)
                    {
                        List<UserDetail> list = new List<UserDetail>();
                        while (rd2.Read())
                        {
                            list.Add(new UserDetail()
                            {
                                idlinea = Convert.ToInt32(rd2["idlinea"]),
                                cantidad = Convert.ToInt32(rd2["cantidad"]),
                                item = rd2["descripcion"].ToString().Trim(),
                                codItem = Convert.ToInt32(rd2["coditem"]),
                                precioUnitario = Convert.ToDouble(rd2["preciounitario"]),
                                total = Convert.ToDouble(rd2["total"]),
                                iva = Convert.ToInt32(rd2["iva"]),
                                ice = Convert.ToInt32(rd2["ice"]),
                                irbpnr = Convert.ToInt32(rd2["irbpnr"]),
                                codigoIce = Convert.ToInt32(rd2["codigoice"]),
                                codigoPorcentajeIce = Convert.ToInt32(rd2["codigoporcentajeice"]),
                                baseImponibleIce = Convert.ToInt32(rd2["baseimponibleice"]),
                                tarifaIce = Convert.ToInt32(rd2["tarifaice"]),
                                ValorIce = Convert.ToInt32(rd2["valorice"]),
                                codigoIrbpnr = Convert.ToInt32(rd2["codigoirbpnr"]),
                                codigoPorcentajeIrbpnr = Convert.ToInt32(rd2["codigoporcentajeirbpnr"]),
                                baseImponibleIrbpnr = Convert.ToInt32(rd2["baseimponibleirbpnr"]),
                                tarifaIrbpnr = Convert.ToInt32(rd2["tarifairbpnr"]),
                                valorIrbpnr = Convert.ToInt32(rd2["valorirbpnr"])
                            });
                            userData.Detalle = list;
                        }
                    }
                    rd2.Close();

                    comm5.Parameters["@SEQUENTIAL"].Value = sequential;
                    NpgsqlDataReader rd3 = comm5.ExecuteReader();
                    if (rd3.HasRows)
                    {
                        List<UserPayments> list = new List<UserPayments>();
                        while (rd3.Read())
                        {
                            list.Add(new UserPayments()
                            {
                                idlinea = Convert.ToInt32(rd3["idlinea"]),
                                fp = rd3["fp"].ToString().Trim(),
                                total = Convert.ToDouble(rd3["total"]),
                                plazo = Convert.ToInt32(rd3["plazo"]),
                                unidadtiempo = rd3["unidadtiempo"].ToString().Trim()
                            });
                            userData.fPagos = list;
                        }
                    }
                    rd3.Close();
                    string jsonResult = JsonConvert.SerializeObject(userData);
                    jsonApi(rdr, jsonResult, comm2, aff, sequential);
                }
            }
            rd.Close();
        }

        public void jsonApi(NpgsqlDataReader rd, string jsonDat, NpgsqlCommand comm2, int aff, string uniqueSequential)
        {
            try
            {
                var myData = JsonConvert.DeserializeObject<UserData>(jsonDat);
                string tok = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(tokenn));
                string dat = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(JsonConvert.SerializeObject(myData)));

                BodyApi body = new BodyApi() { token = tok, data = dat};
                byte[] fullData = UTF8Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(body));

                request = WebRequest.Create(domain) as HttpWebRequest;
                request.CookieContainer = new CookieContainer();
                if (!myData.Equals(null))
                {
                    bool is_in = false;
                    if (rd.HasRows)
                    {
                        while (rd.Read())
                        {
                            string subSeq = Convert.ToString(rd["DocGenerado"]);
                            string uniqueSequential2 = subSeq.Substring(1);
                            if (uniqueSequential == uniqueSequential2)
                            {
                                is_in = true;
                            }
                        }
                    }
                    rd.Close();

                    if (!is_in)
                    {
                        request = WebRequest.Create(domain) as HttpWebRequest;
                        request.Timeout = 10 * 1000;
                        request.Method = "POST";
                        request.ContentLength = fullData.Length;
                        request.ContentType = "application/json; charset=utf-8";
                        Stream postStream = request.GetRequestStream();
                        postStream.Write(fullData, 0, fullData.Length);
                        postStream.Close();
                        HttpWebResponse httpWebResponse = request.GetResponse() as HttpWebResponse;
                        StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream());
                        string bodyResult = streamReader.ReadToEnd().Trim();
                        streamReader.Close();
                        EventLog.WriteEntry("Status Code: " + httpWebResponse.StatusCode, EventLogEntryType.Information);
                        ResponseApi responseApi = JsonConvert.DeserializeObject<ResponseApi>(bodyResult);
                        comm2.Parameters.Add("@COMPANY", NpgsqlTypes.NpgsqlDbType.Varchar);
                        comm2.Parameters["@COMPANY"].Value = responseApi.Company;
                        comm2.Parameters.Add("@RESULT", NpgsqlTypes.NpgsqlDbType.Boolean);
                        comm2.Parameters["@RESULT"].Value = responseApi.Result;
                        comm2.Parameters.Add("@JSONERROR", NpgsqlTypes.NpgsqlDbType.Varchar);
                        comm2.Parameters["@JSONERROR"].Value = responseApi.JsonError;
                        EventLog.WriteEntry("JsonError: " + responseApi.JsonError, EventLogEntryType.Error);
                        if (responseApi.EstructuraError != null)
                        {
                            comm2.Parameters.Add("@ESTRUCTURAERROR", NpgsqlTypes.NpgsqlDbType.Varchar);
                            comm2.Parameters["@ESTRUCTURAERROR"].Value = responseApi.EstructuraError;
                            EventLog.WriteEntry("EstructuraError: " + responseApi.EstructuraError, EventLogEntryType.Error);
                        }
                        else
                        {
                            comm2.Parameters.Add("@ESTRUCTURAERROR", NpgsqlTypes.NpgsqlDbType.Varchar);
                            comm2.Parameters["@ESTRUCTURAERROR"].Value = "Without Error";
                        }
                        if (responseApi.DetalleError != null)
                        {
                            comm2.Parameters.Add("@DETALLEERROR", NpgsqlTypes.NpgsqlDbType.Varchar);
                            comm2.Parameters["@DETALLEERROR"].Value = responseApi.DetalleError;
                            EventLog.WriteEntry("DetalleError: " + responseApi.DetalleError, EventLogEntryType.Error);
                        }
                        else
                        {
                            comm2.Parameters.Add("@DETALLEERROR", NpgsqlTypes.NpgsqlDbType.Varchar);
                            comm2.Parameters["@DETALLEERROR"].Value = "All OK";
                        }
                        comm2.Parameters.Add("@DOCGENERADO", NpgsqlTypes.NpgsqlDbType.Varchar);
                        comm2.Parameters["@DOCGENERADO"].Value = responseApi.DocGenerado;
                        comm2.Parameters.Add("@SECUENCIAL", NpgsqlTypes.NpgsqlDbType.Varchar);
                        comm2.Parameters["@SECUENCIAL"].Value = responseApi.secuencial;
                        comm2.Parameters.Add("@EMPRESA", NpgsqlTypes.NpgsqlDbType.Varchar);
                        comm2.Parameters["@EMPRESA"].Value = responseApi.rucEmpresa;
                        aff = aff + comm2.ExecuteNonQuery();
                        comm2.Parameters.Clear();
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                msg = ex.Message + "\n" + ex.HelpLink + "\n" + ex.Source + "\n" + ex.ErrorCode + "\n" + ex.Data;
                EventLog.WriteEntry("Error ocurrido en el transcurso del servicio: " + msg, EventLogEntryType.Error);
            }
            catch (Exception ex)
            {
                msg = ex.Message + "\n" + ex.Source + "\n" + ex.StackTrace + "\n" + ex.Data;
                EventLog.WriteEntry("Error ocurrido en el transcurso del servicio: " + msg, EventLogEntryType.Error);
            }
        }

        private void timerSc_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (blBandera) return;

            timerSc.Interval = Int32.Parse(time) * 60000;

            try
            {
                blBandera= true;
                EventLog.WriteEntry("Started Service!", EventLogEntryType.Information);
                connectionNpgsql();
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(ex.Message, EventLogEntryType.Error);
            }

            blBandera = false;
        }

        public void readAllSettings()
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;

                if (appSettings.Count == 0)
                {
                    EventLog.WriteEntry("Error en el archivo App.config", EventLogEntryType.Error);
                }
                else
                {
                    foreach (var settingKey in appSettings.AllKeys)
                    {
                        if (settingKey == "Server")
                        {
                            server = appSettings[settingKey];
                        }
                        else if (settingKey == "Port")
                        {
                            prt = appSettings[settingKey];
                        }
                        else if (settingKey == "Database")
                        {
                            db = appSettings[settingKey];
                        }
                        else if (settingKey == "UserId")
                        {
                            userId = appSettings[settingKey];
                        }
                        else if (settingKey == "Password")
                        {
                            passwd = appSettings[settingKey];
                        }
                        else if (settingKey == "Domain")
                        {
                            domain = appSettings[settingKey];
                        }
                        else if (settingKey == "Token")
                        {
                            tokenn = appSettings[settingKey];
                        }
                        else if (settingKey == "Time")
                        {
                            time = appSettings[settingKey];
                        }
                    }
                }
            }
            catch (ConfigurationErrorsException ex)
            {
                EventLog.WriteEntry("Error con el archivo App.config: " + ex.Message, EventLogEntryType.Error);
            }
        }
    }

    public class BodyApi
    {
        public string token { get; set; }
        public string data { get; set; }
    }

    public class UserData
    {
        public DateTime fecha { get; set; }
        public string cliente { get; set; }
        public string direccion { get; set; }
        public string telefono { get; set; }
        public string ruc { get; set; }
        public string tipoComprobante { get; set; }
        public string tipoIdentificador { get; set; }
        public string correo { get; set; }
        public string establecimiento { get; set; }
        public string ptoEmision { get; set; }
        public string rucEmpresa { get; set; }
        public string secuencial { get; set; }
        public int ambiente { get; set; }
        public string razonSocial { get; set; }
        public string nombreComercial { get; set; }
        public string direccionMatriz { get; set; }
        public string obligadoContabilidad { get; set; }
        public string numeroCE { get; set; }
        public string claveAcceso { get; set; }
        public double importeSinImpuestos { get; set; }
        public double descuento { get; set; }
        public double importeTotal { get; set; }
        public double baseIva12 { get; set; }
        public double valorIva12 { get; set; }
        public double baseIva0 { get; set; }
        public string adicionales { get; set; }
        public List<UserDetail> Detalle { get; set; }
        public List<UserPayments> fPagos { get; set; }
    }

    public class UserDetail
    {
        public int idlinea { get; set; }
        public int cantidad { get; set; }
        public string item { get; set; }
        public int codItem { get; set; }
        public double precioUnitario { get; set; }
        public double total { get; set; }
        public int iva { get; set; }
        public int ice { get; set; }
        public int irbpnr { get; set; }
        public int codigoIce { get; set; }
        public int codigoPorcentajeIce { get; set; }
        public int baseImponibleIce { get; set; }
        public int tarifaIce { get; set; }
        public int ValorIce { get; set; }
        public int codigoIrbpnr { get; set; }
        public int codigoPorcentajeIrbpnr { get; set; }
        public int baseImponibleIrbpnr { get; set; }
        public int tarifaIrbpnr { get; set; }
        public int valorIrbpnr { get; set; }
    }

    public class UserPayments
    {
        public int idlinea { get; set; }
        public string fp { get; set; }
        public double total { get; set; }
        public int plazo { get; set; }
        public string unidadtiempo { get; set; }
    }

    public class ResponseApi
    {
        public string Company { get; set; }
        public bool Result { get; set; }
        public string JsonError { get; set; }
        public string EstructuraError { get; set; }
        public string DetalleError { get; set; }
        public string DocGenerado { get; set; }
        public string secuencial { get; set; }
        public string rucEmpresa { get; set; }
    }
}
