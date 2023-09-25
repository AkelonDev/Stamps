using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sungero.Core;
using System.IO;
using System.Drawing;
using System.Drawing.Text;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using iTextSharp;
using iTextSharp.text.pdf;
using Akelon.StampModule;
using Akelon.StampModule.Structures.Module;

namespace Akelon.StampModule.Isolated.GenerateStampArea
{
  public class IsolatedFunctions
  {

    #region Константы.
    /// <summary>
    /// Отступ от края страницы по ширине.
    /// </summary>
    public const int WidthIndent = 20;
    
    /// <summary>
    /// Отступ от края страницы по высоте.
    /// </summary>
    public const int HeightIndent = 20;
    
    /// <summary>
    /// Положение на странице, левый верхний угол.
    /// </summary>
    public const string UpLeft = "UpLeft";
    
    /// <summary>
    /// Положение на странице, правый верхний угол.
    /// </summary>
    public const string UpRight = "UpRight";
    
    /// <summary>
    /// Положение на странице, нижний левый угол.
    /// </summary>
    public const string DownLeft = "DownLeft";
    
    /// <summary>
    /// Положение на странице, нижний правый угол.
    /// </summary>
    public const string DownRight = "DownRight";
    
    /// <summary>
    /// Положение на документе, первая страница.
    /// </summary>
    public const string FirstPage = "FirstPage";
    
    /// <summary>
    /// Положение на документе, каждая страница.
    /// </summary>
    public const string EachPage = "EachPage";
    
    /// <summary>
    /// Положение на документе, последняя страница.
    /// </summary>
    public const string EndPage = "EndPage";
    #endregion
    
    #region Обработка штампа через ITextSharp, при генерации через Drawing.
    /// <summary>
    /// Сгенерировать штамп из картинки для PDF.
    /// </summary>
    /// <param name="versionStream">Версия документа.</param>
    /// <param name="settings">Настройки штампа.</param>
    /// <returns>Версия со штампом.</returns>
    [Public]
    public virtual Stream GenerateImageStamp(Stream versionStream, Structures.Module.IStampParameters settings)
    {
      var imageStream = new MemoryStream(settings.StampImage);
      
      var bitmapImage = new Bitmap(imageStream);
      var bitmap = new Bitmap(bitmapImage.Width, bitmapImage.Height);
      var graphics = Graphics.FromImage(bitmap);
      bitmap.Save(new System.IO.MemoryStream(), ImageFormat.Png);
      
      // Установить штамп в версию документа в формате PDF.
      var byteArrPdfVersion = AddImageToPdf(versionStream, imageStream, settings);
      imageStream.Close();
      
      return new System.IO.MemoryStream(byteArrPdfVersion);
      
    }
    
    /// <summary>
    /// Сгенерировать штамп из Drawing, для PDF.
    /// </summary>
    /// <param name="versionStream">Версия документа.</param>
    /// <param name="settings">Настройки штампа.</param>
    /// <returns>Версия со штампом.</returns>
    [Public]
    public virtual Stream GenerateStamp(Stream versionStream, Structures.Module.IStampParameters settings)
    {
      var stampStream = new System.IO.MemoryStream();
      
      // Сформировать штамп.
      string stampText = String.Format("{0} \n № {1} от {2}", "Company", "RegNumber", "RegDate");
      var bitmap = new Bitmap(1, 1);
      var font = new System.Drawing.Font("Arial", 25, FontStyle.Regular, GraphicsUnit.Pixel);
      var graphics = Graphics.FromImage(bitmap);
      bitmap = new Bitmap(bitmap, new Size((int)graphics.MeasureString(stampText, font).Width, (int)graphics.MeasureString(stampText, font).Height));
      graphics = Graphics.FromImage(bitmap);
      graphics.Clear(System.Drawing.Color.Empty);
      graphics.SmoothingMode = SmoothingMode.AntiAlias;
      graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
      graphics.DrawString(stampText, font, new SolidBrush(System.Drawing.Color.Black), 0, 0);
      graphics.Flush();
      graphics.Dispose();
      bitmap.Save(stampStream, ImageFormat.Png);
      
      // Установить штамп в версию документа в формате PDF.
      var byteArrPdfVersion = AddImageToPdf(versionStream, stampStream, settings);
      stampStream.Close();
      
      return new System.IO.MemoryStream(byteArrPdfVersion);
      
    }
    
