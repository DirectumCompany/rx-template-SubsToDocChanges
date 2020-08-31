using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.SubsToDocChangesTemplate.NoticeSetting;
using DocumentTypeGuid = DirRX.SubsToDocChangesTemplate.PublicConstants.Module.DocumentTypeGuid;

namespace DirRX.SubsToDocChangesTemplate
{
  partial class NoticeSettingClientHandlers
  {

    public virtual IEnumerable<Enumeration> RoleFiltering(IEnumerable<Enumeration> query)
    {
      if (_obj.DocumentType != null)
      {
        if (_obj.DocumentType.DocumentTypeGuid == DocumentTypeGuid.IncomingLetter)
          query = query.Where(x => x != Role.Signatory && x != Role.Approver && x != Role.PreparedBy && x != Role.Responsible);
        
        if (_obj.DocumentType.DocumentTypeGuid == DocumentTypeGuid.OutgoingLetter)
          query = query.Where(x => x != Role.Assignee && x != Role.Addressee && x != Role.Responsible);
        
        if (_obj.DocumentType.DocumentTypeGuid == DocumentTypeGuid.Order)
          query = query.Where(x => x != Role.Addressee && x != Role.Responsible);
        
        if (_obj.DocumentType.DocumentTypeGuid == DocumentTypeGuid.CompanyDirective)
          query = query.Where(x => x != Role.Addressee && x != Role.Responsible);
        
        if (_obj.DocumentType.DocumentTypeGuid == DocumentTypeGuid.Memo)
          query = query.Where(x => x != Role.Responsible);
        
        if (_obj.DocumentType.DocumentTypeGuid == DocumentTypeGuid.Contract || _obj.DocumentType.DocumentTypeGuid == DocumentTypeGuid.SupAgreement)
          query = query.Where(x => x != Role.Assignee && x != Role.PreparedBy && x != Role.Addressee);
      }
      return query;
    }

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      _obj.State.Properties.Employee.IsEnabled = !_obj.AllUsersFlag.GetValueOrDefault();
      _obj.State.Properties.Role.IsEnabled = _obj.Role == null;
      
      Functions.NoticeSetting.SetAvailabilityNoticeSettingsEvents(_obj);
    }

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      _obj.State.Properties.Employee.IsRequired = !_obj.AllUsersFlag.GetValueOrDefault();
      _obj.State.Properties.Employee.IsEnabled = _obj.State.IsInserted && !_obj.AllUsersFlag.GetValueOrDefault();
       _obj.State.Properties.AllUsersFlag.IsEnabled = _obj.State.IsInserted;
      _obj.State.Properties.Role.IsEnabled = _obj.State.IsInserted && _obj.State.Properties.Role.IsEnabled;
      _obj.State.Properties.DocumentType.IsEnabled = _obj.State.IsInserted && _obj.State.Properties.DocumentType.IsEnabled;
      
      Functions.NoticeSetting.SetAvailabilityNoticeSettingsEvents(_obj);
      
      if (_obj.Role != null && _obj.DocumentType != null && Functions.NoticeSetting.Remote.IsSameRoleSettingExists(_obj, _obj.Role.Value, _obj.DocumentType))
        e.AddWarning(DirRX.SubsToDocChangesTemplate.NoticeSettings.Resources.SameSettingExists, _obj.Info.Actions.GetSameSetting);
    }

  }
}