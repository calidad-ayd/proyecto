using System;
using System.Collections.Generic;
using ComunidadPractica.Models;
using System.Data;
using System.Data.SqlClient;

namespace ComunidadPractica.Handlers
{
    public class MiembroHandler : UsuarioHandler
    {
        public List<MiembroModel> obtenerMiembrosPorPagina(int pagina, int tuplasPorPaginas)
        {

            List<MiembroModel> miembros = new List<MiembroModel>();

            string consulta = "SELECT U.nombre, U.correoPK, U.sexo, U.contacto, U.pais, M.condicionLaboral"
                + " FROM Usuario U JOIN Miembro M ON M.correoFK = U.correoPK"
                + " ORDER BY U.nombre OFFSET " + (pagina * tuplasPorPaginas) + " ROWS FETCH NEXT " + tuplasPorPaginas + " ROWS ONLY";

            DataTable tablaResultado = consultarTabla(consulta);

            foreach (DataRow columna in tablaResultado.Rows)
            {
                string correo = Convert.ToString(columna["correoPK"]);
                miembros.Add(new MiembroModel
                {
                    nombre = Convert.ToString(columna["nombre"]),
                    correo = correo,
                    sexo = Convert.ToString(columna["sexo"]),
                    contacto = Convert.ToString(columna["contacto"]),
                    pais = Convert.ToString(columna["pais"]),
                    condicionLaboral = Convert.ToString(columna["condicionLaboral"]),
                });
            }
            return miembros;
        }

        public MiembroModel obtenerMiembro(string correo)
        {
            MiembroModel miembro = null;

            string consulta = "SELECT U.nombre, U.correoPK, U.sexo, U.contacto, U.pais, M.condicionLaboral" +
                " FROM Usuario U JOIN Miembro M ON M.correoFK = U.correoPK " +
                " WHERE U.correoPK = '" + correo + "'";

            DataTable tablaResultado = consultarTabla(consulta);
            if (tablaResultado.Rows.Count > 0)
            {

                DataRow row = tablaResultado.Rows[0];
                miembro = new MiembroModel
                {
                    nombre = Convert.ToString(row["nombre"]),
                    correo = correo,
                    sexo = Convert.ToString(row["sexo"]),
                    contacto = Convert.ToString(row["contacto"]),
                    pais = Convert.ToString(row["pais"]),
                    condicionLaboral = Convert.ToString(row["condicionLaboral"]),
                    habilidades = obtenerHabilidadesMiembro(correo),
                    idiomas = obtenerIdiomasMiembro(correo)
                };

            }

            return miembro;

        }
        public List<string> obtenerIdiomasMiembro(string correo)
        {
            List<string> idiomas = new List<string>();
            string consulta = "Select idioma FROM Idiomas_Miembro WHERE correoFK = '" + correo + "'";
            DataTable tablaResultado = consultarTabla(consulta);
            foreach (DataRow columna in tablaResultado.Rows)
            {
                idiomas.Add(Convert.ToString(columna["idioma"]));
            }
            return idiomas;
        }

        public List<string> obtenerHabilidadesMiembro(string correo)
        {
            List<string> habilidades = new List<string>();
            string consulta = "Select habilidad FROM Habilidades_Miembro WHERE correoFK = '" + correo + "'";
            DataTable tablaResultado = consultarTabla(consulta);
            foreach (DataRow columna in tablaResultado.Rows)
            {
                habilidades.Add(Convert.ToString(columna["habilidad"]));
            }
            return habilidades;
        }

        public List<string> obtenerTodosLosCorreos()
        {
            //Para tomar los correos de todos los miembros para enviar correos masivos
            List<string> correos = new List<string>();

            string consulta = "SELECT correoFK FROM Miembro";
            DataTable tablaResultado = consultarTabla(consulta);
            foreach (DataRow columna in tablaResultado.Rows)
            {
                correos.Add(Convert.ToString(columna["correoFK"]));
            }
            return correos;
        }

        
        public int insertarEnMiembro(MiembroModel miembro)
        {
            bool exito;
            int revisa = 0;

            string consulta = "INSERT INTO Miembro (correoFK, condicionLaboral, tipoMiembro) " +
                              "VALUES (@correoFK, @condicionLaboral, 'Activo')";

            SqlCommand comandoParaConsulta = new SqlCommand(consulta, conexion);
            comandoParaConsulta.Parameters.AddWithValue("@correoFK", miembro.correo);
            comandoParaConsulta.Parameters.AddWithValue("@condicionLaboral", miembro.condicionLaboral);

            conexion.Open();
            try
            {
                exito = comandoParaConsulta.ExecuteNonQuery() >= 1;
                conexion.Close();
                if (exito)
                {
                    insertarHabilidades(miembro.correo, miembro.habilidades);
                    insertarIdiomas(miembro.correo, miembro.idiomas);
                }
                else
                {
                    revisa = Constantes.Estado.ERROR_NOT_DEFINED;
                }
                
                
            }
            catch (SqlException e)
            {
                conexion.Close();
                if (e.Number == Constantes.ErroresSQL.SQL_DUPLICATE_PK)
                {
                    revisa = Constantes.ErroresSQL.SQL_DUPLICATE_PK;
                }
                else
                {
                    revisa = Constantes.Estado.ERROR_NOT_DEFINED;
                }
            }
            return revisa;
        }