    /// <summary>
    /// Добавление картинки в PDF.
    /// </summary>
    /// <param name="versionStream">Версия документа.</param>
    /// <param name="imageStream">Картинка со штампом.</param>
    /// <param name="width">Ширина картинки.</param>
    /// <param name="height">Высота картинки.</param>
    /// <returns>PDF версия документа со штампом.</returns>
    public byte[] AddImageToPdf(Stream versionStream, Stream imageStream, Structures.Module.IStampParameters settings)
    {
      var pdfReader = new PdfReader(versionStream);
      PdfReader.unethicalreading = true;
      
      using (var newVersionStream = new System.IO.MemoryStream())
      {
        using  (var pdfStamper = new PdfStamper(pdfReader, newVersionStream, '\0', true))
        {
          var color = new iTextSharp.text.BaseColor(0, 0, 0);
          
          // Установить положение штампа на документе.
          if (settings.DocumentPosition == EachPage)
          {
            // Установить штамп на каждую станицу.
            for (int page = 1; page <= pdfReader.NumberOfPages; page++)
            {
              var canvas = pdfStamper.GetOverContent(page);
              canvas.SetColorFill(color);
              canvas.SetColorStroke(color);
              imageStream.Position = 0;
              
              // Установить размер и позицию картинки на странице.
              var image = iTextSharp.text.Image.GetInstance(imageStream);
              image = SetSizeImage(image, pdfReader, page, settings, false);
              canvas.AddImage(image);
            }
          }
          else
          {
            // Установить штамп на первую или последнюю страницу.
            var page = settings.DocumentPosition == FirstPage ? 1 : pdfReader.NumberOfPages;
            var canvas = pdfStamper.GetOverContent(page);
            canvas.SetColorFill(color);
            canvas.SetColorStroke(color);
            imageStream.Position = 0;
            
            // Установить размер и позицию картинки на странице.
            var image = iTextSharp.text.Image.GetInstance(imageStream);
            image = SetSizeImage(image, pdfReader, page, settings, false);
            canvas.AddImage(image);
          }
        }
        
        return newVersionStream.ToArray();
      }
    }
    #endregion
    
    #region Обработка штрихкода и QR-кода, через ITextSharp.
    /// <summary>
    /// Сгенерировать штрихкод для PDF документа.
    /// </summary>
    /// <param name="versionStream">Версия документа.</param>
    /// <param name="settings">Настройки штампа.</param>
    /// <returns>Версия со штампом.</returns>
    [Public]
    public virtual Stream GenerateBarCode(Stream versionStream, Structures.Module.IStampParameters settings)
    {
      var byteArrPdfVersion = SetBarCode(versionStream, settings);
      return new System.IO.MemoryStream(byteArrPdfVersion);
    }
    
    /// <summary>
    /// Установить штрикхкод.
    /// </summary>
    /// <param name="versionStream">Версия документа.</param>
    /// <param name="settings">Настройки штампа.</param>
    /// <returns>Версия со штампом.</returns>
    public byte[] SetBarCode(Stream versionStream, Structures.Module.IStampParameters settings)
    {
      var pdfReader = new PdfReader(versionStream);
      PdfReader.unethicalreading = true;
      
      using (var newVersionStream = new System.IO.MemoryStream())
      {
        using  (var pdfStamper = new PdfStamper(pdfReader, newVersionStream, '\0', true))
        {
          // Формирование штрихкода
          var barcode = new Barcode128();
          barcode.Code = "Company RegNumber RegDate";
          barcode.ChecksumText = true;
          barcode.TextAlignment = iTextSharp.text.Element.ALIGN_CENTER;
          var color = new iTextSharp.text.BaseColor(0, 0, 0);
          
          // Установить положение штампа на документе.
          if (settings.DocumentPosition == EachPage)
          {
            // Установить штамп на каждую станицу.
            for (int page = 1; page <= pdfReader.NumberOfPages; page++)
            {
              var canvas = pdfStamper.GetOverContent(page);
              canvas.SetColorFill(color);
              canvas.SetColorStroke(color);

              // Установить размер и позицию картинки на странице.
              var image = barcode.CreateImageWithBarcode(canvas, color, color);
              image = SetSizeImage(image, pdfReader, page, settings, true);
              canvas.AddImage(image);
            }
          }
          else
          {
            // Установить штамп на первую или последнюю страницу.
            var page = settings.DocumentPosition == FirstPage ? 1 : pdfReader.NumberOfPages;
            var canvas = pdfStamper.GetOverContent(page);
            canvas.SetColorFill(color);
            canvas.SetColorStroke(color);

            // Установить размер и позицию картинки на странице.
            var image = barcode.CreateImageWithBarcode(canvas, color, color);
            image = SetSizeImage(image, pdfReader, page, settings, true);
            canvas.AddImage(image);
          }
          
          pdfStamper.Writer.CloseStream = false;
          pdfStamper.Close();
          newVersionStream.Close();
          
          return newVersionStream.ToArray();
        }
      }
    }
    
    /// <summary>
    /// Сгенерировать QR-код для PDF документа.
    /// </summary>
    /// <param name="versionStream">Версия документа.</param>
    /// <param name="settings">Настройки штампа.</param>
    /// <returns>Версия со штампом.</returns>
    [Public]
    public virtual Stream GenerateQRCode(Stream versionStream, Structures.Module.IStampParameters settings)
    {
      var byteArrPdfVersion = SetQRCode(versionStream, settings);
      return new System.IO.MemoryStream(byteArrPdfVersion);
    }
    
