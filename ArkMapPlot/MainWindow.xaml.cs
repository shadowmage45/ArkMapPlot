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

        private List<MemberData> displayedMembers = new List<MemberData>();

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
            JArray arr;
            string className = string.Empty;
            string displayName = string.Empty;
            string fileName = string.Empty;
            string fileData = null;
            string[] classNames;
            string[] displayNames;
            string classMapName = "ark_data_en.json";
            string classMapData = File.ReadAllText(classMapName);
            JObject classMapJson = JObject.Parse(classMapData);
            JArray classArray = (JArray)classMapJson["creatures"];
            int arcnt = classArray.Count;
            classNames = new string[arcnt];
            displayNames = new string[arcnt];
            for (int i = 0; i < arcnt; i++)
            {
                classNames[i] = classArray[i]["class"].Value<string>();
                displayNames[i] = classArray[i]["name"].Value<string>();
                displayName = displayNames[i];
                className = classNames[i];
                fileName = "dinoData/"+className + ".json";
                if (File.Exists(fileName))
                {
                    fileData = File.ReadAllText(fileName);
                    //print("fileData: " + fileData);
                    arr = JArray.Parse(fileData);
                    //print(arr.Count);
                    data = new ClassData(className, displayName);
                    data.loadMembers(arr);
                    displayToClass.Add(displayName, className);
                    classData.Add(className, data);
                }
                else
                {
                    //print("Did not locate any file for: " + fileName);
                }
            }
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
                updateMemberData(null);
                //print("Selected class: " + ClassList.SelectedItem);
            };
            MemberData.SelectionChanged += delegate (object sender, SelectionChangedEventArgs e)
            {
                string cName = (string)ClassList.SelectedItem;
                MemberData member = (MemberData)MemberData.SelectedItem;
                updateMemberData(member);
                if (member != null)
                {
                    //print("Selected member: " + member.DisplayName + " of class: " + cName);
                }
                MapImage.ToolTip = member.DisplayName;
            };
            MapImage.MouseMove += delegate (object sender, MouseEventArgs e) 
            {
                System.Windows.Point pt = e.GetPosition(MapImage);
                print("Move-Pixels: " + pt);
            };
            MapImage.MouseLeftButtonDown += delegate (object sender, MouseButtonEventArgs e)
            {
                System.Windows.Point pt = e.GetPosition(MapImage);
                print("Clck-Pixels: " + pt);
            };
            loadMap(string.Empty);
            MapImage.ToolTip = "Foobar";
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
            //print("Selected: " + displayName);
            string className = displayToClass[displayName];
            ClassData data = classData[className];
            //print("Num of members: " + data.members.Count);
            displayedMembers = data.members;
            MemberData.ItemsSource = displayedMembers;
            MemberData.Items.Refresh();
            updateMemberData(null);
        }

        private void updateMemberData(MemberData data)
        {
            MemberInfoBlock.Text = string.Empty;
            if (data != null)
            {
                data.updateDisplay(MemberInfoBlock);
            }
        }

        private void updateMapPins(ClassData data)
        {

        }

        private BitmapImage loadImage(string file)
        {
            string path = System.IO.Path.GetFullPath(file);
            //print("File to load: " + file+ " :: "+path);
            BitmapImage bi = null;
            Uri uri = new Uri(path);
            bi = new BitmapImage(uri);
            return bi;
        }

        private void updateMapTooltip(int pX, int pY)
        {

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
            //MainWindow.print("arrCnt: " + len+" for name: "+className);
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
        private readonly string className;
        private readonly string displayName;
        private readonly float lat;
        private readonly float lon;
        private readonly float x, y, z;
        private readonly bool isFemale = false;
        private readonly int baseLevels;
        private readonly int health;
        private readonly int stam;
        private readonly int oxy;
        private readonly int food;
        private readonly int weight;
        private readonly int damage;
        private readonly int speed;
        
        public string DisplayName { get { return displayName; } }
        public int Level { get { return baseLevels; } }
        public bool Female { get { return isFemale; } }
        public int Health { get { return health; } }
        public int Stamina { get { return stam; } }
        public int Oxygen { get { return oxy; } }
        public int Food { get { return food; } }
        public int Damage { get { return damage; } }
        public int Weight { get { return weight; } }
        public int Speed { get { return speed; } }

        public MemberData(string className)
        {
            this.className = className;
            this.displayName = this.GetHashCode().ToString();
        }

        public MemberData(string className, JObject obj)
        {
            this.className = className;
            this.displayName = (obj["id"] as JValue).Value<string>();
            this.lat = (obj["lat"] as JValue).Value<float>();
            this.lon = (obj["lon"] as JValue).Value<float>();
            this.x = (obj["x"] as JValue).Value<float>();
            this.y = (obj["y"] as JValue).Value<float>();
            this.z = (obj["z"] as JValue).Value<float>();

            JToken tok = obj["female"];
            if (tok != null)
            {
                isFemale = tok.Value<bool>();
            }

            tok = obj["baseLevel"];
            if (tok != null)
            {
                baseLevels = tok.Value<int>();
            }
            else
            {
                baseLevels = 0;
                //MainWindow.print("ERROR: could not find base level token in object: "+obj.ToString());
                //MainWindow.print("Class name: " + className + " :: " + displayName);
            }

            JObject wildLevels = obj["wildLevels"] as JObject;
            if (wildLevels != null)
            {
                //MainWindow.print("wildObj: " + wildLevels.ToString());
                health = wildLevels["health"]!=null ? wildLevels["health"].Value<int>() : 0;
                stam = wildLevels["stamina"]!=null ? wildLevels["stamina"].Value<int>() : 0;
                oxy = wildLevels["oxygen"]!=null ? wildLevels["oxygen"].Value<int>() : 0;
                food = wildLevels["food"]!=null ? wildLevels["food"].Value<int>() : 0;
                weight = wildLevels["weight"]!=null? wildLevels["weight"].Value<int>() : 0;
                damage = wildLevels["melee"]!=null? wildLevels["melee"].Value<int>() : 0;
                speed = wildLevels["speed"]!=null? wildLevels["speed"].Value<int>() : 0;
            }
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
            block.Text = data;
        }
    }
}
