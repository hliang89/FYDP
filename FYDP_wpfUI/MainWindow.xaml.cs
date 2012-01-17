using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Windows.Threading;
using System.IO;
using FYDP_DataStructure;
using System.IO.Ports;


namespace FYDP_wpfUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private double q_1, q_2, q_3, q_4 = 0;
        SerialPort MCU_serialPort = new SerialPort();
        List<string> toBeWrittenFiles = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
            InitializeSliderValues();
            InitializeObjectControls();
           
                //// Create a new SerialPort object with default settings.

                //// Allow the user to set the appropriate properties.
                //MCU_serialPort.PortName = "COM3";
                //MCU_serialPort.BaudRate = 1000000;
                //MCU_serialPort.Parity = Parity.None;
                //MCU_serialPort.DataBits = 8;
                //MCU_serialPort.StopBits = StopBits.One;
                //MCU_serialPort.Handshake = Handshake.None;

                //// Set the read/write timeouts
                //MCU_serialPort.ReadTimeout = 500;
                //MCU_serialPort.WriteTimeout = 500;

                //MCU_serialPort.Open();
                
            Thread readThread = new Thread(ReadRawDataFromFile);
            readThread.Start();
        }

        public void ReadRawDataFromFile()
        {
            List<string> allLines = new List<string>();
            string[] heavyRotationAll3axis = File.ReadAllLines(@"C:\Users\h.liang\Documents\output_Jan_09.txt");
            string[] horizontalLines = File.ReadAllLines(@"C:\Users\h.liang\Dropbox\Engineering\Test Cases\Horizontal Rotation.txt");
            string[] leftRightLines = File.ReadAllLines(@"C:\Users\h.liang\Dropbox\Engineering\Test Cases\Left and Right.txt");
            string[] upAndDown = File.ReadAllLines(@"C:\Users\h.liang\Dropbox\Engineering\Test Cases\Up and Down.txt");

            //foreach (string s in horizontalLines)
            //{
            //    allLines.Add(s);
            //}
            //foreach (string s in leftRightLines)
            //{
            //    allLines.Add(s);
            //}
            //foreach (string s in upAndDown)
            //{
            //    allLines.Add(s);
            //}
            foreach (string s in heavyRotationAll3axis)
            {
                allLines.Add(s);
            }

            string[] lines = allLines.ToArray();
            
            List<string> message_List = new List<string>(); // used for displaying the output from serial to the output window during running

            while (true)
            {
                try
                {
                    string firstLine = lines[0];
                    SerialPort_Input input = new SerialPort_Input(firstLine);
                    ConvertToQuaternion conv = new ConvertToQuaternion(input);
                    foreach (string line in lines)
                    {
                        input = new SerialPort_Input(line);
                        conv.UpdateValue(input);
                        q_1 = conv.ASq_1;
                        q_2 = conv.ASq_2;
                        q_3 = conv.ASq_3;
                        q_4 = conv.ASq_4;

                        Console.WriteLine("{0} {1} {2} {3}", q_1, q_2, q_3, q_4);

                        //OnQuadUpdatedEvent(EventArgs.Empty);

                        mainViewport.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send,
                            new Action(delegate() { RotateObject(); }));
                        //Conv_update(conv);
                        //conv.Updated+=new ConvertToQuaternion.EventHandler(conv_Updated);
                    }
                }
                catch
                {
                }
                Thread.Sleep(1000);
            }
        }


        public void ReadRawDataFromSerialPort()
        {
            int stream_line_counter = 0;
            List<string> message_List = new List<string>();

            double prev_serialPort_accel_x = 0;
            double prev_serialPort_accel_y = 0;
            double prev_serialPort_accel_z = 0;
            double prev_serialPort_gyro_x = 0;
            double prev_serialPort_gyro_y = 0;
            double prev_serialPort_gyro_z = 0;
            double prev_serialPort_magneto_x = 0;
            double prev_serialPort_magneto_y = 0;
            double prev_serialPort_magneto_z = 0;

            int serial_line_count = 0;

            int counter = 0;
            while (counter < 10)
            {
                string filler = MCU_serialPort.ReadLine();
                counter++;
            }

            string firstLine = MCU_serialPort.ReadLine();
            SerialPort_Input input = new SerialPort_Input(firstLine);
            ConvertToQuaternion conv = new ConvertToQuaternion(input);

            while (true)
            {
                try
                {
                    string line = MCU_serialPort.ReadLine();


                        input = new SerialPort_Input(line);
                        conv.UpdateValue(input);
                        q_1 = conv.ASq_1;
                        q_2 = conv.ASq_2;
                        q_3 = conv.ASq_3;
                        q_4 = conv.ASq_4;

                        Console.WriteLine("Quaternion: {0} {1} {2} {3}", q_1, q_2, q_3, q_4);
                        Console.WriteLine("Raw data: {0} {1} {2} {3} {4} {5} {6} {7} {8}", input.SP_AccelX, input.SP_AccelY, input.SP_AccelZ, input.SP_GyroX,
                            input.SP_GyroY, input.SP_GyroZ, input.SP_MagnetoX, input.SP_MagentoY, input.SP_MagnetoZ);
                        Console.WriteLine(line);
                        //OnQuadUpdatedEvent(EventArgs.Empty);

                        mainViewport.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send,
                            new Action(delegate() { RotateObject(); }));

                        mainViewport.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send,
                               new Action(delegate() { WriteToFile(line); }));

                    
                  
                }
                catch
                {
                }
                Thread.Sleep(10);
            }
        }

        private void WriteToFile(string line)
        {
            toBeWrittenFiles.Add(line);
        }

        private void InitializeObjectControls()
        {
            foreach (UIElement e in objectControlPanel.Children)
            {
                Slider slider = new Slider();
                slider = e as Slider;

                if (slider != null)
                {
                    slider.Minimum = 0;
                    slider.Maximum = 1;

                    slider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(objectControlSlider_ValueChanged);
                }
            }
        }

        private void objectControlSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            RotateObjectManual();
        }

        private void RotateObjectManual()
        {
            try
            {
                ModelVisual3D visualObject = new ModelVisual3D();
                ModelVisual3D visualObject2 = new ModelVisual3D();
                if (mainViewport.Children.Count > 4)
                {
                    visualObject = mainViewport.Children[3] as ModelVisual3D;

                    if (visualObject != null)
                    {
                        Model3DGroup objectGroup = new Model3DGroup();
                        objectGroup = visualObject.Content as Model3DGroup;

                        if (objectGroup != null)
                        {
                            RotateTransform3D cubeTransform = new RotateTransform3D();
                            objectGroup.Transform = cubeTransform;

                            QuaternionRotation3D cubeQuaternion = new QuaternionRotation3D();

                            cubeTransform.Rotation = cubeQuaternion;

                            //cubeQuaternion.Quaternion = new Quaternion(sliderRotateX.Value, sliderRotateY.Value, sliderRotateZ.Value, sliderRotateW.Value);
                            cubeQuaternion.Quaternion = new Quaternion(sliderRotateX.Value, sliderRotateY.Value, sliderRotateZ.Value, sliderRotateW.Value);
                        }
                    }

                    visualObject2 = mainViewport.Children[4] as ModelVisual3D;
                    if (visualObject2 != null)
                    {
                        Model3DGroup objectGroup = new Model3DGroup();
                        objectGroup = visualObject2.Content as Model3DGroup;

                        if (objectGroup != null)
                        {
                            RotateTransform3D cubeTransform = new RotateTransform3D();
                            objectGroup.Transform = cubeTransform;

                            QuaternionRotation3D cubeQuaternion = new QuaternionRotation3D();

                            cubeTransform.Rotation = cubeQuaternion;

                            cubeQuaternion.Quaternion = new Quaternion(sliderRotateX.Value, sliderRotateY.Value, sliderRotateZ.Value, sliderRotateW.Value);
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private void RotateObject()
        {
            try
            {
                ModelVisual3D visualObject = new ModelVisual3D();
                ModelVisual3D visualObject2 = new ModelVisual3D();
                if (mainViewport.Children.Count > 3)
                {
                    visualObject = mainViewport.Children[1] as ModelVisual3D;

                    if (visualObject != null)
                    {
                        Model3DGroup objectGroup = new Model3DGroup();
                        objectGroup = visualObject.Content as Model3DGroup;

                        if (objectGroup != null)
                        {
                            RotateTransform3D cubeTransform = new RotateTransform3D();
                            objectGroup.Transform = cubeTransform;
                            QuaternionRotation3D currentQuat3D = new QuaternionRotation3D();
                            currentQuat3D = (QuaternionRotation3D)cubeTransform.Rotation;

                            QuaternionRotation3D newQuad3D = new QuaternionRotation3D();
                            newQuad3D.Quaternion = new Quaternion(q_1, q_2, q_3, q_4);
                            
                            //newQuad3D.Quaternion = new Quaternion(q_1 + currentQuat3D.Quaternion.X,
                            //    q_2 + currentQuat3D.Quaternion.Y,
                            //    q_3 + currentQuat3D.Quaternion.Z,
                            //        q_4 + currentQuat3D.Quaternion.W);

                            cubeTransform.Rotation = newQuad3D;

                            //cubeQuaternion.Quaternion = new Quaternion(sliderRotateX.Value, sliderRotateY.Value, sliderRotateZ.Value, sliderRotateW.Value);
                        }
                    }

                    //visualObject2 = mainViewport.Children[2] as ModelVisual3D;
                    //if (visualObject2 != null)
                    //{
                    //    Model3DGroup objectGroup = new Model3DGroup();
                    //    objectGroup = visualObject2.Content as Model3DGroup;

                    //    if (objectGroup != null)
                    //    {
                    //        RotateTransform3D cubeTransform = new RotateTransform3D();
                    //        objectGroup.Transform = cubeTransform;

                    //        QuaternionRotation3D cubeQuaternion = new QuaternionRotation3D();

                    //        cubeTransform.Rotation = cubeQuaternion;

                    //        cubeQuaternion.Quaternion = new Quaternion(q_1, q_2, q_3, q_4);
                    //    }
                    //}

                    //ModelVisual3D visualObject3 = mainViewport.Children[3] as ModelVisual3D;
                    //if (visualObject3 != null)
                    //{
                    //    Model3DGroup objectGroup = new Model3DGroup();
                    //    objectGroup = visualObject3.Content as Model3DGroup;

                    //    if (objectGroup != null)
                    //    {
                    //        RotateTransform3D cubeTransform = new RotateTransform3D();
                    //        objectGroup.Transform = cubeTransform;

                    //        QuaternionRotation3D cubeQuaternion = new QuaternionRotation3D();

                    //        cubeTransform.Rotation = cubeQuaternion;

                    //        cubeQuaternion.Quaternion = new Quaternion(q_1, q_2, q_3, -q_4);
                    //    }
                    //}

                    //ModelVisual3D visualObject4 = mainViewport.Children[4] as ModelVisual3D;
                    //if (visualObject4 != null)
                    //{
                    //    Model3DGroup objectGroup = new Model3DGroup();
                    //    objectGroup = visualObject4.Content as Model3DGroup;

                    //    if (objectGroup != null)
                    //    {
                    //        RotateTransform3D cubeTransform = new RotateTransform3D();
                    //        objectGroup.Transform = cubeTransform;

                    //        QuaternionRotation3D cubeQuaternion = new QuaternionRotation3D();

                    //        cubeTransform.Rotation = cubeQuaternion;

                    //        cubeQuaternion.Quaternion = new Quaternion(q_1, q_2, q_3, -q_4);
                    //    }
                    //}
                }
            }
            catch (Exception e)
            {
            }
        }

        private void InitializeSliderValues()
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

        private Model3DGroup CreateTriangleModel(Point3D p0, Point3D p1, Point3D p2)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            mesh.Positions.Add(p0);
            mesh.Positions.Add(p1);
            mesh.Positions.Add(p2);
            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(2);
            Vector3D normal = CalculateNormal(p0, p1, p2);
            mesh.Normals.Add(normal);
            mesh.Normals.Add(normal);
            mesh.Normals.Add(normal);
            Material material = new DiffuseMaterial(
                new SolidColorBrush(Colors.DarkKhaki));
            GeometryModel3D model = new GeometryModel3D(
                mesh, material);
            Model3DGroup group = new Model3DGroup();
            group.Children.Add(model);
            return group;
        }
        private Vector3D CalculateNormal(Point3D p0, Point3D p1, Point3D p2)
        {
            Vector3D v0 = new Vector3D(
                p1.X - p0.X, p1.Y - p0.Y, p1.Z - p0.Z);
            Vector3D v1 = new Vector3D(
                p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
            return Vector3D.CrossProduct(v0, v1);
        }

        private void ClearViewport()
        {
            ModelVisual3D m;
            for (int i = mainViewport.Children.Count - 1; i >= 0; i--)
            {
                m = (ModelVisual3D)mainViewport.Children[i];
                if (m.Content is DirectionalLight == false)
                    mainViewport.Children.Remove(m);
            }
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

        private void simpleButtonClick(object sender, RoutedEventArgs e)
        {
            ClearViewport();

            MeshGeometry3D triangleMesh = new MeshGeometry3D();
            Point3D point0 = new Point3D(0, 0, 0);
            Point3D point1 = new Point3D(5, 0, 0);
            Point3D point2 = new Point3D(0, 0, 5);
            triangleMesh.Positions.Add(point0);
            triangleMesh.Positions.Add(point1);
            triangleMesh.Positions.Add(point2);
            triangleMesh.TriangleIndices.Add(0);
            triangleMesh.TriangleIndices.Add(2);
            triangleMesh.TriangleIndices.Add(1);
            Vector3D normal = new Vector3D(0, 1, 0);
            triangleMesh.Normals.Add(normal);
            triangleMesh.Normals.Add(normal);
            triangleMesh.Normals.Add(normal);
            Material material = new DiffuseMaterial(
                new SolidColorBrush(Colors.DarkKhaki));
            GeometryModel3D triangleModel = new GeometryModel3D(
                triangleMesh, material);
            ModelVisual3D model = new ModelVisual3D();
            model.Content = triangleModel;
            this.mainViewport.Children.Add(model);
        }

        private void cubeButtonClick(object sender, RoutedEventArgs e)
        {
            ClearViewport();

            Model3DGroup cube = new Model3DGroup();
            Point3D p0 = new Point3D(0, 0, 0);
            Point3D p1 = new Point3D(5, 0, 0);
            Point3D p2 = new Point3D(5, 0, 20);
            Point3D p3 = new Point3D(0, 0, 20);
            Point3D p4 = new Point3D(0, 5, 0);
            Point3D p5 = new Point3D(5, 5, 0);
            Point3D p6 = new Point3D(5, 5, 20);
            Point3D p7 = new Point3D(0, 5, 20);
            //front side triangles
            cube.Children.Add(CreateTriangleModel(p3, p2, p6));
            cube.Children.Add(CreateTriangleModel(p3, p6, p7));
            //right side triangles
            cube.Children.Add(CreateTriangleModel(p2, p1, p5));
            cube.Children.Add(CreateTriangleModel(p2, p5, p6));
            //back side triangles
            cube.Children.Add(CreateTriangleModel(p1, p0, p4));
            cube.Children.Add(CreateTriangleModel(p1, p4, p5));
            //left side triangles
            cube.Children.Add(CreateTriangleModel(p0, p3, p7));
            cube.Children.Add(CreateTriangleModel(p0, p7, p4));
            //top side triangles
            cube.Children.Add(CreateTriangleModel(p7, p6, p5));
            cube.Children.Add(CreateTriangleModel(p7, p5, p4));
            //bottom side triangles
            cube.Children.Add(CreateTriangleModel(p2, p3, p0));
            cube.Children.Add(CreateTriangleModel(p2, p0, p1));

            ModelVisual3D model = new ModelVisual3D();
            model.Content = cube;
            this.mainViewport.Children.Add(model);
        }

        private void sliderTestQuat_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                ModelVisual3D visualObject = new ModelVisual3D();
                visualObject = mainViewport.Children[1] as ModelVisual3D;

                if (visualObject != null)
                {
                    Model3DGroup objectGroup = new Model3DGroup();
                    objectGroup = visualObject.Content as Model3DGroup;

                    if (objectGroup != null)
                    {
                        RotateTransform3D cubeTransform = new RotateTransform3D();
                        objectGroup.Transform = cubeTransform;

                        QuaternionRotation3D cubeQuaternion = new QuaternionRotation3D();

                        cubeTransform.Rotation = cubeQuaternion;

                        cubeQuaternion.Quaternion = new Quaternion(q_1, q_2, q_3, q_4);
                    }
                }
            }
            catch
            {
            }
        }

        

        public event EventHandler quadUpdatedEvent;

        protected virtual void OnQuadUpdatedEvent(EventArgs e)
        {
            if (quadUpdatedEvent != null)
            {
                quadUpdatedEvent(this, e);
            }
        }

        private void cubeButton2_Click(object sender, RoutedEventArgs e)
        {
            Model3DGroup cube = new Model3DGroup();
            Point3D p0 = new Point3D(0, 10, 0);
            Point3D p1 = new Point3D(5, 10, 0);
            Point3D p2 = new Point3D(5, 10, 5);
            Point3D p3 = new Point3D(0, 10, 5);
            Point3D p4 = new Point3D(0, 15, 0);
            Point3D p5 = new Point3D(5, 15, 0);
            Point3D p6 = new Point3D(5, 15, 5);
            Point3D p7 = new Point3D(0, 15, 5);
            //front side triangles
            cube.Children.Add(CreateTriangleModel(p3, p2, p6));
            cube.Children.Add(CreateTriangleModel(p3, p6, p7));
            //right side triangles
            cube.Children.Add(CreateTriangleModel(p2, p1, p5));
            cube.Children.Add(CreateTriangleModel(p2, p5, p6));
            //back side triangles
            cube.Children.Add(CreateTriangleModel(p1, p0, p4));
            cube.Children.Add(CreateTriangleModel(p1, p4, p5));
            //left side triangles
            cube.Children.Add(CreateTriangleModel(p0, p3, p7));
            cube.Children.Add(CreateTriangleModel(p0, p7, p4));
            //top side triangles
            cube.Children.Add(CreateTriangleModel(p7, p6, p5));
            cube.Children.Add(CreateTriangleModel(p7, p5, p4));
            //bottom side triangles
            cube.Children.Add(CreateTriangleModel(p2, p3, p0));
            cube.Children.Add(CreateTriangleModel(p2, p0, p1));

            ModelVisual3D model = new ModelVisual3D();
            model.Content = cube;
            this.mainViewport.Children.Add(model);
        }

        private void cubeButton3_Click(object sender, RoutedEventArgs e)
        {
            Model3DGroup cube = new Model3DGroup();
            Point3D p0 = new Point3D(0, 15, 0);
            Point3D p1 = new Point3D(5, 15, 0);
            Point3D p2 = new Point3D(5, 15, 5);
            Point3D p3 = new Point3D(0, 15, 5);
            Point3D p4 = new Point3D(0, 20, 0);
            Point3D p5 = new Point3D(5, 20, 0);
            Point3D p6 = new Point3D(5, 20, 5);
            Point3D p7 = new Point3D(0, 20, 5);
            //front side triangles
            cube.Children.Add(CreateTriangleModel(p3, p2, p6));
            cube.Children.Add(CreateTriangleModel(p3, p6, p7));
            //right side triangles
            cube.Children.Add(CreateTriangleModel(p2, p1, p5));
            cube.Children.Add(CreateTriangleModel(p2, p5, p6));
            //back side triangles
            cube.Children.Add(CreateTriangleModel(p1, p0, p4));
            cube.Children.Add(CreateTriangleModel(p1, p4, p5));
            //left side triangles
            cube.Children.Add(CreateTriangleModel(p0, p3, p7));
            cube.Children.Add(CreateTriangleModel(p0, p7, p4));
            //top side triangles
            cube.Children.Add(CreateTriangleModel(p7, p6, p5));
            cube.Children.Add(CreateTriangleModel(p7, p5, p4));
            //bottom side triangles
            cube.Children.Add(CreateTriangleModel(p2, p3, p0));
            cube.Children.Add(CreateTriangleModel(p2, p0, p1));

            ModelVisual3D model = new ModelVisual3D();
            model.Content = cube;
            this.mainViewport.Children.Add(model);
        }

        private void cubeButton4_Click(object sender, RoutedEventArgs e)
        {
            Model3DGroup cube = new Model3DGroup();
            Point3D p0 = new Point3D(0, 25, 0);
            Point3D p1 = new Point3D(5, 25, 0);
            Point3D p2 = new Point3D(5, 25, 5);
            Point3D p3 = new Point3D(0, 25, 5);
            Point3D p4 = new Point3D(0, 30, 0);
            Point3D p5 = new Point3D(5, 30, 0);
            Point3D p6 = new Point3D(5, 30, 5);
            Point3D p7 = new Point3D(0, 30, 5);
            //front side triangles
            cube.Children.Add(CreateTriangleModel(p3, p2, p6));
            cube.Children.Add(CreateTriangleModel(p3, p6, p7));
            //right side triangles
            cube.Children.Add(CreateTriangleModel(p2, p1, p5));
            cube.Children.Add(CreateTriangleModel(p2, p5, p6));
            //back side triangles
            cube.Children.Add(CreateTriangleModel(p1, p0, p4));
            cube.Children.Add(CreateTriangleModel(p1, p4, p5));
            //left side triangles
            cube.Children.Add(CreateTriangleModel(p0, p3, p7));
            cube.Children.Add(CreateTriangleModel(p0, p7, p4));
            //top side triangles
            cube.Children.Add(CreateTriangleModel(p7, p6, p5));
            cube.Children.Add(CreateTriangleModel(p7, p5, p4));
            //bottom side triangles
            cube.Children.Add(CreateTriangleModel(p2, p3, p0));
            cube.Children.Add(CreateTriangleModel(p2, p0, p1));

            ModelVisual3D model = new ModelVisual3D();
            model.Content = cube;
            this.mainViewport.Children.Add(model);
        }

        private void sphereButton_Click(object sender, RoutedEventArgs e)
        {
            mainViewport.Children.Add(CreateSphere(new Point3D(0,0,0),5,20,20,Colors.Blue));
            //Point3D centroid = new Point3D(0, 0, 0);
            //double radius = 10;

            //List<List<Point3D>> pointsArray = new List<List<Point3D>>();
            ////want to wrapp this centroid with many points
            //for (int i = 0; i < 20; i++)
            //{
            //    List<Point3D> pointsList = new List<Point3D>();
            //    for (int j = 0; j < 20; j++)
            //    {
                    
            //    }
            //}
        }

        public ModelVisual3D CreateSphere(Point3D center, double radius, int u, int v, Color color)
        {
            Model3DGroup spear = new Model3DGroup();

            if (u < 2 || v < 2)
                return null;
            Point3D[,] pts = new Point3D[u, v];
            for (int i = 0; i < u; i++)
            {
                for (int j = 0; j < v; j++)
                {
                    pts[i, j] = GetPosition(radius,
                    i * 180 / (u - 1), j * 360 / (v - 1));
                    pts[i, j] += (Vector3D)center;
                }
            }

            Point3D[] p = new Point3D[4];
            for (int i = 0; i < u - 1; i++)
            {
                for (int j = 0; j < v - 1; j++)
                {
                    p[0] = pts[i, j];
                    p[1] = pts[i + 1, j];
                    p[2] = pts[i + 1, j + 1];
                    p[3] = pts[i, j + 1];
                    spear.Children.Add(CreateTriangleFace(p[0], p[1], p[2], color));
                    spear.Children.Add(CreateTriangleFace(p[2], p[3], p[0], color));
                }
            }
            ModelVisual3D model = new ModelVisual3D();
            model.Content = spear;
            return model;
        }

        public Model3DGroup CreateTriangleFace(Point3D p0, Point3D p1, Point3D p2, Color color)
        {
            MeshGeometry3D mesh = new MeshGeometry3D(); mesh.Positions.Add(p0); mesh.Positions.Add(p1); mesh.Positions.Add(p2); mesh.TriangleIndices.Add(0); mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(2);

            Vector3D normal = VectorHelper.CalcNormal(p0, p1, p2);
            mesh.Normals.Add(normal);
            mesh.Normals.Add(normal);
            mesh.Normals.Add(normal);

            Material material = new DiffuseMaterial(
                new SolidColorBrush(color));
            GeometryModel3D model = new GeometryModel3D(
                mesh, material);
            Model3DGroup group = new Model3DGroup();
            group.Children.Add(model);
            return group;
        }

        private Point3D GetPosition(double radius, double theta, double phi)
        {
            Point3D pt = new Point3D();
            double snt = Math.Sin(theta * Math.PI / 180);
            double cnt = Math.Cos(theta * Math.PI / 180);
            double snp = Math.Sin(phi * Math.PI / 180);
            double cnp = Math.Cos(phi * Math.PI / 180);
            pt.X = radius * snt * cnp;
            pt.Y = radius * cnt;
            pt.Z = -radius * snt * snp;
            return pt;
        }

        private class VectorHelper
        {
            public static Vector3D CalcNormal(Point3D p0, Point3D p1, Point3D p2)
            {
                Vector3D v0 = new Vector3D(p1.X - p0.X, p1.Y - p0.Y, p1.Z - p0.Z);
                Vector3D v1 = new Vector3D(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
                return Vector3D.CrossProduct(v0, v1);
            }
        }

        private void Download_Button_Click(object sender, RoutedEventArgs e)
        {
            System.IO.File.WriteAllLines(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/output_Jan_06.txt",toBeWrittenFiles.ToArray());
        }

    }

    
}