    /// <summary>
    /// Установить QR-код.
    /// </summary>
    /// <param name="versionStream">Версия документа</param>
    /// <param name="settings">Настройки штампа.</param>
    /// <returns>Версия со штампом.</returns>
    public byte[] SetQRCode(Stream versionStream, Structures.Module.IStampParameters settings)
    {
      var pdfReader = new PdfReader(versionStream);
      PdfReader.unethicalreading = true;
      
      using (var newVersionStream = new System.IO.MemoryStream())
      {
        using  (var pdfStamper = new PdfStamper(pdfReader, newVersionStream, '\0', true))
        {
          // Формирование QR-кода.
          var сodeValue = String.Format("{0} \n № {1} от {2}", "Company", "RegNumber", "RegDate");
          var barcodeQRCode = new BarcodeQRCode(сodeValue, settings.SizeWidth.Value, settings.SizeHeight.Value, null);
          var imageQR = barcodeQRCode.GetImage();
          
          // Установить положение штампа на документе.
          if (settings.DocumentPosition == EachPage)
          {
            // Установить штамп на каждую станицу.
            for (int page = 1; page <= pdfReader.NumberOfPages; page++)
            {
              var canvas = pdfStamper.GetOverContent(page);
              imageQR = SetSizeImage(imageQR, pdfReader, page, settings, true);
              canvas.AddImage(imageQR);
            }
          }
          else
          {
            // Установить штамп на первую или последнюю страницу.
            var page = settings.DocumentPosition == FirstPage ? 1 : pdfReader.NumberOfPages;
            var canvas = pdfStamper.GetOverContent(page);
            imageQR = SetSizeImage(imageQR, pdfReader, page, settings, true);
            canvas.AddImage(imageQR);
          }
          
          pdfStamper.Writer.CloseStream = false;
          pdfStamper.Close();
          newVersionStream.Close();
          
          return newVersionStream.ToArray();
        }
      }
    }
    #endregion
    
    #region Обработка размера и положения штампа.
    /// <summary>
    /// Установить размер и позицию изображения на странице.
    /// </summary>
    /// <param name="image">Изображение.</param>
    /// <param name="pdfReader">Документ открытый в PdfReader.</param>
    /// <param name="numberPage">Номер страницы.</param>
    /// <param name="settings">Настройки штампа.</param>
    /// <returns>Подготовленное изображение.</returns>
    public iTextSharp.text.Image SetSizeImage(iTextSharp.text.Image image, PdfReader pdfReader, int numberPage, Structures.Module.IStampParameters settings, bool isBarOrQRCode)
    {
      int width = 0;
      int height = 0;
      
      if (isBarOrQRCode)
      {
        width = (int)image.Width;
        height = (int)image.Height;
      }
      else
      {
        width = settings.SizeWidth.Value;
        height = settings.SizeHeight.Value;
        image.ScaleAbsoluteWidth(width);
        image.ScaleAbsoluteHeight(height);
      }
      
      var rectangle = pdfReader.GetPageSize(numberPage);
      var position = GetPagePosition(width, height, settings.PagePosition, rectangle);
      image.SetAbsolutePosition(position[0], position[1]);
      
      return image;
    }
    
    /// <summary>
    /// Определить позицию расположения на странице.
    /// </summary>
    /// <param name="width">Ширина элемента.</param>
    /// <param name="height">Высота элемента.</param>
    /// <param name="position">Название позиции.</param>
    /// <param name="rectangle">Фигура для расположения элемента.</param>
    /// <returns>Координаты расположения XY.</returns>
    public float[] GetPagePosition(int width, int height, string position, iTextSharp.text.Rectangle rectangle)
    {
      switch (position)
      {
        case DownLeft:
          return new float[] { rectangle.Left + WidthIndent, rectangle.Bottom + HeightIndent };
        case DownRight:
          return new float[] { rectangle.Right - WidthIndent - width, rectangle.Bottom + HeightIndent };
        case UpLeft:
          return new float[] { rectangle.Left + WidthIndent, rectangle.Top - HeightIndent - height };
        case UpRight:
          return new float[] { rectangle.Right - WidthIndent - width, rectangle.Top - HeightIndent - height };
        default:
          return new float[] { rectangle.Left + WidthIndent, rectangle.Bottom + HeightIndent };
      }
    }
    
    /// <summary>
    /// Получить размер изображения.
    /// </summary>
    /// <param name="imageFile">Изображение.</param>
    /// <returns>Размер изображения.</returns>
    [Public]
    public virtual List<int> GetSizePicture(Stream imageStream)
    {
      var imageBitmap = new Bitmap(imageStream);
      return new List<int>() { imageBitmap.Width, imageBitmap.Height };
    }
    #endregion
  }
}