
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace form.Services
{
    public static class SqliteSchemaBootstrap
    {
        private const string ConnectionString = "Data Source=usuarios.db";

        public static void EnsureSchema()
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            EnableForeignKeys(conn);

            // --- Formularios: agregar columnas si faltan ---
            var columnasForm = GetColumnas(conn, "Formularios");

            if (!columnasForm.Contains("EstadoRevision"))
            {
                Exec(conn, "ALTER TABLE Formularios ADD COLUMN EstadoRevision TEXT DEFAULT 'Aprobado';");
            }
            if (!columnasForm.Contains("UpdatedAt"))
            {
                Exec(conn, "ALTER TABLE Formularios ADD COLUMN UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP;");
            }

            // --- Auditoria: agregar columna Detalle si falta ---
            var columnasAud = GetColumnas(conn, "Auditoria");
            if (!columnasAud.Contains("Detalle"))
            {
                Exec(conn, "ALTER TABLE Auditoria ADD COLUMN Detalle TEXT;");
            }

            // --- SolicitudesEdicion: crear si no existe ---
            Exec(conn, @"
                CREATE TABLE IF NOT EXISTS SolicitudesEdicion (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    FormularioId INTEGER NOT NULL,
                    PropuestoPor TEXT NOT NULL,
                    Motivo TEXT,
                    Estado TEXT NOT NULL DEFAULT 'Pendiente',  -- Pendiente|Aprobado|Rechazado
                    AntesJson TEXT NOT NULL,
                    DespuesJson TEXT NOT NULL,
                    CambiosJson TEXT NOT NULL,
                    FechaPropuesta DATETIME DEFAULT CURRENT_TIMESTAMP,
                    RevisadoPor TEXT,
                    FechaRevision DATETIME,
                    FOREIGN KEY (FormularioId) REFERENCES Formularios(Id) ON DELETE CASCADE
                );
            ");
        }

        private static HashSet<string> GetColumnas(SqliteConnection conn, string tabla)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"PRAGMA table_info({tabla});";
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                var nombre = r["name"]?.ToString();
                if (!string.IsNullOrWhiteSpace(nombre))
                    set.Add(nombre);
            }
            return set;
        }

        private static void Exec(SqliteConnection conn, string sql)
        {
            using var c = conn.CreateCommand();
            c.CommandText = sql;
            c.ExecuteNonQuery();
        }

        private static void EnableForeignKeys(SqliteConnection conn)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "PRAGMA foreign_keys = ON;";
            cmd.ExecuteNonQuery();
        }
    }
}
