using System;
using System.Collections.Generic;
using ComunidadPractica.Models;
using ComunidadPractica.Controllers;
using System.Data;
using System.Data.SqlClient;


namespace ComunidadPractica.Handlers
{
	public class EncuestaHandler : BaseHandler
	{
		public List<EncuestaModel> obtenerEncuestasPorPagina(int pagina, int tuplasPorPagina)
		{
			List<EncuestaModel> encuestas = new List<EncuestaModel>();
			string consulta = "SELECT E.encuestaIdPK, E.titulo, FORMAT(E.fechaCierre, 'yyyy-MM-dd') as fechaCierre"
							+ " FROM Encuesta E"
							+ " ORDER BY E.fechaCierre DESC OFFSET " + (pagina * tuplasPorPagina)
							+ " ROWS FETCH NEXT " + tuplasPorPagina + " ROWS ONLY";

			DataTable tablaResultado = consultarTabla(consulta);
			foreach (DataRow columna in tablaResultado.Rows)
			{
				encuestas.Add(new EncuestaModel
				{
					id = Convert.ToInt32(columna["encuestaIdPK"]),
					titulo = Convert.ToString(columna["titulo"]),
					fechaCierre = Convert.ToString(columna["fechaCierre"])
				});
			}
			return encuestas;
		}

		public List<EncuestaModel> obtenerEncuestasPorFiltros(string titulo, string categoria, string topico) {

			List<EncuestaModel> encuestas = new List<EncuestaModel>();
			string consulta;

			if (categoria.Length == 0) 
			{
				categoria = "Todas";
			}
			if (topico.Length == 0)
			{
				topico = "Todos";
			}

			if (categoria.Equals("Todas") && topico.Equals("Todos"))
			{
				// Todos
				consulta = "SELECT DISTINCT E.titulo, E.fechaCierre, E.encuestaIdPK " +
					" FROM Encuesta as E" + 
					" WHERE E.titulo like '%" + titulo + "%'";
			}
			else if (categoria.Equals("Todas") && !topico.Equals("Todos"))
			{
				// Particular en topico, pero padre como categoria
				consulta = "SELECT DISTINCT E.encuestaIdPK, E.titulo, E.fechaCierre" +
				" FROM Pertenece_Encuesta P" +
				" JOIN Encuesta E ON P.encuestaIdFK = E.encuestaIdPK" +
				" JOIN CategoriaTopico C ON P.nombreTopicoFK = C.nombrePK" +
				" WHERE C.nombrePK = '" + topico + "'" +
				" AND E.titulo like '%" + titulo + "%'";
			}
			else if (!categoria.Equals("Todas") && topico.Equals("Todos"))
			{
				// Particular en topico, pero padre como categoria
				consulta = "SELECT DISTINCT E.encuestaIdPK, E.titulo, E.fechaCierre" +
				" FROM Pertenece_Encuesta P" +
				" JOIN Encuesta E ON P.encuestaIdFK = E.encuestaIdPK" +
				" JOIN CategoriaTopico C ON P.nombreTopicoFK = C.nombrePK" +
				" WHERE C.categoriaPadreFK = '" + categoria + "'" +
				" AND E.titulo like '%" + titulo + "%'";
			}
			else
			{
				// Particular en ambos
				consulta = "SELECT E.encuestaIdPK, E.titulo, E.fechaCierre" +
				" FROM Pertenece_Encuesta P" +
				" JOIN Encuesta E ON P.encuestaIdFK = E.encuestaIdPK" +
				" JOIN CategoriaTopico C ON P.nombreTopicoFK = C.nombrePK" +
				" WHERE C.categoriaPadreFK = '" + categoria + "' AND C.nombrePK = '" + topico + "'" +
				" AND E.titulo like '%" + titulo + "%'";
			}

			DataTable tablaResultado = consultarTabla(consulta);
			foreach (DataRow columna in tablaResultado.Rows)
			{
				encuestas.Add(
				new EncuestaModel
				{
					id = Convert.ToInt32(columna["encuestaIdPK"]),
					titulo = Convert.ToString(columna["titulo"]),
					fechaCierre = Convert.ToString(columna["fechaCierre"])
				});
			}
			return encuestas;
		}

