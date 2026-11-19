using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using System.IO;
using iTextSharp;
using iTextSharp.text;
using iTextSharp.text.pdf;
using SkiaSharp;

namespace Akelon.StampModule.Isolated.GenerateStampArea
{
  public partial class IsolatedFunctions
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

    /// <summary>
    /// Кегль по умолчанию.
    /// </summary>
    public const float fontSizeDefault = 18.75f;
    #endregion

    #region Обработка штампа через ITextSharp.
    /// <summary>
    /// Сгенерировать штамп из картинки для PDF.
    /// </summary>
    /// <param name="versionStream">Версия документа.</param>
    /// <param name="settings">Настройки штампа.</param>
    /// <returns>Версия со штампом.</returns>
    [Public]
    public virtual Stream GenerateImageStamp(Stream versionStream, Structures.Module.IStampParameters settings)
    {
      using (var imageStream = new MemoryStream(settings.StampImage))
      {
        imageStream.Position = 0;
        var byteArrPdfVersion = AddImageToPdf(versionStream, imageStream, settings);
        return new MemoryStream(byteArrPdfVersion);
      }
    }

    /// <summary>
    /// Сгенерировать текстовый штамп, для PDF.
    /// </summary>
    /// <param name="versionStream">Версия документа.</param>
    /// <param name="settings">Настройки штампа.</param>
    /// <returns>Версия со штампом.</returns>
    [Public]
    public virtual Stream GenerateTextStamp(Stream versionStream, Structures.Module.IStampParameters settings)
    {
      var pdfReader = new PdfReader(versionStream);
      PdfReader.unethicalreading = true;

      using (var newVersionStream = new MemoryStream())
      {
        using (var pdfStamper = new PdfStamper(pdfReader, newVersionStream, '\0', true))
        {
          var fontSizePt = settings.FontSize != null ? (float)settings.FontSize : fontSizeDefault;
          var pathFont = GetFontPath(settings.StampFont);
          var font = BaseFont.CreateFont(pathFont, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
          var baseColor = GetFontColor(settings.FontColor);
          var fontColor = new BaseColor(baseColor[1], baseColor[1], baseColor[2]);
          var stampText = String.Format("{0} № {1} от {2}", "Company", "RegNumber", "RegDate");

          // Определить страницы для штампа.
          var pages = settings.DocumentPosition == EachPage
            ? Enumerable.Range(1, pdfReader.NumberOfPages).ToList()
            : new List<int> { settings.DocumentPosition == FirstPage ? 1 : pdfReader.NumberOfPages };

          foreach (var page in pages)
          {
            var canvas = pdfStamper.GetOverContent(page);
            canvas.SetColorFill(fontColor);
            canvas.SetColorStroke(fontColor);

            // Размер текста с учётом межстрочного интервала, для позиционирования.
            float textBlockWidth = font.GetWidthPoint(stampText.Trim(), fontSizePt);
            float textBlockHeight = fontSizePt * 1.2f;

            // Позиция текста: под логотипом (если он есть) или по той же логике позиции.
            var pagePosition = GetPagePosition((int)Math.Ceiling(textBlockWidth),
                                          (int)Math.Ceiling(textBlockHeight),
                                          settings.PagePosition,
                                          pdfReader.GetPageSize(page));

            canvas.BeginText();
            canvas.SetFontAndSize(font, fontSizePt);
            canvas.SetTextMatrix(pagePosition[0], pagePosition[1]);
            canvas.ShowText(stampText);
            canvas.EndText();
          }
        }

        return new MemoryStream(newVersionStream.ToArray());
      }
    }

    /// <summary>
    /// Получить цвет шрифта.
    /// </summary>
    /// <param name="fontColor">Цвет шрифта.</param>
    /// <returns>Цвет шрифта RGB.</returns>
    private int[] GetFontColor(string fontColor)
    {
      // Установить по умолчанию чёрный цвет.
      var baseColor = new int[] { 0, 0, 0 };

      if (fontColor == "Blue")
        baseColor = new int[] { 0, 0, 255 };
      else if (fontColor == "Red")
        baseColor = new int[] { 255, 0, 0 };
      else if (fontColor == "Orange")
        baseColor = new int[] { 255, 110, 0 };

      return baseColor;
    }

    /// <summary>
    /// Получить путь шрифта.
    /// </summary>
    /// <param name="stampFont">Настрйока шрифта штампа.</param>
    /// <returns>Путь до шрифта.</returns>
    private string GetFontPath(string stampFont)
    {
      // Кандидаты путей к TTF: Windows + типичные Linux-пути.
      var candidates = new Dictionary<string, string[]>
      {
        {
          "Arial", new string[]
          {
            "arial.ttf",
            "Arial.ttf",
            @"C:\Windows\Fonts\arial.ttf"} },
        {
          "TimesNewRoman", new string[]
          {
            "times.ttf",
            "Times.ttf",
            @"C:\Windows\Fonts\times.ttf"} },
        {
          "LiberationSans", new string[]
          {
            "/usr/share/fonts/truetype/liberation/LiberationSans-Regular.ttf",
            "/usr/share/fonts/liberation/LiberationSans-Regular.ttf",
            "LiberationSans-Regular.ttf"} },
        {
          "DejaVuSans", new string[]
          {
            "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf",
            "/usr/share/fonts/dejavu/DejaVuSans.ttf",
            "DejaVuSans.ttf"} }
      };

      if (candidates.TryGetValue(stampFont, out var candidate))
      {
        foreach (var path in candidate)
        {
          if (File.Exists(path))
            return path;
        }
      }

      throw AppliedCodeException.Create("CreateStampFont: Не найден TTF-шрифт с поддержкой кириллицы. Установите выбранный шрифт на сервер приложений.");
    }

    /// <summary>
    /// Добавление картинки в PDF.
    /// </summary>
    /// <param name="versionStream">Версия документа.</param>
    /// <param name="imageStream">Картинка со штампом.</param>
    /// <param name="settings">Настройки штампа.</param>
    /// <returns>PDF версия документа со штампом.</returns>
    public byte[] AddImageToPdf(Stream versionStream, Stream imageStream, Structures.Module.IStampParameters settings)
    {
      var pdfReader = new PdfReader(versionStream);
      PdfReader.unethicalreading = true;

      using (var newVersionStream = new System.IO.MemoryStream())
      {
        using (var pdfStamper = new PdfStamper(pdfReader, newVersionStream, '\0', true))
        {
          // Определить страницы для штампа.
          var pages = settings.DocumentPosition == EachPage
            ? Enumerable.Range(1, pdfReader.NumberOfPages).ToList()
            : new List<int> { settings.DocumentPosition == FirstPage ? 1 : pdfReader.NumberOfPages };

          foreach (var page in pages)
          {
            var canvas = pdfStamper.GetOverContent(page);
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
        using (var pdfStamper = new PdfStamper(pdfReader, newVersionStream, '\0', true))
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
        using (var pdfStamper = new PdfStamper(pdfReader, newVersionStream, '\0', true))
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

    #region Обработка штампа через SkiaSharp.
    /// <summary>
    /// Сгенерировать штамп отрисовкой в SkiaSharp.
    /// </summary>
    /// <param name="versionStream">Версия документа.</param>
    /// <param name="settings">Настройки штампа.</param>
    /// <returns>Версия со штампом.</returns>
    [Public]
    public virtual Stream GenerateDrawingStamp(Stream versionStream, Structures.Module.IStampParameters settings)
    {
      var stampText = String.Format("{0} № {1} от {2}", "Company", "RegNumber", "RegDate");
      var width = settings.SizeWidth.Value;
      var height = settings.SizeHeight.Value;

      // Сформировать поверхность для рендеринга штампа.
      var surface = SKSurface.Create(new SKImageInfo(width, height));
      var canvas = surface.Canvas;

      // Очистить фон, повернуть холст для эффекта оттиска, задать цвет.
      canvas.Clear(SKColors.Transparent);
      canvas.RotateDegrees(-12, width / 2f, height / 2f);
      var baseColor = GetFontColor(settings.FontColor);
      var stampColor = new SKColor((byte)baseColor[1], (byte)baseColor[1], (byte)baseColor[2]);

      // Настроить кисть для рамки.
      var borderPaint = new SKPaint
      {
        Color = stampColor,
        Style = SKPaintStyle.Stroke,
        StrokeWidth = 6,
        IsAntialias = true
      };

      // Отрисовать внешнюю и внутреннюю рамки.
      var outerRect = new SKRoundRect(new SKRect(40, 40, width - 40, height - 40), 12, 12);
      var innerBounds = new SKRect(50, 50, width - 50, height - 50);
      var innerRect = new SKRoundRect(innerBounds, 8, 8);
      canvas.DrawRoundRect(outerRect, borderPaint);
      borderPaint.StrokeWidth = 2;
      canvas.DrawRoundRect(innerRect, borderPaint);

      // Настроить кисть для текста.
      var textPaint = new SKPaint
      {
        Color = stampColor,
        IsAntialias = true
      };

      // Получить настройки шрифта.
      var pathFont = GetFontPath(settings.StampFont);
      var font = new SKFont(SKTypeface.FromFile(pathFont, settings.FontSize.Value));

      // Отцентрировать текст по внутренней рамке.    
      var metricsFont = font.Metrics;
      var centerX = innerBounds.MidX;
      var textY = innerBounds.MidY - (metricsFont.Ascent + metricsFont.Descent) / 2f;
      canvas.DrawText(stampText, centerX, textY, SKTextAlign.Center, font, textPaint);
      canvas.Restore();

      // Сформировать объект картинки и произвести установку в версию документа.
      var image = surface.Snapshot();
      var data = image.Encode(SKEncodedImageFormat.Png, 100);
      var imageStream = data.AsStream();
      imageStream.Position = 0;
      var byteArrPdfVersion = AddImageToPdf(versionStream, imageStream, settings);

      return new MemoryStream(byteArrPdfVersion);
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
    /// <param name="isBarOrQRCode">Признак что штамп является Bar или QR кодом.</param>
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
    /// <param name="imageStream">Изображение.</param>
    /// <returns>Размер изображения.</returns>
    [Public]
    public virtual List<int> GetSizePicture(Stream imageStream)
    {
      if (imageStream.CanSeek)
        imageStream.Position = 0;

      var image = iTextSharp.text.Image.GetInstance(imageStream);
      return new List<int>() { (int)image.Width, (int)image.Height };
    }
    #endregion
  }
}