using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MvcCoreProceduresEF.Data;
using MvcCoreProceduresEF.Models;
using System.Data;
using System.Data.Common;

#region STORED PROCEDURES
/*
create procedure SP_ESPECIALIDADES_DOCTOR
as
	select distinct ESPECIALIDAD from DOCTOR
go
create procedure SP_UPDATE_DOCTOR_ESPE
(@especialidad nvarchar(50), @incremento int)
as
	update DOCTOR set SALARIO = SALARIO + @incremento
	where ESPECIALIDAD=@especialidad
go
create procedure SP_FINDDOCTORS_ESPE
(@especialidad nvarchar(50))
as
	select * from DOCTOR
	where ESPECIALIDAD=@especialidad
go 
 */
#endregion

namespace MvcCoreProceduresEF.Repositories
{
    public class RepositoryDoctores
    {
        private HospitalContext context;

        public RepositoryDoctores(HospitalContext context)
        {
            this.context = context;
        }

        public async Task<List<string>> GetEspecialidadesAsync()
        {
            string sql = "SP_ESPECIALIDADES_DOCTOR";
            using (DbCommand com =
                this.context.Database.GetDbConnection().CreateCommand())
            {
                com.CommandType = CommandType.StoredProcedure;
                com.CommandText = sql;
                await com.Connection.OpenAsync();
                DbDataReader reader =
                    await com.ExecuteReaderAsync();
                List<string> especialidades = new List<string>();
                while (await reader.ReadAsync())
                {
                    string espe = reader["ESPECIALIDAD"].ToString();
                    especialidades.Add(espe);
                }
                await reader.CloseAsync();
                await com.Connection.CloseAsync();
                return especialidades;
            }
        }

        public async Task<List<Doctor>> 
            GetDoctoresEspecialidadAsync(string especialidad)
        {
            string sql = "SP_FINDDOCTORS_ESPE @especialidad";
            SqlParameter pamEspe =
                new SqlParameter("@especialidad", especialidad);
            var consulta = await this.context.Doctores
                .FromSqlRaw(sql, pamEspe).ToListAsync();
            List<Doctor> doctores = consulta;
            return doctores;
        }

        public async Task UpdateDoctorEspecialidadAsync
            (string especialidad, int incremento)
        {
            string sql = "SP_UPDATE_DOCTOR_ESPE @especialidad " 
                + ", @incremento";
            SqlParameter pamEspe =
                new SqlParameter("@especialidad", especialidad);
            SqlParameter pamInc =
                new SqlParameter("@incremento", incremento);
            await this.context.Database.ExecuteSqlRawAsync
                (sql, pamInc, pamEspe);
        }

        //METODO UPDATE CON ENTITY FRAMEWORK
        public async Task UpdateDoctoresEspecialidadEFAsync
            (string especialidad, int incremento)
        {
            //DEBEMOS RECUPERAR LOS DATOS A MODIFICAR/ELIMINAR
            //DESDE EL CONTEXT
            var consulta = from datos in this.context.Doctores
                           where datos.Especialidad == especialidad
                           select datos;
            List<Doctor> doctores = await consulta.ToListAsync();
            //RECORREMOS LOS DOCTORES
            foreach (Doctor doc in doctores)
            {
                doc.Salario += incremento;
            }
            await this.context.SaveChangesAsync();
        }
    }
}
