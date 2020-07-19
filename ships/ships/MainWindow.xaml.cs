using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.IO.Ports;
using System.Windows.Media.Animation;
using System.Data;

namespace ships
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ship[] boats = new ship[2];
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void btnStart_Click(object sender, RoutedEventArgs e)
        {
            sea.Children.Clear();
            int t = -600;
            double[] coord = new double[4];
            boats[0] = new ship(t, sea, "COM5");
            await Task.Delay(1000);
            boats[1] = new ship(t, sea, "COM8");
            double[] shift = { Math.Min(0, Math.Min(boats[0].pos[0], boats[1].pos[0])), Math.Min(0, Math.Min(boats[0].pos[1], boats[1].pos[1])) };
            boats[0].pos[0] -= shift[0];
            boats[1].pos[0] -= shift[0];
            boats[0].pos[1] -= shift[1];
            boats[1].pos[1] -= shift[1];


            sea.Width = Math.Max(shift[0], Math.Max(boats[0].pos[0], boats[1].pos[0]));
            sea.Height = Math.Max(shift[1], Math.Max(boats[0].pos[1], boats[1].pos[1]));

            for (int i = 0; i < 2; i++)
            {
                Canvas.SetLeft(boats[i].mark, boats[i].pos[0]);
                Canvas.SetTop(boats[i].mark, boats[i].pos[1]);
            }
            

            int count = 0;


            List<sound> pop0 = new List<sound>();
            List<sound> pop1 = new List<sound>();
            while (1==1)
            {
                string[] rec = { boats[0].ard.recieve(), boats[1].ard.recieve() };
                count++;
                boats[0].move();
                boats[1].move();
                if (rec[0] != "") { pop0.Add(new sound(boats[0], sea, rec[0])); }
                if (rec[1] != "") { pop1.Add(new sound(boats[1], sea, rec[1])); }
                if (pop0.Count > 0 && pop0[0] != null && pop0[0].delete == true) { pop0.RemoveAt(0); }
                if (pop1.Count > 0 && pop1[0] != null && pop1[0].delete == true) { pop1.RemoveAt(0); }
                if (pop0.Count > 0)
                {
                    int a = collision(pop0, boats[1]);
                    if (a != -1) 
                    {
                        boats[1].ard.send(pop0[a].msg);
                        transmission.AppendText(pop0[a].msg);
                        pop0.RemoveAt(a); 

                    }

                }
                if (pop1.Count > 0)
                {
                    int b = collision(pop1, boats[0]);
                    if (b != -1) 
                    {
                        boats[0].ard.send(pop1[b].msg);
                        transmission.AppendText(pop1[b].msg);
                        pop1.RemoveAt(b); 
                    }
                }
                

                await Task.Delay(1);
            }
        }

        private int collision(List<sound> pops, ship boat)
        {
            double[] boatLoc = { Canvas.GetLeft(boat.mark) + boat.mark.Width / 2, Canvas.GetTop(boat.mark) + boat.mark.Height / 2 };
            
            for (int i = 0; i < pops.Count; i++)
            {
                sound pop = pops[i];
                if (pop.mark.Width > 0)
                {
                    double[] soundLoc = { Canvas.GetLeft(pop.mark) + pop.mark.Width / 2, Canvas.GetTop(pop.mark) + pop.mark.Height / 2 };
                    double circle = Math.Pow(boatLoc[0] - soundLoc[0], 2) + Math.Pow(boatLoc[1] - soundLoc[0], 2);
                    if (circle < Math.Pow(pop.mark.Width / 2, 2))
                    {
                        return i;
                    }
                }
            }
            return -1;
        }
    }

    public class ship
    {
        public double speed;
        public double bearing;
        public double[] vel = new double[2];
        public double[] pos = new double[2];
        public Ellipse mark = new Ellipse();
        public arduino ard;

        public ship(int t, Canvas sea, string com)
        {
            Random rnd = new Random();
            speed = rnd.NextDouble() * 7 + 6;
            bearing = rnd.NextDouble() * Math.PI;
            vel[0] = speed * Math.Sin(bearing);
            vel[1] = speed * Math.Cos(bearing);
            for (int i = 0; i < 2; i++) { pos[i] = vel[i] * t; }

            SolidColorBrush mySolidColorBrush = new SolidColorBrush();
            mySolidColorBrush.Color = Color.FromArgb(255, 255, 0, 0);
            mark.Fill = mySolidColorBrush;
            mark.StrokeThickness = 2;
            mark.Stroke = Brushes.Black;
            mark.Width = 300;
            mark.Height = 300;
            sea.Children.Add(mark);

            ard = new arduino(com);
        }


        public void move()
        {
            pos[0] = Canvas.GetLeft(mark);
            pos[1] = Canvas.GetTop(mark);
            DoubleAnimation[] move = { new DoubleAnimation(), new DoubleAnimation() };
            for (int i = 0; i < 2; i++)
            {
                move[i].By = vel[i] * 1;
                move[i].Duration = TimeSpan.FromMilliseconds(1);
            }
            mark.BeginAnimation(Canvas.LeftProperty, move[0], 0);
            mark.BeginAnimation(Canvas.TopProperty, move[1], 0);
        }
    }

    public class arduino
    {
        public SerialPort _serialPort;

        public arduino(string com)
        {
            _serialPort = new SerialPort(com, 9600);
            _serialPort.Open();
        }

        public string recieve()
        {
            string a = _serialPort.ReadExisting();
            return a;
        }

        public void send(string msg)
        {
            
            _serialPort.Write(msg);
        }

    }

    public class sound
    {
        public Ellipse mark = new Ellipse();
        private Canvas seas;
        public bool delete = false;
        public string msg;

        public sound(ship boat, Canvas sea, string message)
        {
            msg = message;
            seas = sea;
            double c = 3.43;
            DoubleAnimation grow = new DoubleAnimation();
            DoubleAnimation move = new DoubleAnimation();
            mark.StrokeThickness = 20;
            mark.Stroke = Brushes.Black;
            mark.Width = 0;
            mark.Height = 0;
            sea.Children.Add(mark);
            Canvas.SetLeft(mark, boat.pos[0]);
            Canvas.SetTop(mark, boat.pos[1]);
            grow.From = 0; 
            grow.To = 6000 * c * 2;
            grow.Duration = TimeSpan.FromMilliseconds(6000);

            move.By = -1 * grow.To / 2;
            move.Duration = grow.Duration;
            grow.Completed += new EventHandler(grow_comp);
            mark.BeginAnimation(Ellipse.WidthProperty, grow, 0);
            mark.BeginAnimation(Ellipse.HeightProperty, grow, 0);
            mark.BeginAnimation(Canvas.LeftProperty, move, 0); 
            mark.BeginAnimation(Canvas.TopProperty, move, 0);
                        
        }

        private void grow_comp(object sender, EventArgs e)
        {
            seas.Children.Remove(mark);
            delete = true;
        }
    }
}
