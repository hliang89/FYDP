using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FYDP_wpfUI_v2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public double firstSegmentX; 
        public double firstSegmentY;
        public double firstSegmentZ;
        public double firstSegmentW;

        public double FirstSegmentX { get { return firstSegmentX; } set { firstSegmentX = value; NotifyPropertyChanged("FirstSegmentX"); } }
        public double FirstSegmentY { get { return firstSegmentY; } set { firstSegmentY = value; NotifyPropertyChanged("FirstSegmentY"); } }
        public double FirstSegmentZ { get { return firstSegmentZ; } set { firstSegmentZ = value; NotifyPropertyChanged("FirstSegmentZ"); } }
        public double FirstSegmentW { get { return firstSegmentW; } set { firstSegmentW = value; NotifyPropertyChanged("FirstSegmentW"); } }

        public Quaternion firstSegQuaterion = new Quaternion();

        ModelVisual3D modelVisual = new ModelVisual3D();

        Model3DGroup allModels = new Model3DGroup();

        public MainWindow()
        {
            firstSegQuaterion.X = FirstSegmentX;
            firstSegQuaterion.Y = FirstSegmentY;
            firstSegQuaterion.Z = FirstSegmentZ;
            firstSegQuaterion.W = FirstSegmentW;

            InitializeComponent();
            InitializeCameraControls();
            
            InitializeCustomControls();
            InitializeObjectRotateControls();
        }

        

        private void InitializeCameraControls()
        {
            sliderCamX.Minimum = -100;
            sliderCamY.Minimum = -100;
            sliderCamZ.Minimum = -100;
            sliderDirX.Minimum = -100;
            sliderDirY.Minimum = -100;
            sliderDirZ.Minimum = -100;

            sliderCamX.Maximum = 100;
            sliderCamY.Maximum = 100;
            sliderCamZ.Maximum = 100;
            sliderDirX.Maximum = 100;
            sliderDirY.Maximum = 100;
            sliderDirZ.Maximum = 100;

            sliderCamX.Value = 8;
            sliderCamY.Value = 9;
            sliderCamZ.Value = 10;
            sliderDirX.Value = -8;
            sliderDirY.Value = -9;
            sliderDirZ.Value = -10;

            foreach (UIElement e in cameraControlPanel.Children)
            {
                Slider slider = new Slider();

                slider = e as Slider;
                if (slider != null)
                {
                    slider.IsSnapToTickEnabled = true;
                    slider.TickFrequency = 1;

                    slider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(slider_ValueChanged);

                }
            }
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SetCamera();
        }

        private void SetCamera()
        {
            PerspectiveCamera camera = (PerspectiveCamera)mainViewport.Camera;

            Point3D position = new Point3D(
                sliderCamX.Value,
                sliderCamY.Value,
                sliderCamZ.Value
            );
            Vector3D lookDirection = new Vector3D(
                sliderDirX.Value,
                sliderDirY.Value,
                sliderDirZ.Value
            );
            camera.Position = position;
            camera.LookDirection = lookDirection;
        }

        private void InitializeObjectRotateControls()
        {
            foreach (UIElement e in objectControlPanel.Children)
            {
                StackPanel segmentControlPanel = new StackPanel();
                segmentControlPanel = e as StackPanel;

                if (segmentControlPanel != null)
                {
                    if (segmentControlPanel.Name == "firstSegmentControlPanel")
                    {
                        foreach (UIElement i in segmentControlPanel.Children)
                        {
                            Slider slider = i as Slider;
                            if (slider != null)
                            {
                                slider.Minimum = 0;
                                slider.Maximum = 1;

                                slider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(FirstSegmentRotateSlider_ValueChanged);
                            }
                        }
                    }
                    else if (segmentControlPanel.Name == "secondSegmentControlPanel")
                    {
                        foreach (UIElement i in segmentControlPanel.Children)
                        {
                            Slider slider = i as Slider;
                            if (slider != null)
                            {
                                slider.Minimum = 0;
                                slider.Maximum = 1;

                                slider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(SecondSegmentRotateSlider_ValueChanged);
                            }
                        }
                    }


                }
               
            }
        }

        private void FirstSegmentRotateSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
             try
            {
                ModelVisual3D allObjectVisual = new ModelVisual3D();
                allObjectVisual = (ModelVisual3D)mainViewport.Children[1];

                Model3DGroup firstJoint = (Model3DGroup)allObjectVisual.Content;

                Quaternion quaternion = new Quaternion(rotateFirstSegmentXSlider.Value,
                    rotateFirstSegmentYSlider.Value,
                    rotateFirstSegmentZSlider.Value,
                    rotateFirstSegmentWSlider.Value);
                RotateObject(firstJoint, quaternion);
            }
            catch
            {
            }
        }

        private void SecondSegmentRotateSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                ModelVisual3D allObjectVisual = new ModelVisual3D();
                allObjectVisual = (ModelVisual3D)mainViewport.Children[1];

                Model3DGroup all3DGroup = (Model3DGroup)allObjectVisual.Content;
                
                Model3DGroup firstJoint = (Model3DGroup)all3DGroup.Children[0];

                Model3DGroup firstSegment = (Model3DGroup)firstJoint.Children[firstJoint.Children.Count-1];
                Model3DGroup secondJoint = (Model3DGroup)firstSegment.Children[firstSegment.Children.Count - 1];
                Model3DGroup secondSegment = (Model3DGroup)secondJoint.Children[secondJoint.Children.Count - 1];
                //Model3DGroup secondJoint = (Model3DGroup)firstSegment.Children[0];
                //Model3DGroup secondSegment = (Model3DGroup)secondJoint.Children[0];
                Quaternion quaternion = new Quaternion(rotateSecondSegmentXSlider.Value,
                    rotateSecondSegmentYSlider.Value,
                    rotateSecondSegmentZSlider.Value,
                        rotateSecondSegmentWSlider.Value);

                RotateObject(secondSegment, quaternion);
            }
            catch (Exception ex)
            {
            }
        }

        private void RotateObject(Model3DGroup modelGroup, Quaternion quaternion)
        {
            try
            {
                RotateTransform3D objectTransform = new RotateTransform3D();
                modelGroup.Transform = objectTransform;
                QuaternionRotation3D currentQuad3D = new QuaternionRotation3D();
                currentQuad3D = (QuaternionRotation3D)objectTransform.Rotation;

                QuaternionRotation3D newQuad3D = new QuaternionRotation3D();
                newQuad3D.Quaternion = quaternion;
                objectTransform.Rotation = newQuad3D;
            }
            catch
            {
            }
        }

        private void InitializeCustomControls()
        {
            mainViewport.Children.Add(modelVisual);
            modelVisual.Content = allModels;

           sphereButton.Click+=new RoutedEventHandler(sphereButton_Click);
        }

        private void sphereButton_Click(object sender, RoutedEventArgs e)
        {
            AddFirstJoint();
            //allModels.Children.Add(FYDP_3Dobject_Util.MakeSphere(new Point3D(0, 0, 0), 5, 20, 20, Colors.Red));
        }

        private void AddFirstJoint()
        {
            Model3DGroup firstJoint = FYDP_3Dobject_Util.MakeSphere(new Point3D(0, 0, 0), 5, 25, 25, Colors.Red);
            
            AddFirstSegment(firstJoint);
            allModels.Children.Add(firstJoint);
        }

        public event EventHandler FirstSegmentValueChangedEvent;

        protected virtual void OnFirstSegmentValueChangedEvent(EventArgs e)
        {
            if (FirstSegmentValueChangedEvent != null)
            {
                FirstSegmentValueChangedEvent(this, e);
            }
        }

        private void AddFirstSegment(Model3DGroup firstJoint)
        {
            Model3DGroup firstSegment = FYDP_3Dobject_Util.GetCylinder(FYDP_3Dobject_Util.GetSurfaceMaterial(Colors.Brown), new Point3D(0, 0, 0), 2, 20);

            RotateTransform3D firstSegmentTransform = new RotateTransform3D();
            firstSegment.Transform = firstSegmentTransform;

            QuaternionRotation3D firstSegQuatRot = new QuaternionRotation3D();
            Quaternion firstSegQuat = firstSegQuaterion;

            firstSegQuatRot.Quaternion = firstSegQuat;
            firstSegmentTransform.Rotation = firstSegQuatRot;
            firstSegment.Transform = firstSegmentTransform;

            AddSecondJoint(firstSegment);
            firstJoint.Children.Add(firstSegment);
        }


        private void AddSecondJoint(Model3DGroup firstSegment)
        {
            Model3DGroup secondJoint = FYDP_3Dobject_Util.MakeSphere(new Point3D(0, 0, 0), 5, 25, 25, Colors.Red);
            secondJoint.Transform = new TranslateTransform3D(new Vector3D(0, 0, 20));

            AddSecondSegment(secondJoint);
            firstSegment.Children.Add(secondJoint);
        }

        private void AddSecondSegment(Model3DGroup secondJoint)
        {
            Model3DGroup secondSegment = FYDP_3Dobject_Util.GetCylinder(FYDP_3Dobject_Util.GetSurfaceMaterial(Colors.Brown), new Point3D(0, 0, 0), 2, 20);
            secondSegment.Transform = new TranslateTransform3D(new Vector3D(0, 0, 0));

            secondJoint.Children.Add(secondSegment);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this,
                  new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
