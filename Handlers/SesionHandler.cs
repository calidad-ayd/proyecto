using System;
using System.Data.SqlClient;

namespace ComunidadPractica.Handlers
{
    public class SesionHandler : BaseHandler
    {
        public enum Rol : ushort
        {
            Null           = 0,
            Externo        = 1,
            Activo         = 2,
            Nucleo         = 3,
            Coordinador    = 4
        }

        public enum Permiso : ushort
        {
            Null                  = 0,
            MiembroEditar         = 1,
            EncuestaCrear         = 2,
            EncuestaVerListado    = 3,
            EncuestaContestar     = 4,
            EncuestaVerRespuestas = 5,
            EncuestaEliminar      = 6,
            CursoProponer         = 7,
            CursoAprobar          = 8,
            CursoCrear            = 9,
            CursoVer              = 10
        }

        public string consultarRol(string correo) {
            var tabla = consultarTabla("SELECT tipoMiembro FROM Miembro WHERE correoFK = '" + correo + "'");
            if (tabla.Rows.Count > 0)
            {
                return Convert.ToString(tabla.Rows[0]["tipoMiembro"]);
            }
            return "Externo";
        }

        public Rol autentificarUsuario(string correo, string password) {

            string consulta = "SELECT salt, password FROM Usuario WHERE correoPK = @correo";
            SqlCommand comandoParaConsulta = new SqlCommand(consulta, conexion);
            comandoParaConsulta.Parameters.AddWithValue("@correo", correo);

            SqlDataAdapter adaptadorParaTabla = new SqlDataAdapter(comandoParaConsulta);
            var tabla = new System.Data.DataTable();

            conexion.Open();
            comandoParaConsulta.ExecuteNonQuery();
            conexion.Close();
            adaptadorParaTabla.Fill(tabla);

            if (tabla.Rows.Count < 1){
                return Rol.Null;
            }
         
            var dataRow = tabla.Rows[0];
            var salt = Convert.ToString(dataRow["salt"]);
            var passwordRemota = Convert.ToString(dataRow["password"]);
            var passwordHasheada = Criptografia.hashearPassword(password, salt);

            if (passwordHasheada == passwordRemota) {
                var rol = consultarRol(correo);
                if (rol == "Externo")
                {
                    return Rol.Externo;
                }
                else 
                {
                    return parsearEnum<Rol>(rol);
                }
            }

            return Rol.Null;
        }

        public static T parsearEnum<T>(string rol)
        {
            return (T)Enum.Parse(typeof(T), rol, true);
        }


    }
}