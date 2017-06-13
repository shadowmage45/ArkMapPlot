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
            
            string[] classNameMap = new string[0];
            int len = classNameMap.Length;
            for (int i = 0; i < len; i++)
            {
                displayName = classNameMap[i].Split(',')[0];
                className = classNameMap[i].Split(',')[1];
                fileName = className + ".json";
                fileData = File.ReadAllText(fileName);
                obj = JObject.Parse(fileData);
                arr = obj.Children<JArray>().First();
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
            };
            MemberList.SelectionChanged += delegate (object sender, SelectionChangedEventArgs e)
            {
                Console.Out.WriteLine("Selected memeber: " + MemberList.SelectedItem);
            };
        }

        private void loadMap(string configFileName)
        {
            string mapName = string.Empty;
            string mapImageFileName = string.Empty;            
            System.Drawing.Image img = System.Drawing.Image.FromFile(mapImageFileName);
            int width = img.Width;
            int height = img.Height;

            BitmapSource src = BitmapSource.Create(width, height, 100, 100, PixelFormats.Rgb24, null, new Bitmap(img)., 8*3);

            MapImage.Source = src;
        }

        private void updateMemberList(string displayName)
        {
            System.Console.Out.WriteLine("Selected: " + displayName);
            string className = displayToClass[displayName];
            ClassData data = classData[className];
            MemberList.Items.Clear();
            foreach (MemberData member in data.members)
            {
                MemberList.Items.Add(member.displayName);
            }

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

        }
    }

    public class MemberData
    {
        public readonly string className;
        public readonly MemberInfo data;
        public virtual string displayName { get { return data.displayName; } }
        public MemberData(string className)
        {
            this.className = className;
            data = new MemberInfoDino();
        }
    }

    public class MemberInfo
    {        
        public virtual string displayName { get { return GetHashCode().ToString(); } }
    }

    public class MemberInfoDino : MemberInfo
    {

    }
}
