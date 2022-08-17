using Common;
using Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Xml;
using System.Windows.Shapes;
using Brushes = System.Drawing.Brushes;
using Pen = System.Drawing.Pen;
using Size = System.Drawing.Size;
using WpfApp1.DIalogs;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly MapHandler mapHandler = new MapHandler();
        readonly ScaleTransform scale = new ScaleTransform();
        int i = 0;
        int j = 0;
        int count = 0;
        Rectangle rectangle1 = new Rectangle();
        Rectangle rectangle2 = new Rectangle();
        bool drawEllipse = false, drawPolygon = false, addText = false, clearPerformed = false, textShape = false;
        UIElement lastElement;
        TextBlock lastText = null;
        List<UIElement> elements = new List<UIElement>();
        PointCollection points = new PointCollection();

        public MainWindow()
        {
            InitializeComponent();
            canvas.LayoutTransform = scale;
            canvas.Background = new SolidColorBrush(Colors.White);
        }

        private void DrawEntities()
        {
            for (int i = 0; i < MapHandler.ArraySize; i++)
            {
                for (int j = 0; j < MapHandler.ArraySize; j++)
                {
                    if (MapHandler.BigArray[i, j] != null && MapHandler.BigArray[i, j].entityPlaced)
                    {
                        DrawEntity(MapHandler.BigArray[i, j], i, j);
                    }
                }
            }
        }

        private void DrawEntity(EntityWrapper entity, int x, int y)
        {
            Rectangle rect = new Rectangle
            {
                Width = MapHandler.ArraySize / 100d,
                Height = MapHandler.ArraySize / 100d,
                Stroke = new SolidColorBrush(Colors.Black),
                StrokeThickness = MapHandler.ArraySize / 100d / 4.0
            };
            switch (entity.entityType)
            {
                case EntityType.Substation:
                    rect.Fill = new SolidColorBrush(Colors.Teal);
                    break;
                case EntityType.Switch:
                    rect.Fill = new SolidColorBrush(Colors.Red);
                    break;
                case EntityType.Node:
                    rect.Fill = new SolidColorBrush(Colors.Green);
                    break;
                default:
                    break;
            }
            rect.ToolTip = "Name:" + entity.powerEntity.Name + "\nID:" + entity.powerEntity.Id.ToString();
            Canvas.SetLeft(rect, x * MapHandler.ArraySize / 100d - 2);
            Canvas.SetTop(rect, (MapHandler.ArraySize - y) * MapHandler.ArraySize / 100d - 2);
            canvas.Children.Add(rect);
        }

        private void DrawMap()
        {
            DrawLines();
            DrawEntities();
        }

        private void DrawLines()
        {
            for (int i = 0; i < MapHandler.ArraySize; i++)
            {
                for (int j = 0; j < MapHandler.ArraySize; j++)
                {
                    if (MapHandler.BigArray[i, j] != null && MapHandler.BigArray[i, j].lineDrawn)
                    {
                        DrawLine(MapHandler.BigArray[i, j], i, j);
                    }
                }
            }
        }
        private void DrawLine(EntityWrapper entityWrapper, int i, int j)
        {
            string lineType = entityWrapper.lineType.ToString();
            Uri uri = new Uri(@"..\..\..\Common\Pictures\" + lineType + ".png", UriKind.Relative);
            BitmapImage bitmapImage = new BitmapImage(uri);
            ImageBrush imageBrush = new ImageBrush
            {
                ImageSource = bitmapImage
            };
            imageBrush.Freeze();
            Rectangle rect = new Rectangle
            {
                Name = "coord_" + i + "_" + j,
                Width = MapHandler.ArraySize / 100d,
                Height = MapHandler.ArraySize / 100d,
                Stroke = null,
                Fill = imageBrush
            };
            foreach (var item in entityWrapper.lines)
            {
                rect.ToolTip += "Name:" + item.Name + "\nID:" + item.Id.ToString() + "\n";
            }
            Canvas.SetLeft(rect, i * MapHandler.ArraySize / 100d - 2);
            Canvas.SetTop(rect, (MapHandler.ArraySize - j) * MapHandler.ArraySize / 100d - 2);
            rect.MouseRightButtonDown += Line_HandleRightClick;
            canvas.Children.Add(rect);
        }

        private void Line_HandleRightClick(object sender, EventArgs e)
        {
            string temp = ((Rectangle)sender).Name;
            string[] splitted = temp.Split('_');
            int tempint1 = int.Parse(splitted[1]);
            int tempint2 = int.Parse(splitted[2]);
            if (!(tempint1 == i && tempint2 == j))
            {
                i = tempint1;
                j = tempint2;
                count = 0;
            }
            int X1, Y1, X2, Y2;
            EntityWrapper entityWrapper = MapHandler.BigArray[i, j];
            canvas.Children.Remove(rectangle1);
            canvas.Children.Remove(rectangle2);
            if (count >= entityWrapper.lineEntities.Count)
            {
                count = 0;
                return;
            }
            X1 = MapHandler.GetTuple(entityWrapper.lineEntities[count].Item1).Item1;
            Y1 = MapHandler.GetTuple(entityWrapper.lineEntities[count].Item1).Item2;
            X2 = MapHandler.GetTuple(entityWrapper.lineEntities[count].Item2).Item1;
            Y2 = MapHandler.GetTuple(entityWrapper.lineEntities[count].Item2).Item2;

            Canvas.SetLeft(rectangle1, X1 * MapHandler.ArraySize / 100d - 2);
            Canvas.SetTop(rectangle1, (MapHandler.ArraySize - Y1) * MapHandler.ArraySize / 100d - 2);
            canvas.Children.Add(rectangle1);
            Canvas.SetLeft(rectangle2, X2 * MapHandler.ArraySize / 100d - 2);
            Canvas.SetTop(rectangle2, (MapHandler.ArraySize - Y2) * MapHandler.ArraySize / 100d - 2);
            canvas.Children.Add(rectangle2);
            count++;
        }

        private void SetRect(Rectangle rect)
        {
            rect.Opacity = 0.75;
            rect.Width = MapHandler.ArraySize / 100d;
            rect.Height = MapHandler.ArraySize / 100d;
            rect.Stroke = null;
            rect.Fill = new SolidColorBrush(Colors.White);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            drawPolygon = true;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            addText = true;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if(clearPerformed)
            {
                foreach (var item in elements)
                {
                    if(!canvas.Children.Contains(item))  
                        canvas.Children.Add(item);
                }
                return;
            }
            if(lastElement != null)
            {
                if (canvas.Children.Contains(lastElement))
                {
                    canvas.Children.Remove(lastElement);
                    elements.Remove(lastElement);
                    if (lastText != null && canvas.Children.Contains(lastText))
                    {
                        canvas.Children.Remove(lastText);
                        elements.Remove(lastText);
                    }
                }
            }
            //Undo
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            if(lastElement != null)
            {
                if (!canvas.Children.Contains(lastElement))
                {
                    canvas.Children.Add(lastElement);
                    if(!elements.Contains(lastElement))
                        elements.Add(lastElement);

                    if (lastText != null && !canvas.Children.Contains(lastText))
                    {
                        canvas.Children.Add(lastText);
                        if(!elements.Contains(lastText))
                            elements.Add(lastText);
                    }
                }
            }
            //Redo
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            clearPerformed = true;
            canvas.Children.Clear();
            lastElement = null;
            DrawMap();
            //Clear
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(drawPolygon && points.Count > 2)
            {
                DrawPolygon();
                drawPolygon = false;
            }
        }

        private void Canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(drawEllipse)
            {
                DrawEllipse(e);
                drawPolygon = false;
                addText = false;
            }
            else if(drawPolygon)
            {
                points.Add(new System.Windows.Point(e.GetPosition(canvas).X, e.GetPosition(canvas).Y));
                return;
            }
            else if(addText)
            {
                DrawText(e);
                lastText = null;
            }
            points.Clear();
            drawEllipse = false;
            drawPolygon = false;
            addText = false;
            textShape = false;
            if (clearPerformed)
            {
                elements.Clear();
                clearPerformed = false;
                lastText = null;
            }
        }
        private void DrawPolygon()
        {
            var dialog = new PolygonWindow();
            var result = dialog.ShowDialog();

            TextBlock textBlock = new TextBlock
            {
                Text = dialog.NewText,
                FontSize = 12,
                Foreground = dialog.NewTextColor
            };

                Canvas.SetTop(textBlock, points.Average(t => t.Y));
                Canvas.SetLeft(textBlock, points.Average(t => t.X));

                Polygon polygon = new Polygon
                {
                    Fill = dialog.NewFill,
                    Stroke = dialog.NewStroke,
                    StrokeThickness = dialog.NewThickness,
                    Points = new PointCollection(points)
                };
                points.Clear();
                polygon.MouseLeftButtonDown += UIElement_Modify;

                canvas.Children.Add(polygon);
                canvas.Children.Add(textBlock);
                lastElement = polygon;
                lastText = textBlock;
                elements.Add(polygon);
                elements.Add(textBlock);
        }

        private void DrawText(MouseButtonEventArgs e)
        {
            double X = e.GetPosition(canvas).X, Y = e.GetPosition(canvas).Y;
            var dialog = new TextWindow();
            var result = dialog.ShowDialog();

            TextBlock textBlock = new TextBlock
            {
                Text = dialog.NewText,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = dialog.NewTextColor,
                FontSize = dialog.NewSize
            };
            textBlock.MouseLeftButtonDown += UIElement_Modify;
            Canvas.SetTop(textBlock, Y);
            Canvas.SetLeft(textBlock, X);
            canvas.Children.Add(textBlock);
            lastElement = textBlock;
            elements.Add(textBlock);
        }



        private void DrawEllipse(MouseButtonEventArgs e)
        {
            double X = e.GetPosition(canvas).X, Y = e.GetPosition(canvas).Y;
            var dialog = new EllipseWindow();
            var result = dialog.ShowDialog();

            TextBlock textBlock = new TextBlock
            {
                Text = dialog.NewText,
                Foreground = dialog.NewTextColor,
                FontSize = 12
            };

            Canvas.SetTop(textBlock, Y + dialog.NewHeight/4);
            Canvas.SetLeft(textBlock, X + dialog.NewWidth/4);

            Ellipse ellipse = new Ellipse
            {
                Width = dialog.NewWidth,
                Height = dialog.NewHeight,
                Fill = dialog.NewFill,
                Stroke = dialog.NewStroke,
                StrokeThickness = dialog.NewThickness
            };
            ellipse.MouseLeftButtonDown += UIElement_Modify;
            Canvas.SetTop(ellipse, Y);
            Canvas.SetLeft(ellipse, X);

            canvas.Children.Add(ellipse);
            canvas.Children.Add(textBlock);
            lastElement = ellipse;
            lastText = textBlock;
            elements.Add(ellipse); 
            elements.Add(textBlock);
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            scale.ScaleX = ((Slider)sender).Value;
            scale.ScaleY = ((Slider)sender).Value;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            drawEllipse = true;
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)canvas.RenderSize.Width,
                (int)canvas.RenderSize.Height, 96d, 96d, PixelFormats.Default);
            rtb.Render(canvas);

            BitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(rtb));

            using (var fs = System.IO.File.OpenWrite(DateTime.Now.ToString().Replace("/","_").Replace(":","_") + ".png"))
            {
                pngEncoder.Save(fs);
            }
        }

        private void DrawMapCall(bool noNodes = false)
        {
            canvas.Children.Clear();
            canvas.Width = canvas.Height = Int32.Parse(canvasSize.Text) * (Int32.Parse(canvasSize.Text) / 100d);
            MapHandler.ArraySize = Int32.Parse(canvasSize.Text);
            SetRect(rectangle1);
            SetRect(rectangle2);
            DateTime dateTime = DateTime.Now;
            mapHandler.CalculateEntities(noNodes);
            DrawMap();
            timeNeeded.Content = (DateTime.Now - dateTime).TotalSeconds;
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            DrawMapCall();
        }

        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            DrawMapCall(true);
        }

        private void UIElement_Modify(object sender, EventArgs e)
        {
            if (sender.GetType() == typeof(Ellipse))
            {
                ModifyEllipse((Ellipse)sender);
                return;
            }
            if (sender.GetType() == typeof(Polygon))
            {
                ModifyPolygon((Polygon)sender);
                return;
            }
            if (sender.GetType() == typeof(TextBlock))
            {
                ModifyTextBlock((TextBlock)sender);
                return;
            }
            return;
        }

        private void ModifyEllipse(Ellipse ellipse)
        {
            var dialog = new EllipseWindow(true);
            dialog.ShowDialog();
            ellipse.StrokeThickness = dialog.NewThickness;
            ellipse.Fill = dialog.NewFill;
            ellipse.Stroke = dialog.NewStroke;
        }
        private void ModifyPolygon(Polygon polygon)
        {
            var dialog = new PolygonWindow(true);
            dialog.ShowDialog();
            polygon.StrokeThickness = dialog.NewThickness;
            polygon.Fill = dialog.NewFill;
            polygon.Stroke = dialog.NewStroke;
        }
        private void ModifyTextBlock(TextBlock textBlock)
        {
            var dialog = new TextWindow(true);
            dialog.ShowDialog();
            textBlock.Foreground = dialog.NewTextColor;
            textBlock.FontSize = dialog.NewSize;
        }
    }
}
