using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ComunidadPractica.Handlers
{
    public class BaseHandler
    {
        protected SqlConnection conexion;
        protected string rutaConexion;

        public BaseHandler()
        {
            rutaConexion = ConfigurationManager.ConnectionStrings["ComunidadConnection"].ToString();
            conexion = new SqlConnection(rutaConexion);
        }

        public DataTable consultarTabla(string consulta)
        {
            SqlCommand comandoParaConsulta = new SqlCommand(consulta, conexion);
            SqlDataAdapter adaptadorParaTabla = new SqlDataAdapter(comandoParaConsulta);
            DataTable consultaFormatoTabla = new DataTable();
            conexion.Open();
            adaptadorParaTabla.Fill(consultaFormatoTabla);
            conexion.Close();
            return consultaFormatoTabla;
        }

        public int obtenerCantidadTuplas(string tabla, string consultaParticular = "")
        {
            int valor = 0;
            SqlCommand comandoParaConsulta = new SqlCommand("SELECT count(*) FROM " + tabla + " " + consultaParticular, conexion);
            conexion.Open();
            valor = (Int32)comandoParaConsulta.ExecuteScalar();
            conexion.Close();
            return valor;
        }
    }
}