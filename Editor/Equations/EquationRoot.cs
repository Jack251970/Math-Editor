using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Clowd.Clipboard;
using Clowd.Clipboard.Formats;

namespace Editor
{
    public sealed class EquationRoot : EquationContainer
    {
        private static readonly string ClassName = nameof(EquationRoot);

        public static readonly ClipboardFormat<string> ClipboardXmlFormat
            = ClipboardFormat.CreateCustomFormat(
                $"{typeof(MathEditorData).FullName}.{nameof(MathEditorData.XmlString)}",
                new TextUtf8Converter());

        public EditorControl? Editor { get; set; } = null;
        private readonly Caret _vCaret;
        private readonly Caret _hCaret;
        private readonly string fileVersion = "1.4";
        private readonly string sessionString = Guid.NewGuid().ToString();

        public EquationRoot(IMainWindow owner, Caret vCaret, Caret hCaret)
            : base(owner, null!)
        {
            ApplySymbolGap = true;
            _vCaret = vCaret;
            _hCaret = hCaret;
            ActiveChild = new RowContainer(owner, this, 0.3);
            childEquations.Add(ActiveChild);
            ActiveChild.Location = Location = new Point(15, 15);
            AdjustCarets();
        }

        public override void ChildCompletedUndo(EquationBase child)
        {
            CalculateSize();
            AdjustCarets();
        }

        public void SaveFile(Stream stream)
        {
            var xDoc = new XDocument();
            var root = new XElement(GetType().Name); //ActiveChild.Serialize();
            root.Add(new XAttribute("fileVersion", fileVersion));
            root.Add(new XAttribute("appVersion", Constants.Version));
            TextManager.OptimizeForSave(this);
            root.Add(TextManager.Serialize(true));
            root.Add(ActiveChild.Serialize());
            xDoc.Add(root);
            xDoc.Save(stream);
            TextManager.RestoreAfterSave(this);
        }

        public async Task LoadFileAsync(Stream stream)
        {
            UndoManager.ClearAll();
            DeSelect();
            var xDoc = XDocument.Load(stream, LoadOptions.PreserveWhitespace);
            var root = xDoc.Root ?? throw new InvalidOperationException("File is empty or corrupted.");
            XAttribute fileVersionAttribute;
            XAttribute appVersionAttribute;

            if (root.Name == GetType().Name)
            {
                var formattingElement = root.Element("TextManager")!;
                TextManager.DeSerialize(formattingElement);
                fileVersionAttribute = root.Attributes("fileVersion").First();
                appVersionAttribute = root.Attributes("appVersion").First();
                root = root.Element("RowContainer") ?? throw new InvalidOperationException("File is corrupted.");
            }
            else
            {
                fileVersionAttribute = root.Attributes("fileVersion").First();
                appVersionAttribute = root.Attributes("appVersion").First();
            }
            var appVersion = appVersionAttribute != null ? appVersionAttribute.Value : "Unknown";
            if (fileVersionAttribute == null || fileVersionAttribute.Value != fileVersion)
            {
                await ContentDialogHelper.ShowAsync(Owner, Localize.EquationRoot_FileVersionDifferent(appVersion, Environment.NewLine),
                    Localize.Error(), MessageBoxButton.OK);
            }
            ActiveChild.DeSerialize(root);
            CalculateSize();
            AdjustCarets();
        }

        public override void HandleMouseDrag(Point mousePoint)
        {
            if (!Owner.IsSelecting)
            {
                ActiveChild.StartSelection();
                Owner.IsSelecting = true;
            }
            ActiveChild.HandleMouseDrag(mousePoint);
            AdjustCarets();
        }

        public override void HandleMouseDoubleClick(Point mousePoint)
        {
            ActiveChild.HandleMouseDoubleClick(mousePoint);
            AdjustCarets();
        }

