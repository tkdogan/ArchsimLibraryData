﻿using System;
using System.Collections.Generic;
using System.Linq;
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
using Excel;
using System.Diagnostics;
using System.IO;
using ArchsimLib;
using Microsoft.Win32;
using NPOI.XSSF.UserModel;

namespace Excel2JSON
{
   /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenBth_Click(object sender, RoutedEventArgs e)
        {
            string file =
                @"C:\Users\Timur\Dropbox (Personal)\_CSHARP_PROJECTS\ArchsimLibraryData\160603_ExcelLibraryEditor.xlsx";



            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "XLSX files (*.xlsx)|*.xlsx|All files (*.*)|*.*";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (openFileDialog.ShowDialog() == true)
            {
                //foreach (string filename in openFileDialog.FileNames)
                //    lbFiles.Items.Add(Path.GetFileName(filename));
                file = openFileDialog.FileName;
            }




            var lib = Excel2Lib(file);

            string JsonPath = System.IO.Path.GetDirectoryName(file);
            string JsonName = System.IO.Path.GetFileNameWithoutExtension(file) + ".json" ;
            string JsonFile = System.IO.Path.Combine(JsonPath, JsonName);


            Logger.WriteLine("Finished... writing JSON library to "+ JsonFile);

            loggerBox.Text = Logger.log.ToString();

           File.WriteAllText(JsonFile, lib.toJSON());

        }

        private static Library Excel2Lib(string file)
        {
            XSSFWorkbook wb;


            // get sheets list from xlsx
            using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                wb = new XSSFWorkbook(fs);
            }

            //Logger.WriteLine("Worksheets in file:");
            for (int k = 0; k < wb.Count; k++)
            {
             //   Debug.WriteLine("Worksheet " + wb.GetSheetAt(k).SheetName);
              //  Logger.WriteLine("Worksheet " + wb.GetSheetAt(k).SheetName);
            }

            Library lib = LibraryDefaults.getHardCodedDefaultLib();

            foreach(var o in Parse.Objects<OpaqueMaterial>((XSSFSheet)wb.GetSheet("Material"))) lib.OpaqueMaterials.Add(o);
        
            //if (lib.OpaqueMaterials.GroupBy(x => x.Name).Any(g => g.Count() > 1))
            //{
            //    var hash = new HashSet<string>();
            //    var duplicates = lib.OpaqueMaterials.Where(x => !hash.Add(x.Name));
            //    foreach (var d in duplicates) {
            //        Debug.WriteLine("WARNING: Duplicate name " +d.Name);
            //        Logger.WriteLine("WARNING: Duplicate name " + d.Name);
            //    }
            //     hash = new HashSet<string>();
            //    lib.OpaqueMaterials = lib.OpaqueMaterials.Where(x => hash.Add(x.Name)).ToList();
            //}

            foreach (var o in Parse.Objects<GlazingConstructionSimple>((XSSFSheet)wb.GetSheet("GlazingConstructionSimple"))) lib.GlazingConstructionsSimple.Add(o);
            foreach (var o in Parse.Objects<ZoneLoad>((XSSFSheet)wb.GetSheet("ZoneLoad"))) lib.ZoneLoads.Add(o);

            foreach(var o in   Parse.Objects<ZoneConditioning>((XSSFSheet)wb.GetSheet("ZoneConditioning"))) lib.ZoneConditionings.Add(o);
            foreach(var o in   Parse.Objects<ZoneVentilation>((XSSFSheet)wb.GetSheet("ZoneVentilation"))) lib.ZoneVentilations.Add(o);
            foreach (var o in  Parse.Objects<ZoneConstruction>((XSSFSheet)wb.GetSheet("ZoneConstruction"))) lib.ZoneConstructions.Add(o);

            foreach (var o in Parse.Objects<DomHotWater>((XSSFSheet)wb.GetSheet("DomHotWater"))) lib.DomHotWaters.Add(o);
            foreach (var o in Parse.Objects<WindowSettings>((XSSFSheet)wb.GetSheet("Window"))) lib.WindowSettings.Add(o);

            foreach (var o in  Parse.Schedule((XSSFSheet)wb.GetSheet("Schedule"), ref lib)) lib.YearSchedules.Add(o);

            foreach (var o in Parse.ArraySchedule((XSSFSheet)wb.GetSheet("ArraySchedule"), ref lib)) lib.ArraySchedules.Add(o);

            foreach (var o in Parse.Constructions((XSSFSheet) wb.GetSheet("Construction"), ref lib)) lib.OpaqueConstructions.Add(o);
            foreach (var o in Parse.Zone((XSSFSheet)wb.GetSheet("Zone"), ref lib)) lib.ZoneDefinitions.Add(o);


            return lib;

        }
    }
}
