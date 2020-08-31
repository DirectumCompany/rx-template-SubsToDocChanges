using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.SubsToDocChangesTemplate.NoticeSetting;

namespace DirRX.SubsToDocChangesTemplate
{
  partial class NoticeSettingSharedHandlers
  {

    public virtual void RoleChanged(Sungero.Domain.Shared.EnumerationPropertyChangedEventArgs e)
    {
      Functions.NoticeSetting.SetAvailabilityNoticeSettingsEvents(_obj);
      
      if (e.NewValue != null && _obj.DocumentType != null)
        Functions.NoticeSetting.SetNoticeSettingsEvents(_obj);
    }

    public virtual void AllUsersFlagChanged(Sungero.Domain.Shared.BooleanPropertyChangedEventArgs e)
    {
      if (e.NewValue == true)
        _obj.Employee = null;
      
      _obj.State.Properties.Employee.IsRequired = !_obj.AllUsersFlag.GetValueOrDefault();
      _obj.State.Properties.Employee.IsEnabled = !_obj.AllUsersFlag.GetValueOrDefault();
    }

    public virtual void DocumentTypeChanged(DirRX.SubsToDocChangesTemplate.Shared.NoticeSettingDocumentTypeChangedEventArgs e)
    {
      if (e.NewValue != null && e.NewValue != e.OldValue)
        _obj.Role = null;
      
      Functions.NoticeSetting.SetAvailabilityNoticeSettingsEvents(_obj);
      
      if (e.NewValue != null && _obj.Role != null)
        Functions.NoticeSetting.SetNoticeSettingsEvents(_obj);
    }

  }
}