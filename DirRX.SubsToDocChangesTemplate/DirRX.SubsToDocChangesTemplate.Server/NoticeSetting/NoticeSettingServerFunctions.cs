using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.SubsToDocChangesTemplate.NoticeSetting;

namespace DirRX.SubsToDocChangesTemplate.Server
{
  partial class NoticeSettingFunctions
  {
    /// <summary>
    /// Отправить уведомления по событию в документе.
    /// </summary>
    /// <param name="documentChange">Событие.</param>
    [Public]
    public static void CollectAndSendNoticesByEvent(DirRX.SubsToDocChangesTemplate.Structures.Module.IDocumentEvents documentChange)
    {
      var documentEvent = documentChange.DocumentEvent;
      var document = Sungero.Docflow.OfficialDocuments.Get(documentChange.DocumentId);
      var documentType = Sungero.Docflow.DocumentTypes.GetAll(t => t.DocumentTypeGuid == documentChange.EntityType).FirstOrDefault();
      
      var performers = GetPerformersForEvent(documentEvent, document, documentType, documentChange.UserId);
      
      #region Отправка уведомлений.
      
      if (performers.Any())
      {
        string defaultEventDescription = Functions.NoticeSetting.GetNoticeDescriptionByEvent(documentEvent);
        
        var serviceUsersRole = Sungero.CoreEntities.Roles.GetAll(x => x.Sid == Sungero.Domain.Shared.SystemRoleSid.ServiceUsers).FirstOrDefault();
        var serviceUser = serviceUsersRole.RecipientLinks.Where(x => x.Member.Name == "Service User").FirstOrDefault().Member;
        
        var comment = documentChange.Comment;
        var activeText = string.Empty;
        var position = comment.IndexOf("-");
        switch (documentEvent)
        {
          case "AddRelation":
            if (position >= 0)
              activeText = DirRX.SubsToDocChangesTemplate.NoticeSettings.Resources.AddRelationActiveTextFormat(comment.Substring(position + 2));
            SendNotice(document, defaultEventDescription, performers.Keys.ToList(), serviceUser, activeText);
            break;
          case "RemoveRelation":
            if (position >= 0)
              activeText = DirRX.SubsToDocChangesTemplate.NoticeSettings.Resources.RemoveRelationActiveTextFormat(comment.Substring(position + 2));
            SendNotice(document, defaultEventDescription, performers.Keys.ToList(), serviceUser, activeText);
            break;
          default:
            SendNotice(document, defaultEventDescription, performers.Keys.ToList(), serviceUser, activeText);
            break;
            
        }
      }
      
      #endregion
    }
    
    /// <summary>
    /// Отправить уведомление.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <param name="eventDescription">Тема по-умолчанию.</param>
    /// <param name="performers">Получатели уведомлений.</param>
    /// <param name="serviceUser">Пользователь от которого отправляется уведомление.</param>
    /// <param name="activeText">Текст уведомления.</param>
    private static void SendNotice(Sungero.Docflow.IOfficialDocument document,
                                   string eventDescription,
                                   List<Sungero.Company.IEmployee> performers,
                                   Sungero.CoreEntities.IRecipient serviceUser,
                                   string activeText)
    {
      var notice = Sungero.Workflow.SimpleTasks.Create();
      
      notice.ActiveText = activeText;
      notice.Attachments.Add(document);
      notice.Subject = string.Format("{0}. {1}", eventDescription, document.Name);
      notice.NeedsReview = false;

      foreach (var performer in performers)
      {
        var routeStep = notice.RouteSteps.AddNew();
        routeStep.AssignmentType = Sungero.Workflow.SimpleTaskRouteSteps.AssignmentType.Notice;
        routeStep.Performer = performer;
        routeStep.Deadline = null;
      }
      
      notice.Author = Sungero.CoreEntities.Users.As(serviceUser);
      notice.StartedBy = Sungero.CoreEntities.Users.As(serviceUser);
      
      if (notice.Subject.Length > Sungero.Workflow.Tasks.Info.Properties.Subject.Length)
        notice.Subject = notice.Subject.Substring(0, Sungero.Workflow.Tasks.Info.Properties.Subject.Length);
      
      notice.Start();
    }
    
