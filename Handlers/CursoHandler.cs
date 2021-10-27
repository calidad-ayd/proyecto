using System;
using System.Collections.Generic;
using System.Web;
using ComunidadPractica.Models;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace ComunidadPractica.Handlers
{
    public class CursoHandler : BaseHandler
    {
        public CursoModel obtenerCurso(string nombreCurso, int version)
        {
            CursoModel curso = new CursoModel();
            string consulta = "SELECT C.nombrePK, C.descripcion, C.contenido, C.estado, C.educadorFK, C.costo, C.versionPK "
                            + "FROM Curso C "
                            + $"WHERE C.nombrePK = '{nombreCurso}' AND versionPK = {version}";

            DataTable tablaResultado = consultarTabla(consulta);
            if (tablaResultado.Rows.Count > 0)
            {
                DataRow columna = tablaResultado.Rows[0];
                curso.nombre = Convert.ToString(columna["nombrePK"]);
                curso.version = Convert.ToInt32(columna["versionPK"]);
                curso.descripcion = Convert.ToString(columna["descripcion"]);
                curso.contenidos = Convert.ToString(columna["contenido"]);
                curso.estado = Convert.ToByte(columna["estado"]);
                curso.educador = Convert.ToString(columna["educadorFK"]);
                curso.costo = Convert.ToSingle(columna["costo"]);

                curso.categorias = obtenerCategoriasCurso(nombreCurso);
                curso.topicos = obtenerTopicosCurso(nombreCurso);
                curso.secciones = obtenerSeccionesCurso(curso);
            }
            return curso;
        }

        public List<Seccion> obtenerSeccionesCurso(CursoModel curso)
        {
            List<Seccion> seccions = new List<Seccion>();
            string consulta = "SELECT S.nombrePK, S.descripcion "
                            + "FROM Curso C "
                            + "JOIN Seccion S ON S.cursoFK = C.nombrePK AND S.versionCursoFK = C.versionPK "
                            + $"WHERE C.nombrePK = '{curso.nombre}' AND C.versionPK = {curso.version}";

            DataTable tablaResultado = consultarTabla(consulta);
            foreach (DataRow columna in tablaResultado.Rows)
            {
                Seccion seccion = new Seccion
                {
                    nombre = Convert.ToString(columna["nombrePK"]),
                    descripcion = Convert.ToString(columna["descripcion"]),
                };
                seccion.materiales = obtenerMaterialesSeccion(curso, seccion);
                seccions.Add(seccion);
            }

            return seccions;
        }

        public List<Material> obtenerMaterialesSeccion(CursoModel curso, Seccion seccion)
        {
            List<Material> materiales = new List<Material>();
            string consulta = "SELECT M.nombrePK, M.descripcion "
                            + "FROM Seccion S "
                            + "JOIN Material M ON M.cursoFK = S.cursoFK AND M.versionCursoFK = S.versionCursoFK AND M.seccionFK = S.nombrePK "
                            + $"WHERE S.cursoFK = '{curso.nombre}' AND S.versionCursoFK = '{curso.version}' AND S.nombrePK = '{seccion.nombre}'";

            DataTable tablaResultado = consultarTabla(consulta);
            foreach (DataRow columna in tablaResultado.Rows)
            {
                Material material = new Material
                {
                    nombre = Convert.ToString(columna["nombrePK"]),
                    descripcion = Convert.ToString(columna["descripcion"]),
                };
                
                string direccionCurso = HttpContext.Current.Server.MapPath("~/App_Data/Material_Curso/") + curso.nombre + "_" + curso.version.ToString() + "\\";
                string direccionSeccion = direccionCurso + "seccion_" + seccion.nombre + "\\";
                string direccionMaterial = direccionSeccion + "material_" + material.nombre + "\\";
                
                string[] archivos = Directory.GetFiles(direccionMaterial);
                if (Path.GetFileName(archivos[0]).Equals("link.txt"))
                {
                    material.link = File.ReadAllText(archivos[0]);
                    material.path = archivos[0];
                }
                else
                {
                    material.path = archivos[0];
                }

                materiales.Add(material);
            }

            return materiales;
        }

        public List<CursoModel> obtenerCursosPorPagina(int pagina, int tuplasPorPaginas, int aprobado)
        {
            List<CursoModel> cursos = new List<CursoModel>();

            string consulta = "SELECT C.nombrePK , C.descripcion, C.contenido, C.educadorFK, U.nombre AS 'educador', C.costo "
                            + "FROM Curso C JOIN Usuario U ON U.correoPK = C.educadorFK "
                            + $"WHERE C.estado = {aprobado} ORDER BY nombrePK "
                            + $"OFFSET {(pagina * tuplasPorPaginas)} ROWS FETCH NEXT {tuplasPorPaginas} ROWS ONLY";

            DataTable tablaResultado = consultarTabla(consulta);

            foreach (DataRow columna in tablaResultado.Rows)
            {
                string nombre = Convert.ToString(columna["nombrePK"]);
                cursos.Add(new CursoModel
                {
                    nombre = nombre,
                    descripcion = Convert.ToString(columna["descripcion"]),
                    contenidos = Convert.ToString(columna["contenido"]),
                    educador = Convert.ToString(columna["educadorFK"]),
                    topicos = obtenerTopicosCurso(nombre),
                    categorias = obtenerCategoriasCurso(nombre),
                    nombreEducador = Convert.ToString(columna["educador"]),
                    costo = Convert.ToSingle(columna["costo"]),
                    estado = Convert.ToByte(aprobado)
                });
            }
            return cursos;
        }

        public List<string> obtenerTopicosCurso(string nombre)
        {
            List<string> topicos = new List<string>();
            string consulta = "SELECT nombreTopicoFK FROM Pertenece_Curso "
                            + $"WHERE nombreCursoFK = '{nombre}'";
            DataTable tablaResultado = consultarTabla(consulta);
            foreach (DataRow columna in tablaResultado.Rows)
            {
                topicos.Add(Convert.ToString(columna["nombreTopicoFK"]));
            }
            return topicos;
        }

        public int insertarTopicosCurso(string nombreCurso, int version, List<string> topicos)
        {
            int estado = Constantes.Estado.ERROR_NOT_DEFINED;
            string consulta = "";
            foreach (string topico in topicos)
            {
                consulta += "INSERT INTO Pertenece_Curso VALUES ('" + nombreCurso + "', '" + topico + "', "+version+"); ";

            }
            SqlCommand comandoParaConsulta = new SqlCommand(consulta, conexion);
            try
            {
                conexion.Open();
                if(comandoParaConsulta.ExecuteNonQuery() >= 1)
                {
                    estado = Constantes.Estado.SUCCESS;
                }
                conexion.Close();
            }
            catch (SqlException e)
            {
                conexion.Close();
                estado = e.Number;
            }
            return estado;
        }

        public int proponerCurso(CursoModel curso)
        {
            curso.version = 1; ;
            curso.educador = Controllers.SesionController.obtenerModeloSesion(HttpContext.Current).correo;
            string consulta = "INSERT INTO Curso (nombrePK, descripcion, contenido, estado, educadorFK, costo, versionPK) "
                            + "VALUES (@nombrePK, @descripcion, @contenido, 0, @educadorFK, @costo, @version) ";

            SqlCommand comandoParaConsulta = new SqlCommand(consulta, conexion);

            comandoParaConsulta.Parameters.AddWithValue("@nombrePK", curso.nombre);
            comandoParaConsulta.Parameters.AddWithValue("@descripcion", curso.descripcion);
            comandoParaConsulta.Parameters.AddWithValue("@contenido", curso.contenidos);
            comandoParaConsulta.Parameters.AddWithValue("@educadorFK", curso.educador);
            comandoParaConsulta.Parameters.AddWithValue("@costo", curso.costo);
            comandoParaConsulta.Parameters.AddWithValue("@version", curso.version);

            conexion.Open();
            int estado = Constantes.Estado.ERROR_NOT_DEFINED;

            try
            {
                if (comandoParaConsulta.ExecuteNonQuery() >= 1)
                {
                    estado = Constantes.Estado.SUCCESS;
                }
                conexion.Close();
            }
            catch (SqlException e)
            {
                conexion.Close();
                estado = e.Number;
            }
            if (estado == Constantes.Estado.SUCCESS)
            {
                estado = insertarTopicosCurso(curso.nombre, curso.version, curso.topicos);
            }

            return estado;
        }

        public bool cambiarEstadoCurso(string nombreCurso, int estado)
        {
            string consulta = "UPDATE Curso SET estado = " + estado + " WHERE nombrePK = '" + nombreCurso + "'";
            SqlCommand comandoParaConsulta = new SqlCommand(consulta, conexion);
            conexion.Open();
            bool exito = comandoParaConsulta.ExecuteNonQuery() >= 1;
            conexion.Close();
            return exito;
        }

        public List<string> obtenerCategorias()
        {
            List<string> categorias = new List<string>();
            string consulta = "SELECT nombrePK FROM CategoriaTopico WHERE categoriaPadreFK is NULL";
            DataTable tablaResultado = consultarTabla(consulta);
            foreach (DataRow columna in tablaResultado.Rows)
            {
                categorias.Add(Convert.ToString(columna["nombrePK"]));
            }
            return categorias;
        }

        public List<string> obtenerCategoriasCurso(string nombre)
        {
            List<string> categorias = new List<string>();
            string consulta = "SELECT DISTINCT C.categoriaPadreFK FROM Pertenece_Curso P "
                            + "JOIN CategoriaTopico C ON P.nombreTopicoFK = C.nombrePK "
                            + "WHERE nombreCursoFK = '" + nombre + "'";
            DataTable tablaResultado = consultarTabla(consulta);
            foreach (DataRow columna in tablaResultado.Rows)
            {
                categorias.Add(Convert.ToString(columna["categoriaPadreFK"]));
            }
            return categorias;
        }

        public List<string> obtenerNombreCursosAprobados(string educador = "")
        {
            List<string> nombresCursos = new List<string>();
            string consulta = "SELECT nombrePK FROM Curso WHERE estado = 1";
            if(educador != "")
            {
                consulta += " AND educadorFK = @educador";
            }
            SqlCommand comandoParaConsulta = new SqlCommand(consulta, conexion);
            if(educador != "")
            {
                comandoParaConsulta.Parameters.AddWithValue("@educador", educador);
            }
            DataTable tablaResultado = new DataTable();
            SqlDataAdapter adaptadorParaTabla = new SqlDataAdapter(comandoParaConsulta);
            conexion.Open();
            adaptadorParaTabla.Fill(tablaResultado);
            conexion.Close();

            foreach (DataRow columna in tablaResultado.Rows)
            {
                nombresCursos.Add(Convert.ToString(columna["nombrePK"]));
            }
            return nombresCursos;
        }

        public List<string> obtenerTopicos(List<string> categoriasPadre)
        {
            List<string> topicos = new List<string>();
            if (categoriasPadre != null)
            {
                string consulta = "SELECT * FROM CategoriaTopico WHERE ";
                foreach (string categoria in categoriasPadre)
                {
                    consulta += "categoriaPadreFK = '" + categoria + "' OR ";
                }
                consulta = consulta.TrimEnd(new char[] { ' ', 'O', 'R' }) + ";";
                DataTable tablaResultado = consultarTabla(consulta);
                foreach (DataRow columna in tablaResultado.Rows)
                {
                    topicos.Add(Convert.ToString(columna["nombrePK"]));
                }
            }
            return topicos;
        }

        public int editarCurso(CursoModel curso)
        {
            curso.version = obtenerUltimaVersion(curso.nombre);
            curso.categorias = obtenerCategoriasCurso(curso.nombre);
            curso.topicos = obtenerTopicosCurso(curso.nombre);

            crearCurso(curso, "INSERT");
            return curso.version;
        }

        public void crearCurso(CursoModel curso, string tipo = "UPDATE")
        {
            curso.version += 1;
            if (crearVersion(curso, tipo))
            {
                string direccionCurso = HttpContext.Current.Server.MapPath("~/App_Data/Material_Curso/") + curso.nombre + "_" + curso.version.ToString() + "\\";
                Directory.CreateDirectory(direccionCurso);
                try
                {
                    crearSecciones(curso, direccionCurso);
                }
                catch (Exception exception)
                {
                    throw exception;
                }
            }
        }

        public bool crearVersion(CursoModel curso, string tipo)
        {
            string consulta;
            SqlCommand comandoParaConsulta;
            if (tipo.Equals("UPDATE"))
            {
                consulta = "UPDATE Curso SET versionPK = @version "
                         + "WHERE nombrePK = @nombre AND versionPK = 0";
                comandoParaConsulta = new SqlCommand(consulta, conexion);
            }
            else if (tipo.Equals("INSERT"))
            {
                consulta = "INSERT INTO Curso (nombrePK, descripcion, contenido, estado, educadorFK, costo, versionPK) "
                         + "VALUES (@nombre, @descripcion, @contenido, @estado, @educador, @costo, @version) ";
                comandoParaConsulta = new SqlCommand(consulta, conexion);
                comandoParaConsulta.Parameters.AddWithValue("@descripcion", curso.descripcion);
                comandoParaConsulta.Parameters.AddWithValue("@contenido", curso.contenidos);
                comandoParaConsulta.Parameters.AddWithValue("@estado", curso.estado);
                comandoParaConsulta.Parameters.AddWithValue("@educador", curso.educador);
                comandoParaConsulta.Parameters.AddWithValue("@costo", curso.costo);
            }
            else
            {
                return false;
            }

            comandoParaConsulta.Parameters.AddWithValue("@nombre", curso.nombre);
            comandoParaConsulta.Parameters.AddWithValue("@version", curso.version);

            conexion.Open();
            bool exito = comandoParaConsulta.ExecuteNonQuery() > 0;
            conexion.Close();
            return exito;
        }

        public void crearSecciones(CursoModel curso, string direccionCurso)
        {
            foreach (var seccion in curso.secciones)
            {
                insertarSeccion(curso, seccion);
                var direccionSeccion = direccionCurso + "seccion_" + seccion.nombre + "\\";
                Directory.CreateDirectory(direccionSeccion);
                try
                {
                    crearMateriales(curso, seccion, direccionSeccion);
                }
                catch (Exception exception)
                {
                    throw exception;
                }
            }
        }

        public bool insertarSeccion(CursoModel curso, Seccion seccion)
        {
            string consulta = "INSERT INTO Seccion (cursoFK, nombrePK, descripcion, versionCursoFK) "
                            + "VALUES (@nombreCurso, @nombre, @descripcion, @version)";
            SqlCommand comandoParaConsulta = new SqlCommand(consulta, conexion);
            comandoParaConsulta.Parameters.AddWithValue("@nombreCurso", curso.nombre);
            comandoParaConsulta.Parameters.AddWithValue("@nombre", seccion.nombre);
            comandoParaConsulta.Parameters.AddWithValue("@descripcion", seccion.descripcion);
            comandoParaConsulta.Parameters.AddWithValue("@version", curso.version);
            conexion.Open();
            bool Exito = comandoParaConsulta.ExecuteNonQuery() > 0;
            conexion.Close();
            return Exito;
        }

        public void crearMateriales(CursoModel curso, Seccion seccion, string direccionSeccion)
        {
            foreach (var material in seccion.materiales)
            {
                insertarMaterial(curso, seccion, material);
                var direccionMaterial = direccionSeccion + "material_" + material.nombre + "\\";
                Directory.CreateDirectory(direccionMaterial);
                if (material.file != null && material.file.ContentLength > 0)
                {
                    guardarArchivoHttpPosted(direccionMaterial, material.file);
                }
                else if(material.link != null)
                {
                    try
                    {
                        escribirTXT(direccionMaterial, "link.txt", material.link);
                    }
                    catch (Exception exception)
                    {
                        throw exception;
                    }
                }
                else if (material.path != null)
                {
                    try
                    {
                        duplicarArchivo(direccionMaterial, material.path);
                    }
                    catch (Exception exception)
                    {
                        throw exception;
                    }
                }
            }
        }
        public bool insertarMaterial(CursoModel curso, Seccion seccion, Material material)
        {
            string consulta = "INSERT INTO Material (cursoFk, seccionFK, nombrePK, descripcion, versionCursoFK) "
                            + "VALUES (@nombreCurso, @nombreSeccion, @nombre, @descripcion, @version)";
            SqlCommand comandoParaConsulta = new SqlCommand(consulta, conexion);
            comandoParaConsulta.Parameters.AddWithValue("@nombreCurso", curso.nombre);
            comandoParaConsulta.Parameters.AddWithValue("@nombreSeccion", seccion.nombre);
            comandoParaConsulta.Parameters.AddWithValue("@nombre", material.nombre);
            comandoParaConsulta.Parameters.AddWithValue("@descripcion", material.descripcion);
            comandoParaConsulta.Parameters.AddWithValue("@version", curso.version);
            conexion.Open();
            bool Exito = comandoParaConsulta.ExecuteNonQuery() > 0;
            conexion.Close();
            return Exito;
        }

        public void guardarArchivoHttpPosted(string direccion, HttpPostedFileBase fileBase)
        {
            try
            {
                string fileName = Path.GetFileName(fileBase.FileName);
                string filePath = Path.Combine(direccion, fileName);
                fileBase.SaveAs(filePath);
            }
            catch (Exception exception)
            {
                throw (exception);
            }
        }

        public void escribirTXT(string direccion, string nombreArchivo, string linea)
        {
            try
            {
                StreamWriter sw = File.CreateText(direccion + nombreArchivo);
                sw.Write(linea);
                sw.Close();
            }
            catch (Exception exception)
            {
                throw (exception);
            }
        }

        public void duplicarArchivo(string direccionDestino, string archivoOrigen)
        {
            string archivoDestino = direccionDestino + Path.GetFileName(archivoOrigen);
            try
            {
                File.Copy(archivoOrigen, archivoDestino, true);
            }
            catch (Exception exception)
            {
                throw (exception);
            }
        }

        public List<CursoModel> obtenerCursosPorFiltros(string titulo, string categoria, string topico, int estado)
        {
            List<CursoModel> cursos = new List<CursoModel>();

            string consulta = "SELECT DISTINCT C.nombrePK, U.nombre AS 'Educador', C.descripcion, C.contenido, C.educadorFK, C.costo, C.versionPK "
                            + "FROM Usuario U "
                            + "JOIN Curso C ON U.correoPK = C.educadorFK "
                            + "JOIN Pertenece_Curso P ON P.nombreCursoFK = C.nombrePK "
                            + "JOIN CategoriaTopico T ON P.nombreTopicoFK = T.nombrePK "
                            + $"WHERE C.nombrePK like '%{titulo}%' "
                            + $"AND C.estado = {estado}";

            if (!topico.Equals("Todos"))
            {
                consulta += $" AND T.nombrePK = '{topico}' ";
            }
            if (!categoria.Equals("Todas"))
            {
                consulta += $" AND T.categoriaPadreFK = '{categoria}' ";
            }

            DataTable tablaResultado = consultarTabla(consulta);
            foreach (DataRow columna in tablaResultado.Rows)
            {
                string nombre = Convert.ToString(columna["nombrePK"]);
                cursos.Add(
                new CursoModel
                {
                    nombre = nombre,
                    descripcion = Convert.ToString(columna["descripcion"]),
                    contenidos = Convert.ToString(columna["contenido"]),
                    educador = Convert.ToString(columna["educadorFK"]),
                    topicos = obtenerTopicosCurso(nombre),
                    categorias = obtenerCategoriasCurso(nombre),
                    nombreEducador = Convert.ToString(columna["educador"]),
                    costo = Convert.ToSingle(columna["costo"]),
                    version = Convert.ToInt32(columna["versionPK"])
                });
            }
            return cursos;
        }

        public int inscribirUsuarioEnCurso(string correoUsuario, string nombreCurso, int version)
        {
            int exito = 0;
            string consulta = "INSERT INTO Participa_Curso VALUES (@nombreCurso, @correoUsuario, @porcentaje, @version)";
            SqlCommand comandoParaConsulta = new SqlCommand(consulta, conexion);
            comandoParaConsulta.Parameters.AddWithValue("@nombreCurso", nombreCurso);
            comandoParaConsulta.Parameters.AddWithValue("@correoUsuario", correoUsuario);
            comandoParaConsulta.Parameters.AddWithValue("@porcentaje", 0);
            comandoParaConsulta.Parameters.AddWithValue("@version", version);

            try
            {
                conexion.Open();
                exito = comandoParaConsulta.ExecuteNonQuery();
                conexion.Close();
            }
            catch (SqlException e)
            {
                if (e.Number == Constantes.ErroresSQL.SQL_DUPLICATE_PK)
                {
                    exito = -1;
                }
            }

            return exito;
        }

        public int obtenerPorcentaje(string correo, string nombreCurso)
        {
            int version = obtenerVersion(correo, nombreCurso);
            string consulta = "SELECT porcentajeCurso FROM Participa_Curso "
                            + $"WHERE nombreCursoFK = '{nombreCurso}' AND correoUsuarioFK = '{correo}' AND versionCursoFK = '{version}'" ;
            DataTable tablaResultado = consultarTabla(consulta);
            DataRow resultado = tablaResultado.Rows[0];
            int porcentaje = Convert.ToInt32(resultado["porcentajeCurso"]);
            return porcentaje;
        }

        public bool insertarCompletaMaterial(string nombreCurso, string correoEstudiante, string nombreMaterial, string nombreSeccion)
        {
            bool exito = true;
            int version = obtenerVersion(correoEstudiante, nombreCurso);
            string consulta = "INSERT INTO Completa_Material (nombreCursoFK, correoUsuarioFK, nombreMaterialFK, nombreSeccionFK, versionCursoFK) "
                            + "VALUES (@nombreCurso, @correoEstudiante, @nombreMaterial, @nombreSeccion, @versionCursoFK) ";
            SqlCommand comandoParaConsulta = new SqlCommand(consulta, conexion);
            comandoParaConsulta.Parameters.AddWithValue("@nombreCurso", nombreCurso);
            comandoParaConsulta.Parameters.AddWithValue("@correoEstudiante", correoEstudiante);
            comandoParaConsulta.Parameters.AddWithValue("@nombreMaterial", nombreMaterial);
            comandoParaConsulta.Parameters.AddWithValue("@nombreSeccion", nombreSeccion);
            comandoParaConsulta.Parameters.AddWithValue("@versionCursoFK", version);
            
            try
            {
                conexion.Open();
                exito = comandoParaConsulta.ExecuteNonQuery() > 0;
                conexion.Close();
            }
            catch
            {
                conexion.Close();
                exito = false;
            }
            return exito;
        }

        public List<ParticipaCurso> obtenerEstudiantesCurso(string nombreCurso, int version)
        {
            List<ParticipaCurso> estudiantes = new List<ParticipaCurso>();
            string consulta = "SELECT nombreCursoFK, correoUsuarioFK, porcentajeCurso, versionCursoFK "
                            + $"FROM Participa_Curso WHERE nombreCursoFK = '{nombreCurso}' AND versionCursoFK = {version} "
                            + "ORDER BY porcentajeCurso DESC";

            DataTable tablaResultado = consultarTabla(consulta);
            foreach (DataRow columna in tablaResultado.Rows)
            {
                estudiantes.Add(
                new ParticipaCurso
                {
                    nombreCurso = Convert.ToString(columna["nombreCursoFK"]),
                    correoEstudiante = Convert.ToString(columna["correoUsuarioFK"]),
                    porcentaje = Convert.ToInt32(columna["porcentajeCurso"]),
                    version = Convert.ToInt32(columna["versionCursoFK"])
                });
            }
            return estudiantes;
        }

        public static bool perteneceCurso(string correo, string nombreCurso)
        {
            bool exito = false;
            CursoHandler cursoHandler = new CursoHandler();
            //Llamar metodo perteneceCurso no estatico.
            string consulta = "SELECT COUNT(*) AS 'cuenta' " +
                              "FROM Participa_Curso " +
                              "WHERE nombreCursoFK = '" + nombreCurso + "' " +
                              "AND correoUsuarioFK = '" + correo + "'";
            DataTable tablaResultado = cursoHandler.consultarTabla(consulta);
            int resultado = Convert.ToInt32(tablaResultado.Rows[0]["cuenta"]);
            if(resultado > 0)
            {
                exito = true;
            }
            return exito;
        }

        public static int obtenerUltimaVersion(string nombreCurso)
        {
            CursoHandler cursoHandler = new CursoHandler();
            string consulta = "SELECT MAX(C.versionPK) AS ultimaVersion "
                            + "FROM Curso C WHERE C.nombrePK = @nombre ";
            SqlCommand comando = new SqlCommand(consulta, cursoHandler.conexion);
            comando.Parameters.AddWithValue("@nombre", nombreCurso);
            
            cursoHandler.conexion.Open();
            int resultado = Convert.ToInt32(comando.ExecuteScalar());
            cursoHandler.conexion.Close();

            return resultado;
        }

        public int obtenerVersion(string correoUsuario, string nombreCurso)
        {
            string consulta = "SELECT versionCursoFK "
                            + "FROM Participa_Curso "
                            + $"WHERE correoUsuarioFK = '{correoUsuario}' AND nombreCursoFK = '{nombreCurso}'";
            DataTable tablaResultado = consultarTabla(consulta);
            int resultado = Convert.ToInt32(tablaResultado.Rows[0]["versionCursoFK"]);

            return resultado;
        }

        public bool tengoCurso(string correoUsuario, int estado)
        {
            bool tengoCurso = false;
            string consulta = "SELECT COUNT(*) AS CantidadCursos FROM Curso WHERE educadorFK = @correoUsuario AND estado = @estado";
            SqlCommand comandoParaConsulta = new SqlCommand(consulta, conexion);
            comandoParaConsulta.Parameters.AddWithValue("@correoUsuario", correoUsuario);
            comandoParaConsulta.Parameters.AddWithValue("@estado", estado);
            SqlDataAdapter adaptadorParaTabla = new SqlDataAdapter(comandoParaConsulta);
            DataTable tablaResultado = new DataTable();
            conexion.Open();
            adaptadorParaTabla.Fill(tablaResultado);
            conexion.Close();
            if (Convert.ToInt32(tablaResultado.Rows[0]["CantidadCursos"]) > 0)
            {
                tengoCurso = true;
            }
            return tengoCurso;
        }
    }
}
    



