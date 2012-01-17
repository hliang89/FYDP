using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Media3D;


namespace FYDP_wpfUI_v2
{
    public class Triangle
    {
        public Point3D P0 { get; set; }
        public Point3D P1 { get; set; }
        public Point3D P2 { get; set; }

        public Triangle Clone(double z, bool switchP1andP2)
        {
            var newTriangle = new Triangle();
            newTriangle.P0 = GetPointAdjustedBy(this.P0, new Point3D(0, 0, z));

            var point1 = GetPointAdjustedBy(this.P1, new Point3D(0, 0, z));
            var point2 = GetPointAdjustedBy(this.P2, new Point3D(0, 0, z));

            if (!switchP1andP2)
            {
                newTriangle.P1 = point1;
                newTriangle.P2 = point2;
            }
            else
            {
                newTriangle.P1 = point2;
                newTriangle.P2 = point1;
            }
            return newTriangle;
        }

        private Point3D GetPointAdjustedBy(Point3D point, Point3D adjustBy)
        {
            var newPoint = new Point3D { X = point.X, Y = point.Y, Z = point.Z };
            newPoint.Offset(adjustBy.X, adjustBy.Y, adjustBy.Z);
            return newPoint;
        }
    }

    public class CircleAssitor
    {
        public CircleAssitor()
        {
            CurrentTriangle = new Triangle();
        }

        public Point3D FirstPoint { get; set; }
        public Point3D LastPoint { get; set; }
        public Triangle CurrentTriangle { get; set; }
    }


    public class FYDP_3Dobject_Util
    {
        public static MaterialGroup GetSurfaceMaterial(Color colour)
        {
            var materialGroup = new MaterialGroup();
            var emmMat = new EmissiveMaterial(new SolidColorBrush(colour));
            materialGroup.Children.Add(emmMat);
            materialGroup.Children.Add(new DiffuseMaterial(new SolidColorBrush(colour)));
            var specMat = new SpecularMaterial(new SolidColorBrush(Colors.White), 30);
            materialGroup.Children.Add(specMat);
            return materialGroup;
        }

        public static Model3DGroup GetCylinder(MaterialGroup materialGroup, Point3D midPoint, double radius, double depth)
        {
            var cylinder = new Model3DGroup();
            var nearCircle = new CircleAssitor();
            var farCircle = new CircleAssitor();

            var twoPi = Math.PI * 2;
            var firstPass = true;

            double x;
            double y;

            var increment = 0.1d;
            for (double i = 0; i < twoPi + increment; i = i + increment)
            {
                x = (radius * Math.Cos(i));
                y = (-radius * Math.Sin(i));

                farCircle.CurrentTriangle.P0 = midPoint;
                farCircle.CurrentTriangle.P1 = farCircle.LastPoint;
                farCircle.CurrentTriangle.P2 = new Point3D(x + midPoint.X, y + midPoint.Y, midPoint.Z);

                nearCircle.CurrentTriangle = farCircle.CurrentTriangle.Clone(depth, true);

                if (!firstPass)
                {
                    cylinder.Children.Add(FYDP_3Dobject_Util.CreateTriangleModel(materialGroup, farCircle.CurrentTriangle));
                    cylinder.Children.Add(FYDP_3Dobject_Util.CreateTriangleModel(materialGroup, nearCircle.CurrentTriangle));

                    cylinder.Children.Add(FYDP_3Dobject_Util.CreateTriangleModel(materialGroup, farCircle.CurrentTriangle.P2, farCircle.CurrentTriangle.P1, nearCircle.CurrentTriangle.P2));
                    cylinder.Children.Add(FYDP_3Dobject_Util.CreateTriangleModel(materialGroup, nearCircle.CurrentTriangle.P2, nearCircle.CurrentTriangle.P1, farCircle.CurrentTriangle.P2));
                }
                else
                {
                    farCircle.FirstPoint = farCircle.CurrentTriangle.P1;
                    nearCircle.FirstPoint = nearCircle.CurrentTriangle.P1;
                    firstPass = false;
                }
                farCircle.LastPoint = farCircle.CurrentTriangle.P2;
                nearCircle.LastPoint = nearCircle.CurrentTriangle.P2;
            }
            return cylinder;
        }

        public static Model3DGroup MakeSphere(Point3D centroid, double radius, int x_points, int y_points, Color colour)
        {
            Model3DGroup sphere = new Model3DGroup();

            Point3D[,] pts = new Point3D[x_points, y_points];
            for (int i = 0; i < x_points; i++)
            {
                for (int j = 0; j < y_points; j++)
                {
                    pts[i, j] = GetPosition(radius,
                    i * 180 / (x_points - 1), j * 360 / (y_points - 1));
                    pts[i, j] += (Vector3D)centroid;
                }
            }

            Point3D[] p = new Point3D[4];
            for (int i = 0; i < x_points - 1; i++)
            {
                for (int j = 0; j < y_points - 1; j++)
                {
                    p[0] = pts[i, j];
                    p[1] = pts[i + 1, j];
                    p[2] = pts[i + 1, j + 1];
                    p[3] = pts[i, j + 1];
                    sphere.Children.Add(CreateTriangleFace(p[0], p[1], p[2], colour));
                    sphere.Children.Add(CreateTriangleFace(p[2], p[3], p[0], colour));
                }
            }
           
            return sphere;
        }

        private static Point3D GetPosition(double radius, double theta, double phi)
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

        public static Model3DGroup CreateTriangleFace(Point3D p0, Point3D p1, Point3D p2, Color color)
        {
            MeshGeometry3D mesh = new MeshGeometry3D(); mesh.Positions.Add(p0); mesh.Positions.Add(p1); mesh.Positions.Add(p2); mesh.TriangleIndices.Add(0); mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(2);

            Vector3D normal = CalcNormal(p0, p1, p2);
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

        public static Vector3D CalcNormal(Point3D p0, Point3D p1, Point3D p2)
        {
            Vector3D v0 = new Vector3D(p1.X - p0.X, p1.Y - p0.Y, p1.Z - p0.Z);
            Vector3D v1 = new Vector3D(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
            return Vector3D.CrossProduct(v0, v1);
        }

        private static Model3DGroup CreateTriangleModel(MaterialGroup materialGroup, Triangle triangle)
        {
            return CreateTriangleModel(materialGroup, triangle.P0, triangle.P1, triangle.P2);
        }

        private static Model3DGroup CreateTriangleModel(Material material, Point3D p0, Point3D p1, Point3D p2)
        {
            var mesh = new MeshGeometry3D();
            mesh.Positions.Add(p0);
            mesh.Positions.Add(p1);
            mesh.Positions.Add(p2);
            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(2);
            var normal = CalcNormal(p0, p1, p2);
            mesh.Normals.Add(normal);
            mesh.Normals.Add(normal);
            mesh.Normals.Add(normal);

            var model = new GeometryModel3D(mesh, material);

            var group = new Model3DGroup();
            group.Children.Add(model);
            return group;
        }
     }

}
