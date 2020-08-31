using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.SubsToDocChangesTemplate.NoticeSetting;
using DocumentTypeGuid = DirRX.SubsToDocChangesTemplate.PublicConstants.Module.DocumentTypeGuid;

namespace DirRX.SubsToDocChangesTemplate.Shared
{
  partial class NoticeSettingFunctions
  {

    /// <summary>
    /// Получить описание события.
    /// </summary>
    /// <param name="documentEvent">Название события.</param>
    /// <returns>Описание события.</returns>
    public static string GetNoticeDescriptionByEvent(string documentEvent)
    {
      switch (documentEvent)
      {
          case "CreateVersion": return DirRX.SubsToDocChangesTemplate.NoticeSettings.Resources.DocumentNoticeCreateVersionDescription;
          case "DeleteVersion": return DirRX.SubsToDocChangesTemplate.NoticeSettings.Resources.DocumentNoticeDeleteVersionDescription;
          case "AddRelation": return DirRX.SubsToDocChangesTemplate.NoticeSettings.Resources.DocumentNoticeAddRelationDescription;
          case "RemoveRelation": return DirRX.SubsToDocChangesTemplate.NoticeSettings.Resources.DocumentNoticeRemoveRelationDescription;
          case "EnOnApproval": return DirRX.SubsToDocChangesTemplate.NoticeSettings.Resources.DocumentNoticeEnOnApprovalDescription;
          case "Registration": return DirRX.SubsToDocChangesTemplate.NoticeSettings.Resources.DocumentNoticeRegistrationDescription;
          case "Unregistration": return DirRX.SubsToDocChangesTemplate.NoticeSettings.Resources.DocumentNoticeUnregistrationDescription;
          case "Approve": return DirRX.SubsToDocChangesTemplate.NoticeSettings.Resources.DocumentNoticeApproveDescription;
          case "EnOnRework": return DirRX.SubsToDocChangesTemplate.NoticeSettings.Resources.DocumentNoticeEnOnReworkDescription;
          case "EnAborted": return DirRX.SubsToDocChangesTemplate.NoticeSettings.Resources.DocumentNoticeEnAbortedDescription;
          case "EnReviewed": return DirRX.SubsToDocChangesTemplate.NoticeSettings.Resources.DocumentNoticeEnReviewedDescription;
          case "ExOnExecution": return DirRX.SubsToDocChangesTemplate.NoticeSettings.Resources.DocumentNoticeExOnExecutionDescription;
          case "ExExecuted": return DirRX.SubsToDocChangesTemplate.NoticeSettings.Resources.DocumentNoticeExExecutedDescription;
          case "ExAborted": return DirRX.SubsToDocChangesTemplate.NoticeSettings.Resources.DocumentNoticeExAbortedDescription;
          case "CEOnApproval": return DirRX.SubsToDocChangesTemplate.NoticeSettings.Resources.DocumentNoticeCEOnApprovalDescription;
          case "CESigned": return DirRX.SubsToDocChangesTemplate.NoticeSettings.Resources.DocumentNoticeCESignedDescription;
          case "CEUnsigned": return DirRX.SubsToDocChangesTemplate.NoticeSettings.Resources.DocumentNoticeCEUnsignedDescription;
          case "TAChange": return DirRX.SubsToDocChangesTemplate.NoticeSettings.Resources.DocumentNoticeTAChangeDescription;
          default: return string.Empty;
      }
    }

