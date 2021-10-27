using System;
using System.Collections.Generic;
using System.Web;
using ComunidadPractica.Models;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using ComunidadPractica.Controllers;

namespace ComunidadPractica.Handlers
{
    public class UsuarioHandler : BaseHandler
    {
        public List<UsuarioModel> obtenerTodosLosUsuarios()
        {
            List<UsuarioModel> usuario = new List<UsuarioModel>();
            string consulta = "SELECT nombre, correoPK, sexo, contacto, pais FROM Usuario";
            DataTable tablaResultado = consultarTabla(consulta);
            foreach (DataRow columna in tablaResultado.Rows)
            {
                usuario.Add(
                new UsuarioModel
                {
                    nombre = Convert.ToString(columna["nombre"]),
                    correo = Convert.ToString(columna["correoPK"]),
                    sexo = Convert.ToString(columna["sexo"]),
                    contacto = Convert.ToString(columna["contacto"]),
                    pais = Convert.ToString(columna["pais"])
                });
            }
            return usuario;
        }

        public int actualizarUsuario(UsuarioModel miembro)
        {
            int codigoError = 0;

            string consulta = "UPDATE Usuario SET nombre = @nombre, sexo = @sexo, contacto = @contacto, pais = @pais " +
                              "WHERE correoPK = @correoPK";

            SqlCommand comandoParaConsulta = new SqlCommand(consulta, conexion);
            comandoParaConsulta.Parameters.AddWithValue("@correoPK", miembro.correo);
            comandoParaConsulta.Parameters.AddWithValue("@nombre", miembro.nombre);
            comandoParaConsulta.Parameters.AddWithValue("@sexo", miembro.sexo);
            comandoParaConsulta.Parameters.AddWithValue("@contacto", miembro.contacto);
            comandoParaConsulta.Parameters.AddWithValue("@pais", miembro.pais);

            conexion.Open();
            try
            {
                var numeroTuplasAfectadas = comandoParaConsulta.ExecuteNonQuery();
                conexion.Close();
                if (numeroTuplasAfectadas == 0)
                {
                    codigoError = Constantes.Estado.ERROR_NOT_DEFINED;
                }
            }
            catch(Exception e)
            {
                conexion.Close();
                codigoError = Constantes.Estado.ERROR_NOT_DEFINED;
            }


            return codigoError;
        }

        public UsuarioModel obtenerUsuario(string correo)
        {
            UsuarioModel usuario = null;

            string consulta = "SELECT * FROM Usuario WHERE correoPK = '" + correo + "'";

            DataTable tablaResultado = consultarTabla(consulta);
            if (tablaResultado.Rows.Count > 0)
            {
                DataRow columna = tablaResultado.Rows[0];
                usuario = new UsuarioModel
                {
                    nombre = Convert.ToString(columna["nombre"]),
                    correo = Convert.ToString(columna["correoPK"]),
                    sexo = Convert.ToString(columna["sexo"]),
                    contacto = Convert.ToString(columna["contacto"]),
                    pais = Convert.ToString(columna["pais"])
                };

            }

            return usuario;
        }

        public int insertarUsuario(UsuarioModel usuario)
        {
            string consulta = "INSERT INTO Usuario (correoPK, nombre, sexo, contacto, pais, salt, password)" +
                              " VALUES (@correoPK, @nombre, @sexo, @contacto, @pais, @salt, @password)";

            SqlCommand comandoParaConsulta = new SqlCommand(consulta, conexion);

            var saltUnica = Criptografia.crearSalt(32);
            comandoParaConsulta.Parameters.AddWithValue("@salt", saltUnica);
            comandoParaConsulta.Parameters.AddWithValue("@password", Criptografia.hashearPassword(usuario.contrasenna, saltUnica));
            comandoParaConsulta.Parameters.AddWithValue("@correoPK", usuario.correo);
            comandoParaConsulta.Parameters.AddWithValue("@nombre", usuario.nombre);
            comandoParaConsulta.Parameters.AddWithValue("@sexo", usuario.sexo);
            comandoParaConsulta.Parameters.AddWithValue("@contacto", usuario.contacto);
            comandoParaConsulta.Parameters.AddWithValue("@pais", usuario.pais);

            int error = 0;
            try
            {
                conexion.Open();
                comandoParaConsulta.ExecuteNonQuery();
            }
            catch (SqlException e)
            {
                error = Constantes.Estado.ERROR_NOT_DEFINED;
                if (e.Number == Constantes.ErroresSQL.SQL_DUPLICATE_PK)
                    error = Constantes.ErroresSQL.SQL_DUPLICATE_PK;
            }
            conexion.Close();
            return error;
        }

    }

}