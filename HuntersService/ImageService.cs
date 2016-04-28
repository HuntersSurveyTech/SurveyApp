using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Font = System.Drawing.Font;
using Image = System.Drawing.Image;

namespace HuntersService
{
    public class ImageService
    {
        public static byte[] Watermark(byte[] image, string watermarkText)
        {
            var texts = watermarkText.Split(new string[] {"[@n@]"}, StringSplitOptions.RemoveEmptyEntries);

            var text1 = texts[0];
            var text2 = texts[1];
            var text3 = texts[2];

            using (var ms = new MemoryStream(image))
            {
                //Read the File into a Bitmap.
                using (Bitmap bmp = new Bitmap(ms))
                {
                    using (Graphics grp = Graphics.FromImage(bmp))
                    {
                        //Set the Color of the Watermark text.
                        Brush brush = new SolidBrush(Color.White);

                        //Set the Font and its size.
                        Font font = new System.Drawing.Font("Arial", 70, FontStyle.Bold, GraphicsUnit.Pixel);

                        //Determine the size of the Watermark text.
                        SizeF textSize1 =grp.MeasureString(text1, font);
                        SizeF textSize2 = grp.MeasureString(text2, font);
                        SizeF textSize3 = grp.MeasureString(text3, font);
                        
                        //Prepare the background rectangle and draw
                        int bgRectWidth;
                        int bgRectHeight = (int) (textSize1.Height + textSize2.Height + textSize3.Height)+ 20;
                        if (textSize2.Width >= textSize3.Width)
                        {
                            bgRectWidth = (int)textSize2.Width + 20;
                        }
                        else bgRectWidth = (int)textSize3.Width + 20;
                        Brush rectBrush = new SolidBrush(Color.FromArgb(180,219,70,153));
                        grp.FillRectangle(rectBrush, 0, (bmp.Height - bgRectHeight), bgRectWidth, bgRectHeight);

                        //Position the text and draw it on the image.
                        Point position1 = new Point(10, (bmp.Height - ((int)(textSize1.Height + textSize2.Height + textSize3.Height )+ 10)));
                        grp.DrawString(text1, font, brush, position1);


                        Point position2 = new Point(10, (bmp.Height - ((int)(textSize2.Height + textSize3.Height) + 10)));
                        grp.DrawString(text2, font, brush, position2);

                        Point position3 = new Point(10, (bmp.Height - ((int)(textSize3.Height) + 10)));
                        grp.DrawString(text3, font, brush, position3);

                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            //Save the Watermarked image to the MemoryStream.
                            bmp.Save(memoryStream, ImageFormat.Jpeg);
                            memoryStream.Position = 0;

                            return memoryStream.ToArray();

                        }
                    }
                }
            }

        }

        public static byte[] ConvertToPDF(byte[] data)
        {
            Document document = new Document();
            using (var imageStream = new MemoryStream(data))
            {

                using (var pdfStream = new MemoryStream())
                {
                    PdfWriter.GetInstance(document, pdfStream);
                    document.Open();

                    var image = iTextSharp.text.Image.GetInstance(imageStream);
                    image.ScaleToFit(document.PageSize.Width - 1, document.PageSize.Height);
                    image.SetAbsolutePosition(0, document.PageSize.Height - image.ScaledHeight);
                    document.Add(image);
                    document.Close();

                    return pdfStream.ToArray();
                }
            }

        }
    }
}
