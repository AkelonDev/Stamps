using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace Akelon.StampModule.Client
{
  public class ModuleFunctions
  {
    /// <summary>
    /// Запуск пробного формирования штампов.
    /// </summary>
    public virtual void TestStart()
    {      
      var dialog = Dialogs.CreateInputDialog(Akelon.StampModule.Resources.DialogTopicSetStamp);
      var document = dialog.AddSelect(Akelon.StampModule.Resources.DialogFieldDocument, true, Sungero.Docflow.OfficialDocuments.Null);
      string result = Akelon.StampModule.Resources.DialogMsgNotVersions;
      
      if (dialog.Show() == DialogButtons.Ok)
      {
        if (document.Value.Versions.Count() != 0)    
          result = Functions.Module.Remote.CreateDocumentStamp(document.Value);

        Dialogs.NotifyMessage(result);
      }
    }
  }
}