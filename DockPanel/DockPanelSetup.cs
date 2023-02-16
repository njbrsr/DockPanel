using System;
using System.Collections.Generic;
using Eto.Drawing;
using Eto.Forms;
using Rhino.UI;
using System.IO;

namespace DockPanel.Views
{
    /// <summary>
    /// Required class GUID, used as the panel Id
    /// </summary>
    [System.Runtime.InteropServices.Guid("0E7780CA-F004-4AE7-B918-19E68BF7C7C9")]


    public class SampleCsEtoPanel : Panel, IPanel
    {
        MyConduit conduit = new MyConduit { Enabled = true };
        Rhino.RhinoDoc doc = Rhino.RhinoDoc.ActiveDoc;

        
        

        readonly uint m_document_sn = 0;

        /// <summary>
        /// Provide easy access to the SampleCsEtoPanel.GUID
        /// </summary>
        public static System.Guid PanelId => typeof(SampleCsEtoPanel).GUID;

        string html = "<head><script src='https://cdnjs.cloudflare.com/ajax/libs/Chart.js/2.9.4/Chart.js'></script></head><body><p> Runoff limit <input type = 'range' min = '0' max = '100' value = '50' class='slider' id='runoffLimit'></p><p id='valTracker'>value</p><canvas id='myChart' style='width:100%;max-width:700px'></canvas><script>var xValues = [50,60,70,80,90,100,110,120,130,140,150]; var yValues = [7,8,8,9,9,9,10,11,14,14,15]; new Chart('myChart', {type: 'line', data: {    labels: xValues, datasets: [{       backgroundColor: 'rgba(0,0,0,1.0)',      borderColor: 'rgba(0,0,0,0.1)',      data: yValues    }]},options:{}});</script><script>document.getElementById('runoffLimit').onchange=function(){window.location='greenscenario:runofflimit';document.getElementById('valTracker').innerHTML=document.getElementById('runoffLimit').value};</script></body></html>";
        List<Rhino.DocObjects.ObjRef> crv = new List<Rhino.DocObjects.ObjRef>();
        Rhino.Geometry.Mesh msh;

        public WebView webView { get; private set; }
        
        public bool IndexLoaded = false; 

        /// <summary>
        /// Required public constructor with NO parameters
        /// </summary>
        /// 
        private void CommunicateWithWebView(string path)
        {
            switch (path)
            {
                case "runofflimit":
                    string text = webView.ExecuteScript("return document.getElementById('runoffLimit').value;");
                    Rhino.RhinoApp.WriteLine($"Runoff limit: {text}");
                    break;
                case "select_road":
                    crv = new List<Rhino.DocObjects.ObjRef>();
                    webView.ExecuteScript("document.getElementById('valTracker').innerHTML='test'");
                    Rhino.DocObjects.ObjectType filter_curve = Rhino.DocObjects.ObjectType.Curve;
                    Rhino.DocObjects.ObjRef[] objref_curve = null;
                    Rhino.Commands.Result rc_curve = Rhino.Input.RhinoGet.GetMultipleObjects("Select Road Network", false, filter_curve, out objref_curve);
                    for(int i = 0; i < objref_curve.Length; i++)
                    {
                        Rhino.RhinoApp.WriteLine($"{i}");
                        Rhino.RhinoApp.WriteLine($"{objref_curve.Length}");
                        crv.Add(objref_curve[i]);
                    }
                    conduit.AddCurve(crv);
                    break;

                case "select_terrain":
                    Rhino.DocObjects.ObjectType filter_mesh = Rhino.DocObjects.ObjectType.Mesh;
                    Rhino.DocObjects.ObjRef objref_mesh = null;
                    Rhino.Commands.Result rc_mesh = Rhino.Input.RhinoGet.GetOneObject("Select Terrain Mesh:", false, filter_mesh, out objref_mesh);
                    msh = objref_mesh.Mesh();
                    break;
            }

        }
        public SampleCsEtoPanel(uint documentSerialNumber)
        {
            m_document_sn = documentSerialNumber;
            webView = new WebView { Height = 1000 };
            Title = GetType().Name;

            


            string htmlText = File.ReadAllText("C:/Users/brasin/dev/DockPanel/DockPanel/Resources/test.html");

            webView.LoadHtml(htmlText);
            webView.DocumentLoading += (sender, e) =>
            {
                if (e.Uri.Scheme == "greenscenario") //search for markers in html
                {
                    e.Cancel = true; // prevent navigation
                    CommunicateWithWebView(e.Uri.PathAndQuery);
                }
            };

            var layout = new DynamicLayout { DefaultSpacing = new Size(5, 5), Padding = new Padding(0) };
            layout.BeginHorizontal();
            layout.Add(webView, true, true);
            layout.EndHorizontal();
            Content = layout;
        }




        public string Title { get; }

        /// <summary>
        /// Example of proper way to display a message box
        /// </summary>
        /// <summary>
        /// Sample of how to display a child Eto dialog
        /// </summary>


        #region IPanel methods
        public void PanelShown(uint documentSerialNumber, ShowPanelReason reason)
        {
            // Called when the panel tab is made visible, in Mac Rhino this will happen
            // for a document panel when a new document becomes active, the previous
            // documents panel will get hidden and the new current panel will get shown.
            Rhino.RhinoApp.WriteLine($"Panel shown for document {documentSerialNumber}, this serial number {m_document_sn} should be the same");
        }

        public void PanelHidden(uint documentSerialNumber, ShowPanelReason reason)
        {
            // Called when the panel tab is hidden, in Mac Rhino this will happen
            // for a document panel when a new document becomes active, the previous
            // documents panel will get hidden and the new current panel will get shown.
            Rhino.RhinoApp.WriteLine($"Panel hidden for document {documentSerialNumber}, this serial number {m_document_sn} should be the same");
        }

        public void PanelClosing(uint documentSerialNumber, bool onCloseDocument)
        {
            // Called when the document or panel container is closed/destroyed
            Rhino.RhinoApp.WriteLine($"Panel closing for document {documentSerialNumber}, this serial number {m_document_sn} should be the same");
        }
        #endregion IPanel methods
    }

    public class MyConduit : Rhino.Display.DisplayConduit
    {
        private List<Rhino.DocObjects.ObjRef> curves;
        public MyConduit()
        {
            curves = new List<Rhino.DocObjects.ObjRef>();
        }
        public void AddCurve(List<Rhino.DocObjects.ObjRef> curve)
        {
            curves = curve;
        }

        public List<Rhino.DocObjects.ObjRef> Curves
        {
            get { return curves; }
        }
        public int CurveCount
        {
            get { return curves.Count; }
        }

        protected override void DrawOverlay(Rhino.Display.DrawEventArgs e)
        {
            for (int i = 0; i < curves.Count; i++)
            {
                e.Display.DrawCurve(curves[i].Curve(), System.Drawing.Color.Blue);
            }
        }
    }
}