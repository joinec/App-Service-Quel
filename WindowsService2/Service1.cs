using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.ServiceProcess;
using System.Text;

namespace WindowsService2
{
    public partial class Service1 : ServiceBase
    {
        private bool blBandera = false;
        private NpgsqlConnection conn;
        private NpgsqlConnection conn2;
        private NpgsqlConnection conn3;
        private NpgsqlConnection conn4;
        private NpgsqlConnection conn5;
        private NpgsqlCommand comm;
        private NpgsqlCommand comm2;

        private NpgsqlCommand comm4;
        private NpgsqlCommand comm5;
        private NpgsqlCommand comm6;
        private NpgsqlCommand comm7;

        private HttpWebRequest request;
        private static string server;
        private static string prt;
        private static string db;
        private static string userId;
        private static string passwd;
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

            try
            {

                EventLog.WriteEntry("Conexion exitosa a la base de datos", EventLogEntryType.Information);

                timerSc.Start();
            }
            catch (NpgsqlException ex)
            {
                msg = ex.Message + "\n" + ex.HelpLink + "\n" + ex.Source + "\n" + ex.ErrorCode + "\n" + ex.Data;
                EventLog.WriteEntry("Error al conectar a la BD: " + msg, EventLogEntryType.Error);
            }
            catch (Exception ex)
            {
                msg = ex.Message + "\n" + ex.Source + "\n" + ex.StackTrace + "\n" + ex.Data;
                EventLog.WriteEntry("Error: " + msg, EventLogEntryType.Error);
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
            try
            {
                string ConnectionString = "server=" + server + ";" + "port=" + prt + ";" + "user id=" + userId + ";" + "password=" + passwd + ";" + "database=" + db + ";";

                conn = new NpgsqlConnection(ConnectionString);
                conn2 = new NpgsqlConnection(ConnectionString);
                conn3 = new NpgsqlConnection(ConnectionString);
                conn4 = new NpgsqlConnection(ConnectionString);
                conn5 = new NpgsqlConnection(ConnectionString);

                //string query = "insert into \"result_api\" (\"Company\", \"Result\", \"JsonError\", \"EstructuraError\", \"DetalleError\", \"DocGenerado\", \"secuencial\", \"empresa\") values (@COMPANY, @RESULT, @JSONERROR, @ESTRUCTURAERROR, @DETALLEERROR, @DOCGENERADO, @SECUENCIAL, @EMPRESA) on conflict (id) do nothing";
                string queryInsertResult = "INSERT INTO result_api (id, \"Company\", \"Result\", \"JsonError\", \"EstructuraError\", \"DetalleError\", \"DocGenerado\", secuencial, empresa) select  * from (SELECT 0 as id  , cast(@COMPANY as varchar) as Company, @RESULT as Result,@JSONERROR as JsonError,@ESTRUCTURAERROR as EstructuraError, @DETALLEERROR as DetalleError, @DOCGENERADO as DocGenerado,cast(@SECUENCIAL as varchar) as secuencial, @EMPRESA as empresa ) as a where not exists ( select * from result_api as  b where a.secuencial = b.secuencial )";
                string queryCabeceraFac = "select * from cabecerafactura() as( fecha date,  cliente character,  direccion character,  telefono character,  ruc character, tipocomprobante character varying,  tipoidentificador text,  correo bpchar,  establecimiento text,  ptoemision text, rucempresa character (13),  secuencial text,  ambiente integer,  razonsocial character varying,  nombrecomercial character (60), direccionmatriz character (40),  obligadocontabilidad character varying,  numeroce character varying,  claveacceso character varying, importesinimpuestos numeric (12,2),  descuento numeric (12,2),  importetotal numeric,  baseiva12 numeric,  valoriva12 numeric (12,2), baseiva0 numeric,  adicionales character varying,  secuencialunico text)";
                string queryDetalleFac = "select * from detallefactura(@SEQUENTIAL) as (idlinea smallint , cantidad numeric, descripcion character(50) , coditem varchar(100), precioUnitario numeric,  total numeric, iva numeric, ice numeric,  irbpnr numeric,  codigoIce numeric, codigoPorcentajeIce numeric,baseImponibleIce numeric, tarifaIce numeric,ValorIce numeric,codigoIrbpnr numeric, codigoPorcentajeIrbpnr numeric, baseImponibleIrbpnr numeric, tarifaIrbpnr numeric,valorIrbpnr numeric)";
                string queryFpagosFac = "select * from fpfactura(@SEQUENTIAL) as (idlinea smallint, fp varchar(2), total numeric, plazo varchar(20), unidadtiempo varchar(10))";

                string queryCabeceraNC = "select * from cabeceranc() as( fecha date,  cliente character,  direccion character,  telefono character,  ruc character, tipocomprobante character varying,  tipoidentificador text,  correo bpchar,  establecimiento text,  ptoemision text, rucempresa character (13),  secuencial text,  ambiente integer,  razonsocial character varying,  nombrecomercial character (60), direccionmatriz character (40),  obligadocontabilidad character varying, tipoDocAfectado varchar(2), secuencialDocAfectado varchar (15),fechaDocAfectado date,motivoDev varchar(20), numeroce character varying,  claveacceso character varying, importesinimpuestos numeric (12,2),  descuento numeric (12,2),  importetotal numeric,  baseiva12 numeric,  valoriva12 numeric (12,2), baseiva0 numeric,  adicionales character varying,  secuencialunico text)";
                string queryDetalleNC = "select * from detallenc(@SEQUENTIAL) as (idlinea smallint, cantidad numeric, descripcion character(50) , coditem varchar(100), precioUnitario numeric,  total numeric,iva numeric, ice numeric,  irbpnr numeric,  codigoIce numeric, codigoPorcentajeIce numeric, baseImponibleIce numeric, tarifaIce numeric,ValorIce numeric, codigoIrbpnr numeric, codigoPorcentajeIrbpnr numeric, baseImponibleIrbpnr numeric, tarifaIrbpnr numeric, valorIrbpnr numeric)";

                comm = new NpgsqlCommand(queryCabeceraFac, conn);
                comm2 = new NpgsqlCommand(queryInsertResult, conn2);
                //comm3 = new NpgsqlCommand("select * from result_api", conn2);
                comm4 = new NpgsqlCommand(queryDetalleFac, conn3);
                comm5 = new NpgsqlCommand(queryFpagosFac, conn3);

                comm6 = new NpgsqlCommand(queryCabeceraNC, conn4);
                comm7 = new NpgsqlCommand(queryDetalleNC, conn5);

                comm4.Parameters.Add("@SEQUENTIAL", NpgsqlTypes.NpgsqlDbType.Varchar);
                comm5.Parameters.Add("@SEQUENTIAL", NpgsqlTypes.NpgsqlDbType.Varchar);

                comm7.Parameters.Add("@SEQUENTIAL", NpgsqlTypes.NpgsqlDbType.Varchar);

                conn.Open();
                conn2.Open();
                conn3.Open();
                conn4.Open();
                conn5.Open();

                /*
               NpgsqlConnection.ClearPool(conn);
               NpgsqlConnection.ClearPool(conn2);
               NpgsqlConnection.ClearPool(conn3);
                */

                NpgsqlDataReader rdFC = comm.ExecuteReader();
                formatJson(rdFC, 1);
                rdFC.Close();

                NpgsqlDataReader rdNC = comm6.ExecuteReader();
                formatJson(rdNC, 2);
                rdNC.Close();

                conn.Close();
                conn2.Close();
                conn3.Close();
                conn4.Close();
                conn5.Close();

            }
            catch (NpgsqlException ex)
            {

                EventLog.WriteEntry("Error en : connectionNpgsql " + ex.Message, EventLogEntryType.Error);

            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("Error en : connectionNpgsql " + ex.Message, EventLogEntryType.Error);
            }
        }

