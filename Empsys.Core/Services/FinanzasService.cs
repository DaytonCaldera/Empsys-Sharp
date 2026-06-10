using System;
using Empsys.Core.Models;

namespace Empsys.Core.Services
{
    public class FinanzasService
    {
        /// <summary>
        /// Mantiene la esencia de la función 'intereses' de Pascal pero corrigiendo precisión y cálculo de tiempo.
        /// </summary>
        public decimal CalcularIntereses(Contrato contrato, DateTime fechaCorte)
        {
            if (contrato.MontoPrestamo <= 0 || contrato.TasaInteresMensual <= 0)
                return 0;

            // 1. Calcular la diferencia exacta de tiempo entre la fecha de corte (hoy/pago) y el inicio
            TimeSpan diferenciaTiempo = fechaCorte - contrato.FechaInicio;

            if (diferenciaTiempo.Days <= 0) return 0;

            // 2. Esencia del negocio: Convertir días a meses comerciales (30 días por mes)
            // Si el cliente se pasa un solo día del mes, las casas de empeño suelen cobrar el mes completo.
            // Usamos Math.Ceiling para redondear hacia arriba (Regla Comercial tipica).
            double mesesCalculados = Math.Ceiling(diferenciaTiempo.TotalDays / 30.0);

            // Si prefieres que sea exacto por día (fraccionario), cambiarías la línea de arriba por:
            // decimal mesesCalculados = (decimal)(diferenciaTiempo.TotalDays / 30.0);

            // 3. Aplicar la fórmula matemática usando tipo 'decimal' para evitar pérdida de centavos
            decimal factorInteres = contrato.TasaInteresMensual / 100;
            decimal totalInteres = contrato.MontoPrestamo * factorInteres * (decimal)mesesCalculados;

            // Redondeamos a 2 decimales estándar de moneda
            return Math.Round(totalInteres, 2);
        }
    }
}