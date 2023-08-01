using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using System.IO;

namespace DirRX.SubsToDocChangesTemplate.Server
{
  public class ModuleJobs
  {

    /// <summary>
    /// Отправка уведомлений по документам.
    /// </summary>
    public virtual void SendDocumentNotices()
    {
      Logger.DebugFormat("Старт фонового процесса: \"Отправка уведомлений по документам\".");
      var today = Calendar.Now;
      var paramKey = Constants.Module.SendDocumentNoticesLastDateDocflowParamName;
      var previousDate = GetLastExecutionProccessDateTime(paramKey);
      if (!previousDate.HasValue)
        previousDate = Calendar.BeginningOfDay(today);
      
      // Получить настройки уведомлений.
      var settings = NoticeSettings.GetAll(s => s.Status == Sungero.CoreEntities.DatabookEntry.Status.Active);
      if (settings.Any())
      {
        var docTypes = settings.Select(s => s.DocumentType.DocumentTypeGuid.ToUpper()).Distinct().ToList();
        var docTypesString = string.Join("','", docTypes);
        
        using (var commandManager = SQL.GetCurrentConnection().CreateCommand())
        {
          commandManager.CommandText = string.Format(Queries.Module.SelectNewDocChanges, docTypesString);
          var parameter = commandManager.CreateParameter();
          parameter.ParameterName = "@previousDate";
          parameter.Direction = System.Data.ParameterDirection.Input;
          parameter.DbType = System.Data.DbType.DateTime;
          parameter.Value = previousDate.Value;
          commandManager.Parameters.Add(parameter);
          
          var commantText = commandManager.CommandText;
          
          var documentsChanges = new List<Structures.Module.IDocumentEvents>();
          var reader = commandManager.ExecuteReader();
          try
          {
            while (reader.Read())
            {
              var documentId = SafeGetLong(reader, reader.GetOrdinal("EntityId")) ?? 0;
              var eventInitiatorId = SafeGetLong(reader, reader.GetOrdinal("User")) ?? 0;
              var entityType = SafeGetGuid(reader, reader.GetOrdinal("EntityType"));
              var documentEvent = SafeGetString(reader, reader.GetOrdinal("ActionName"));
              var comment = SafeGetString(reader, reader.GetOrdinal("Comment"));
              
              var documentChange = Structures.Module.DocumentEvents.Create(documentId, eventInitiatorId, entityType, documentEvent, comment);
              documentsChanges.Add(documentChange);
            }
          }
          finally
          {
            reader.Close();
          }
          
          foreach (var change in documentsChanges)
            DirRX.SubsToDocChangesTemplate.PublicFunctions.NoticeSetting.CollectAndSendNoticesByEvent(change);
        }
        
      }
      UpdateLastNotificationDate(paramKey, today);
    }
    
    #region Запись и считывание дат последнего выполнения фоновых процессов
    
    /// <summary>
    /// Получить дату последнего выполнения фонового процесса.
    /// </summary>
    /// <param name="key">Имя ключа в таблице параметров.</param>
    /// <returns>Дата последнего выполнения.</returns>
    public static DateTime? GetLastExecutionProccessDateTime(string key)
    {
      var command = string.Format(Queries.Module.SelectDocflowParamsValue, key);
      try
      {
        var executionResult = Sungero.Docflow.PublicFunctions.Module.ExecuteScalarSQLCommand(command);
        var dateString = string.Empty;
        if ((executionResult is DBNull) || string.IsNullOrWhiteSpace(executionResult.ToString()))
          return null;

        dateString = executionResult.ToString();
        Logger.DebugFormat("Время последнего выполнения данного фонового процесса с ключом {0} записанное в БД: {1} (UTC)", key, dateString);

        return Calendar.FromUtcTime(DateTime.Parse(dateString, null, System.Globalization.DateTimeStyles.AdjustToUniversal));
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("При получении времени последнего выполнения фонового процесса с ключом {0} возникла ошибка", ex, key);
        return null;
      }
    }
    
    /// <summary>
    /// Обновить дату последней рассылки уведомлений.
    /// </summary>
    /// <param name="key">Имя ключа в таблице параметров.</param>
    /// <param name="notificationDate">Дата рассылки уведомлений.</param>
    public static void UpdateLastNotificationDate(string key, DateTime notificationDate)
    {
      var newDate = notificationDate.Add(-Calendar.UtcOffset).ToString("yyyy-MM-ddTHH:mm:ss.ffff+0");
      Sungero.Docflow.PublicFunctions.Module.ExecuteSQLCommandFormat(Queries.Module.InsertOrUpdateDocflowParamsValue, new[] { key, newDate });
      Logger.DebugFormat("Зафиксировано время выполнения фонового процесса с ключом {0} {1} (UTC)", key, newDate);
    }
    
    #endregion
    
    #region Получить строку из значения System.Data.IDataReader
    
    public static string SafeGetString(System.Data.IDataReader reader, int colIndex)
    {
      if(!reader.IsDBNull(colIndex))
        return reader.GetString(colIndex);
      return string.Empty;
    }
    
    #endregion
    
    #region Получить строку из значения System.Data.IDataReader
    
    public static string SafeGetGuid(System.Data.IDataReader reader, int colIndex)
    {
      if(!reader.IsDBNull(colIndex))
        return reader.GetGuid(colIndex).ToString();
      return string.Empty;
    }
    
    #endregion
    
    #region Получить целое число из значения System.Data.IDataReader
    
    public static long? SafeGetLong(System.Data.IDataReader reader, int colIndex)
    {
      if(!reader.IsDBNull(colIndex))
        return reader.GetInt64(colIndex);
      return null;
    }
    
    #endregion
    
    #region Получить дату из значения System.Data.IDataReader
    
    public static DateTime? SafeGetDateTime(System.Data.IDataReader reader, int colIndex)
    {
      if(!reader.IsDBNull(colIndex))
        return reader.GetDateTime(colIndex);
      return null;
    }
    
    #endregion

  }
}