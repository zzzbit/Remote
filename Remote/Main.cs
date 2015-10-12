using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Threading;
using System.Net;
using System.IO;
using System.Web;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
//操作系统信息
using System.Management; 
namespace Remote
{
    public partial class Main : Form
    {
        int n = 0;
        bool stop = false;
        string username;
        string pwd;
        int sleeptime = 3000;
        public Main(string username,string pwd)
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
            this.username = username;
            this.pwd = pwd;
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (Form f in Application.OpenForms)
            {
                if (f.Name != "Main")
                {
                    f.Dispose();
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (stop == true)
            {
                timer1.Enabled = false;
                stop = false;
                label1.Text = "已停止监控";
                button1.Text = "启用被监控";
            }
            else
            {
                timer1.Enabled = true;
                stop = true;
                label1.Text = "正在监控";
                button1.Text = "停止监控";
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //获取是否有任务需要执行
            //发送请求
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create("http://www.zhangzhizhi.cn/page/Remote/do_execute.php?type=1&username=" + username + "&pwd=" + pwd);

            //获得响应
            string res = string.Empty;
            try
            {
                //获取响应流
                HttpWebResponse response = (HttpWebResponse)myReq.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("GB2312"));
                res = reader.ReadToEnd();
                reader.Close();
                response.Close();
                //操作返回值
                JObject obj = JObject.Parse(res);
                string message = null;
                if ((String)obj["code"] == "0")
                {
                    foreach (JObject msg in obj["msg"])
                    {
                        listBox1.Items.Add((String)msg["myorder"] + ":" + (String)msg["arg"]);
                        switch ((String)msg["myorder"])
                        {
                            case "basic":
                                message = System.Net.Dns.GetHostName();
                                feedback(message, (String)msg["id"]);
                                break;
                            case "os":
                                message = Environment.OSVersion.ToString();
                                feedback(message, (String)msg["id"]);
                                break;
                            case "exe":
                                System.Diagnostics.Process.Start((String)msg["arg"]);
                                feedback("exe", (String)msg["id"]);
                                break;
                            default:
                                feedback("default", (String)msg["id"]);
                                break;
                        }
                    }
                }
                else
                {
                }
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message);
            }
        }


        private string request(string type,string order,string arg)
        {
            order = HttpUtility.UrlEncode(order, Encoding.GetEncoding("gb2312"));
            arg = HttpUtility.UrlEncode(arg, Encoding.GetEncoding("gb2312"));
            //发送请求
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create("http://www.zhangzhizhi.cn/page/Remote/do_request.php?type=" + type + "&username=" + username + "&pwd=" + pwd + "&myorder=" + order + "&arg=" + arg);

            //获得响应
            string res = string.Empty;
            try
            {
                //获取响应流
                HttpWebResponse response = (HttpWebResponse)myReq.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("GB2312"));
                res = reader.ReadToEnd();
                reader.Close();
                response.Close();
                //操作返回值
                JObject obj = JObject.Parse(res);
                return (String)obj["id"];
            }
            catch (Exception)
            {
                return "0";
            }
        }
        private void getresult(object id)
        {
            int i;
            for (i = 0; i < 5; i++)
            {
                //发送请求
                HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create("http://www.zhangzhizhi.cn/page/Remote/do_getresult.php?id=" + (String)id + "&username=" + username + "&pwd=" + pwd);

                //获得响应
                string res = string.Empty;
                try
                {
                    //获取响应流
                    HttpWebResponse response = (HttpWebResponse)myReq.GetResponse();
                    StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("GB2312") );
                    res = reader.ReadToEnd();
                    reader.Close();
                    response.Close();
                    //操作返回值
                    JObject obj = JObject.Parse(res);
                    if ((String)obj["code"] == "0")
                    {
                        MessageBox.Show((String)obj["result"]);
                        textBox1.Text = "";
                        break;
                    }
                    else
                    {
                        Thread.Sleep(sleeptime);
                    }
                }
                catch (Exception)
                {

                }
            }
            if (i == 5)
            {
                MessageBox.Show("被监控设备未在线");
            }
        }
        private string feedback(string result, string id)
        {
            result = HttpUtility.UrlEncode(result, Encoding.GetEncoding("gb2312"));
            //发送请求
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create("http://www.zhangzhizhi.cn/page/Remote/do_feedback.php?id=" + id + "&username=" + username + "&pwd=" + pwd + "&result=" + result);

            //获得响应
            string res = string.Empty;
            try
            {
                //获取响应流
                HttpWebResponse response = (HttpWebResponse)myReq.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("GB2312"));
                res = reader.ReadToEnd();
                reader.Close();
                response.Close();
                //操作返回值
                JObject obj = JObject.Parse(res);
                return (String)obj["code"];
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string id = request("1", "basic", "0");
            if (id == "0")
            {
                MessageBox.Show("请求失败");
            }
            else
            {
                Thread thread = new Thread(new ParameterizedThreadStart(getresult));
                thread.Start(id);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string id = request("1","exe" ,textBox1.Text);
            if (id == "0")
            {
                MessageBox.Show("请求失败");
            }
            else
            {
                Thread thread = new Thread(new ParameterizedThreadStart(getresult));
                thread.Start(id);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string id = request("1", "os", "0");
            if (id == "0")
            {
                MessageBox.Show("请求失败");
            }
            else
            {
                Thread thread = new Thread(new ParameterizedThreadStart(getresult));
                thread.Start(id);
            }
        }

        private void button13_Click(object sender, EventArgs e)
        {
            string id = request("2", "hello", "0");
            if (id == "0")
            {
                MessageBox.Show("请求失败");
            }
            else
            {
                Thread thread = new Thread(new ParameterizedThreadStart(getresult));
                thread.Start(id);
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            string id = request("2", "rmessage", "0");
            if (id == "0")
            {
                MessageBox.Show("请求失败");
            }
            else
            {
                Thread thread = new Thread(new ParameterizedThreadStart(getresult));
                thread.Start(id);
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            string id = request("2", "wmessage", "0");
            if (id == "0")
            {
                MessageBox.Show("请求失败");
            }
            else
            {
                Thread thread = new Thread(new ParameterizedThreadStart(getresult));
                thread.Start(id);
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            string id = request("2", "makephone", "0");
            if (id == "0")
            {
                MessageBox.Show("请求失败");
            }
            else
            {
                Thread thread = new Thread(new ParameterizedThreadStart(getresult));
                thread.Start(id);
            }
        }
    }

}
