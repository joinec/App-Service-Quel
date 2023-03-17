using System;
using System.Collections.Generic;

namespace WindowsService2
{

    public class RootobjectNC
    {
        public DateTime fecha { get; set; }
        public string cliente { get; set; }
        public string direccion { get; set; }
        public string telefono { get; set; }
        public string ruc { get; set; }
        public int tipoComprobante { get; set; }
        public int tipoIdentificador { get; set; }
        public string correo { get; set; }
        public string establecimiento { get; set; }
        public string ptoEmision { get; set; }
        public string rucEmpresa { get; set; }
        public string secuencial { get; set; }
        public string ambiente { get; set; }
        public string razonSocial { get; set; }
        public string nombreComercial { get; set; }
        public string direccionMatriz { get; set; }
        public string obligadoContabilidad { get; set; }
        public string tipoDocAfectado { get; set; }
        public string secuencialDocAfectado { get; set; }
        public string motivoDev { get; set; }
        public DateTime fechaDocSustento { get; set; }
        public string claveAcceso { get; set; }
        public float importeSinImpuestos { get; set; }
        public float descuento { get; set; }
        public float importeTotal { get; set; }
        public float baseIva12 { get; set; }
        public float valorIva12 { get; set; }
        public float baseIva0 { get; set; }
        public string adicionales { get; set; }
        public List<DetalleNC> Detalle { get; set; }
    }

    public class DetalleNC
    {
        public int idlinea { get; set; }
        public float cantidad { get; set; }
        public string item { get; set; }
        public int codItem { get; set; }
        public float precioUnitario { get; set; }
        public float total { get; set; }
        public int iva { get; set; }
        public int ice { get; set; }
        public int irbpnr { get; set; }
        public string codigoIce { get; set; }
        public string codigoPorcentajeIce { get; set; }
        public int baseImponibleIce { get; set; }
        public int tarifaIce { get; set; }
        public int ValorIce { get; set; }
        public int codigoIrbpnr { get; set; }
        public int codigoPorcentajeIrbpnr { get; set; }
        public int baseImponibleIrbpnr { get; set; }
        public int tarifaIrbpnr { get; set; }
        public int valorIrbpnr { get; set; }
    }


}