        public override bool ConsumeMouseClick(Point mousePoint)
        {
            if (Keyboard.IsKeyDown(Owner, Key.LeftShift) || Keyboard.IsKeyDown(Owner, Key.RightShift))
            {
                Owner.IsSelecting = true;
                ActiveChild.StartSelection();
                ActiveChild.HandleMouseDrag(mousePoint);
            }
            else
            {
                ActiveChild.ConsumeMouseClick(mousePoint); //never forget, EquationRoot has only one child at all times!!                
                Owner.IsSelecting = true; //else DeSelect() might not work!
                DeSelect();
            }
            AdjustCarets();
            return true;
        }

        public override void SelectAll()
        {
            DeSelect();
            ActiveChild.SelectAll();
            if (!Owner.IsSelecting)
            {
                Owner.IsSelecting = true;
            }
        }

        public void HandleUserCommand(CommandDetails commandDetails)
        {
            if (commandDetails.CommandType == CommandType.Text)
            {
                ConsumeText(commandDetails.UnicodeString); //ConsumeText() will call DeSelect() itself. No worries here
            }
            else
            {
                var undoCount = UndoManager.UndoCount + 1;
                if (Owner.IsSelecting)
                {
                    ActiveChild.RemoveSelection(true);
                }
                ((EquationContainer)ActiveChild).ExecuteCommand(commandDetails.CommandType, commandDetails.CommandParam);
                if (Owner.IsSelecting && undoCount < UndoManager.UndoCount)
                {
                    UndoManager.ChangeUndoCountOfLastAction(1);
                }
                CalculateSize();
                AdjustCarets();
                DeSelect();
            }
        }

        public void AdjustCarets()
        {
            _vCaret.Location = ActiveChild.GetVerticalCaretLocation();
            _vCaret.CaretLength = ActiveChild.GetVerticalCaretLength();
            var innerMost = ((RowContainer)ActiveChild).GetInnerMostEquationContainer();
            _hCaret.Location = innerMost.GetHorizontalCaretLocation();
            _hCaret.CaretLength = innerMost.GetHorizontalCaretLength();
            // When the carets move, rerender them and force them to be visible
            Editor?.ForceCaretVisible(true);
        }

        public override CopyDataObject Copy(bool removeSelection)
        {
            var temp = base.Copy(removeSelection) ??
                throw new InvalidOperationException("Copy failed in EquationRoot.");

            // Prepare data object for clipboard
            _ = Task.Run(async () =>
            {
                using var handle = await ClipboardAvalonia.OpenAsync();
                var rootElement = new XElement(GetType().Name);
                rootElement.Add(new XElement("SessionId", sessionString));
                rootElement.Add(TextManager.Serialize(true));
                rootElement.Add(new XElement("payload", temp.XElement));
                handle.SetFormat(ClipboardXmlFormat, rootElement.ToString());

                if (temp.Image != null)
                {
                    handle.SetImage(temp.Image);
                }
                if (temp.Text != null)
                {
                    handle.SetText(temp.Text);
                }
            });


            // Remove selection if needed
            if (removeSelection)
            {
                DeSelect();
                AdjustCarets();
            }

            return temp;
        }

        public override void Paste(XElement xe)
        {
            var id = xe.Element("SessionId")?.Value;
            if (id != sessionString)
            {
                TextManager.ProcessPastedXML(xe);
            }
            var undoCount = UndoManager.UndoCount + 1;
            if (Owner.IsSelecting)
            {
                ActiveChild.RemoveSelection(true);
            }
            ActiveChild.Paste(xe.Element("payload")!.Elements().First());
            if (Owner.IsSelecting && undoCount < UndoManager.UndoCount)
            {
                UndoManager.ChangeUndoCountOfLastAction(1);
            }
            CalculateSize();
            AdjustCarets();
            DeSelect();
        }

        public bool PasteFromClipBoard()
        {
            // Get data from clipboard with retries
            var data = Owner.ClipboardHelper.PasteObject;

            // Parse and paste data
            var success = false;
            try
            {
                if (data is MathEditorData mathEditorData)
                {
                    var element = XElement.Parse(mathEditorData.XmlString, LoadOptions.PreserveWhitespace);
                    Paste(element);
                    success = true;
                }
                else if (data is string textData)
                {
                    ConsumeText(textData);
                    success = true;
                }
            }
            catch (Exception e)
            {
                var dataType = data != null ? "XML Data" : "Text Data";
                EditorLogger.Error(ClassName, $"Failed to parse from {dataType} data", e);
            }
            return success;
        }

