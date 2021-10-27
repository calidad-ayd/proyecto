using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ComunidadPractica.Models;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace ComunidadPractica.Handlers
{
    public class ItemHandler : BaseHandler
    {
        public ItemModel obtenerItemEncuesta(int encuestaID, int numItem)
        {
            ItemModel item = new ItemModel();
            string consulta = "SELECT I.numItemPK, I.enunciado, I.encuestaIdFK FROM Encuesta E JOIN Item I ON E.encuestaIdPK = I.encuestaIdFK " +
                "WHERE E.encuestaIdPK = " + encuestaID + " AND I.numItemPK = " + numItem;
            DataTable tablaResultado = consultarTabla(consulta);
            if (tablaResultado.Rows.Count == 0)
            {
                item = null;
            }
            else
            {
                DataRow columna = tablaResultado.Rows[0];
                item = new ItemModel
                {
                    numItem = Convert.ToInt32(columna["numItemPK"]),
                    enunciado = Convert.ToString(columna["enunciado"]),
                    opciones = obtenerOpcionesSegunItem(encuestaID,numItem)
                };
            }
            return item;
        }
        public List<ItemModel> obtenerItemsSegunEncuesta(int encuestaID)
        {
            List<ItemModel> items = new List<ItemModel>();
            DataTable tablaResultado = consultarTabla("SELECT encuestaIdFK, numItemPK, enunciado FROM Item " +
                "WHERE encuestaIdFK = " + encuestaID);

            if (tablaResultado.Rows.Count > 0)
            {
                foreach (DataRow columna in tablaResultado.Rows)
                {
                    int numItem = Convert.ToInt32(columna["numItemPK"]);
                    items.Add(new ItemModel
                    {
                        enunciado = Convert.ToString(columna["enunciado"]),
                        numItem = numItem,
                        opciones = obtenerOpcionesSegunItem(encuestaID, numItem)
                    });
                }
            }

            return items;
        }

        public List<string> obtenerOpcionesSegunItem(int encuestaID, int numItem)
        {
            List<string> opciones = new List<string>();
            string consulta = "SELECT valorPK FROM Opcion WHERE numItemFK = " + numItem + " AND encuestaIDFK = " + encuestaID;
            DataTable tablaResultado = consultarTabla(consulta);
            foreach (DataRow columna in tablaResultado.Rows)
            {
                opciones.Add(Convert.ToString(columna["valorPK"]));
            }
            return opciones;
        }

        public bool eliminarItem(int encuestaID, int ItemID)
        {
            string consulta = "DELETE FROM Item WHERE encuestaIdFK = @encuestaIdPK AND numItemPK = @numItem";
            SqlCommand comandoParaConsulta = new SqlCommand(consulta, conexion);
            comandoParaConsulta.Parameters.AddWithValue("@encuestaIdPK", encuestaID);
            comandoParaConsulta.Parameters.AddWithValue("@numItem", ItemID);
            conexion.Open();
            bool exito = comandoParaConsulta.ExecuteNonQuery() >= 1;
            conexion.Close();
            return exito;
        }

        public bool eliminarOpcion(int encuestaID, int ItemID, string opcion)
        {
            string consulta = "DELETE FROM Opcion " +
            "WHERE encuestaIdFK = @encuestaIdPK AND numItemFK = @numItem AND valorPK = @valorPK";

            SqlCommand comandoParaConsulta = new SqlCommand(consulta, conexion);
            comandoParaConsulta.Parameters.AddWithValue("@encuestaIdPK", encuestaID);
            comandoParaConsulta.Parameters.AddWithValue("@numItem", ItemID);
            comandoParaConsulta.Parameters.AddWithValue("@valorPK", opcion);
            conexion.Open();
            bool exito = comandoParaConsulta.ExecuteNonQuery() >= 1;
            conexion.Close();
            return exito;
        }

        public bool crearOpcion(int encuestaID, int ItemID, string opcion)
        {
            string consulta = "INSERT INTO Opcion VALUES(@numItem, @encuestaIdPK, @valorPK)";

            SqlCommand comandoParaConsulta = new SqlCommand(consulta, conexion);
            comandoParaConsulta.Parameters.AddWithValue("@encuestaIdPK", encuestaID);
            comandoParaConsulta.Parameters.AddWithValue("@numItem", ItemID);
            comandoParaConsulta.Parameters.AddWithValue("@valorPK", opcion);
            conexion.Open();
            bool exito = comandoParaConsulta.ExecuteNonQuery() >= 1;
            conexion.Close();
            return exito;
        }


    }
}