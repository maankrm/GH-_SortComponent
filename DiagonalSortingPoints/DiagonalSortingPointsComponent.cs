﻿using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using System.Runtime.InteropServices;

using Rhino.DocObjects;
using Rhino.Collections;
using GH_IO;
using GH_IO.Serialization;


//██████╗ ██╗ █████╗  ██████╗  ██████╗ ███╗   ██╗ █████╗ ██╗     
//██╔══██╗██║██╔══██╗██╔════╝ ██╔═══██╗████╗  ██║██╔══██╗██║     
//██║  ██║██║███████║██║  ███╗██║   ██║██╔██╗ ██║███████║██║     
//██║  ██║██║██╔══██║██║   ██║██║   ██║██║╚██╗██║██╔══██║██║     
//██████╔╝██║██║  ██║╚██████╔╝╚██████╔╝██║ ╚████║██║  ██║███████╗
//╚═════╝ ╚═╝╚═╝  ╚═╝ ╚═════╝  ╚═════╝ ╚═╝  ╚═══╝╚═╝  ╚═╝╚══════╝

//███████╗ ██████╗ ██████╗ ████████╗██╗███╗   ██╗ ██████╗        
//██╔════╝██╔═══██╗██╔══██╗╚══██╔══╝██║████╗  ██║██╔════╝        
//███████╗██║   ██║██████╔╝   ██║   ██║██╔██╗ ██║██║  ███╗       
//╚════██║██║   ██║██╔══██╗   ██║   ██║██║╚██╗██║██║   ██║       
//███████║╚██████╔╝██║  ██║   ██║   ██║██║ ╚████║╚██████╔╝       
//╚══════╝ ╚═════╝ ╚═╝  ╚═╝   ╚═╝   ╚═╝╚═╝  ╚═══╝ ╚═════╝        

//██████╗  ██████╗ ██╗███╗   ██╗████████╗███████╗                
//██╔══██╗██╔═══██╗██║████╗  ██║╚══██╔══╝██╔════╝                
//██████╔╝██║   ██║██║██╔██╗ ██║   ██║   ███████╗                
//██╔═══╝ ██║   ██║██║██║╚██╗██║   ██║   ╚════██║                
//██║     ╚██████╔╝██║██║ ╚████║   ██║   ███████║  Parastorm lab 
//╚═╝      ╚═════╝ ╚═╝╚═╝  ╚═══╝   ╚═╝   ╚══════╝                



namespace DiagonalSortingPoints
{
    public class DiagonalSortingPointsComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public DiagonalSortingPointsComponent()
          : base("DiagonalSortingPoints", "Nickname",
            "Description",
            "Sets", "List")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("ListOfPoints", "L", "List of Point3d To Sort Them Diagonally Like O'clock Wise",
                GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "P", "Sorted Points", GH_ParamAccess.list);
            //pManager.AddVectorParameter("Vectors", "V", "vectors between points and avg", GH_ParamAccess.list);
            pManager.AddNumberParameter("Keys", "K", "Sorted Keys", GH_ParamAccess.list);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Point3d> pointsList = new List<Point3d>();
            if (pointsList == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "There Are No Data Yet ! Please Input a List Of Points");
                return;
            }
            List<Point3d> pAll = new List<Point3d>();

            double xaxe = 0;
            double yaxe = 0;
            double zaxe = 0;
            List<Point3d> ListP = new List<Point3d>();


            if (!DA.GetDataList(0, ListP)) return;


            //Algorithm
            // Get An Average Point Between A List Of Points (Start)------->
            for (int i = 0; i < ListP.Count; i++)
            {

                Point3d point0 = ListP[i];
                pAll.Add(point0);

                xaxe += point0.X / ListP.Count;
                yaxe += point0.Y / ListP.Count;
                zaxe += point0.Z / ListP.Count;



            }

            Point3d avg = new Point3d(xaxe, yaxe, zaxe);


            // Get An Average Point Between A List Of Points (End)--------->
            // Create A List Of 2PVectors  (Start)------------------------->

            List<Vector3d> Vectors = new List<Vector3d>();

            for (int i = 0; i < ListP.Count; i++)
            {
                Vector3d vecs = ListP[i] - avg;
                Vectors.Add(vecs);
            }
            // Create A List Of 2PVectors  (End)--------------------------->
            // Deconstruct Plane (Start)----------------------------------->

            Vector3d px = new Vector3d(1, 0, 0);
            Vector3d py = new Vector3d(0, 1, 0);
            // Deconstruct Plane (End)------------------------------------->
            // Compute a Dot Product Values of 2Vectors (Start)------------>

            List<double> dot1 = new List<double>();
            List<double> dot2 = new List<double>();
            List<double> angles = new List<double>();



            for (int i = 0; i < Vectors.Count; i++)
            {
                double D1 = Vector3d.Multiply(Vectors[i], px);
                double D2 = Vector3d.Multiply(Vectors[i], py);
                // Compute a Dot Product Values of 2Vectors (End)------------------->

                // Compute Angles (keys) (Start)------------------------------------>

                double a = Math.Atan2(D1, D2);
                double angle = a * (180 / Math.PI);
                // Compute Angles (keys) (End)-------------------------------------->

                dot1.Add(D1);
                dot2.Add(D2);
                angles.Add(angle);

            }

            // Compute a Dot Product Values of 2Vectors (End)------------------>

            // Sorting DataKeys (Start)---------------------------------------->

            double[] k = angles.ToArray();
            angles.Sort();

            // Sorting DataKeys (End)------------------------------------------>

            // Sorting Data (Start)-------------------------------------------->
            RhinoList<double> sortPointsX = new RhinoList<double>();
            RhinoList<double> sortPointsY = new RhinoList<double>();
            RhinoList<double> sortPointsZ = new RhinoList<double>();

            double[] X0 = new double[pAll.Count];
            double[] Y0 = new double[pAll.Count];
            double[] Z0 = new double[pAll.Count];


            for (int i = 0; i < pAll.Count; i++)
            {

                Point3d pxyz = pAll[i];
                X0[i] = pxyz.X;
                sortPointsX.Add(pxyz.X);
                Y0[i] = pxyz.Y;
                sortPointsY.Add(pxyz.Y);
                Z0[i] = pxyz.Z;
                sortPointsZ.Add(pxyz.Z);

            }

            sortPointsX.Sort(k);
            sortPointsY.Sort(k);
            sortPointsZ.Sort(k);




            for (int i = 0; i < pAll.Count; i++)
            {
                Point3d PL = new Point3d(sortPointsX[i], sortPointsY[i], sortPointsZ[i]);

                pointsList.Add(PL);

            }

            // Sorting Data (End)---------------------------------------------->



            //output
            DA.SetDataList(0, pointsList);
            DA.SetDataList(1, k);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// You can add image files to your project resources and access them like this:
        /// return Resources.IconForThisComponent;
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Properties.Resources.DiagonalSortingPoint_icon;

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("639AC073-00C3-434E-8440-0E73C571E392");
    }
}