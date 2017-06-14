using System;
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
using Newtonsoft.Json.Linq;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace ArkMapPlot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private Dictionary<string, ClassData> classData = new Dictionary<string, ClassData>();
        private Dictionary<string, string> displayToClass = new Dictionary<string, string>();

        public MainWindow()
        {
            loadData();
            InitializeComponent();
            populateWindow();
        }

        private void loadData()
        {
            classData.Clear();
            displayToClass.Clear();
            ClassData data;
            JObject obj;
            JArray arr;
            string className = string.Empty;
            string displayName = string.Empty;
            string fileName = string.Empty;
            string fileData = null;
            
            string[] classNameMap = new string[1];
            classNameMap[0] = "test,test";
            int len = classNameMap.Length;
            for (int i = 0; i < len; i++)
            {
                displayName = classNameMap[i].Split(',')[0];
                className = classNameMap[i].Split(',')[1];
                fileName = className + ".json";
                fileData = File.ReadAllText(fileName);
                print("fileData: " + fileData);
                obj = JObject.Parse(fileData);
                print(obj.Count);
                arr = obj["array"] as JArray;
                data = new ClassData(className, displayName);
                data.loadMembers(arr);
                displayToClass.Add(displayName, className);
                classData.Add(className, data);
            }
            /**
            * Below here is manually added test data.
            **/
            displayToClass.Add("test1", "test1name");
            displayToClass.Add("test2", "test2name");

            data = new ClassData("test1name", "test1");
            data.members.Add(new MemberData("test1Name"));
            data.members.Add(new MemberData("test1Name"));
            classData.Add("test1name", data);

            data = new ClassData("test2name", "test2");
            data.members.Add(new MemberData("test2name"));
            data.members.Add(new MemberData("test2name"));
            data.members.Add(new MemberData("test2name"));
            classData.Add("test2name", data);
        }

        private void populateWindow()
        {
            foreach(string name in displayToClass.Keys)
            {
                ClassList.Items.Add(name);
            }
            ClassList.SelectionChanged += delegate (object sender, SelectionChangedEventArgs e) 
            {
                updateMemberList((string)ClassList.SelectedItem);
                //MemberInfo.SelectedItem = null;
                updateMemberData(null);
                print("Selected class: " + ClassList.SelectedItem);
            };
            MemberList.SelectionChanged += delegate (object sender, SelectionChangedEventArgs e)
            {
                string cName = (string)ClassList.SelectedItem;
                string mName = (string)MemberList.SelectedItem;
                ClassData cd = classData[displayToClass[cName]];
                MemberData md = cd.members.Find(m => m.displayName == mName);
                updateMemberData(md);
                print("Selected member: " + MemberList.SelectedItem);
            };

            loadMap(string.Empty);
        }

        private void loadMap(string configFileName)
        {
            //TODO load values from map config setup
            string mapName = string.Empty;
            string mapImageFileName = "ark-map.png";
            MapImage.Source = loadImage(mapImageFileName);
        }

        private void updateMemberList(string displayName)
        {
            print("Selected: " + displayName);
            string className = displayToClass[displayName];
            ClassData data = classData[className];
            print("Num of members: " + data.members.Count);
            MemberList.Items.Clear();
            foreach (MemberData member in data.members)
            {
                MemberList.Items.Add(member.displayName);
            }
        }

        private void updateMemberData(MemberData data)
        {
            if (data == null)
            {
                //MemberInfo.Items.Clear();
                MemberList.SelectedItem = null;
                return;
            }
            //data.updateDisplay(MemberInfo);
            data.updateDisplay(MemberInfoBlock);
        }

        private BitmapImage loadImage(string file)
        {
            string path = System.IO.Path.GetFullPath(file);
            print("File to load: " + file+ " :: "+path);
            BitmapImage bi = null;
            Uri uri = new Uri(path);
            bi = new BitmapImage(uri);
            return bi;
        }

        public static void print(string line)
        {
            Console.Out.WriteLine(line);
        }

        public static void print(object var)
        {
            Console.Out.WriteLine(var == null ? "null" : var.ToString());
        }
    }

    public class ClassData
    {
        public readonly string displayName;
        public readonly string className;
        public readonly List<MemberData> members = new List<MemberData>();

        public ClassData(string className, string displayName)
        {
            this.className = className;
            this.displayName = displayName;            
        }

        public void loadMembers(JArray members)
        {
            int len = members.Count;
            MainWindow.print("arrCnt: " + len);
            JObject obj;
            for (int i = 0; i < len; i++)
            {
                obj = members[i] as JObject;
                if (obj != null)
                {
                    MemberData data = new MemberData(className, obj);
                    this.members.Add(data);
                }
            }
        }
    }

    public class MemberData
    {
        public readonly string className;
        public readonly string displayName;
        public readonly float lat;
        public readonly float lon;
        public readonly float x, y, z;
        public readonly bool isFemale = false;
        public readonly int baseLevels;
        public readonly int health;
        public readonly int stam;
        public readonly int oxy;
        public readonly int food;
        public readonly int damage;
        public readonly int speed;

        public MemberData(string className)
        {
            this.className = className;
            this.displayName = this.GetHashCode().ToString();
        }

        public MemberData(string className, JObject obj)
        {
            this.className = className;
            this.displayName = this.GetHashCode().ToString();
            this.lat = (obj["latitude"] as JValue).Value<float>();
            this.lon = (obj["longitude"] as JValue).Value<float>();
            this.x = (obj["x"] as JValue).Value<float>();
            this.y = (obj["y"] as JValue).Value<float>();
            this.z = (obj["z"] as JValue).Value<float>();
            this.isFemale = (obj["female"] as JValue).Value<bool>();
            this.baseLevels = (obj["baseLevels"] as JValue).Value<int>();
        }

        public void updateDisplay(ListBox list)
        {
            ItemCollection c = list.Items;
            c.Clear();
            c.Add("id : "+displayName);
            c.Add("lat: " + lat);
            c.Add("lon: " + lon);
            c.Add("x  : " + x);
            c.Add("y  : " + y);
            c.Add("z  : " + z);
            c.Add("fem: " + isFemale);
            c.Add("lev: " + baseLevels);
        }

        public void updateDisplay(TextBlock block)
        {
            string data = string.Empty;
            data = "id : " + displayName;
            data += "\nlat: " + lat;
            data += "\nlon: " + lon;
            data += "\nx  : " + x;
            data += "\ny  : " + y;
            data += "\nz  : " + z;
            data += "\nfem: " + isFemale;
            data += "\nlev: " + baseLevels;
            block.Text = data;
        }
    }
}
