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
        private MemberData selectedMember = null;
        private Dictionary<MemberData, Image> pins = new Dictionary<ArkMapPlot.MemberData, Image>();

        public static string pinRedPath = "pin_red.png";
        public static string pinBluePath = "pin_blue.png";
        public static BitmapImage pinRedImage;
        public static BitmapImage pinBlueImage;
        public static string logFile = "log.txt";

        public MainWindow()
        {
            StreamWriter sw = new StreamWriter(new FileStream(logFile, FileMode.OpenOrCreate));
            System.Console.SetOut(sw);
            try
            {
                loadData();
                InitializeComponent();
                populateWindow();
            }
            catch (Exception e)
            {
                print(e.StackTrace);
            }
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

            pinRedImage = loadImage(pinRedPath);
            pinBlueImage = loadImage(pinBluePath);
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
                string cName = displayToClass[(string)ClassList.SelectedItem];
                selectedMember = null;
                updateMapPins(classData[cName]);
                print("Selected class: " + ClassList.SelectedItem);
            };
            MemberData.SelectionChanged += delegate (object sender, SelectionChangedEventArgs e)
            {
                string cName = (string)ClassList.SelectedItem;
                MemberData member = (MemberData)MemberData.SelectedItem;
                selectedMember = member;
                updateMemberData(member);
                updateMapPins(classData[displayToClass[cName]]);
                print("Selected member: " + member);
            };
            //MapImage.MouseMove += delegate (object sender, MouseEventArgs e)
            //{   
            //    System.Windows.Point pt = e.GetPosition(MapImage);
            //    Point pt1 = e.GetPosition(MapCanvas);
            //};
            //MapImage.MouseLeftButtonDown += delegate (object sender, MouseButtonEventArgs e)
            //{
            //    System.Windows.Point pt = e.GetPosition(MapImage);
            //    print("Clck-Pixels: " + pt);
            //};
            //MapImage.MouseWheel += delegate (object sender, MouseWheelEventArgs e)
            //{
            //    print("Wheel: " + e.Delta);
            //    //TransformGroup tg = MapCanvas.tra
            //    //var rt = MapCanvas.RenderTransform;
            //    //((ScaleTransform)rt).ScaleX += e.Delta;
            //    //((ScaleTransform)rt).ScaleY += e.Delta;
            //    //MapCanvas.RenderTransform = rt;
            //};
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
            MapCanvas.Children.RemoveRange(1, MapCanvas.Children.Count - 1);
            int len = data.members.Count;
            MemberData mData;
            float x, y;
            float width = 1430;
            float height = 1430;
            float latMax = 100;
            float lonMax = 100;
            for (int i = 0; i < len; i++)
            {
                mData = data.members[i];
                Image pin = new Image();
                pin.IsHitTestVisible = false;
                if (mData == selectedMember) { continue; }
                pin.Source = pinBlueImage;
                x = (mData.Lon / lonMax) * width - 16;
                y = (mData.Lat / latMax) * height - 16;
                MapCanvas.Children.Add(pin);
                Canvas.SetLeft(pin, x);
                Canvas.SetTop(pin, y);
                print("Added Pin at: "+x+","+y);
            }
            if (selectedMember != null)
            {
                mData = selectedMember;
                Image pin = new Image();
                pin.IsHitTestVisible = false;
                pin.Source = pinRedImage;
                x = (mData.Lon / lonMax) * width - 16;
                y = (mData.Lat / latMax) * height - 16;
                MapCanvas.Children.Add(pin);                
                Canvas.SetLeft(pin, x);
                Canvas.SetTop(pin, y);
                print("Added Selected Member Pin at: " + x + "," + y);
            }
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
            Console.Out.Flush();
        }

        public static void print(object var)
        {
            Console.Out.WriteLine(var == null ? "null" : var.ToString());
            Console.Out.Flush();
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
        public float Lat { get { return lat; } }
        public float Lon { get { return lon; } }
        public float X { get { return x; } }
        public float Y { get { return y; } }

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
    
    public class ZoomBorder : Border
    {
        private UIElement child = null;
        private Point origin;
        private Point start;

        private TranslateTransform GetTranslateTransform(UIElement element)
        {
            return (TranslateTransform)((TransformGroup)element.RenderTransform)
              .Children.First(tr => tr is TranslateTransform);
        }

        private ScaleTransform GetScaleTransform(UIElement element)
        {
            return (ScaleTransform)((TransformGroup)element.RenderTransform)
              .Children.First(tr => tr is ScaleTransform);
        }

        public override UIElement Child
        {
            get { return base.Child; }
            set
            {
                if (value != null && value != this.Child)
                    this.Initialize(value);
                base.Child = value;
            }
        }

        public void Initialize(UIElement element)
        {
            this.child = element;
            if (child != null)
            {
                TransformGroup group = new TransformGroup();
                ScaleTransform st = new ScaleTransform();
                group.Children.Add(st);
                TranslateTransform tt = new TranslateTransform();
                group.Children.Add(tt);
                child.RenderTransform = group;
                child.RenderTransformOrigin = new Point(0.0, 0.0);
                this.MouseWheel += child_MouseWheel;
                this.MouseLeftButtonDown += child_MouseLeftButtonDown;
                this.MouseLeftButtonUp += child_MouseLeftButtonUp;
                this.MouseMove += child_MouseMove;
                this.PreviewMouseRightButtonDown += new MouseButtonEventHandler(
                  child_PreviewMouseRightButtonDown);
            }
        }

        public void Reset()
        {
            if (child != null)
            {
                // reset zoom
                var st = GetScaleTransform(child);
                st.ScaleX = 1.0;
                st.ScaleY = 1.0;

                // reset pan
                var tt = GetTranslateTransform(child);
                tt.X = 0.0;
                tt.Y = 0.0;
            }
        }

        #region Child Events

        private void child_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (child != null)
            {
                var st = GetScaleTransform(child);
                var tt = GetTranslateTransform(child);

                double zoom = e.Delta > 0 ? .2 : -.2;
                if (!(e.Delta > 0) && (st.ScaleX < .4 || st.ScaleY < .4))
                    return;

                Point relative = e.GetPosition(child);
                double abosuluteX;
                double abosuluteY;

                abosuluteX = relative.X * st.ScaleX + tt.X;
                abosuluteY = relative.Y * st.ScaleY + tt.Y;

                st.ScaleX += zoom;
                st.ScaleY += zoom;

                tt.X = abosuluteX - relative.X * st.ScaleX;
                tt.Y = abosuluteY - relative.Y * st.ScaleY;
            }
        }

        private void child_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (child != null)
            {
                var tt = GetTranslateTransform(child);
                start = e.GetPosition(this);
                origin = new Point(tt.X, tt.Y);
                this.Cursor = Cursors.Hand;
                child.CaptureMouse();
            }
        }

        private void child_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (child != null)
            {
                child.ReleaseMouseCapture();
                this.Cursor = Cursors.Arrow;
            }
        }

        void child_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Reset();
        }

        private void child_MouseMove(object sender, MouseEventArgs e)
        {
            if (child != null)
            {
                if (child.IsMouseCaptured)
                {
                    var tt = GetTranslateTransform(child);
                    Vector v = start - e.GetPosition(this);
                    tt.X = origin.X - v.X;
                    tt.Y = origin.Y - v.Y;
                }
            }
        }

        #endregion
    }

}
