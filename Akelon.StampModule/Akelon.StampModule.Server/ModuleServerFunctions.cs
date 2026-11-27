using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Docflow;
using System.IO;

namespace Akelon.StampModule.Server
{
  public partial class ModuleFunctions
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
        settings.StampFont = stampSetting.StampFont.Value.Value;
        settings.FontSize = stampSetting.FontSize;
        /* Настройка цвета передаётся в формате <Name>\<Hex>, для упрощения обработки.
           iTextSharp - без усложеняни может определить цвет по имени.
           SkiaSharp - валидно рабоет с переданным цветом в Hex.
           При этом само значение, в рамках демонстрационного решения, понятно пользователю.*/
        settings.FontColor = stampSetting.FontColor;
        
        // Запустить создание штампа по его виду.
        using (var versionStream = new System.IO.MemoryStream())
        {
          // Преобразовать последнюю версию документа в .PDF.
          if (document.LastVersion.AssociatedApplication.Extension.ToLower() != Sungero.Docflow.PublicConstants.OfficialDocument.Extensions.Pdf)
            document = ConvertToPDF(document);
          
          document.LastVersion.Body.Read().CopyTo(versionStream);

          // Сгенировать штамп для PDF версии документа с учётом настроек штампа.
          if (stampSetting.StampKind == Akelon.StampModule.StampSetting.StampKind.TextStamp)
          {
            var newVersion = IsolatedFunctions.GenerateStampArea.GenerateTextStamp(versionStream, settings);
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
          else if (stampSetting.StampKind == Akelon.StampModule.StampSetting.StampKind.DrawingStamp)
          {
            var newVersion = IsolatedFunctions.GenerateStampArea.GenerateDrawingStamp(versionStream, settings);
            document.LastVersion.Body.Write(newVersion);
          }
          else
          {
            return Akelon.StampModule.Resources.MsgNotSettingByStampKind;
          }
          
          // Занести версию со штампом в последнюю версию документа.
          document.LastVersion.AssociatedApplication = Sungero.Content.AssociatedApplications.GetByExtension(Sungero.Docflow.PublicConstants.OfficialDocument.Extensions.Pdf);
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
        document.CreateVersionFrom(newVersion, Sungero.Docflow.PublicConstants.OfficialDocument.Extensions.Pdf);
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