        public void formatJson(NpgsqlDataReader rd, int tipo)
        {

            if (rd.HasRows && tipo == 1)
            {
                int aff = 0;
                UserData userData = new UserData();
                while (rd.Read())
                {

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
                                cantidad = float.Parse(rd2["cantidad"].ToString()),
                                item = rd2["descripcion"].ToString().Trim(),
                                codItem = Convert.ToInt32(rd2["coditem"]),
                                precioUnitario = Convert.ToDouble(rd2["preciounitario"]),
                                total = Convert.ToDouble(rd2["total"]),
                                iva = Convert.ToInt32(rd2["iva"]),
                                ice = Convert.ToInt32(rd2["ice"]),
                                irbpnr = Convert.ToInt32(rd2["irbpnr"]),
                                codigoIce = Convert.ToInt32(rd2["codigoice"]),
                                codigoPorcentajeIce = Convert.ToInt32(rd2["codigoporcentajeice"]),
                                baseImponibleIce = Int32.Parse(rd2["baseimponibleice"].ToString()),
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

                    jsonApi(jsonResult, comm2, aff, sequential, tipo);
                }

                rd.Close();

            }
            else if (rd.HasRows && tipo == 2)
            {
                int aff = 0;
                RootobjectNC userData = new RootobjectNC();
                while (rd.Read())
                {

                    userData.fecha = Convert.ToDateTime(rd["fecha"]);
                    userData.cliente = rd["cliente"].ToString().Trim();
                    userData.direccion = rd["direccion"].ToString().Trim();
                    userData.telefono = rd["telefono"].ToString().Trim();
                    userData.ruc = rd["ruc"].ToString().Trim();
                    userData.tipoComprobante = Int32.Parse(rd["tipocomprobante"].ToString().Trim());
                    userData.tipoIdentificador = Int32.Parse(rd["tipoidentificador"].ToString().Trim());
                    userData.correo = rd["correo"].ToString().Trim();
                    userData.establecimiento = rd["establecimiento"].ToString().Trim();
                    userData.ptoEmision = rd["ptoemision"].ToString().Trim();
                    userData.rucEmpresa = rd["rucempresa"].ToString().Trim();
                    userData.secuencial = rd["secuencial"].ToString().Trim();
                    userData.ambiente = rd["ambiente"].ToString().Trim();
                    userData.razonSocial = rd["razonsocial"].ToString().Trim();
                    userData.nombreComercial = rd["nombrecomercial"].ToString().Trim();
                    userData.direccionMatriz = rd["direccionmatriz"].ToString().Trim();
                    userData.obligadoContabilidad = rd["obligadocontabilidad"].ToString().Trim();

                    userData.tipoDocAfectado = rd["tipodocafectado"].ToString().Trim();
                    userData.motivoDev = rd["motivodev"].ToString().Trim();
                    userData.secuencialDocAfectado = rd["secuencialdocafectado"].ToString().Trim();
                    userData.fechaDocSustento = Convert.ToDateTime(rd["fechadocafectado"]);

                    userData.claveAcceso = rd["claveacceso"].ToString().Trim();
                    userData.importeSinImpuestos = float.Parse(rd["importesinimpuestos"].ToString().Trim());
                    userData.descuento = float.Parse(rd["descuento"].ToString());
                    userData.importeTotal = float.Parse(rd["importetotal"].ToString());
                    userData.baseIva12 = float.Parse(rd["baseiva12"].ToString());
                    userData.valorIva12 = float.Parse(rd["valoriva12"].ToString());
                    userData.baseIva0 = float.Parse(rd["baseiva0"].ToString());
                    userData.adicionales = rd["adicionales"].ToString().Trim();

                    string sequential = rd["secuencialunico"].ToString().Trim();

                    comm7.Parameters["@SEQUENTIAL"].Value = sequential;

                    NpgsqlDataReader rd2 = comm7.ExecuteReader();
                    if (rd2.HasRows)
                    {
                        List<DetalleNC> list = new List<DetalleNC>();
                        while (rd2.Read())
                        {
                            list.Add(new DetalleNC()
                            {
                                idlinea = Convert.ToInt32(rd2["idlinea"]),
                                cantidad = float.Parse(rd2["cantidad"].ToString()),
                                item = rd2["descripcion"].ToString().Trim(),
                                codItem = Convert.ToInt32(rd2["coditem"]),
                                precioUnitario = float.Parse(rd2["preciounitario"].ToString()),
                                total = float.Parse(rd2["total"].ToString()),
                                iva = Convert.ToInt32(rd2["iva"]),
                                ice = Convert.ToInt32(rd2["ice"]),
                                irbpnr = Convert.ToInt32(rd2["irbpnr"]),
                                codigoIce = rd2["codigoice"].ToString(),
                                codigoPorcentajeIce = rd2["codigoporcentajeice"].ToString(),
                                baseImponibleIce = Int32.Parse(rd2["baseimponibleice"].ToString()),
                                tarifaIce = Int32.Parse(rd2["tarifaice"].ToString()),
                                ValorIce = Int32.Parse(rd2["valorice"].ToString()),
                                codigoIrbpnr = Convert.ToInt32(rd2["codigoirbpnr"]),
                                codigoPorcentajeIrbpnr = Convert.ToInt32(rd2["codigoporcentajeirbpnr"]),
                                baseImponibleIrbpnr = Int32.Parse(rd2["baseimponibleirbpnr"].ToString()),
                                tarifaIrbpnr = Convert.ToInt32(rd2["tarifairbpnr"]),
                                valorIrbpnr = Int32.Parse(rd2["valorirbpnr"].ToString())
                            });

                            userData.Detalle = list;
                        }
                    }
                    rd2.Close();

                    string jsonResult = JsonConvert.SerializeObject(userData);
                    jsonApi(jsonResult, comm2, aff, sequential, tipo);
                }
                rd.Close();
            }
            else
            {
                EventLog.WriteEntry("formatJson contiene rows: " + rd.HasRows.ToString() + "en tipo de documento  " + tipo, EventLogEntryType.Warning);

            }

        }