        public int agregarMiembro(MiembroModel miembro)
        {
            string consulta = "INSERT INTO Usuario (correoPK, nombre, sexo, contacto, pais, salt, password)" +
                              " VALUES (@correoPK, @nombre, @sexo, @contacto, @pais, @salt, @password)";

            SqlCommand comandoParaConsulta = new SqlCommand(consulta, conexion);

            var saltUnica = Criptografia.crearSalt(32);
            comandoParaConsulta.Parameters.AddWithValue("@salt", saltUnica);
            comandoParaConsulta.Parameters.AddWithValue("@password", Criptografia.hashearPassword(miembro.contrasenna, saltUnica));
            comandoParaConsulta.Parameters.AddWithValue("@correoPK", miembro.correo);
            comandoParaConsulta.Parameters.AddWithValue("@nombre", miembro.nombre);
            comandoParaConsulta.Parameters.AddWithValue("@sexo", miembro.sexo);
            comandoParaConsulta.Parameters.AddWithValue("@contacto", miembro.contacto);
            comandoParaConsulta.Parameters.AddWithValue("@pais", miembro.pais);

            var exito = -1;
            conexion.Open();
            try
            {
                exito = comandoParaConsulta.ExecuteNonQuery();
                conexion.Close();
                if (exito > 0)
                {
                    exito = insertarEnMiembro(miembro);
                }
            }
            catch (SqlException e)
            {
                conexion.Close();
                if(e.Number == Constantes.ErroresSQL.SQL_DUPLICATE_PK)
                {
                    exito = insertarEnMiembro(miembro);
                    if (exito != Constantes.Estado.ERROR_NOT_DEFINED && exito != Constantes.ErroresSQL.SQL_DUPLICATE_PK)
                    {
                        exito = actualizarUsuario(miembro);
                    }
                }
            }
            if (exito > 0)
            {
                exito = insertarEnMiembro(miembro);
            }
            return exito;
        }

      
        public int actualizarInformacionMiembro(MiembroModel miembro)
        {
            string consultaMiembro = "UPDATE Miembro SET condicionLaboral = @condicionLaboral WHERE correoFK = @correoFK";

            SqlCommand comandoParaConsulta = new SqlCommand(consultaMiembro, conexion);

            comandoParaConsulta.Parameters.AddWithValue("@correoFK", miembro.correo);
            comandoParaConsulta.Parameters.AddWithValue("@condicionLaboral", miembro.condicionLaboral);

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
                    return Constantes.ErroresSQL.SQL_DUPLICATE_PK;
            }
            conexion.Close();
            if((error = eliminarHabilidades(miembro.correo)) != 0)
                return error;
            if ((error = insertarHabilidades(miembro.correo, miembro.habilidades)) != 0)
                return error;
            if ((error = eliminarIdiomas(miembro.correo)) != 0)
                return error;
            error = insertarIdiomas(miembro.correo, miembro.idiomas);
            return error;
        }

        public int eliminarHabilidades(string correoMiembro)
        {
            string consulta = "DELETE FROM Habilidades_Miembro WHERE correoFK = @correoFK";
            SqlCommand comandoParaConsulta = new SqlCommand(consulta, conexion);
            comandoParaConsulta.Parameters.AddWithValue("@correoFK", correoMiembro);
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

        public int eliminarIdiomas(string correoMiembro)
        {
            string consulta = "DELETE FROM Idiomas_Miembro WHERE correoFK = @correoFK";
            SqlCommand comandoParaConsulta = new SqlCommand(consulta, conexion);
            comandoParaConsulta.Parameters.AddWithValue("@correoFK", correoMiembro);
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

        public int insertarHabilidades(string correoMiembro, List<string> habilidades)
        {
            if (habilidades == null)
                return 0; 

            int error = 0;

            try
            {
                foreach (string habilidad in habilidades)
                {
                    string consulta = "INSERT INTO Habilidades_Miembro (correoFK, habilidad) " +
                                      "VALUES (@correoFK, @habilidad)";

                    SqlCommand comandoParaConsulta = new SqlCommand(consulta, conexion);
                    
                    comandoParaConsulta.Parameters.AddWithValue("@correoFK", correoMiembro);
                    comandoParaConsulta.Parameters.AddWithValue("@habilidad", habilidad);

                    conexion.Open();
                    comandoParaConsulta.ExecuteNonQuery();
                    conexion.Close();
                }
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

        public int insertarIdiomas(string correoMiembro, List<string> idiomas)
        {
            if (idiomas == null)
                return 0;

            int error = 0;

            try
            {
                foreach(string idioma in idiomas)
                {
                    string consulta = "INSERT INTO Idiomas_Miembro (correoFK, idioma)" +
                          " VALUES (@correoFK, @idiomas)";

                    SqlCommand comandoParaConsulta = new SqlCommand(consulta, conexion);
                    comandoParaConsulta.Parameters.AddWithValue("@correoFK", correoMiembro);
                    comandoParaConsulta.Parameters.AddWithValue("@idiomas", idioma);

                    conexion.Open();
                    comandoParaConsulta.ExecuteNonQuery();
                    conexion.Close();
                }
            }
            catch (SqlException e)
            {
                error = Constantes.Estado.ERROR_NOT_DEFINED;
                if (e.Number == Constantes.ErroresSQL.SQL_DUPLICATE_PK)
                    error = Constantes.ErroresSQL.SQL_DUPLICATE_PK;
            }
            return error; 
        }

    }
}