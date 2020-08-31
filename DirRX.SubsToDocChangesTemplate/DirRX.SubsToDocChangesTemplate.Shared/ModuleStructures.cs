using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.SubsToDocChangesTemplate.Structures.Module
{

  [Public]
  partial class DocumentEvents
  {
    public int DocumentId { get; set; }
    
    public int UserId { get; set; }
    
    public string EntityType { get; set; }
    
    public string DocumentEvent { get; set; }
    
    public string Comment { get; set; }
    
  }

}