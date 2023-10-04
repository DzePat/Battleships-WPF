﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
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
using System.Windows.Shapes;

namespace Battleships_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Classes.Player MainPlayer = new Classes.Player(1, 3);
        public static MainWindow Instance { get; private set; }

        public static string MouseHover;

        public static List<Image> Images = new List<Image>();
        public static string[] boatLibrary = new string[] { "BigBoat", "MediumBoat", "LittleBoat" };
        public static List<Classes.BoatTemplate> boatTemplates = new List<Classes.BoatTemplate>();

        //Preview Window variables
        public Classes.PreviewWindow previewWindow = new Classes.PreviewWindow();
        public Classes.Boat currentPreviewBoat;
        public Classes.Match currentMatch;

        public static string projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
        public MainWindow()
        {
            InitializeComponent();

            Instance = this;
            PopulateBoatTemplates();

            currentPreviewBoat = new Classes.Boat("BigBoat", 3);

            LoadImages();   //Fill Images list with images, of the elements in Boat Library

            currentMatch = new Classes.Match(-1, -1, -1, -1);
            currentMatch.CreateMap(watertiles, watertiles2);
            
            previewWindow.CreateBoatImage(ImageCanvas);
            currentMatch.CreateTitleImage(TitleCanvas);
        }

        private Classes.Boat addBoatParts(Classes.Boat boat)
        {
            for (int i = 0;i < boat.BoatSize;i++)
            {
                boat.parts.Add(new Classes.BoatParts(0, 0));
            }
            return boat;
        }

        private Classes.Boat SetBoatPartPositions(Classes.Boat boat)
        {
            int originalRow = boat.row_number;
            int originalCol = boat.column_number;
            foreach(Classes.BoatParts boatPart in boat.parts)
            {
                if(boat.currentOrientation == "Horizontal")
                {
                    boatPart.rowPos = originalRow;
                    boatPart.colPos = originalCol;
                    originalCol += 1;
                }
                else if(boat.currentOrientation == "Vertical")
                {
                    boatPart.rowPos = originalRow;
                    boatPart.colPos = originalCol;
                    originalRow += 1;
                }
            }
            return boat;
        }
        private void TitleButton_Click(object sender, RoutedEventArgs e)
        {
            //Gömmer huvudmenyn när man klickar på 'Begin Game'
            TitleCanvas.Visibility = Visibility.Collapsed;
            TitleButton.Visibility = Visibility.Collapsed;
            watertiles.Visibility = Visibility.Visible;
            watertiles2.Visibility = Visibility.Visible;
            ButtonGrid.Visibility = Visibility.Visible;
            Border1.Visibility = Visibility.Visible;
            Border2.Visibility = Visibility.Visible;
            Border3.Visibility = Visibility.Visible;
        }
        void LoadImages()
        {
            //FIXME: Move to library class when we have one...

            
            Classes.Boat boat = new Classes.Boat(GetBoatTemplate(currentPreviewBoat.boatName));
            

            for(int i = 0; i < boatLibrary.Length; i++)
            {
                TransformedBitmap transformBmp = new TransformedBitmap();
                BitmapImage bmpImage = new BitmapImage();

                bmpImage.BeginInit();

                string boatName = boatLibrary[i];

                bmpImage.UriSource = new Uri(projectDirectory + $"\\Images\\Boats\\{boatName}.png", UriKind.RelativeOrAbsolute);

                bmpImage.EndInit();

                transformBmp.BeginInit();

                transformBmp.Source = bmpImage;

                RotateTransform transform = new RotateTransform(0);

                transformBmp.Transform = transform;

                transformBmp.EndInit();

                Image BodyImage = new Image
                {
                    Width = 51,
                    Height = 153,
                    Name = boatName,
                    Source = transformBmp
                    //new BitmapImage(new Uri(projectDirectory+"\\Images\\BigBoat\\BigBoat.png", UriKind.Absolute)), 
                    //transformBmp
                };
                BodyImage.MouseMove += BodyImage_MouseMove;
            }
        }

        void PopulateBoatTemplates()
        {
            for (int i = 0; i < boatLibrary.Length; i++)
            {
                Classes.BoatTemplate boatTemplate = new Classes.BoatTemplate() { boatName = boatLibrary[i] };

                if(boatTemplate.boatName == "BigBoat")
                {
                    boatTemplate.boatSize = 3;
                }
                else if (boatTemplate.boatName == "MediumBoat")
                {
                    boatTemplate.boatSize = 2;
                }
                else if (boatTemplate.boatName == "LittleBoat")
                {
                    boatTemplate.boatSize = 1;
                }

                TransformedBitmap transformBmp = new TransformedBitmap();
                BitmapImage bmpImage = new BitmapImage();

                bmpImage.BeginInit();

                string boatName = boatLibrary[i];

                bmpImage.UriSource = new Uri(projectDirectory + $"\\Images\\Boats\\{boatName}.png", UriKind.RelativeOrAbsolute);

                bmpImage.EndInit();

                transformBmp.BeginInit();

                transformBmp.Source = bmpImage;

                RotateTransform transform = new RotateTransform(0);

                transformBmp.Transform = transform;

                transformBmp.EndInit();

                Image BodyImage = new Image
                {
                    Width = 51,
                    Height = 153,
                    Name = boatName,
                    Source = transformBmp
                    //new BitmapImage(new Uri(projectDirectory+"\\Images\\BigBoat\\BigBoat.png", UriKind.Absolute)), 
                    //transformBmp
                };

                boatTemplate.boatImage = BodyImage;
                MainPlayer.boats.Add(addBoatParts(new Classes.Boat(boatTemplate)));
                boatTemplates.Add(boatTemplate);
            }
        }

        public Classes.BoatTemplate GetBoatTemplate(string boatName)
        {
            Classes.BoatTemplate boatTemplate = new Classes.BoatTemplate();

            foreach (var template in boatTemplates)
            {
                if(template.boatName == boatName)
                {
                    boatTemplate = template;
                }
            }

            return boatTemplate;
        }

        public void CreateBoatImage(int rotationValue=0, string boatName="BigBoat")
        {
            previewWindow.CreateBoatImage(ImageCanvas, rotationValue, boatName);
        }

        private void GridDrop(object sender, DragEventArgs e)
        {
            object data = e.Data.GetData(DataFormats.Serializable);
            if (data is UIElement element)
            {
                element.Uid = Images[Classes.PreviewWindow.currentBoatVisualIndex].Name;
                var Pos = (UIElement)e.Source;
                int c = Grid.GetColumn(Pos);
                int r = Grid.GetRow(Pos);
                Grid.SetColumn(element, c);
                Grid.SetRow(element, r);

                Image image = (Image)data;

                int boatSize = currentPreviewBoat.BoatSize;

                if (currentPreviewBoat.currentOrientation == "Vertical")
                {
                    Image boatImage = data as Image;
                    if (boatImage != null)
                    {
                        boatImage.Height /= 3;
                        boatImage.Height *= boatSize;
                    }
                    MainPlayer.boats[Classes.PreviewWindow.currentBoatVisualIndex].currentOrientation = "Vertical";
                    Grid.SetRowSpan(element, boatSize);
                    Grid.SetColumnSpan(element, 1);
                }
                else if (currentPreviewBoat.currentOrientation == "Horizontal")
                {
                    Image boatImage = data as Image;
                    if (boatImage != null)
                    {
                        boatImage.Width /= 3;
                        boatImage.Width *= boatSize;
                    }
                    MainPlayer.boats[Classes.PreviewWindow.currentBoatVisualIndex].currentOrientation = "Horizontal";
                    Grid.SetColumnSpan(element, boatSize);
                    Grid.SetRowSpan(element, 1);
                }
                UIElement checker = null;
                foreach (UIElement el in watertiles.Children)
                {
                    if (el.Uid == element.Uid)
                    {
                        checker = el;
                    }
                }
                if (checker != null)
                {
                    watertiles.Children.Remove(checker);
                }
                MainPlayer.boats[Classes.PreviewWindow.currentBoatVisualIndex].column_number = c;
                MainPlayer.boats[Classes.PreviewWindow.currentBoatVisualIndex].row_number = r;
                MainPlayer.boats[Classes.PreviewWindow.currentBoatVisualIndex] = SetBoatPartPositions(MainPlayer.boats[Classes.PreviewWindow.currentBoatVisualIndex]);
                watertiles.Children.Add(element);

            }
        }
        private void Drag_Leave(object sender, DragEventArgs e)
        {
            if (e.OriginalSource == ImageCanvas)
            {
                object data = e.Data.GetData(DataFormats.Serializable);
                if (data is UIElement element)
                {
                    ImageCanvas.Children.Remove(element);
                }
            }
            else
            {
                object data = e.Data.GetData(DataFormats.Serializable);
                if (data is UIElement element)
                {
                    watertiles.Children.Remove(element);
                }
            }

        }

        public static void BodyImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Image currentImage = Images[Classes.PreviewWindow.currentBoatVisualIndex];
                DragDrop.DoDragDrop(currentImage, new DataObject(DataFormats.Serializable, currentImage), DragDropEffects.Move);
            }
        }
        private void ImageDrop(object sender, DragEventArgs e)
        {
            Point dropposition = e.GetPosition(ImageCanvas);
            Canvas.SetLeft(ImageCanvas.Children[0], dropposition.X-25);
            Canvas.SetTop(ImageCanvas.Children[0], dropposition.Y-50);
        }

        public void button_Click(object sender, RoutedEventArgs e)
        {
            Button srcButton = e.Source as Button;
            string buttonpressed = srcButton.Name;
            ButtonX.Text = buttonpressed;              
        }

        public void button_Enter(object sender, MouseEventArgs e)
        {
            Button srcButton = e.Source as Button;
            string buttonHover = srcButton.Name;
            MousePositionText.Text = $"MouseOver : {buttonHover}";
            MouseHover = buttonHover;

        }

        private void SpinButton_Click(object sender, RoutedEventArgs e)
        {
            previewWindow.SpinBoat(ImageCanvas, currentPreviewBoat);
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            previewWindow.NextButton_Click(ImageCanvas);
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            previewWindow.PreviousButton_Click(ImageCanvas);
        }
    }
}