        public void jsonApi(string jsonDat, NpgsqlCommand comm2, int aff, string uniqueSequential , int tipo)
        {
            string endpoint = "";
            

            if (tipo == 1)
            {
                endpoint = domain + "/api/create/invoice";
                var myData = JsonConvert.DeserializeObject<UserData>(jsonDat);
                try
                {

                    string tok = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(tokenn));
                    string dat = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(JsonConvert.SerializeObject(myData)));

                    BodyApi body = new BodyApi() { token = tok, data = dat };
                    byte[] fullData = UTF8Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(body));

                    request = WebRequest.Create(endpoint) as HttpWebRequest;
                    request.CookieContainer = new CookieContainer();
                    if (!myData.Equals(null))
                    {
                        bool is_in = true;
                        if (is_in)
                        {
                            request = WebRequest.Create(endpoint) as HttpWebRequest;
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
                            EventLog.WriteEntry("Result: " + bodyResult, EventLogEntryType.Information);

                            comm2.Parameters.Add("@COMPANY", NpgsqlTypes.NpgsqlDbType.Varchar);
                            comm2.Parameters.Add("@RESULT", NpgsqlTypes.NpgsqlDbType.Boolean);
                            comm2.Parameters.Add("@JSONERROR", NpgsqlTypes.NpgsqlDbType.Varchar);
                            comm2.Parameters.Add("@ESTRUCTURAERROR", NpgsqlTypes.NpgsqlDbType.Varchar);
                            comm2.Parameters.Add("@DETALLEERROR", NpgsqlTypes.NpgsqlDbType.Varchar);
                            comm2.Parameters.Add("@DOCGENERADO", NpgsqlTypes.NpgsqlDbType.Varchar);
                            comm2.Parameters.Add("@SECUENCIAL", NpgsqlTypes.NpgsqlDbType.Varchar);
                            comm2.Parameters.Add("@EMPRESA", NpgsqlTypes.NpgsqlDbType.Varchar);

                            if (responseApi.Result != true && responseApi.Result != false)
                            {
                                comm2.Parameters["@COMPANY"].Value = "N/A";
                                comm2.Parameters["@RESULT"].Value = false;
                                comm2.Parameters["@JSONERROR"].Value = "N/A";
                                comm2.Parameters["@ESTRUCTURAERROR"].Value = "N/A";
                                comm2.Parameters["@DETALLEERROR"].Value = responseApi.Response;
                                comm2.Parameters["@DOCGENERADO"].Value = "N/A";
                                comm2.Parameters["@SECUENCIAL"].Value = uniqueSequential;
                                comm2.Parameters["@EMPRESA"].Value = "N/A";
                            }
                            else
                            {
                                comm2.Parameters["@COMPANY"].Value = responseApi.Company;
                                comm2.Parameters["@RESULT"].Value = responseApi.Result;
                                comm2.Parameters["@JSONERROR"].Value = responseApi.JsonError;

                                if (responseApi.EstructuraError != null)
                                {
                                    comm2.Parameters["@ESTRUCTURAERROR"].Value = responseApi.EstructuraError;
                                }
                                else
                                {
                                    comm2.Parameters["@ESTRUCTURAERROR"].Value = "Without Error";
                                }
                                if (responseApi.DetalleError != null)
                                {
                                    comm2.Parameters["@DETALLEERROR"].Value = responseApi.DetalleError;
                                }
                                else
                                {
                                    comm2.Parameters["@DETALLEERROR"].Value = "All OK";
                                }

                                comm2.Parameters["@DOCGENERADO"].Value = responseApi.DocGenerado;
                                comm2.Parameters["@SECUENCIAL"].Value = uniqueSequential;
                                comm2.Parameters["@EMPRESA"].Value = responseApi.rucEmpresa;
                            }
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
            else if(tipo == 2)
            {
                endpoint = domain + "/api/create/note";
                var myData = JsonConvert.DeserializeObject<RootobjectNC>(jsonDat);
                try
                {

                    string tok = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(tokenn));
                    string dat = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(JsonConvert.SerializeObject(myData)));

                    BodyApi body = new BodyApi() { token = tok, data = dat };
                    byte[] fullData = UTF8Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(body));

                    request = WebRequest.Create(endpoint) as HttpWebRequest;
                    request.CookieContainer = new CookieContainer();
                    if (!myData.Equals(null))
                    {
                        bool is_in = true;
                        if (is_in)
                        {
                            request = WebRequest.Create(endpoint) as HttpWebRequest;
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
                            EventLog.WriteEntry("Result: " + bodyResult, EventLogEntryType.Information);

                            comm2.Parameters.Add("@COMPANY", NpgsqlTypes.NpgsqlDbType.Varchar);
                            comm2.Parameters.Add("@RESULT", NpgsqlTypes.NpgsqlDbType.Boolean);
                            comm2.Parameters.Add("@JSONERROR", NpgsqlTypes.NpgsqlDbType.Varchar);
                            comm2.Parameters.Add("@ESTRUCTURAERROR", NpgsqlTypes.NpgsqlDbType.Varchar);
                            comm2.Parameters.Add("@DETALLEERROR", NpgsqlTypes.NpgsqlDbType.Varchar);
                            comm2.Parameters.Add("@DOCGENERADO", NpgsqlTypes.NpgsqlDbType.Varchar);
                            comm2.Parameters.Add("@SECUENCIAL", NpgsqlTypes.NpgsqlDbType.Varchar);
                            comm2.Parameters.Add("@EMPRESA", NpgsqlTypes.NpgsqlDbType.Varchar);

                            if (responseApi.Result != true && responseApi.Result != false)
                            {
                                comm2.Parameters["@COMPANY"].Value = "N/A";
                                comm2.Parameters["@RESULT"].Value = false;
                                comm2.Parameters["@JSONERROR"].Value = "N/A";
                                comm2.Parameters["@ESTRUCTURAERROR"].Value = "N/A";
                                comm2.Parameters["@DETALLEERROR"].Value = responseApi.Response;
                                comm2.Parameters["@DOCGENERADO"].Value = "N/A";
                                comm2.Parameters["@SECUENCIAL"].Value = uniqueSequential;
                                comm2.Parameters["@EMPRESA"].Value = "N/A";
                            }
                            else
                            {
                                comm2.Parameters["@COMPANY"].Value = responseApi.Company;
                                comm2.Parameters["@RESULT"].Value = responseApi.Result;
                                comm2.Parameters["@JSONERROR"].Value = responseApi.JsonError;

                                if (responseApi.EstructuraError != null)
                                {
                                    comm2.Parameters["@ESTRUCTURAERROR"].Value = responseApi.EstructuraError;
                                }
                                else
                                {
                                    comm2.Parameters["@ESTRUCTURAERROR"].Value = "Without Error";
                                }
                                if (responseApi.DetalleError != null)
                                {
                                    comm2.Parameters["@DETALLEERROR"].Value = responseApi.DetalleError;
                                }
                                else
                                {
                                    comm2.Parameters["@DETALLEERROR"].Value = "All OK";
                                }

                                comm2.Parameters["@DOCGENERADO"].Value = responseApi.DocGenerado;
                                comm2.Parameters["@SECUENCIAL"].Value = uniqueSequential;
                                comm2.Parameters["@EMPRESA"].Value = responseApi.rucEmpresa;
                            }
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
           
        }

        private void timerSc_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (blBandera) return;

            timerSc.Interval = Int32.Parse(time) * 60000;

            blBandera = true;
            EventLog.WriteEntry("Servicio iniciado", EventLogEntryType.Information);
            connectionNpgsql();


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
        public float cantidad { get; set; }
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
        public string Response { get; set; }
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
