using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Net;
using System.IO;
using System.Web;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
namespace Remote
{
    public partial class Login : Form
    {
        string username;
        string pwd;
        public Login()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen; 
        }

        private void button1_Click(object sender, EventArgs e)
        {
            username = HttpUtility.UrlEncode(textBox1.Text, Encoding.GetEncoding("gb2312"));
            pwd = HttpUtility.UrlEncode(textBox2.Text, Encoding.GetEncoding("gb2312"));
            //发送请求
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create("http://www.zhangzhizhi.cn/page/Remote/do_login.php?username=" + username + "&pwd=" + pwd);

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
                if ((String)obj["code"] == "0")
                {
                    //跳转
                    new Main(username, pwd).Show();
                    this.Hide();
                }
                else {
                    MessageBox.Show("用户名或密码错误");
                }
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message);
            }
        }

        private void textBox2_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1_Click(sender, e);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //跳转
            new Register().Show();
            this.Hide();
        }

        private void Login_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (Form f in Application.OpenForms)
            {
                if (f.Name != "Login")
                {
                    f.Dispose();
                }
            }
        }
    }
}
