using System;
using Sungero.Core;

namespace DirRX.SubsToDocChangesTemplate.Constants
{
  public static class Module
  {

    /// <summary>
    /// Имя параметра в таблице Sungero_Docflow_Params для фиксации последнего выполнения фонового процесса рассылки уведомлений по документам.
    /// </summary>
    public const string SendDocumentNoticesLastDateDocflowParamName = "SendDocumentNoticesLastDate";
    
    public static class DocumentTypeGuid
    {
      [Public]
      public const string IncomingLetter = "8dd00491-8fd0-4a7a-9cf3-8b6dc2e6455d";
      
      [Public]
      public const string OutgoingLetter = "d1d2a452-7732-4ba8-b199-0a4dc78898ac";
      
      [Public]
      public const string Order = "9570e517-7ab7-4f23-a959-3652715efad3";
      
      [Public]
      public const string CompanyDirective = "264ada4e-b272-4ecc-a115-1246c9556bfa";
      
      [Public]
      public const string Memo = "95af409b-83fe-4697-a805-5a86ceec33f5";
      
      [Public]
      public const string Contract = "f37c7e63-b134-4446-9b5b-f8811f6c9666";
      
      [Public]
      public const string SupAgreement = "265f2c57-6a8a-4a15-833b-ca00e8047fa5";
    }
  }
}