    /// <summary>
    /// Получить сотрудников для отправки уведомлений.
    /// </summary>
    /// <param name="documentEvent">Событие.</param>
    /// <param name="document">Документ.</param>
    /// <param name="documentType">Тип документа.</param>
    /// <param name="eventInitiatorId">ИД пользователя инициатора события.</param>
    /// <returns>Список сотрудников (словарь: Key - сотрудник, Value - Роль).</returns>
    [Public]
    public static System.Collections.Generic.Dictionary<Sungero.Company.IEmployee, Enumeration> GetPerformersForEvent(string documentEvent, Sungero.Docflow.IOfficialDocument document,
                                                                                                                      Sungero.Docflow.IDocumentType documentType, long eventInitiatorId)
    {
      var performers = new Dictionary<Sungero.Company.IEmployee, Enumeration>();
      
      // Автор.
      AddPerformer(performers, Sungero.Company.Employees.As(document.Author), DirRX.SubsToDocChangesTemplate.NoticeSetting.Role.Author, documentType, documentEvent, eventInitiatorId);
      
      // Исполнитель (Входящее письмо, Приказ, Распоряжение, Служебная записка).
      AddPerformer(performers, document.Assignee, DirRX.SubsToDocChangesTemplate.NoticeSetting.Role.Assignee, documentType, documentEvent, eventInitiatorId);
      
      // Подписант (все, кроме Входящее письмо).
      AddPerformer(performers, document.OurSignatory, DirRX.SubsToDocChangesTemplate.NoticeSetting.Role.Signatory, documentType, documentEvent, eventInitiatorId);
      
      //	Согласующий (все, кроме Входящее письмо).
      var approvalTaskDocumentGroupGuid = Sungero.Docflow.PublicConstants.Module.TaskMainGroup.ApprovalTask;
      var approvalTasks = Sungero.Docflow.ApprovalTasks.GetAll(t => t.AttachmentDetails.Any(att => att.AttachmentId == document.Id &&
                                                                                            att.EntityTypeGuid.ToString() == documentType.DocumentTypeGuid &&
                                                                                            att.GroupId == approvalTaskDocumentGroupGuid));
      var approvalManagerAssignments = Sungero.Docflow.ApprovalManagerAssignments.GetAll(a => approvalTasks.Contains(Sungero.Docflow.ApprovalTasks.As(a.Task)));
      var approvalAssignments = Sungero.Docflow.ApprovalAssignments.GetAll(a => approvalTasks.Contains(Sungero.Docflow.ApprovalTasks.As(a.Task)));
      
      if (approvalManagerAssignments.Any())
      {
        var approvers = approvalManagerAssignments.Select(a => a.Performer).Distinct().ToList();
        foreach (var approver in approvers)
          AddPerformer(performers, Sungero.Company.Employees.As(approver), DirRX.SubsToDocChangesTemplate.NoticeSetting.Role.Approver, documentType, documentEvent, eventInitiatorId);
      }
      
      if (approvalAssignments.Any())
      {
        var approvers = approvalAssignments.Select(a => a.Performer).Distinct().ToList();
        foreach (var approver in approvers)
          AddPerformer(performers, Sungero.Company.Employees.As(approver), DirRX.SubsToDocChangesTemplate.NoticeSetting.Role.Approver, documentType, documentEvent, eventInitiatorId);
      }
      
      //	Подготовил (Исходящее письмо, Приказ, Распоряжение, Служебная записка).
      AddPerformer(performers, document.PreparedBy, DirRX.SubsToDocChangesTemplate.NoticeSetting.Role.PreparedBy, documentType, documentEvent, eventInitiatorId);
      
      //	Ответственный (Договор, Дополнительное соглашение).
      var contractualDocument = Sungero.Contracts.ContractualDocuments.As(document);
      if (contractualDocument != null)
        AddPerformer(performers, contractualDocument.ResponsibleEmployee, DirRX.SubsToDocChangesTemplate.NoticeSetting.Role.Responsible, documentType, documentEvent, eventInitiatorId);
      
      //	Адресат (Входящее письмо, Служебная записка).
      var incomingLetter = Sungero.RecordManagement.IncomingLetters.As(document);
      if (incomingLetter != null)
        AddPerformer(performers, incomingLetter.Addressee, DirRX.SubsToDocChangesTemplate.NoticeSetting.Role.Addressee, documentType, documentEvent, eventInitiatorId);
      
      var memo = Sungero.Docflow.Memos.As(document);
      if (memo != null)
        AddPerformer(performers, memo.Addressee, DirRX.SubsToDocChangesTemplate.NoticeSetting.Role.Addressee, documentType, documentEvent, eventInitiatorId);

      return performers;
    }

