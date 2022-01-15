using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
///using seroconf;
using System.IO.Ports;
using System.Windows.Forms;
using NDI.CapiSample;
using NDI.CapiSample.Data;
using NDI.CapiSample.Protocol;
using WindowsFormsApp1;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using ClassLibrary1;


namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        Capi capi;
        string host = "com3";

        //double[,] T_H ={{ -0.0868, 0.1312, -1.2009},
        //                   { -0.0854, -0.0206, -1.2082},
        //                   { -0.2227, 0.1288, -1.1377},
        //                   { -0.0244, 0.1260, -1.0680}};
        //double[,] T_C ={{0.1335, 0.1388, -0.6536},
        //                   {0.0746, 0.0676, -0.7931},
        //                   {0.0910, -0.0166, -0.7579},
        //                   {0.1887,  0.0695, -0.8431}};
        //double[,] T_H = new double[3, 2];
        double[,] T_H = new double[4, 3];
        double[,] T_C = new double[4, 3];

         
        double[,] V_H ={{0, -0.3, 0.3},
                          {0.15, -0.3, 0.3},
                          {0, -0.15, 0.3},
                          {0, -0.3, 0.45}};
        double[,] V_C ={{-0.0825, -0.0475, -0.0625},
                           {0.0825, -0.0475, -0.0625},
                           {0.0825, 0.0475, -0.0625},
                           {0.0825, -0.0475, 0.0625}};

        double[] translation = new double[3];
        double[] eulerangle = new double[3];

        double[] doubledata;



        List<Socket> ClientProxSocketList = new List<Socket>();
        //定义服务器的IP和端口，端口与服务器对应
        public string IPAddress_Server = "192.168.1.121";//可以是局域网或互联网ip
        public string Port_Server = "55608";


        public Form1()
        {
            InitializeComponent();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            capi = new CapiSerial(host);
            Run(capi);
            ServerStart();
            //log("初始化完成");
            // log("接收位置数据");
            richTextBox1.Text = ("初始化完成\n");
            richTextBox1.Text += ("接收位置数据\n");
        }
        #region T_H虚拟点
        private void button2_Click(object sender, EventArgs e)
        {
            Reciever1(capi);
        }
        private void button4_Click(object sender, EventArgs e)
        {
            Reciever2(capi);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Reciever3(capi);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Reciever4(capi);
        }
        #endregion
        #region 获取T_C坐标
        private void button9_Click(object sender, EventArgs e)
        {
            Reciever5(capi);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            Reciever6(capi);
        }

        private void button11_Click(object sender, EventArgs e)
        {
            Reciever7(capi);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            Reciever8(capi);
           // richTextBox1.Text += ("请重新获取位置数据\n");
        }
        #endregion

        private void button7_Click(object sender, EventArgs e)//计算
        {
            for (int i = 0; i < 4; i++)
            {
                T_H[i, 2] = -T_H[i, 2];
                T_C[i, 2] = -T_C[i, 2];
            }
            Matrix<double> Tracker_HoloLens = DenseMatrix.OfArray(T_H);
            Matrix<double> Tracker_Cube = DenseMatrix.OfArray(T_C);
            Matrix<double> Virtual_HoloLens = DenseMatrix.OfArray(V_H);
            Matrix<double> Virtual_Cube = DenseMatrix.OfArray(V_C);

            Matrix<double> tf1 = function.GetTF(Tracker_HoloLens, Virtual_HoloLens);
            Matrix<double> tf2 = function.GetTF(Virtual_Cube, Tracker_Cube);

            Matrix<double> TF = tf1 * tf2;
            Matrix<double> Rotm = TF.SubMatrix(0, 3, 0, 3);

            translation = function.TF_translation(TF);//从转换矩阵提取平移向量
            eulerangle = function.rotm2angle(Rotm);//旋转矩阵转欧拉角
            for (int k = 0; k < eulerangle.Length; k++)//弧度转角度
            {
                eulerangle[k] = eulerangle[k] * 180 / Math.PI;
            }
            Console.WriteLine("{0},{1},{2},{3},{4},{5}", translation[0], translation[1], translation[2], eulerangle[0], eulerangle[1], eulerangle[2]);
            doubledata = new double[6] { translation[0], translation[1], translation[2], eulerangle[0], eulerangle[1], eulerangle[2] };
        }

        private void button8_Click(object sender, EventArgs e)//发送
        {
            SendMsg();
        }





        private void button3_Click(object sender, EventArgs e)
        {
            log("Tracking stoped and disconnected");

            Stopped(capi);
        }



        private static void Run(Capi cAPI)
        {


            if (!cAPI.Connect())
            {
                log("Could not connect to " + cAPI.GetConnectionInfo());
                //  PrintHelp();
                return;
            }
            log("Connected");

            if (!cAPI.Initialize())
            {
                log("Could not initialize.");
                return;
            }
            log("Initialized");


            if (!InitializePorts(cAPI))
            {
                return;
            }
            if (!cAPI.TrackingStart())
            {
                log("Could not start tracking.");
                return;
            }
            log("TrackingStarted");
        }
        #region Reciever
        private void Reciever1(Capi cAPI)
        {
            //for (int i = 0; i < 1; i++)
            // {

            if (!cAPI.IsConnected)
            {
                log("Disconnected while tracking.");

            }
            string s;
            List<Tool> tools = cAPI.SendBX();
            foreach (var t in tools)
            { 
                log(t.ToString());
                s = t.ToString();
                richTextBox1.Text += (s + "\n");
                string a = s.Replace(" ", "");

                string[] str = a.Split(',');

                //string[] str1 = { str[3], str[4], str[5] };
                for (int i = 3; i < 6; ++i)
                {
                    try
                    {
                        double fe = double.Parse(str[i]);
                        T_H[0, i - 3] = fe / 1000;
                    }
                    catch
                    {
                        log("请重新获取位置数据");
                        richTextBox1.Text += "请重新获取位置数据" + " ";
                    }
                }

            }
        }

        private void Reciever2(Capi cAPI)
        {
            //for (int i = 0; i < 1; i++)
            // {

            if (!cAPI.IsConnected)
            {
                log("Disconnected while tracking.");

            }
            string s;
            List<Tool> tools = cAPI.SendBX();
            foreach (var t in tools)
            {
                log(t.ToString());
                s = t.ToString();
                richTextBox1.Text += (s + "\n");
                string a = s.Replace(" ", "");

                string[] str = a.Split(',');

                // string[] str1 = { str[3], str[4], str[5] };
                for (int i = 3; i < 6; ++i)
                {
                    try
                    {
                        double fe = double.Parse(str[i]);
                        T_H[1, i - 3] = fe / 1000;
                    }
                    catch
                    {
                        log("请重新获取位置数据");
                        richTextBox1.Text += "请重新获取位置数据" + " ";
                    }
                }

            }
        }

        private void Reciever3(Capi cAPI)
        {
            //for (int i = 0; i < 1; i++)
            // {

            if (!cAPI.IsConnected)
            {
                log("Disconnected while tracking.");

            }
            string s;
            List<Tool> tools = cAPI.SendBX();
            foreach (var t in tools)
            {
                log(t.ToString());
                s = t.ToString();
                richTextBox1.Text += (s + "\n");
                string a = s.Replace(" ", "");

                string[] str = a.Split(',');

                // string[] str1 = { str[3], str[4], str[5] };
                for (int i = 3; i < 6; ++i)
                {
                    try
                    {
                        double fe = double.Parse(str[i]);
                        T_H[2, i - 3] = fe / 1000;
                    }
                    catch
                    {
                        log("请重新获取位置数据");
                        richTextBox1.Text += "请重新获取位置数据" + " ";
                    }
                }

            }
        }

        private void Reciever4(Capi cAPI)
        {
            //for (int i = 0; i < 1; i++)
            // {

            if (!cAPI.IsConnected)
            {
                log("Disconnected while tracking.");

            }
            string s;
            List<Tool> tools = cAPI.SendBX();
            foreach (var t in tools)
            {
                log(t.ToString());
                s = t.ToString();
                richTextBox1.Text += (s + "\n");
                string a = s.Replace(" ", "");

                string[] str = a.Split(',');

                //string[] str1 = { str[3], str[4], str[5] };
                for (int i = 3; i < 6; ++i)
                {
                    try
                    {
                        double fe = double.Parse(str[i]);
                        T_H[3, i - 3] = fe / 1000;
                    }
                    catch
                    {
                        log("请重新获取位置数据");
                        richTextBox1.Text += "请重新获取位置数据" + " ";
                    }
                }

            }
        }
        private void Reciever5(Capi cAPI)
        {
            //for (int i = 0; i < 1; i++)
            // {

            if (!cAPI.IsConnected)
            {
                log("Disconnected while tracking.");

            }
            string s;
            List<Tool> tools = cAPI.SendBX();
            foreach (var t in tools)
            {
                log(t.ToString());
                s = t.ToString();
                richTextBox1.Text += (s + "\n");
                string a = s.Replace(" ", "");

                string[] str = a.Split(',');

                //string[] str1 = { str[3], str[4], str[5] };
                for (int i = 3; i < 6; ++i)
                {
                    try
                    {
                        double fe = double.Parse(str[i]);
                        T_C[0, i - 3] = fe / 1000;
                    }
                    catch
                    {
                        log("请重新获取位置数据");
                        richTextBox1.Text += "请重新获取位置数据" + " ";
                    }
                }

            }
        }
        private void Reciever6(Capi cAPI)
        {
            //for (int i = 0; i < 1; i++)
            // {

            if (!cAPI.IsConnected)
            {
                log("Disconnected while tracking.");

            }
            string s;
            List<Tool> tools = cAPI.SendBX();
            foreach (var t in tools)
            {
                log(t.ToString());
                s = t.ToString();
                richTextBox1.Text += (s + "\n");
                string a = s.Replace(" ", "");

                string[] str = a.Split(',');

                //string[] str1 = { str[3], str[4], str[5] };
                for (int i = 3; i < 6; ++i)
                {
                    try
                    {
                        double fe = double.Parse(str[i]);
                        T_C[1, i - 3] = fe / 1000;
                    }
                    catch
                    {
                        log("请重新获取位置数据");
                        richTextBox1.Text += "请重新获取位置数据" + " ";
                    }
                }

            }
        }
        private void Reciever7(Capi cAPI)
        {
            //for (int i = 0; i < 1; i++)
            // {

            if (!cAPI.IsConnected)
            {
                log("Disconnected while tracking.");

            }
            string s;
            List<Tool> tools = cAPI.SendBX();
            foreach (var t in tools)
            {
                log(t.ToString());
                s = t.ToString();
                richTextBox1.Text += (s + "\n");
                string a = s.Replace(" ", "");

                string[] str = a.Split(',');

                //string[] str1 = { str[3], str[4], str[5] };
                for (int i = 3; i < 6; ++i)
                {
                    try
                    {
                        double fe = double.Parse(str[i]);
                        T_C[2, i - 3] = fe / 1000;
                    }
                    catch
                    {
                        log("请重新获取位置数据");
                        richTextBox1.Text += "请重新获取位置数据" + " ";
                    }
                }

            }
        }
        private void Reciever8(Capi cAPI)
        {
            //for (int i = 0; i < 1; i++)
            // {

            if (!cAPI.IsConnected)
            {
                log("Disconnected while tracking.");

            }
            string s;
            List<Tool> tools = cAPI.SendBX();
            foreach (var t in tools)
            {
                log(t.ToString());
                s = t.ToString();
                richTextBox1.Text += (s + "\n");
                string a = s.Replace(" ", "");

                string[] str = a.Split(',');

                //string[] str1 = { str[3], str[4], str[5] };
                for (int i = 3; i < 6; ++i)
                {
                    try
                    {
                        double fe = double.Parse(str[i]);
                        T_C[3, i - 3] = fe / 1000;
                    }
                    catch
                    {
                        log("请重新获取位置数据");
                        richTextBox1.Text += "请重新获取位置数据"+" ";
                    }
                   
                }

            }
        }
        #endregion
        private static void Stopped(Capi cAPI)
        {
            if (!cAPI.TrackingStop())
            {
                log("Could not stop tracking.");
                return;
            }
            log("TrackingStopped");

            if (!cAPI.Disconnect())
            {
                log("Could not disconnect.");
                return;
            }
            log("Disconnected");
        }
        private static void Initial(Capi cAPI)
        {

        }
        private static bool InitializePorts(Capi cAPI)
        {
            // Polaris Section
            // ---
            // Request a new tool port handle so that we can load an SROM
            Port tool = cAPI.PortHandleRequest();
            if (tool == null)
            {
                log("Could not get available port for tool.");
            }
            else if (!tool.LoadSROM("sroms/8700449.rom"))
            {
                log("Could not load SROM file for tool.");
                return false;
            }
            Port tool1 = cAPI.PortHandleRequest();
            if (tool1 == null)
            {
                log("Could not get available port for tool.");
            }
            else if (!tool1.LoadSROM("sroms/probe2.rom"))
            {
                log("Could not load SROM file for tool.");
                return false;
            }
            // ---
            // Initialize all ports not currently initialized
            var ports = cAPI.PortHandleSearchRequest(PortHandleSearchType.NotInit);
            foreach (var port in ports)
            {
                if (!port.Initialize())
                {
                    log("Could not initialize port " + port.PortHandle + ".");
                    return false;
                }

                if (!port.Enable())
                {
                    log("Could not enable port " + port.PortHandle + ".");
                    return false;
                }
            }

            log("Enabled Ports:");
            ports = cAPI.PortHandleSearchRequest(PortHandleSearchType.Enabled);
            foreach (var port in ports)
            {
                port.GetInfo();
                log(port.ToString());
            }

            return true;
        }
        public static void log(string message)
        {
            Console.WriteLine(message);
        }


        #region 服务器
        private void ServerStart()
        {
            //1 创建Socket对象
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //2 绑定端口ip
            socket.Bind(new IPEndPoint(IPAddress.Parse(IPAddress_Server), int.Parse(Port_Server)));
            //3 开启侦听
            socket.Listen(10);//连接等待队列：同时来了100个连接请求，队列里放10个等待连接客户端，其他返回错误信息
                              //4 开始接受客户端的连接
            ThreadPool.QueueUserWorkItem(new WaitCallback(this.AcceptClientConnect), socket);
        }

        public void AcceptClientConnect(object socket)
        {
            var serverSocket = socket as Socket;//强制类型转换  

            function.AppendTextToConsole("服务器端开始接受客户端的连接");

            while (true)//不断的接收
            {
                var proxSocket = serverSocket.Accept();//会阻塞当前线程，因此必须放入异步线程池中
                function.AppendTextToConsole(string.Format("客户端{0}连接上了", proxSocket.RemoteEndPoint.ToString()));
                ClientProxSocketList.Add(proxSocket);//使方法体外部也可以访问到方法体内部的数据

                //不停接收当前连接的客户端发送来的消息
                //不能因为接收一个客户端消息阻塞整个线程，启用线程池
                ThreadPool.QueueUserWorkItem(new WaitCallback(this.ReceiveData), proxSocket);
            }
        }
        //接收客户端消息
        public void ReceiveData(object socket)
        {
            var proxSocket = socket as Socket;
            byte[] data = new byte[1024 * 1024];
            while (true)
            {
                int len = 0;
                try
                {
                    len = proxSocket.Receive(data, 0, data.Length, SocketFlags.None);
                }
                catch (Exception ex)
                {
                    //异常退出,在阻塞线程时与服务器连接中断或断电等等
                    function.AppendTextToConsole(string.Format("接收到客户端{0}非正常退出", proxSocket.RemoteEndPoint.ToString()));
                    ClientProxSocketList.Remove(proxSocket);
                    StopConnect(proxSocket);
                    return;
                }
                if (len <= 0)
                {
                    //客户端正常退出
                    function.AppendTextToConsole(string.Format("接收到客户端{0}正常退出", proxSocket.RemoteEndPoint.ToString()));
                    ClientProxSocketList.Remove(proxSocket);

                    StopConnect(proxSocket);
                    return;//让方法结束。终结当前接收客户端数据的异步线程
                }

                //把接收到的数据放到文本框上
                string str = Encoding.UTF8.GetString(data, 0, len);
                function.AppendTextToConsole(string.Format("接收到客户端{0}的消息是:{1}", proxSocket.RemoteEndPoint.ToString(), str));
            }
        }
        private void StopConnect(Socket proxSocket)
        {
            try
            {
                if (proxSocket.Connected)
                {
                    proxSocket.Shutdown(SocketShutdown.Both);
                    proxSocket.Close(100);//100秒后没有正常关闭则强行关闭
                }
            }
            catch (Exception ex)
            {

            }
        }
        //发送字符串
        private void SendMsg()
        {

            foreach (var proxSocket in ClientProxSocketList)
            {

                if (proxSocket.Connected)
                {
                    byte[] data = function.ToBytes(doubledata);
                    proxSocket.Send(data, 0, data.Length, SocketFlags.None);
                }
            }
        }

        #endregion

    }

}

