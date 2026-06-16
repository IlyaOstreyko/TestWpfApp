using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;
using System.IO;
using TestWpfApp.Data.DataModels;
namespace TestWpfApp.Service
{
    public static class WordGenerator
    {
        public static void GenerateQuestionsDoc(
        List<TestQuestion> questions,
        string templatePath,
        string outputPath,
        string nameTheme,
        string nameSpec)
        {
            File.Copy(templatePath, outputPath, true);

            using var document = WordprocessingDocument.Open(outputPath, true);

            var body = document.MainDocumentPart.Document.Body;
            ReplaceTextContentControl(body, "SpecialityName", nameSpec);

            ReplaceTextContentControl(body, "ThemeName", nameTheme);

            // Ищем таблицу
            var table = body.Descendants<Table>().FirstOrDefault();

            if (table == null)
                throw new Exception("Таблица не найдена");

            // Шаблонная строка
            var templateRow = table.Elements<TableRow>().Last();

            foreach (var question in questions)
            {
                var newRow = (TableRow)templateRow.CloneNode(true);

                ReplaceTextContentControl(newRow, "QuestionText", question.NameQuestion);

                InsertAnswers(newRow, question);

                if (question.ImageQuestionBytes.Count() != 0)
                {
                    InsertImage(
                        document.MainDocumentPart,
                        newRow,
                        "QuestionImage",
                        question.ImageQuestionBytes);
                }
                else { ReplaceTextContentControl(newRow, "QuestionImage", ""); }

                table.AppendChild(newRow);
            }

            // Удаляем строку шаблона
            templateRow.Remove();

            document.MainDocumentPart.Document.Save();
        }

        private static void ReplaceTextContentControl(
            OpenXmlElement root,
            string tag,
            string text)
        {
            var sdt = root.Descendants<SdtElement>()
                .FirstOrDefault(x =>
                    x.SdtProperties?
                     .GetFirstChild<Tag>()?.Val == tag);

            if (sdt == null)
                return;

            var run = sdt.Descendants<Text>().FirstOrDefault();

            if (run != null)
                run.Text = text ?? "";
        }

        private static void InsertAnswers(
            TableRow row,
            TestQuestion q)
        {
            var sdt = row.Descendants<SdtElement>()
                .FirstOrDefault(x =>
                    x.SdtProperties?
                     .GetFirstChild<Tag>()?.Val == "Answers");

            if (sdt == null)
                return;

            var content = sdt.GetFirstChild<SdtContentBlock>();

            if (content == null)
                return;

            content.RemoveAllChildren();

            var answers = new List<(string Text, bool IsCorrect)>
        {
            ("1. " + q.NameAnswerCorrect1 ?? "", true),
            ("2. " + q.NameAnswerIncorrect1 ?? "", false),
            ("3. " + q.NameAnswerIncorrect2 ?? "", false),
            ("4. " + q.NameAnswerIncorrect3 ?? "", false)
        };

            foreach (var answer in answers)
            {
                var paragraph = new Paragraph();

                var run = new Run(
                    new Text(answer.Text));

                if (answer.IsCorrect)
                {
                    run.RunProperties = new RunProperties(
                        new Highlight() { Val = HighlightColorValues.Green });
                }

                paragraph.Append(run);

                content.Append(paragraph);
            }
        }

        private static void InsertImage(
            MainDocumentPart mainPart,
            TableRow row,
            string tag,
            byte[] imageBytes)
        {
            var sdt = row.Descendants<SdtElement>()
                .FirstOrDefault(x =>
                    x.SdtProperties?
                     .GetFirstChild<Tag>()?.Val == tag);

            if (sdt == null)
                return;

            var content = sdt.GetFirstChild<SdtContentBlock>();

            if (content == null)
                return;

            content.RemoveAllChildren();

            var imagePart = mainPart.AddImagePart(ImagePartType.Jpeg);

            using var stream = new MemoryStream(imageBytes);

            imagePart.FeedData(stream);

            var relationshipId = mainPart.GetIdOfPart(imagePart);

            var element = CreateImageElement(relationshipId);

            var paragraph = new Paragraph(
                new Run(element));

            content.Append(paragraph);
        }

        private static Drawing CreateImageElement(string relationshipId)
        {
            return new Drawing(
                new DW.Inline(
                    new DW.Extent() { Cx = 3000000L, Cy = 2000000L },
                    new DW.EffectExtent()
                    {
                        LeftEdge = 0L,
                        TopEdge = 0L,
                        RightEdge = 0L,
                        BottomEdge = 0L
                    },
                    new DW.DocProperties()
                    {
                        Id = (UInt32Value)1U,
                        Name = "Picture"
                    },
                    new DW.NonVisualGraphicFrameDrawingProperties(
                        new A.GraphicFrameLocks() { NoChangeAspect = true }),
                    new A.Graphic(
                        new A.GraphicData(
                            new PIC.Picture(
                                new PIC.NonVisualPictureProperties(
                                    new PIC.NonVisualDrawingProperties()
                                    {
                                        Id = (UInt32Value)0U,
                                        Name = "Image.jpg"
                                    },
                                    new PIC.NonVisualPictureDrawingProperties()),
                                new PIC.BlipFill(
                                    new A.Blip()
                                    {
                                        Embed = relationshipId
                                    },
                                    new A.Stretch(new A.FillRectangle())),
                                new PIC.ShapeProperties(
                                    new A.Transform2D(
                                        new A.Offset() { X = 0L, Y = 0L },
                                        new A.Extents()
                                        {
                                            Cx = 3000000L,
                                            Cy = 2000000L
                                        }),
                                    new A.PresetGeometry(
                                        new A.AdjustValueList())
                                    { Preset = A.ShapeTypeValues.Rectangle }))
                        )
                        { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" }))
                );
        }
    }
}