    /// <summary>
    /// Установить доступность событий в настройках уведомлений.
    /// </summary>
    public void SetAvailabilityNoticeSettingsEvents()
    {
      var roleAndTypeDefined = _obj.DocumentType != null && _obj.Role != null;
      
      _obj.State.Properties.IsSentToApproval.IsVisible = roleAndTypeDefined && _obj.DocumentType.DocumentTypeGuid != DocumentTypeGuid.IncomingLetter && _obj.Role != Role.Approver;
      _obj.State.Properties.IsSigned.IsVisible = roleAndTypeDefined && _obj.DocumentType.DocumentTypeGuid != DocumentTypeGuid.IncomingLetter;
      _obj.State.Properties.IsSentForRework.IsVisible = roleAndTypeDefined && _obj.DocumentType.DocumentTypeGuid != DocumentTypeGuid.IncomingLetter;
      _obj.State.Properties.IsApprovalAborted.IsVisible = roleAndTypeDefined && _obj.DocumentType.DocumentTypeGuid != DocumentTypeGuid.IncomingLetter;
      _obj.State.Properties.IsRegistered.IsVisible = roleAndTypeDefined && _obj.DocumentType.DocumentTypeGuid != DocumentTypeGuid.IncomingLetter;
      _obj.State.Properties.IsUnRegistered.IsVisible = roleAndTypeDefined;
      _obj.State.Properties.IsSentToReview.IsVisible = roleAndTypeDefined &&
        (_obj.DocumentType.DocumentTypeGuid == DocumentTypeGuid.IncomingLetter || _obj.DocumentType.DocumentTypeGuid == DocumentTypeGuid.Memo);
      _obj.State.Properties.IsSentToExecution.IsVisible  = roleAndTypeDefined && _obj.DocumentType.DocumentTypeGuid != DocumentTypeGuid.OutgoingLetter
        && _obj.DocumentType.DocumentTypeGuid != DocumentTypeGuid.Contract && _obj.DocumentType.DocumentTypeGuid != DocumentTypeGuid.SupAgreement;
      _obj.State.Properties.IsExecuted.IsVisible = roleAndTypeDefined && _obj.DocumentType.DocumentTypeGuid != DocumentTypeGuid.OutgoingLetter
        && _obj.DocumentType.DocumentTypeGuid != DocumentTypeGuid.Contract && _obj.DocumentType.DocumentTypeGuid != DocumentTypeGuid.SupAgreement;
      _obj.State.Properties.IsExecutionAborted.IsVisible = roleAndTypeDefined && _obj.DocumentType.DocumentTypeGuid != DocumentTypeGuid.OutgoingLetter
        && _obj.DocumentType.DocumentTypeGuid != DocumentTypeGuid.Contract && _obj.DocumentType.DocumentTypeGuid != DocumentTypeGuid.SupAgreement;
      _obj.State.Properties.IsSentToCounterparty.IsVisible = roleAndTypeDefined &&
        (_obj.DocumentType.DocumentTypeGuid == DocumentTypeGuid.Contract || _obj.DocumentType.DocumentTypeGuid == DocumentTypeGuid.SupAgreement);
      _obj.State.Properties.IsSignedByCounterparty.IsVisible = roleAndTypeDefined &&
        (_obj.DocumentType.DocumentTypeGuid == DocumentTypeGuid.Contract || _obj.DocumentType.DocumentTypeGuid == DocumentTypeGuid.SupAgreement);     
      _obj.State.Properties.IsTotalAmountChanged.IsVisible = roleAndTypeDefined &&
        (_obj.DocumentType.DocumentTypeGuid == DocumentTypeGuid.Contract || _obj.DocumentType.DocumentTypeGuid == DocumentTypeGuid.SupAgreement);
    }
    
    /// <summary>
    /// Установить события в настройках уведомлений.
    /// </summary>
    public void SetNoticeSettingsEvents()
    {
      if (_obj.AllUsersFlag.HasValue && _obj.AllUsersFlag == false)
      {
        var allUsersSetting = Functions.NoticeSetting.Remote.GetAllUserSetting(_obj);
        if (allUsersSetting != null)
        {
          _obj.IsApprovalAborted      = allUsersSetting.IsApprovalAborted;
          _obj.IsExecuted             = allUsersSetting.IsExecuted;
          _obj.IsExecutionAborted     = allUsersSetting.IsExecutionAborted;
          _obj.IsRegistered           = allUsersSetting.IsRegistered;
          _obj.IsRelationAdded        = allUsersSetting.IsRelationAdded;
          _obj.IsRelationDeleted      = allUsersSetting.IsRelationDeleted;
          _obj.IsSentForRework        = allUsersSetting.IsSentForRework;
          _obj.IsSentToApproval       = allUsersSetting.IsSentToApproval;
          _obj.IsSentToCounterparty   = allUsersSetting.IsSentToCounterparty;
          _obj.IsSentToExecution      = allUsersSetting.IsSentToExecution;
          _obj.IsSentToReview         = allUsersSetting.IsSentToReview;
          _obj.IsSigned               = allUsersSetting.IsSigned;
          _obj.IsSignedByCounterparty = allUsersSetting.IsSignedByCounterparty;
          _obj.IsTotalAmountChanged   = allUsersSetting.IsTotalAmountChanged;
          _obj.IsUnRegistered         = allUsersSetting.IsUnRegistered;
          _obj.IsVersionCreated       = allUsersSetting.IsVersionCreated;
          _obj.IsVersionDeleted       = allUsersSetting.IsVersionDeleted;
        }
      }
    }
  }
}