        public static bool CanPasteFromClipboard(out object? data)
        {
            data = null;
            try
            {
                using var handle = ClipboardAvalonia.Open();
                if (handle.ContainsFormat(ClipboardXmlFormat))
                {
                    if (handle.GetFormatType(ClipboardXmlFormat) is string xmlString)
                    {
                        data = new MathEditorData { XmlString = xmlString };
                        return true;
                    }
                }
                else if (handle.ContainsText())
                {
                    var textString = ClipboardAvalonia.GetText();
                    if (!string.IsNullOrEmpty(textString))
                    {
                        data = textString;
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                EditorLogger.Error(ClassName, "Failed to check clipboard data", e);
            }
            return false;
        }

        public override void ConsumeText(string text)
        {
            var undoCount = UndoManager.UndoCount + 1;
            if (Owner.IsSelecting)
            {
                ActiveChild.RemoveSelection(true);
            }
            ActiveChild.ConsumeText(text);
            if (Owner.IsSelecting && undoCount < UndoManager.UndoCount)
            {
                UndoManager.ChangeUndoCountOfLastAction(1);
            }
            CalculateSize();
            AdjustCarets();
            DeSelect();
        }

        public override void DeSelect()
        {
            if (Owner.IsSelecting)
            {
                base.DeSelect();
                Owner.IsSelecting = false;
            }
        }

        public void DrawVisibleRows(DrawingContext dc, double top, double bottom, bool forceBlackBrush)
        {
            ((RowContainer)ActiveChild).DrawVisibleRows(dc, top, bottom, forceBlackBrush);
        }

        public async Task SaveImageToFileAsync(string path)
        {
            var extension = Path.GetExtension(path).ToLower();

            var bmpWidth = (int)Math.Ceiling(Width + Location.X * 2);
            var bmpHeight = (int)Math.Ceiling(Width + Location.Y * 2);

            // Create Avalonia RenderTargetBitmap
            var bitmap = new RenderTargetBitmap(new PixelSize(bmpWidth, bmpHeight), new Vector(96, 96));

            // Disable selection highlight during printing
            var oldSelecting = Owner.IsSelecting;
            Owner.IsSelecting = false;
            try
            {
                // Draw directly into the bitmap using Avalonia drawing context
                using var dc = bitmap.CreateDrawingContext();

                // Fill background white for bmp and jpg
                if (extension is ".bmp" or ".jpg")
                {
                    dc.FillRectangle(Brushes.White, new Rect(0, 0, bmpWidth, bmpHeight));
                }

                // Draw the equation into the bitmap context, forcing black brush for fidelity
                ActiveChild.DrawEquation(dc, true);
            }
            finally
            {
                Owner.IsSelecting = oldSelecting;
            }

            if (extension is not ".png" and not ".jpg" and not ".bmp" and not ".gif" and not ".tif" and not ".wdp")
            {
                throw new InvalidOperationException("Unsupported image format.");
            }

            try
            {
                using Stream s = File.Create(path);
                bitmap.Save(s);
            }
            catch (Exception e)
            {
                EditorLogger.Fatal(ClassName, "Failed to save file", e);
                await ContentDialogHelper.ShowAsync(Owner, Localize.EditorControl_CannotSaveFile(),
                    Localize.Error(), MessageBoxButton.OK);
            }
        }

        /*public async Task PrintAsync(PrintDialog printDialog)
        {
            try
            {
                // Determine printable area
                var printableWidth = printDialog.PrintableAreaWidth;
                var printableHeight = printDialog.PrintableAreaHeight;
                if (printableWidth <= 0 || printableHeight <= 0)
                {
                    // Fallback to page media size if printable area is not provided
                    var pageSize = printDialog.PrintQueue?.DefaultPrintTicket?.PageMediaSize;
                    printableWidth = pageSize?.Width ?? (Width + Location.X * 2 + 50);
                    printableHeight = pageSize?.Height ?? (Height + Location.Y * 2 + 50);
                }

                // Content bounds (same as used for exporting image)
                var contentWidth = Math.Ceiling(Width + Location.X * 2);
                var contentHeight = Math.Ceiling(Height + Location.Y * 2);

                // Scale to fit
                var scaleX = printableWidth / contentWidth;
                var scaleY = printableHeight / contentHeight;
                var scale = Math.Min(scaleX, scaleY);

                var dv = new DrawingVisual();
                // Disable selection highlight during printing
                var oldSelecting = Owner.IsSelecting;
                Owner.IsSelecting = false;
                try
                {
#pragma warning disable IDE0063
                    using (var dc = dv.RenderOpen())
                    {
                        // Center after scaling
                        var translateX = (printableWidth / scale - contentWidth) / 2.0;
                        var translateY = (printableHeight / scale - contentHeight) / 2.0;

                        var tg = new TransformGroup();
                        tg.Children.Add(new ScaleTransform(scale, scale));
                        tg.Children.Add(new TranslateTransform(translateX, translateY));

                        dc.PushTransform(tg);

                        // Optional: white background for better print contrast
                        dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, contentWidth, contentHeight));

                        // Draw entire document in black for printing
                        ActiveChild.DrawEquation(dc, true);

                        dc.Pop();
                    }
#pragma warning restore IDE0063
                }
                finally
                {
                    Owner.IsSelecting = oldSelecting;
                }

                printDialog.PrintVisual(dv, Constants.MathEditorFullName);
            }
            catch (Exception e)
            {
                EditorLogger.Fatal(ClassName, "Failed to print document", e);
                await ContentDialogHelper.ShowAsync(Owner, Localize.EditorControl_CannotPrintFile(),
                    Localize.Error(), MessageBoxButton.OK);
            }
        }*/

        public override bool ConsumeKey(Key key)
        {
            if ((Keyboard.IsKeyDown(Owner, Key.LeftShift) || Keyboard.IsKeyDown(Owner, Key.RightShift)) && (new[] { Key.Right, Key.Left, Key.Up, Key.Down, Key.Home, Key.End }).Contains(key))
            {
                if (!Owner.IsSelecting)
                {
                    Owner.IsSelecting = true;
                    ((RowContainer)ActiveChild).StartSelection();
                }
                ActiveChild.Select(key);
                AdjustCarets();
                return true;
            }
            Key[] handledKeys = [Key.Left, Key.Right, Key.Delete, Key.Up, Key.Down, Key.Enter, Key.Escape, Key.Back, Key.Home, Key.End];
            var result = false;
            if (handledKeys.Contains(key))
            {
                result = true;
                if (Owner.IsSelecting && (new[] { Key.Delete, Key.Enter, Key.Back }).Contains(key))
                {
                    ActiveChild.RemoveSelection(true);
                }
                else
                {
                    ActiveChild.ConsumeKey(key);
                }
                CalculateSize();
                AdjustCarets();
                DeSelect();
            }
            return result;
        }

        public override void RemoveSelection(bool registerUndo)
        {
            if (Owner.IsSelecting)
            {
                ActiveChild.RemoveSelection(registerUndo);
                CalculateSize();
                AdjustCarets();
                DeSelect();
            }
        }

        protected override void CalculateWidth()
        {
            Width = ActiveChild.Width;
        }

        protected override void CalculateHeight()
        {
            Height = ActiveChild.Height;
        }

        public void ZoomOut(int difference)
        {
            FontSize -= difference;
        }

        public void ZoomIn(int difference)
        {
            FontSize += difference;
        }

        public void ChangeFont(FontType fontType)
        {
            Owner.TextFontType = fontType;
            ActiveChild.FontSize = FontSize;
            CalculateSize();
            AdjustCarets();
        }

        public override double FontSize
        {
            get => base.FontSize;
            set
            {
                base.FontSize = value;
                AdjustCarets();
            }
        }
    }
}
