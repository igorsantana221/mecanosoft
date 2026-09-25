using System;

namespace MonkOrc.Api.Helpers
{
    public static class AppTime
    {
        /// <summary>
        /// Retorna o horário atual no fuso horário de Brasília (UTC-3).
        /// </summary>
        public static DateTime Now()
        {
            try
            {
                // Tenta buscar o fuso horário de Brasília (funciona em Windows e Linux)
                var timeZoneId = OperatingSystem.IsWindows() 
                    ? "E. South America Standard Time" 
                    : "America/Sao_Paulo";
                
                var brtTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, brtTimeZone);
            }
            catch (TimeZoneNotFoundException)
            {
                // Fallback de segurança caso o sistema operacional não tenha o timezone configurado
                return DateTime.UtcNow.AddHours(-3);
            }
        }
    }
}
