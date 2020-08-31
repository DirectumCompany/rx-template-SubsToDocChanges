using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.SubsToDocChangesTemplate.NoticeSetting;
using DocumentTypeGuid = DirRX.SubsToDocChangesTemplate.PublicConstants.Module.DocumentTypeGuid;

namespace DirRX.SubsToDocChangesTemplate
{
  partial class NoticeSettingDocumentTypePropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> DocumentTypeFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      return query.Where(x => x.DocumentTypeGuid == DocumentTypeGuid.IncomingLetter || x.DocumentTypeGuid == DocumentTypeGuid.OutgoingLetter || x.DocumentTypeGuid == DocumentTypeGuid.Order
                         || x.DocumentTypeGuid == DocumentTypeGuid.CompanyDirective || x.DocumentTypeGuid == DocumentTypeGuid.Memo
                         || x.DocumentTypeGuid == DocumentTypeGuid.Contract || x.DocumentTypeGuid == DocumentTypeGuid.SupAgreement);
    }
  }

  partial class NoticeSettingServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      if (_obj.Role != null && _obj.DocumentType != null && Functions.NoticeSetting.IsSameRoleSettingExists(_obj, _obj.Role.Value, _obj.DocumentType))
        e.AddError(DirRX.SubsToDocChangesTemplate.NoticeSettings.Resources.SameSettingExists, _obj.Info.Actions.GetSameSetting);
      
      string employeeName = _obj.AllUsersFlag.GetValueOrDefault() ? DirRX.SubsToDocChangesTemplate.NoticeSettings.Resources.NoticeSettingAllUsersName : _obj.Employee.DisplayValue;
      _obj.Name = DirRX.SubsToDocChangesTemplate.NoticeSettings.Resources.NoticeSettingNameTemplateFormat(employeeName, _obj.Info.Properties.Role.GetLocalizedValue(_obj.Role.Value));
    }

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.AllUsersFlag = false;
      _obj.Information = NoticeSettings.Resources.LabelInformation;
      
      #region Блок признаков с событиями.
      
      _obj.IsApprovalAborted      = false;
      _obj.IsExecuted             = false;
      _obj.IsExecutionAborted     = false;
      _obj.IsRegistered           = false;
      _obj.IsRelationAdded        = false;
      _obj.IsRelationDeleted      = false;
      _obj.IsSentForRework        = false;
      _obj.IsSentToApproval       = false;
      _obj.IsSentToCounterparty   = false;
      _obj.IsSentToExecution      = false;
      _obj.IsSentToReview         = false;
      _obj.IsSigned               = false;
      _obj.IsSignedByCounterparty = false;
      _obj.IsTotalAmountChanged   = false;
      _obj.IsUnRegistered         = false;
      _obj.IsVersionCreated       = false;
      _obj.IsVersionDeleted       = false;
      
      #endregion
    }
  }

}