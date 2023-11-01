using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Docflow;
using System.IO;

namespace Akelon.StampModule.Server
{
  public class ModuleFunctions
  {
    /// <summary>
    /// Создать обработку штампа для документа.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <returns>Результат обработки.</returns>
    [Remote(IsPure = true)]
    public string CreateDocumentStamp(IOfficialDocument document)
    {
      if (!document.HasVersions)
        return Akelon.StampModule.Resources.DialogMsgNotVersions;
      
      var docType = document.DocumentKind.DocumentType;
      var stampSetting = GetStampSetting(docType);
      
      if (stampSetting != null)
      {
        // Заполнить структуру с настройками штампа.
        var settings = Structures.Module.StampParameters.Create();
        settings.DocumentPosition = stampSetting.DocumentPosition.Value.Value;
        settings.PagePosition = stampSetting.PagePosition.Value.Value;
        settings.SizeWidth = stampSetting.SizeWidth;
        settings.SizeHeight = stampSetting.SizeHeight;
        settings.StampImage = stampSetting.StampImage;
        
        // Запустить создание штампа по его виду.
        using (var versionStream = new System.IO.MemoryStream())
        {
          if (document.LastVersion.AssociatedApplication.Extension.ToLower() != Sungero.Docflow.PublicConstants.OfficialDocument.PdfExtension)
            document = ConvertToPDF(document);
          
          document.LastVersion.Body.Read().CopyTo(versionStream);

          if (stampSetting.StampKind == Akelon.StampModule.StampSetting.StampKind.DrawingStamp)
          {
            var newVersion = IsolatedFunctions.GenerateStampArea.GenerateStamp(versionStream, settings);
            document.LastVersion.Body.Write(newVersion);
          }
          else if (stampSetting.StampKind == Akelon.StampModule.StampSetting.StampKind.LoadStamp)
          {
            var newVersion = IsolatedFunctions.GenerateStampArea.GenerateImageStamp(versionStream, settings);
            document.LastVersion.Body.Write(newVersion);
          }
          else if (stampSetting.StampKind == Akelon.StampModule.StampSetting.StampKind.Barcode)
          {
            var newVersion = IsolatedFunctions.GenerateStampArea.GenerateBarCode(versionStream, settings);
            document.LastVersion.Body.Write(newVersion);
          }
          else if (stampSetting.StampKind == Akelon.StampModule.StampSetting.StampKind.QRCode)
          {
            var newVersion = IsolatedFunctions.GenerateStampArea.GenerateQRCode(versionStream, settings);
            document.LastVersion.Body.Write(newVersion);
          }
          else
          {
            return Akelon.StampModule.Resources.MsgNotSettingByStampKind;
          }
          
          document.LastVersion.AssociatedApplication = Sungero.Content.AssociatedApplications.GetByExtension(Sungero.Docflow.PublicConstants.OfficialDocument.PdfExtension);
          document.Save();
          return Akelon.StampModule.Resources.MsgStampInstalled;
        }
      }

      return String.Format(Akelon.StampModule.Resources.MsgNotFoundStampSetting, docType.Name);
    }
    
    /// <summary>
    /// Получить настройки штампа по типу документа.
    /// </summary>
    /// <param name="docType">Тип документа.</param>
    /// <returns>Настройка штампа.</returns>
    public Akelon.StampModule.IStampSetting GetStampSetting(IDocumentType docType)
    {
      return Akelon.StampModule.StampSettings.GetAll().FirstOrDefault(s => s.Status == Akelon.StampModule.StampSetting.Status.Active && Equals(s.DocumentType, docType));
    }
    
    /// <summary>
    /// Преобразовать версию документа в PDF.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <returns>Документ с версией в PDF.</returns>
    public IOfficialDocument ConvertToPDF(IOfficialDocument document)
    {
      using (var versionStream = new System.IO.MemoryStream())
      {
        document.LastVersion.Body.Read().CopyTo(versionStream);
        var newVersion = Sungero.Docflow.IsolatedFunctions.PdfConverter.GeneratePdf(versionStream, document.LastVersion.AssociatedApplication.Extension.ToLower());
        document.CreateVersionFrom(newVersion, Sungero.Docflow.PublicConstants.OfficialDocument.PdfExtension);
        document.Save();
      }
      
      return document;
    }
    
    /// <summary>
    /// Получить размер изображения.
    /// </summary>
    /// <param name="imageFile">Изображение.</param>
    /// <returns>Размер изображения.</returns>
    [Public]
    public List<int> GetSizePicture(byte[] imageFile)
    {
      using (var imageStream = new MemoryStream(imageFile))
      {
        return IsolatedFunctions.GenerateStampArea.GetSizePicture(imageStream);
      }
    }
  }
}