    /// <summary>
    /// Добавить участника процесса.
    /// </summary>
    /// <param name="performers">Список текущих участников.</param>
    /// <param name="newPerformer">Новый участник.</param>
    /// <param name="role">Роль.</param>
    /// <param name="documentType">Тип документа.</param>
    /// <param name="documentEvent">Событие.</param>
    /// <param name="eventInitiatorId">ИД пользователя инициатора события.</param>
    public static void AddPerformer(System.Collections.Generic.Dictionary<Sungero.Company.IEmployee, Enumeration> performers,
                                    Sungero.Company.IEmployee newPerformer,
                                    Enumeration role,
                                    Sungero.Docflow.IDocumentType documentType,
                                    string documentEvent, long eventInitiatorId)
    {
      if (newPerformer == null ||
          performers.Keys.Any(e => Sungero.Company.Employees.Equals(e, newPerformer)) || Sungero.CoreEntities.Users.As(newPerformer).Id == eventInitiatorId ||
          !NeedNotice(documentEvent, newPerformer, role, documentType))
        return;
      
      performers.Add(newPerformer, role);
    }
    
    /// <summary>
    /// Получить настройку для пользователя.
    /// </summary>
    /// <param name="employee">Пользователь.</param>
    /// <param name="role">Роль.</param>
    /// <param name="documentType">Тип документа.</param>
    /// <returns>Настройка уведомлений</returns>
    public static INoticeSetting GetSetting(Sungero.Company.IEmployee employee, Enumeration role, Sungero.Docflow.IDocumentType documentType)
    {
      var employeeSetting = NoticeSettings.GetAll().SingleOrDefault(s => s.Role == role && Sungero.Docflow.DocumentTypes.Equals(s.DocumentType, documentType) &&
                                                                    Sungero.Company.Employees.Equals(s.Employee, employee));
      if (employeeSetting == null)
        employeeSetting = NoticeSettings.GetAll().SingleOrDefault(s => s.Role == role && Sungero.Docflow.DocumentTypes.Equals(s.DocumentType, documentType) &&
                                                                  s.AllUsersFlag.HasValue && s.AllUsersFlag.Value);
      
      return employeeSetting;
    }
    
