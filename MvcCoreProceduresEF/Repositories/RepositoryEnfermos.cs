using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MvcCoreProceduresEF.Data;
using MvcCoreProceduresEF.Models;
using System.Data;
using System.Data.Common;

namespace MvcCoreProceduresEF.Repositories
{
    #region STORED PROCEDURES
    /*
    create procedure SP_ALL_ENFERMOS
    as
	    select * from ENFERMO
    go
    create procedure SP_FIND_ENFERMO
    (@inscripcion nvarchar(50))
    as
	    select * from ENFERMO
	    where INSCRIPCION=@inscripcion
    go
    create procedure SP_DELETE_ENFERMO
    (@inscripcion nvarchar(50))
    as
	    delete from ENFERMO where INSCRIPCION=@inscripcion
    go
    create procedure SP_INSERT_ENFERMO
    (@apellido nvarchar(50), @direccion nvarchar(50)
    , @fechanac datetime, @sexo nvarchar(1), @nss nvarchar(12))
    as
        declare @max int
        select @max = CAST(max(INSCRIPCION) AS INT) + 1
        from ENFERMO
	    insert into ENFERMO values (@max, @apellido, @direccion
        , @fechanac, @sexo, @nss)
    go
     */
    #endregion

    public class RepositoryEnfermos
    {
        private HospitalContext context;

        public RepositoryEnfermos(HospitalContext context)
        {
            this.context = context;
        }

        public async Task<List<Enfermo>> GetEnfermosAsync()
        {
            //NECESITAMOS UN COMMAND, VAMOS A UTILIZAR UN using
            //PARA TODO
            //EL COMMAND, EN SU CREACION, NECESITA DE UNA CADENA DE 
            //CONEXION (OBJETO)
            //EL OBJETO CONNECTION NOS LO OFRECE EF
            //las conexiones se crean a partir del context
            using (DbCommand com =
                this.context.Database.GetDbConnection().CreateCommand())
            {
                string sql = "SP_ALL_ENFERMOS";
                com.CommandType = CommandType.StoredProcedure;
                com.CommandText = sql;
                //ABRIMOS LA CONEXION A PARTIR DEL COMMAND
                await com.Connection.OpenAsync();
                //EJECUTAMOS NUESTRO READER
                DbDataReader reader = await com.ExecuteReaderAsync();
                //DEBEMOS MAPEAR LOS DATOS MANUALMENTE
                List<Enfermo> enfermos = new List<Enfermo>();
                while (await reader.ReadAsync())
                {
                    Enfermo enfermo = new Enfermo
                    {
                        Inscripcion = reader["INSCRIPCION"].ToString(),
                        Apellido = reader["APELLIDO"].ToString(),
                        Direccion = reader["DIRECCION"].ToString(),
                        FechaNacimiento = DateTime.Parse
                        (reader["FECHA_NAC"].ToString()),
                        Genero = reader["S"].ToString(),
                        Nss = reader["NSS"].ToString()
                    };
                    enfermos.Add(enfermo);
                }
                await reader.CloseAsync();
                await com.Connection.CloseAsync();
                return enfermos;
            }
        }

        public async Task<Enfermo> FindEnfermoAsync(string inscripcion)
        {
            //PARA LLAMAR A UN PROCEDIMIENTO QUE CONTIENE PARAMETROS
            //LA LLAMADA SE REALIZA MEDIANTE EL NOMBRE DEL PROCEDURE
            //Y CADA PARAMETRO A CONTINUACION EN LA DECLARACION 
            //DEL SQL: SP_PROCEDURE @PAM1, @PAM2
            string sql = "SP_FIND_ENFERMO @inscripcion";
            SqlParameter pamIns = new SqlParameter("@inscripcion"
                , inscripcion);
            //SI LOS DATOS QUE DEVUELVE EL PROCEDURE ESTAN MAPEADOS
            //CON UN MODEL, PODEMOS UTILIZAR EL METODO 
            //FromSqlRaw PARA RECUPERAR DIRECTAMENTE EL MODEL/S
            //NO PODEMOS CONSULTAR Y EXTRAER A LA VEZ CON LINQ, SE DEBE 
            //REALIZAR SIEMPRE EN DOS PASOS
            var consulta = 
                this.context.Enfermos.FromSqlRaw(sql, pamIns);
            //DEBEMOS UTILIZAR AsEnumerable() PARA EXTRAER LOS DATOS
            Enfermo enfermo = await
                consulta.ToAsyncEnumerable().FirstOrDefaultAsync();
            return enfermo;
        }

        public async Task DeleteEnfermoAsync(string inscripcion)
        {
            string sql = "SP_DELETE_ENFERMO";
            SqlParameter pamIns =
                new SqlParameter("@inscripcion", inscripcion);
            using (DbCommand com =
                this.context.Database.GetDbConnection().CreateCommand())
            {
                com.CommandType = CommandType.StoredProcedure;
                com.CommandText = sql;
                com.Parameters.Add(pamIns);
                await com.Connection.OpenAsync();
                await com.ExecuteNonQueryAsync();
                await com.Connection.CloseAsync();
                com.Parameters.Clear();
            }
        }

        public async Task DeleteEnfermoRawAsync(string inscripcion)
        {
            string sql = "SP_DELETE_ENFERMO @inscripcion";
            SqlParameter pamIns =
                new SqlParameter("@inscripcion", inscripcion);
            await this.context.Database
                .ExecuteSqlRawAsync(sql, pamIns);
        }

        public async Task CreateEnfermoAsync
            (string apellido, string direccion, DateTime fechanacimiento
            , string sexo, string nss)
        {
            string sql = "SP_INSERT_ENFERMO @apellido, @direccion " 
                + ", @fechanac, @sexo, @nss";
            SqlParameter pamApe =
                new SqlParameter("@apellido", apellido);
            SqlParameter pamDir =
                new SqlParameter("@direccion", direccion);
            SqlParameter pamFecha =
                new SqlParameter("@fechanac", fechanacimiento);
            SqlParameter pamSe =
                new SqlParameter("@sexo", sexo);
            SqlParameter pamNs =
                new SqlParameter("@nss", nss);
            await this.context.Database
                .ExecuteSqlRawAsync(sql, pamApe, pamDir
                , pamFecha, pamSe, pamNs);
        }
    }
}