		public EncuestaModel obtenerEncuesta(int encuestaID)
		{
			EncuestaModel encuesta = null;
			ItemHandler itemHandler = new ItemHandler();

			string consulta = "SELECT encuestaIdPK, titulo, FORMAT(fechaCierre, 'yyyy-MM-dd') as fechaCierre FROM Encuesta WHERE encuestaIdPK = " + encuestaID;
            DataTable tablaResultado = consultarTabla(consulta);
            if (tablaResultado.Rows.Count > 0)
            {
				DataRow columna = tablaResultado.Rows[0];
				encuesta = new EncuestaModel
				{
					id = Convert.ToInt32(columna["encuestaIdPK"]),
					titulo = Convert.ToString(columna["titulo"]),
					fechaCierre = Convert.ToString(columna["fechaCierre"]),
					items = itemHandler.obtenerItemsSegunEncuesta(encuestaID)
				};

            }
			return encuesta;
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

		public List<string> obtenerTopicos(List<string> categorias)
		{
			List<string> topicos = new List<string>();
			if (categorias != null)
			{
				string consulta;
				if (categorias.Contains("Todos") || categorias.Contains("Todas"))
				{
					consulta = "SELECT nombrePk FROM CategoriaTopico WHERE categoriaPadreFK IS NOT NULL";
				}
				else
				{
					consulta = "SELECT nombrePk FROM CategoriaTopico WHERE ";
					int contador = 1;
					foreach (string categoria in categorias)
					{
						consulta += "categoriaPadreFK = '" + categoria + "' ";
						if (contador < categorias.Count)
						{
							consulta += "OR ";
						}
						contador++;
					}
					consulta = consulta.TrimEnd(new char[] { ' ', 'O', 'R' });
				}
				DataTable tablaResultado = consultarTabla(consulta);
				foreach (DataRow columna in tablaResultado.Rows)
				{
					topicos.Add(Convert.ToString(columna["nombrePk"]));
				}
			}
			return topicos;
		}

		public int crearEncuesta(EncuestaModel encuesta)
		{
			string consulta = "INSERT INTO Encuesta (titulo, fechaCierre, correoFK) "
							+ "VALUES (@titulo, @fechaCierre, @correoFK) ";

			SqlCommand comandoParaConsulta = new SqlCommand(consulta, conexion);
			comandoParaConsulta.Parameters.AddWithValue("@titulo", encuesta.titulo);
			comandoParaConsulta.Parameters.AddWithValue("@fechaCierre", encuesta.fechaCierre);
			var correoMiembro = SesionController.obtenerModeloSesion(System.Web.HttpContext.Current).correo;
			comandoParaConsulta.Parameters.AddWithValue("@correoFK", correoMiembro);

			conexion.Open();
			bool exito = comandoParaConsulta.ExecuteNonQuery() >= 1;
            conexion.Close();

            int id = -1;
			if (exito)
			{
				DataTable tablaResultado = consultarTabla("SELECT IDENT_CURRENT('Encuesta') AS 'encuestaIdPK'");
				foreach (DataRow columna in tablaResultado.Rows)
				{
					id = Convert.ToInt32(columna["encuestaIdPK"]);
				}
				insertarTopicosEncuesta(id, encuesta.topicos);
			}
			return id;
		}

		public bool contestarEncuesta(int id, List<int> numItems, List<string> respuestas, int intento)
		{
			bool exito = false;

			int preguntasMinimas = obtenerCantidadTuplas("Item", "WHERE encuestaIdFK = " + id);
			int preguntasContestadas = 0;
			if (respuestas != null)
			{
				preguntasContestadas = respuestas.Count;
			}

			//Aquí falta verificar si el numItems es nulo
			if (preguntasContestadas == preguntasMinimas)
			{
				for (int i = 0; i < numItems.Count; i++)
				{
					insertarOpcionElegida(id, numItems[i], SesionController.obtenerModeloSesion(System.Web.HttpContext.Current).correo, respuestas[i], intento);
				}
				exito = true;
			}

			return exito;
		}

		public bool eliminarEncuesta(int id)
		{
			string consulta = "DELETE FROM Encuesta WHERE encuestaIdPK = @encuestaIdPK";
			SqlCommand comandoParaConsulta = new SqlCommand(consulta, conexion);

			comandoParaConsulta.Parameters.AddWithValue("@encuestaIdPK", id);
			conexion.Open();
			bool exito = comandoParaConsulta.ExecuteNonQuery() >= 1;
			conexion.Close();

			return exito;
		}

		public int insertarItemEncuesta(int numItem, int id, string enunciado)
		{
			string consulta = "INSERT INTO Item VALUES(@numItem, @encuestaIdPK, @enunciado)";

			SqlCommand comandoParaConsulta = new SqlCommand(consulta, conexion);
			comandoParaConsulta.Parameters.AddWithValue("@numItem", numItem);
			comandoParaConsulta.Parameters.AddWithValue("@encuestaIdPK", id);
			comandoParaConsulta.Parameters.AddWithValue("@enunciado", enunciado);

			conexion.Open();
			bool exito = comandoParaConsulta.ExecuteNonQuery() >= 1;
			conexion.Close();

			int numItemId = -1;
			if (exito)
			{
				DataTable tablaResultado = consultarTabla("SELECT IDENT_CURRENT('Item') AS 'numItemPK'");
				DataRow resultado = tablaResultado.Rows[0];

				if (!resultado.IsNull(0)){
					numItemId = Convert.ToInt32(resultado["numItemPK"]);
				}
			}

			return numItemId;
		}

		public bool insertarTopicosEncuesta(int id, List<string> topicos)
		{
			string consulta = "";
			foreach (string topico in topicos)
			{
				consulta += "INSERT INTO Pertenece_Encuesta VALUES (" + id.ToString() + ", '" + topico + "') ";
			}
			SqlCommand comandoParaConsulta = new SqlCommand(consulta, conexion);
			conexion.Open();
			bool exito = comandoParaConsulta.ExecuteNonQuery() >= 1;
			conexion.Close();
			return exito;
		}

		public bool insertarOpcionElegida(int id, int numItem, string correo, string opcion, int intento)
		{
			string consulta = "INSERT INTO Responde_Item VALUES(@numItem, @encuestaID, @correo, @opcion, @intento)";

			SqlCommand comandoParaConsulta = new SqlCommand(consulta, conexion);
			comandoParaConsulta.Parameters.AddWithValue("@numItem", numItem);
			comandoParaConsulta.Parameters.AddWithValue("@encuestaID", id);
			comandoParaConsulta.Parameters.AddWithValue("@correo", correo);
			comandoParaConsulta.Parameters.AddWithValue("@opcion", opcion);
			comandoParaConsulta.Parameters.AddWithValue("@intento", intento);

			conexion.Open();
			bool exito = comandoParaConsulta.ExecuteNonQuery() >= 1;
			conexion.Close();

			return exito;
		}

		public int obtenerNumeroIntentos(string correo, int encuestaID)
        {
			int intento = 0;

			string consultaIntento = "SELECT MAX(intento) AS [ultimoIntento] FROM Responde_Item WHERE correoFK = '" +
				correo + "' AND encuestaIdFK = " + encuestaID;

			DataTable resultado = consultarTabla(consultaIntento);
			DataRow ultimoIntento = resultado.Rows[0];

			if (!ultimoIntento.IsNull(0))
            {
				intento = Convert.ToInt32(ultimoIntento["ultimoIntento"]);
            }

			return intento;
        }

		public DataTable obtenerRepuestas(int encuestaID)
        {
			string consulta = "SELECT R.correoFK AS [Encuestado], I.enunciado AS [Enunciado],R.opcionItemFK AS [Respuesta], R.intento AS [Intento], R.numItemFK AS [NumItem] FROM Responde_Item R " +
				              "JOIN Item I ON I.encuestaIdFK = R.encuestaIdFK AND I.numItemPK = R.numItemFK WHERE R.encuestaIdFK = " + encuestaID + " ORDER BY intento, numItemFK";

            DataTable resultado = consultarTabla(consulta);

            return resultado;
        }

		public int actualizarRespuestas(EncuestaModel encuesta)
        {
			int cantidadRespuestas = 0;
			Tuple<string, int> respuestaOpcion;
			string consulta;
			DataTable resultado;
			foreach (var item in encuesta.items)
            {
				cantidadRespuestas = 0;
				foreach (string opcion in item.opciones)
				{
					consulta = "SELECT COUNT (*) AS 'Conteo' FROM Responde_Item WHERE " +
						"encuestaIdFK = " + encuesta.id + " AND numItemFK = " + item.numItem + " AND opcionItemFK = '" + opcion + "';";
					resultado = consultarTabla(consulta);
					respuestaOpcion = Tuple.Create(opcion, Convert.ToInt32(resultado.Rows[0]["Conteo"]));
					item.respuestas.Add(respuestaOpcion);
					cantidadRespuestas += Convert.ToInt32(resultado.Rows[0]["Conteo"]);
				}
            }
			return cantidadRespuestas;
        }

		public int obtenerNumeroItems(int encuestaID)
        {
			int numeroItems = 0;
			string consulta = "SELECT MAX(numItemPK) AS [numItems] FROM Item WHERE encuestaIdFK = " + encuestaID;

			DataTable resultado = consultarTabla(consulta);
			DataRow ultimoItem = resultado.Rows[0];

			if (!ultimoItem.IsNull(0))
			{
				numeroItems = Convert.ToInt32(ultimoItem["numItems"]);
			}

			return numeroItems;
        }

	}
}