    /// <summary>
    /// Проверить соответствие настроек.
    /// </summary>
    /// <param name="documentEvent">Событие.</param>
    /// <param name="performer">Исполнитель.</param>
    /// <param name="role">Роль.</param>
    /// <param name="documentType">Тип документа.</param>
    /// <returns>True, если необходимо отправить уведомление.</returns>
    public static bool NeedNotice(string documentEvent, Sungero.Company.IEmployee performer, Enumeration role, Sungero.Docflow.IDocumentType documentType)
    {
      var setting = GetSetting(performer, role, documentType);
      if (setting == null)
        return false;
      
      switch (documentEvent)
      {
        case "CreateVersion":
          return setting.IsVersionCreated.GetValueOrDefault();
        case "DeleteVersion":
          return setting.IsVersionDeleted.GetValueOrDefault();
        case "AddRelation":
          return setting.IsRelationAdded.GetValueOrDefault();
        case "RemoveRelation":
          return setting.IsRelationDeleted.GetValueOrDefault();
        case "EnOnApproval":
          return setting.IsSentToApproval.GetValueOrDefault();
        case "Registration":
          return setting.IsRegistered.GetValueOrDefault();
        case "Unregistration":
          return setting.IsUnRegistered.GetValueOrDefault();
        case "Approve":
          return setting.IsSigned.GetValueOrDefault();
        case "EnOnRework":
          return setting.IsSentForRework.GetValueOrDefault();
        case "EnAborted":
          return setting.IsApprovalAborted.GetValueOrDefault();
        case "EnReviewed":
          return setting.IsSentToReview.GetValueOrDefault();
        case "ExOnExecution":
          return setting.IsSentToExecution.GetValueOrDefault();
        case "ExExecuted":
          return setting.IsExecuted.GetValueOrDefault();
        case "ExAborted":
          return setting.IsExecutionAborted.GetValueOrDefault();
        case "CEOnApproval":
          return setting.IsSentToCounterparty.GetValueOrDefault();
        case "CESigned":
          return setting.IsSignedByCounterparty.GetValueOrDefault();
        case "CEUnsigned":
          return setting.IsSignedByCounterparty.GetValueOrDefault();
        case "TAChange":
          return setting.IsTotalAmountChanged.GetValueOrDefault();

          default: return false;
      }
    }

    /// <summary>
    /// Проверить наличие записи настроек, где текущий пользователь исполнитель той же роли.
    /// </summary>
    /// <param name="role">Роль.</param>
    /// <param name="documentType">Тип документа.</param>
    /// <returns>Признак наличия записи.</returns>
    [Remote(IsPure = true)]
    public bool IsSameRoleSettingExists(Enumeration role, Sungero.Docflow.IDocumentType documentType)
    {
      if (_obj.Employee == null)
        return NoticeSettings.GetAll(s => s.Role.Value == role &&
                                     s.AllUsersFlag.HasValue &&
                                     s.AllUsersFlag == true &&
                                     Sungero.Docflow.DocumentTypes.Equals(s.DocumentType, documentType) &&
                                     !NoticeSettings.Equals(s, _obj)).Any();
      else
        return NoticeSettings.GetAll(s => s.Role.Value == role &&
                                     Sungero.Company.Employees.Equals(s.Employee, _obj.Employee) &&
                                     Sungero.Docflow.DocumentTypes.Equals(s.DocumentType, documentType) &&
                                     !NoticeSettings.Equals(s, _obj)).Any();
    }
    
    /// <summary>
    /// Получить похожую запись настроек.
    /// </summary>
    /// <param name="setting">Создаваемая настройка.</param>
    /// <returns>Настройка.</returns>
    [Remote(IsPure = true)]
    public INoticeSetting GetSameSetting()
    {
      return NoticeSettings.GetAll(s => s.Role == _obj.Role &&
                                   ((_obj.Employee != null && Sungero.Company.Employees.Equals(s.Employee, _obj.Employee)) ||
                                    (_obj.Employee == null && _obj.AllUsersFlag == true && s.AllUsersFlag == true)) &&
                                   Sungero.Docflow.DocumentTypes.Equals(s.DocumentType, _obj.DocumentType) &&
                                   !NoticeSettings.Equals(s, _obj)).FirstOrDefault();
    }
    
    /// <summary>
    /// Получить настройку для всех пользователей.
    /// </summary>
    /// <param name="role">Роль.</param>
    /// <param name="documentType">Тип документа.</param>
    /// <returns>Настройка.</returns>
    [Remote(IsPure = true)]
    public INoticeSetting GetAllUserSetting()
    {
      return NoticeSettings.GetAll(s => s.Role.Value == _obj.Role &&
                                     s.AllUsersFlag.HasValue &&
                                     s.AllUsersFlag == true &&
                                     Sungero.Docflow.DocumentTypes.Equals(s.DocumentType, _obj.DocumentType)).FirstOrDefault();
    }

  }
}